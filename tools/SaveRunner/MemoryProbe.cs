using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using HarmonyLib;
using KMod;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Profiling;

namespace SaveRunner
{
	/// <summary>
	/// The "memory" probe: where a modded game's memory goes. It starts in SaveRunnerMod.OnLoad, so
	/// the launcher puts SaveRunner first in mods.json and every other mod loads after the probe is
	/// measuring. Writes three files next to --saverun-out:
	///   probe.csv          one row per reading: boot, every mod-loading pass, the main menu every
	///                      five real seconds, the load, then every game second of the run
	///   probe-modload.csv  one row per mod and loading pass (DLL, Animation, ...; PostLoad is
	///                      OnAllModsLoaded): what the process and the heaps grew by while that mod loaded
	///   probe-snapshot.csv what the memory is made of at the end of the run (MemorySnapshot)
	/// Mod.Load and Mod.PostLoad are patched by hand here, only for this probe, so the other probes
	/// run exactly as before.
	/// </summary>
	public static class MemoryProbe
	{
		public const string Header = "t,phase,realS,cycle,privateMB,workingSetMB,peakWorkingSetMB,monoUsedMB,monoHeapMB,gcTotalMB,gcCount,"
			+ "unityAllocMB,unityReservedMB,unityUnusedReservedMB,gfxUsedMB,gfxReservedMB,systemUsedMB,textureMB,textureDesiredMB,textureCount,sysCommitMB,sysCommitLimitMB";
		private const string ModLoadHeader = "seq,pass,modId,title,ms,privateMB,privateDeltaMB,workingSetDeltaMB,monoUsedDeltaMB,monoHeapDeltaMB,"
			+ "gcTotalDeltaMB,unityAllocDeltaMB,unityReservedDeltaMB,gcRuns,newTextures,newTextureMB,newReadableMB,newKanims,assemblies";
		private const float MenuEverySeconds = 5f;
		private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

		public static bool On;
		private static StreamWriter series;
		private static StreamWriter modLoad;
		private static int modSeq;
		private static float lastMenuSample = -1f;
		private static bool loadSampled;
		private static ProfilerRecorder gfxUsed, gfxReserved, systemUsed;

		/// <summary>Texture instance id to the id of the mod that created it while it loaded.</summary>
		public static readonly Dictionary<int, string> TextureOwner = new Dictionary<int, string>();
		/// <summary>Mod assembly to the id of the mod that loaded it.</summary>
		public static readonly Dictionary<Assembly, string> AssemblyOwner = new Dictionary<Assembly, string>();
		/// <summary>Per mod id: its title, kanims it added, and its total load deltas.</summary>
		public static readonly Dictionary<string, ModTotals> Mods = new Dictionary<string, ModTotals>();

		public sealed class ModTotals
		{
			public string Title;
			public int Kanims;
			public long PrivateDelta;
			public long MonoHeapDelta;
			public long UnityReservedDelta;
		}

		/// <summary>A copy of every number one row needs, taken in one go.</summary>
		public struct Reading
		{
			public float RealS;
			public long Private, WorkingSet, PeakWorkingSet, MonoUsed, MonoHeap, GcTotal, UnityAlloc, UnityReserved, UnityUnusedReserved;
			public long GfxUsed, GfxReserved, SystemUsed, Texture, TextureDesired, TextureCount, SysCommit, SysCommitLimit;
			public int GcCount;
		}

		/// <summary>Opens the files, writes the first row and hooks mod loading. Called from OnLoad.</summary>
		public static void Start(Harmony harmony)
		{
			try
			{
				StartCore(harmony);
			}
			catch (Exception e)
			{
				Fail("start", e);
				Close();
			}
		}

