using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Profiling;

namespace SaveRunner
{
	/// <summary>
	/// What the memory is made of, once, at the end of a memory run: every loaded Unity object by
	/// type and assembly, the largest textures with their kanim and the mod that created them, the
	/// loaded asset bundles, and a total per mod. Sizes are estimates from the format and dimensions
	/// (Profiler.GetRuntimeMemorySizeLong reads 0 in a release player). One CSV:
	///   section,name,owner,count,estMB,readableMB,detail
	/// readableMB is the CPU-side copy a readable texture or mesh keeps on top of the GPU one.
	/// </summary>
	public static class MemorySnapshot
	{
		private const int TopTextures = 300;
		private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;
		private const string Unattributed = "(game or unattributed)";

		private sealed class Stat
		{
			public int Count;
			public long Bytes;
			public long Readable;
		}

		private struct TextureRow
		{
			public Texture Tex;
			public long Bytes;
			public bool Readable;
			public string Owner;
		}

		public static void Write(string path)
		{
			UnityEngine.Object[] all = Resources.FindObjectsOfTypeAll<UnityEngine.Object>();
			Dictionary<int, string> kanimOf = KanimTextures();
			Dictionary<Type, Stat> types = new Dictionary<Type, Stat>();
			Dictionary<string, Stat> modTextures = new Dictionary<string, Stat>();
			Dictionary<string, int> modInstances = new Dictionary<string, int>();
			List<TextureRow> textures = new List<TextureRow>();
			int gameObjects = 0, components = 0;
			long textureBytes = 0, textureReadable = 0, meshBytes = 0, meshReadable = 0;
			foreach (UnityEngine.Object o in all)
			{
				if (o == null)
					continue;
				Type type = o.GetType();
				if (!types.TryGetValue(type, out Stat stat))
					types[type] = stat = new Stat();
				stat.Count++;
				if (o is Component)
					components++;
				else if (o is GameObject)
					gameObjects++;
				if (MemoryProbe.AssemblyOwner.TryGetValue(type.Assembly, out string asmMod))
					modInstances[asmMod] = (modInstances.TryGetValue(asmMod, out int n) ? n : 0) + 1;
				if (o is Texture tex)
				{
					long bytes = TextureBytes(tex);
					bool readable = IsReadable(tex);
					stat.Bytes += bytes;
					textureBytes += bytes;
					if (readable)
					{
						stat.Readable += bytes;
						textureReadable += bytes;
					}
					string owner = MemoryProbe.TextureOwner.TryGetValue(tex.GetInstanceID(), out string m) ? m : Unattributed;
					if (!modTextures.TryGetValue(owner, out Stat ms))
						modTextures[owner] = ms = new Stat();
					ms.Count++;
					ms.Bytes += bytes;
					if (readable)
						ms.Readable += bytes;
					textures.Add(new TextureRow { Tex = tex, Bytes = bytes, Readable = readable, Owner = owner });
				}
				else if (o is Mesh mesh)
				{
					long bytes = MeshBytes(mesh);
					stat.Bytes += bytes;
					meshBytes += bytes;
					if (Readable(mesh))
					{
						stat.Readable += bytes;
						meshReadable += bytes;
					}
				}
			}

			using (StreamWriter w = new StreamWriter(path, append: false, Encoding.UTF8))
			{
				w.WriteLine("section,name,owner,count,estMB,readableMB,detail");
				Line(w, "summary", "objects", "", all.Length, 0, 0, "Resources.FindObjectsOfTypeAll<Object>");
				Line(w, "summary", "gameObjects", "", gameObjects, 0, 0, "");
				Line(w, "summary", "components", "", components, 0, 0, "");
				Line(w, "summary", "textures", "", textures.Count, textureBytes, textureReadable, "estimated from format, size and mips");
				Line(w, "summary", "meshes", "", types.Where(p => typeof(Mesh).IsAssignableFrom(p.Key)).Sum(p => p.Value.Count), meshBytes, meshReadable, "vertex and index buffers");
				Line(w, "summary", "kanims", "", Assets.Anims?.Count ?? 0, 0, 0, "Assets.Anims");
				Line(w, "summary", "modKanims", "", Assets.ModLoadedKAnims.Count, 0, 0, "Assets.ModLoadedKAnims");
				Line(w, "summary", "managedHeap", "", 0, GC.GetTotalMemory(false), 0, "GC.GetTotalMemory");
				Line(w, "summary", "monoHeap", "", 0, Profiler.GetMonoHeapSizeLong(), 0, "Profiler.GetMonoHeapSizeLong");

				// Per mod: what it loaded and what of it is alive now.
				HashSet<string> modIds = new HashSet<string>(MemoryProbe.Mods.Keys);
				modIds.UnionWith(modTextures.Keys);
				foreach (string id in modIds.OrderByDescending(i => modTextures.TryGetValue(i, out Stat s) ? s.Bytes : 0))
				{
					MemoryProbe.Mods.TryGetValue(id, out MemoryProbe.ModTotals totals);
					modTextures.TryGetValue(id, out Stat ts);
					modInstances.TryGetValue(id, out int instances);
					string detail = "kanims=" + (totals?.Kanims ?? 0).ToString(Inv)
						+ " objects=" + instances.ToString(Inv)
						+ " loadPrivateMB=" + MemoryProbe.Mb(totals?.PrivateDelta ?? 0)
						+ " loadMonoHeapMB=" + MemoryProbe.Mb(totals?.MonoHeapDelta ?? 0)
						+ " loadUnityReservedMB=" + MemoryProbe.Mb(totals?.UnityReservedDelta ?? 0);
					Line(w, "mod", id, totals?.Title ?? "", ts?.Count ?? 0, ts?.Bytes ?? 0, ts?.Readable ?? 0, detail);
				}

				// Per assembly: instances of its types (components, ScriptableObjects, Unity's own objects).
				foreach (IGrouping<Assembly, KeyValuePair<Type, Stat>> g in types.GroupBy(p => p.Key.Assembly).OrderByDescending(g => g.Sum(p => p.Value.Count)))
				{
					string owner = MemoryProbe.AssemblyOwner.TryGetValue(g.Key, out string m) ? m : "";
					Line(w, "assembly", g.Key.GetName().Name, owner, g.Sum(p => p.Value.Count), g.Sum(p => p.Value.Bytes), g.Sum(p => p.Value.Readable), "types=" + g.Count().ToString(Inv));
				}

				foreach (KeyValuePair<Type, Stat> p in types.OrderByDescending(p => p.Value.Count))
				{
					string owner = p.Key.Assembly.GetName().Name;
					if (MemoryProbe.AssemblyOwner.TryGetValue(p.Key.Assembly, out string m))
						owner += " (" + m + ")";
					string kind = typeof(Component).IsAssignableFrom(p.Key) ? "component" : typeof(ScriptableObject).IsAssignableFrom(p.Key) ? "scriptable" : "object";
					Line(w, "type", p.Key.FullName, owner, p.Value.Count, p.Value.Bytes, p.Value.Readable, kind);
				}

				foreach (TextureRow t in textures.OrderByDescending(t => t.Bytes).Take(TopTextures))
				{
					string name = string.IsNullOrEmpty(t.Tex.name) ? "(unnamed)" : t.Tex.name;
					string kanim = kanimOf.TryGetValue(t.Tex.GetInstanceID(), out string k) ? " kanim=" + k : "";
					string detail = t.Tex.GetType().Name + " " + t.Tex.width.ToString(Inv) + "x" + t.Tex.height.ToString(Inv) + " " + t.Tex.graphicsFormat
						+ " mips=" + t.Tex.mipmapCount.ToString(Inv) + kanim;
					Line(w, "texture", name, t.Owner, 1, t.Bytes, t.Readable ? t.Bytes : 0, detail);
				}

				try
				{
					foreach (AssetBundle bundle in AssetBundle.GetAllLoadedAssetBundles())
						if (bundle != null)
							Line(w, "bundle", bundle.name, "", 1, 0, 0, bundle.isStreamedSceneAssetBundle ? "scenes" : "assets");
				}
				catch (Exception e)
				{
					Line(w, "bundle", "(error)", "", 0, 0, 0, e.Message);
				}
			}
		}

