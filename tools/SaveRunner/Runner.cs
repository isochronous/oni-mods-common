using System;
using System.Globalization;
using System.IO;
using UnityEngine;

namespace SaveRunner
{
	/// <summary>
	/// Command line and state of one run.
	///   --saverun=FILE            save file to load (required; a path without spaces is safest through Steam)
	///   --saverun-seconds=N       game seconds to run after the colony has loaded (default 60)
	///   --saverun-speed=1|2|3     game speed while running (default 1)
	///   --saverun-probe=NAME      what to record each game second; "fridges" is the only probe so far
	///   --saverun-out=FILE        CSV the probe writes; FILE.status gets one JSON line when the run ends
	///   --saverun-timeout=SECONDS real time allowed for the whole run before it is called a hang (default 900)
	/// </summary>
	public static class Runner
	{
		public const string Tag = "[SaveRunner] ";

		public static bool Active;
		public static bool Started;
		public static bool Finished;
		public static string SavePath;
		public static double Seconds = 60;
		public static int Speed = 1;
		public static string Probe;
		public static string OutFile;
		public static double TimeoutSeconds = 900;
		public static float StartedAt;
		/// <summary>Real time the main menu appeared, or -1 before that.</summary>
		public static float MenuSeenAt = -1f;

		public static void ReadCommandLine()
		{
			foreach (string arg in Environment.GetCommandLineArgs())
			{
				if (!arg.StartsWith("--saverun", StringComparison.Ordinal))
					continue;
				int eq = arg.IndexOf('=');
				string key = eq < 0 ? arg : arg.Substring(0, eq);
				string value = eq < 0 ? "" : arg.Substring(eq + 1);
				switch (key)
				{
					case "--saverun": SavePath = value; break;
					case "--saverun-seconds": if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double s) && s > 0) Seconds = s; break;
					case "--saverun-speed": if (int.TryParse(value, out int sp)) Speed = Mathf.Clamp(sp, 1, 3); break;
					case "--saverun-probe": Probe = value.ToLowerInvariant(); break;
					case "--saverun-out": OutFile = value; break;
					case "--saverun-timeout": if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double t) && t > 0) TimeoutSeconds = t; break;
				}
			}
			Active = !string.IsNullOrEmpty(SavePath);
			StartedAt = Time.realtimeSinceStartup;
		}

		/// <summary>Records how the run ended and quits the game.</summary>
		public static void Finish(string status, string detail = null)
		{
			if (Finished)
				return;
			Finished = true;
			string line = "{\"status\":\"" + status + "\",\"save\":" + Json(SavePath) + ",\"detail\":" + Json(detail ?? "")
				+ ",\"realSeconds\":" + (Time.realtimeSinceStartup - StartedAt).ToString("F1", CultureInfo.InvariantCulture) + "}";
			Debug.Log(Tag + "finished: " + line);
			if (OutFile != null)
			{
				try { File.WriteAllText(OutFile + ".status", line + "\n"); }
				catch (Exception e) { Debug.LogWarning(Tag + "cannot write status file: " + e.Message); }
			}
			App.Quit();
		}

		private static string Json(string s)
		{
			return "\"" + (s ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
		}
	}
}