		private static void StartCore(Harmony harmony)
		{
			On = true;
			gfxUsed = Recorder("Gfx Used Memory");
			gfxReserved = Recorder("Gfx Reserved Memory");
			systemUsed = Recorder("System Used Memory");
			if (Runner.OutFile != null)
			{
				try
				{
					series = new StreamWriter(Runner.OutFile, append: false, Encoding.UTF8);
					series.WriteLine(Header);
					modLoad = new StreamWriter(Sibling("modload"), append: false, Encoding.UTF8);
					modLoad.WriteLine(ModLoadHeader);
				}
				catch (Exception e)
				{
					Fail("opening its files", e);
				}
			}
			int before = ModsLoadedBefore();
			if (before > 0)
				Debug.LogWarning(Runner.Tag + before + " mod(s) loaded their DLLs before SaveRunner; their load deltas are missing. SaveRunner must be first in mods.json.");
			Row("", "boot", Read());
			Hook(harmony, AccessTools.Method(typeof(Mod), nameof(Mod.Load), new[] { typeof(Content) }), nameof(ModLoadPrefix), nameof(ModLoadPostfix), "Mod.Load");
			Hook(harmony, AccessTools.Method(typeof(Mod), nameof(Mod.PostLoad)), nameof(PostLoadPrefix), nameof(PostLoadPostfix), "Mod.PostLoad");
			// The DLL pass that is running now ends without this postfix; the prefix of the next pass
			// (Strings) marks its end instead.
			Hook(harmony, AccessTools.Method(typeof(KMod.Manager), nameof(KMod.Manager.Load), new[] { typeof(Content) }), nameof(PassPrefix), nameof(PassPostfix), "Manager.Load");
			Debug.Log(Runner.Tag + "memory probe on" + (before == 0 ? ", first in the load order" : ""));
		}

		private static bool warned;

		/// <summary>Logs the first failure of the probe as a warning; later ones stay silent.</summary>
		private static void Fail(string where, Exception e)
		{
			if (warned)
				return;
			warned = true;
			Debug.LogWarning(Runner.Tag + "memory probe, " + where + ": " + e + ". The probe skips from here.");
		}

		/// <summary>The other enabled mods whose DLLs were already loaded when SaveRunner's OnLoad ran.</summary>
		private static int ModsLoadedBefore()
		{
			int n = 0;
			try
			{
				foreach (Mod mod in Global.Instance.modManager.mods)
					if (mod.IsEnabledForActiveDlc() && (mod.loaded_content & Content.DLL) != 0 && mod.staticID != "Isochronous.SaveRunner")
						n++;
			}
			catch (Exception e)
			{
				Debug.LogWarning(Runner.Tag + "cannot read the mod list: " + e.Message);
			}
			return n;
		}

		private static void Hook(Harmony harmony, MethodInfo target, string prefix, string postfix, string name)
		{
			if (target == null)
			{
				Debug.LogWarning(Runner.Tag + name + " not found; the memory probe has no rows for it");
				return;
			}
			try
			{
				harmony.Patch(target, prefix: new HarmonyMethod(typeof(MemoryProbe), prefix), postfix: new HarmonyMethod(typeof(MemoryProbe), postfix));
			}
			catch (Exception e)
			{
				Debug.LogWarning(Runner.Tag + "cannot hook " + name + ": " + e.Message);
			}
		}

		private static ProfilerRecorder Recorder(string counter)
		{
			try { return ProfilerRecorder.StartNew(ProfilerCategory.Memory, counter); }
			catch (Exception e)
			{
				Debug.LogWarning(Runner.Tag + "no profiler counter " + counter + ": " + e.Message);
				return default;
			}
		}

		/// <summary>State carried from a Mod.Load or Mod.PostLoad prefix to its postfix.</summary>
		public sealed class LoadState
		{
			public string Pass;
			public Reading Before;
			public HashSet<int> Textures;
			public int Kanims;
		}

		public static void ModLoadPrefix(Mod __instance, Content content, out LoadState __state)
		{
			__state = null;
			if (!On)
				return;
			try
			{
				Content todo = content & __instance.available_content & ~__instance.loaded_content;
				if (todo == 0)
					return;
				__state = Begin(todo.ToString(), (todo & (Content.DLL | Content.Animation)) != 0);
			}
			catch (Exception e)
			{
				Fail("Mod.Load prefix", e);
				__state = null;
			}
		}

		public static void ModLoadPostfix(Mod __instance, LoadState __state)
		{
			if (__state != null)
				End(__instance, __state);
		}

		public static void PostLoadPrefix(Mod __instance, out LoadState __state)
		{
			__state = null;
			try
			{
				if (On && (__instance.loaded_content & Content.DLL) != 0)
					__state = Begin("PostLoad", true);
			}
			catch (Exception e)
			{
				Fail("Mod.PostLoad prefix", e);
				__state = null;
			}
		}