		/// <summary>Texture instance id to the name of the kanim that holds it, for game and mod kanims.</summary>
		private static Dictionary<int, string> KanimTextures()
		{
			Dictionary<int, string> map = new Dictionary<int, string>();
			IEnumerable<KAnimFile> anims = (IEnumerable<KAnimFile>)Assets.Anims ?? Assets.ModLoadedKAnims;
			foreach (KAnimFile kanim in anims)
			{
				if (kanim == null || kanim.textureList == null)
					continue;
				foreach (Texture2D tex in kanim.textureList)
					if (tex != null)
						map[tex.GetInstanceID()] = kanim.name;
			}
			return map;
		}

		private static void Line(StreamWriter w, string section, string name, string owner, int count, long bytes, long readable, string detail)
		{
			w.WriteLine(string.Join(",", section, MemoryProbe.Csv(name), MemoryProbe.Csv(owner), count.ToString(Inv), MemoryProbe.Mb(bytes), MemoryProbe.Mb(readable), MemoryProbe.Csv(detail)));
		}

		public static bool IsReadable(Texture tex)
		{
			try { return tex.isReadable; }
			catch (Exception) { return false; }
		}

		private static bool Readable(Mesh mesh)
		{
			try { return mesh.isReadable; }
			catch (Exception) { return false; }
		}

