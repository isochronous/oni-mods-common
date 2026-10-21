using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Isochronous.TestRunner
{
	/// <summary>
	/// The shared config folders of the mods under test (PLib's mods/config/NAME, NAME the mod's
	/// assembly name), backed up when a run starts and put back when it ends, so a test (or a
	/// Prepare) may write a mod's settings without touching the player's. The launcher names the
	/// mods (--onitest-config, or "config=" in a live request). The backup lives outside the run
	/// folder, at %LOCALAPPDATA%\IsochronousOniTests\config-backup\NAME, where the launcher and the
	/// warden find a leftover from a game that died mid-run and put it back. NAME\files holds the
	/// folder as it was; NAME\absent says there was none. A backup is written to NAME.tmp and renamed
	/// when complete, and removed only once it has been restored, so a crash at any point leaves
	/// either the player's folder or a whole backup of it.
	/// </summary>
	internal static class ModConfig
	{
		public static readonly List<string> Names = new List<string>();

		public static string BackupRoot => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "IsochronousOniTests", "config-backup");

		public static string ConfigRoot => Path.Combine(KMod.Manager.GetDirectory(), "config");

		/// <summary>
		/// The mod names from "A;B;C" (empty or null: none). A name that is not a plain folder name
		/// (letters, digits, '_', '-', dots only between them) is left out with a warning: "..", "."
		/// or a path would make the backup and the restore act on another folder than the mod's.
		/// </summary>
		public static void SetNames(string list)
		{
			Names.Clear();
			if (string.IsNullOrEmpty(list))
				return;
			foreach (string raw in list.Split(';'))
			{
				string name = raw.Trim();
				if (name.Length == 0 || Names.Contains(name))
					continue;
				if (!IsFolderName(name))
				{
					Debug.LogWarning(Session.Tag + "config name '" + name + "' is not a mod folder name; its config is not backed up");
					continue;
				}
				Names.Add(name);
			}
		}

		internal static bool IsFolderName(string name)
		{
			return System.Text.RegularExpressions.Regex.IsMatch(name, @"^[A-Za-z0-9_-]+(\.[A-Za-z0-9_-]+)*$");
		}

		/// <summary>
		/// Backs up each named mod's config folder. A backup already there is a leftover from a run
		/// that did not finish: it holds the player's folder, so it is put back first. Throws when a
		/// backup cannot be made, and the run does not start: a test must not write a config that
		/// cannot be put back.
		/// </summary>
		public static void Snapshot()
		{
			if (Names.Count == 0)
				return;
			Directory.CreateDirectory(BackupRoot);
			foreach (string name in Names)
			{
				string kept = Path.Combine(BackupRoot, name);
				if (Directory.Exists(kept))
				{
					Debug.LogWarning(Session.Tag + "leftover config backup " + kept + " from an unfinished run; putting it back first");
					RestoreOne(name);
				}
				string live = Path.Combine(ConfigRoot, name);
				string tmp = kept + ".tmp";
				if (Directory.Exists(tmp))
					Directory.Delete(tmp, true);
				Directory.CreateDirectory(tmp);
				if (Directory.Exists(live))
					CopyFolder(live, Path.Combine(tmp, "files"));
				else
					File.WriteAllText(Path.Combine(tmp, "absent"), "");
				Directory.Move(tmp, kept);
				Debug.Log(Session.Tag + "backed up the config of " + name + (Directory.Exists(live) ? "" : " (none yet)"));
			}
		}

		/// <summary>Puts every backed-up config folder back. Returns null, or what could not be restored.</summary>
		public static string Restore()
		{
			var problems = new List<string>();
			foreach (string name in Names)
			{
				try
				{
					RestoreOne(name);
				}
				catch (Exception e)
				{
					Debug.LogWarning(Session.Tag + "cannot put back the config of " + name + ": " + e);
					problems.Add("config of " + name + " not restored (backup left at " + Path.Combine(BackupRoot, name) + "): " + e.Message);
				}
			}
			RemoveEmptyRoot();
			return problems.Count == 0 ? null : string.Join("; ", problems);
		}

		/// <summary>The backup folder goes once nothing is kept in it.</summary>
		private static void RemoveEmptyRoot()
		{
			try
			{
				if (Directory.Exists(BackupRoot) && Directory.GetFileSystemEntries(BackupRoot).Length == 0)
					Directory.Delete(BackupRoot, false);
			}
			catch (Exception e)
			{
				Debug.LogWarning(Session.Tag + "cannot remove the empty config backup folder " + BackupRoot + ": " + e.Message);
			}
		}

		private static void RestoreOne(string name)
		{
			string kept = Path.Combine(BackupRoot, name);
			if (!Directory.Exists(kept))
				return;
			string live = Path.Combine(ConfigRoot, name);
			if (Directory.Exists(live))
				Directory.Delete(live, true);
			string files = Path.Combine(kept, "files");
			if (Directory.Exists(files))
				CopyFolder(files, live);
			Directory.Delete(kept, true);
			Debug.Log(Session.Tag + "put back the config of " + name + (Directory.Exists(live) ? "" : " (none, as before the run)"));
		}

		private static void CopyFolder(string from, string to)
		{
			Directory.CreateDirectory(to);
			foreach (string file in Directory.GetFiles(from))
				File.Copy(file, Path.Combine(to, Path.GetFileName(file)), true);
			foreach (string dir in Directory.GetDirectories(from))
				CopyFolder(dir, Path.Combine(to, Path.GetFileName(dir)));
		}
	}
}