		public static void PostLoadPostfix(Mod __instance, LoadState __state)
		{
			if (__state != null)
				End(__instance, __state);
		}

		public static void PassPrefix(Content content)
		{
			try
			{
				if (On)
					Row("", "before-" + content, Read());
			}
			catch (Exception e) { Fail("Manager.Load prefix", e); }
		}

		public static void PassPostfix(Content content)
		{
			try
			{
				if (On)
					Row("", "after-" + content, Read());
			}
			catch (Exception e) { Fail("Manager.Load postfix", e); }
		}

		// The texture scan allocates, so it runs before the first reading and after the second.
		private static LoadState Begin(string pass, bool scanTextures)
		{
			LoadState state = new LoadState { Pass = pass, Kanims = Assets.ModLoadedKAnims.Count };
			if (scanTextures)
			{
				state.Textures = new HashSet<int>();
				foreach (Texture tex in Resources.FindObjectsOfTypeAll<Texture>())
					if (tex != null)
						state.Textures.Add(tex.GetInstanceID());
			}
			state.Before = Read();
			return state;
		}

		private static void End(Mod mod, LoadState state)
		{
			try
			{
				Reading after = Read();
				string id = mod.label.id ?? mod.staticID ?? "";
				if (!Mods.TryGetValue(id, out ModTotals totals))
					Mods[id] = totals = new ModTotals { Title = mod.title };
				int newTextures = 0;
				long newBytes = 0, newReadable = 0;
				if (state.Textures != null)
				{
					foreach (Texture tex in Resources.FindObjectsOfTypeAll<Texture>())
					{
						if (tex == null || state.Textures.Contains(tex.GetInstanceID()))
							continue;
						newTextures++;
						long bytes = MemorySnapshot.TextureBytes(tex);
						newBytes += bytes;
						if (MemorySnapshot.IsReadable(tex))
							newReadable += bytes;
						TextureOwner[tex.GetInstanceID()] = id;
					}
				}
				int newKanims = Math.Max(0, Assets.ModLoadedKAnims.Count - state.Kanims);
				totals.Kanims += newKanims;
				int assemblies = 0;
				if (state.Pass.Contains("DLL") && mod.loaded_mod_data?.dlls != null)
					foreach (Assembly asm in mod.loaded_mod_data.dlls)
					{
						AssemblyOwner[asm] = id;
						assemblies++;
					}
				totals.PrivateDelta += after.Private - state.Before.Private;
				totals.MonoHeapDelta += after.MonoHeap - state.Before.MonoHeap;
				totals.UnityReservedDelta += after.UnityReserved - state.Before.UnityReserved;
				modSeq++;
				modLoad?.WriteLine(string.Join(",",
					modSeq.ToString(Inv),
					state.Pass.Replace(", ", "+"),
					Csv(id),
					Csv(mod.title),
					((after.RealS - state.Before.RealS) * 1000f).ToString("F0", Inv),
					Mb(after.Private),
					Mb(after.Private - state.Before.Private),
					Mb(after.WorkingSet - state.Before.WorkingSet),
					Mb(after.MonoUsed - state.Before.MonoUsed),
					Mb(after.MonoHeap - state.Before.MonoHeap),
					Mb(after.GcTotal - state.Before.GcTotal),
					Mb(after.UnityAlloc - state.Before.UnityAlloc),
					Mb(after.UnityReserved - state.Before.UnityReserved),
					(after.GcCount - state.Before.GcCount).ToString(Inv),
					state.Textures != null ? newTextures.ToString(Inv) : "",
					state.Textures != null ? Mb(newBytes) : "",
					state.Textures != null ? Mb(newReadable) : "",
					newKanims.ToString(Inv),
					assemblies.ToString(Inv)));
				modLoad?.Flush();
			}
			catch (Exception e)
			{
				Fail("load of " + mod?.title, e);
			}
		}

		/// <summary>Main menu and load: a row every five real seconds until the load starts.</summary>
		public static void MenuTick()
		{
			if (!On || Runner.Started)
				return;
			float now = Time.realtimeSinceStartup;
			if (lastMenuSample >= 0f && now - lastMenuSample < MenuEverySeconds)
				return;
			lastMenuSample = now;
			Row("", Runner.MenuSeenAt >= 0f ? "menu" : "boot", Read());
		}