		/// <summary>
		/// GPU bytes of a texture from its format, size, mips and layers; a render texture adds its
		/// depth buffer and multiplies by its MSAA samples. The profiler's own figure is used when the
		/// player reports one.
		/// </summary>
		public static long TextureBytes(Texture tex)
		{
			try
			{
				long profiled = Profiler.GetRuntimeMemorySizeLong(tex);
				if (profiled > 0)
					return profiled;
				int layers = 1;
				if (tex is Cubemap)
					layers = 6;
				else if (tex is Texture2DArray array)
					layers = array.depth;
				else if (tex is Texture3D volume)
					layers = volume.depth;
				else if (tex is CubemapArray cubes)
					layers = cubes.cubemapCount * 6;
				int mips = Math.Max(1, tex.mipmapCount);
				long bytes = MipChainBytes(tex.graphicsFormat, tex.width, tex.height, mips) * layers;
				if (tex is RenderTexture rt)
				{
					bytes += (long)rt.width * rt.height * rt.depth / 8;
					bytes *= Math.Max(1, rt.antiAliasing);
				}
				return bytes;
			}
			catch (Exception)
			{
				return 0;
			}
		}

		private static long MipChainBytes(GraphicsFormat format, int width, int height, int mips)
		{
			if (format == GraphicsFormat.None)
				return 0;
			long blockSize = GraphicsFormatUtility.GetBlockSize(format);
			long blockW = Math.Max(1u, GraphicsFormatUtility.GetBlockWidth(format));
			long blockH = Math.Max(1u, GraphicsFormatUtility.GetBlockHeight(format));
			long total = 0;
			for (int m = 0; m < mips; m++)
			{
				long w = Math.Max(1, width >> m), h = Math.Max(1, height >> m);
				total += (w + blockW - 1) / blockW * ((h + blockH - 1) / blockH) * blockSize;
			}
			return total;
		}

		private static long MeshBytes(Mesh mesh)
		{
			try
			{
				long profiled = Profiler.GetRuntimeMemorySizeLong(mesh);
				if (profiled > 0)
					return profiled;
				long bytes = 0;
				for (int s = 0; s < mesh.vertexBufferCount; s++)
					bytes += (long)mesh.vertexCount * mesh.GetVertexBufferStride(s);
				int indexSize = mesh.indexFormat == UnityEngine.Rendering.IndexFormat.UInt16 ? 2 : 4;
				for (int sub = 0; sub < mesh.subMeshCount; sub++)
					bytes += (long)mesh.GetIndexCount(sub) * indexSize;
				return bytes;
			}
			catch (Exception)
			{
				return 0;
			}
		}
	}
}