		public static void Loading()
		{
			if (On)
				Row("", "loading", Read());
		}

		public static void Loaded()
		{
			if (On && !loadSampled)
			{
				loadSampled = true;
				Row("", "loaded", Read());
			}
		}

		/// <summary>
		/// First update of the run: one row as it is and one after a full collection, so the live
		/// managed heap is a number two runs can compare.
		/// </summary>
		public static void Begin()
		{
			if (!On)
				return;
			Row("0", "run-start", Read());
			GC.Collect();
			GC.WaitForPendingFinalizers();
			Row("0", "run-start-gc", Read());
		}

		public static void Sample(int t, bool toLog)
		{
			if (!On)
				return;
			Reading r = Read();
			Row(t.ToString(Inv), "run", r);
			if (toLog)
				Debug.Log(Runner.Tag + "t=" + t + ": private " + Mb(r.Private) + " MB, mono heap " + Mb(r.MonoHeap) + " MB, unity reserved " + Mb(r.UnityReserved) + " MB");
		}

		/// <summary>End of the run: a row, a row after a full collection, then the snapshot.</summary>
		public static void End(int t)
		{
			if (!On)
				return;
			string ts = t.ToString(Inv);
			Row(ts, "run-end", Read());
			GC.Collect();
			GC.WaitForPendingFinalizers();
			Row(ts, "run-end-gc", Read());
			if (Runner.OutFile == null)
				return;
			float started = Time.realtimeSinceStartup;
			try
			{
				MemorySnapshot.Write(Sibling("snapshot"));
				Debug.Log(Runner.Tag + "memory snapshot written in " + (Time.realtimeSinceStartup - started).ToString("F1", Inv) + " s");
			}
			catch (Exception e)
			{
				Debug.LogWarning(Runner.Tag + "memory snapshot failed: " + e);
			}
			Row(ts, "after-snapshot", Read());
		}

		/// <summary>Flushes and closes the files; called by Runner.Finish whatever the status.</summary>
		public static void Close()
		{
			if (!On)
				return;
			On = false;
			Dispose(ref series);
			Dispose(ref modLoad);
			Dispose(ref gfxUsed);
			Dispose(ref gfxReserved);
			Dispose(ref systemUsed);
		}

		private static void Dispose(ref StreamWriter writer)
		{
			StreamWriter w = writer;
			writer = null;
			try
			{
				w?.Flush();
			}
			catch (Exception e) { Fail("close", e); }
			try
			{
				w?.Dispose();
			}
			catch (Exception e) { Fail("close", e); }
		}

		private static void Dispose(ref ProfilerRecorder recorder)
		{
			try
			{
				recorder.Dispose();
			}
			catch (Exception e) { Fail("close", e); }
		}

		private static string Sibling(string suffix)
		{
			string dir = Path.GetDirectoryName(Runner.OutFile) ?? "";
			return Path.Combine(dir, Path.GetFileNameWithoutExtension(Runner.OutFile) + "-" + suffix + ".csv");
		}

		private static void Row(string t, string phase, Reading r)
		{
			if (series == null)
				return;
			try
			{
				WriteRow(t, phase, r);
			}
			catch (Exception e)
			{
				Fail("writing probe.csv", e);
				series = null;
			}
		}

		private static void WriteRow(string t, string phase, Reading r)
		{
			string cycle = "";
			try
			{
				if (GameClock.Instance != null)
					cycle = GameClock.Instance.GetCycle().ToString(Inv);
			}
			catch (Exception) { }
			series.WriteLine(string.Join(",",
				t,
				phase,
				r.RealS.ToString("F1", Inv),
				cycle,
				Mb(r.Private),
				Mb(r.WorkingSet),
				Mb(r.PeakWorkingSet),
				Mb(r.MonoUsed),
				Mb(r.MonoHeap),
				Mb(r.GcTotal),
				r.GcCount.ToString(Inv),
				Mb(r.UnityAlloc),
				Mb(r.UnityReserved),
				Mb(r.UnityUnusedReserved),
				Opt(r.GfxUsed),
				Opt(r.GfxReserved),
				Opt(r.SystemUsed),
				Mb(r.Texture),
				Mb(r.TextureDesired),
				r.TextureCount.ToString(Inv),
				Mb(r.SysCommit),
				Mb(r.SysCommitLimit)));
			series.Flush();
		}

		public static Reading Read()
		{
			Reading r = new Reading { RealS = Time.realtimeSinceStartup };
			try
			{
				PROCESS_MEMORY_COUNTERS_EX pmc = new PROCESS_MEMORY_COUNTERS_EX { cb = (uint)Marshal.SizeOf(typeof(PROCESS_MEMORY_COUNTERS_EX)) };
				if (K32GetProcessMemoryInfo(GetCurrentProcess(), ref pmc, pmc.cb))
				{
					r.Private = (long)pmc.PrivateUsage.ToUInt64();
					r.WorkingSet = (long)pmc.WorkingSetSize.ToUInt64();
					r.PeakWorkingSet = (long)pmc.PeakWorkingSetSize.ToUInt64();
				}
				MEMORYSTATUSEX ms = new MEMORYSTATUSEX { dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX)) };
				if (GlobalMemoryStatusEx(ref ms))
				{
					r.SysCommitLimit = (long)ms.ullTotalPageFile;
					r.SysCommit = (long)(ms.ullTotalPageFile - ms.ullAvailPageFile);
				}
			}
			catch (Exception) { }
			r.MonoUsed = Profiler.GetMonoUsedSizeLong();
			r.MonoHeap = Profiler.GetMonoHeapSizeLong();
			r.GcTotal = GC.GetTotalMemory(false);
			r.GcCount = GC.CollectionCount(0);
			r.UnityAlloc = Profiler.GetTotalAllocatedMemoryLong();
			r.UnityReserved = Profiler.GetTotalReservedMemoryLong();
			r.UnityUnusedReserved = Profiler.GetTotalUnusedReservedMemoryLong();
			r.GfxUsed = Value(gfxUsed);
			r.GfxReserved = Value(gfxReserved);
			r.SystemUsed = Value(systemUsed);
			r.Texture = (long)Texture.currentTextureMemory;
			r.TextureDesired = (long)Texture.desiredTextureMemory;
			r.TextureCount = (long)Texture.nonStreamingTextureCount + (long)Texture.streamingTextureCount;
			return r;
		}

		private static long Value(ProfilerRecorder recorder)
		{
			try { return recorder.Valid ? recorder.CurrentValue : -1; }
			catch (Exception) { return -1; }
		}

		public static string Mb(long bytes)
		{
			return (bytes / 1048576.0).ToString("F3", Inv);
		}

		private static string Opt(long bytes)
		{
			return bytes < 0 ? "" : Mb(bytes);
		}

		public static string Csv(string s)
		{
			s = s ?? "";
			return s.IndexOfAny(new[] { ',', '"', '\n', '\r' }) >= 0 ? "\"" + s.Replace("\"", "\"\"") + "\"" : s;
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct PROCESS_MEMORY_COUNTERS_EX
		{
			public uint cb;
			public uint PageFaultCount;
			public UIntPtr PeakWorkingSetSize;
			public UIntPtr WorkingSetSize;
			public UIntPtr QuotaPeakPagedPoolUsage;
			public UIntPtr QuotaPagedPoolUsage;
			public UIntPtr QuotaPeakNonPagedPoolUsage;
			public UIntPtr QuotaNonPagedPoolUsage;
			public UIntPtr PagefileUsage;
			public UIntPtr PeakPagefileUsage;
			public UIntPtr PrivateUsage;
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct MEMORYSTATUSEX
		{
			public uint dwLength;
			public uint dwMemoryLoad;
			public ulong ullTotalPhys;
			public ulong ullAvailPhys;
			public ulong ullTotalPageFile;
			public ulong ullAvailPageFile;
			public ulong ullTotalVirtual;
			public ulong ullAvailVirtual;
			public ulong ullAvailExtendedVirtual;
		}

		[DllImport("kernel32.dll")]
		private static extern IntPtr GetCurrentProcess();

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool K32GetProcessMemoryInfo(IntPtr process, ref PROCESS_MEMORY_COUNTERS_EX counters, uint size);

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX buffer);
	}
}
