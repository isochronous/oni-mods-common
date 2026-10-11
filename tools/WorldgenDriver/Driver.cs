using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace WorldgenDriver
{
	/// <summary>
	/// State for an unattended world generation session, configured from the game's command line:
	///   --worldgen=COORD[;COORD...]   coordinates to generate in this session, in order (required)
	///   --worldgen-list=FILE          same, one coordinate per line (joined with --worldgen)
	///   --worldgen-out=FILE           JSON Lines result file, one object appended per coordinate
	///                                 (default: Klei folder\worldgen-driver\results.jsonl)
	///   --worldgen-timeout=SECONDS    no progress for this long counts as a hang (default 300)
	///   --worldgen-expansion1=0|1     mode this session needs (default: 1 when the first prefix ends in -C)
	///   --worldgen-load=1             also load each generated colony before moving on
	/// After a coordinate completes or fails the game returns to the main menu and starts the next
	/// one; a hang, a crash or a driver error ends the session (the launcher restarts the game for
	/// the rest of the list).
	/// </summary>
	public static class Driver
	{
		public const string Tag = "[WorldgenDriver] ";

		public static bool Active { get; private set; }
		public static readonly List<string> Coordinates = new List<string>();
		public static int Index { get; private set; }
		public static string Coordinate => Index < Coordinates.Count ? Coordinates[Index] : null;
		public static string OutFile { get; private set; }
		public static double TimeoutSeconds { get; private set; } = 300;
		public static bool WantExpansion1 { get; private set; }
		public static bool LoadColony { get; private set; }
		/// <summary>"all": every story trait guaranteed, applied after the coordinate. Anything else: leave the coordinate's story code alone.</summary>
		public static string Stories { get; private set; } = "";
		/// <summary>"all": every DLC mixing toggle on and every world/subworld mixing set to TryMixing; "guarantee": same with GuranteeMixing. Applied after the coordinate.</summary>
		public static string Mixing { get; private set; } = "";
		/// <summary>The coordinate the game reports after everything is applied (differs from the requested one when Stories/Mixing override it).</summary>
		public static string Applied;

		public static string ClusterId;
		public static string Seed;
		public static volatile string Stage = "";
		public static volatile float Percent;
		public static long LastProgressTicks = System.DateTime.UtcNow.Ticks;
		public static long StartTicks = System.DateTime.UtcNow.Ticks;
		public static readonly List<string> Errors = new List<string>();
		/// <summary>True once the current coordinate's result is written (game quitting or going back to the menu).</summary>
		public static bool Finished;
		/// <summary>True once the current coordinate's worldgen screen has been opened.</summary>
		public static bool Started;
		private static bool expansionGiven;
		private static bool quitting;

		public static void ReadCommandLine()
		{
			foreach (string arg in Environment.GetCommandLineArgs())
			{
				if (!arg.StartsWith("--worldgen", StringComparison.Ordinal))
					continue;
				int eq = arg.IndexOf('=');
				string key = eq < 0 ? arg : arg.Substring(0, eq);
				string value = eq < 0 ? "" : arg.Substring(eq + 1);
				switch (key)
				{
					case "--worldgen": AddCoordinates(value.Split(';')); break;
					case "--worldgen-list": if (File.Exists(value)) AddCoordinates(File.ReadAllLines(value)); else Debug.LogWarning(Tag + "list file not found: " + value); break;
					case "--worldgen-out": OutFile = value; break;
					case "--worldgen-timeout": double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double t); if (t > 0) TimeoutSeconds = t; break;
					case "--worldgen-expansion1": WantExpansion1 = value == "1" || value.Equals("true", StringComparison.OrdinalIgnoreCase); expansionGiven = true; break;
					case "--worldgen-stories": Stories = value.ToLowerInvariant(); break;
					case "--worldgen-mixing": Mixing = value.ToLowerInvariant(); break;
					case "--worldgen-load": LoadColony = value == "1" || value.Equals("true", StringComparison.OrdinalIgnoreCase); break;
				}
			}
			Active = Coordinates.Count > 0;
			if (!Active)
				return;
			if (string.IsNullOrEmpty(OutFile))
				OutFile = Path.Combine(Path.Combine(Util.RootFolder(), "worldgen-driver"), "results.jsonl");
			if (!expansionGiven)
				WantExpansion1 = IsSpacedOutCoordinate(Coordinates[0]);
			Debug.Log(Tag + Coordinates.Count + " coordinate(s) requested, first " + Coordinates[0] + " (Spaced Out mode: " + WantExpansion1 + ", timeout " + TimeoutSeconds + " s, load colony: " + LoadColony + ")");
		}

		private static void AddCoordinates(IEnumerable<string> values)
		{
			foreach (string v in values)
			{
				string c = v.Trim();
				if (c.Length > 0 && !c.StartsWith("#"))
					Coordinates.Add(c);
			}
		}

		/// <summary>"CERS-A-100-0-0-0" is a base-game cluster, "M-FLIP-C-100-0-0-0" a Spaced Out one: the letter before the seed.</summary>
		public static bool IsSpacedOutCoordinate(string coordinate)
		{
			string[] parts = coordinate.Split('-');
			return parts.Length >= 5 && parts[parts.Length - 5] == "C";
		}

		public static void Progress(string stage, float percent)
		{
			Stage = stage ?? "";
			Percent = percent;
			LastProgressTicks = System.DateTime.UtcNow.Ticks;
		}

		public static double SecondsSinceProgress => (System.DateTime.UtcNow.Ticks - LastProgressTicks) / (double)TimeSpan.TicksPerSecond;
		public static double SecondsSinceStart => (System.DateTime.UtcNow.Ticks - StartTicks) / (double)TimeSpan.TicksPerSecond;

		/// <summary>Called from the main menu for each coordinate: resets the per-coordinate state.</summary>
		public static void BeginCoordinate()
		{
			Started = true;
			Finished = false;
			ClusterId = null;
			Seed = null;
			Applied = null;
			lock (Errors) Errors.Clear();
			StartTicks = System.DateTime.UtcNow.Ticks;
			Progress("starting", 0f);
		}

		/// <summary>
		/// Writes the result line for the current coordinate once, then either moves on to the next
		/// coordinate (back to the main menu) or quits the game when the list is done or the session
		/// cannot continue (hang, crash, driver error).
		/// </summary>
		public static void Finish(string status, string detail = null)
		{
			if (Finished || quitting)
				return;
			Finished = true;
			WriteResult(status, detail);
			Debug.Log(Tag + "finished " + Coordinate + ": " + status + " at stage \"" + Stage + "\" " + Percent.ToString("P0", CultureInfo.InvariantCulture) + (detail != null ? " (" + detail + ")" : ""));
			bool canContinue = status == "complete" || status == "loaded" || status == "failed" || status == "bad-args";
			Index++;
			if (!canContinue || Index >= Coordinates.Count)
			{
				quitting = true;
				Debug.Log(Tag + (Index >= Coordinates.Count ? "list done, quitting" : "session cannot continue, quitting"));
				App.Quit();
				return;
			}
			Started = false;
			Debug.Log(Tag + "next: " + Coordinate);
			Progress("menu", 0f);
			App.LoadScene("frontend");
		}

		private static void WriteResult(string status, string detail)
		{
			var sb = new StringBuilder();
			sb.Append('{');
			Field(sb, "status", status);
			Field(sb, "coordinate", Coordinate);
			Field(sb, "index", Index.ToString(CultureInfo.InvariantCulture));
			Field(sb, "cluster", ClusterId);
			Field(sb, "seed", Seed);
			Field(sb, "applied", Applied);
			Field(sb, "expansion1", WantExpansion1 ? "1" : "0");
			Field(sb, "stage", Stage);
			Field(sb, "percent", Percent.ToString("0.000", CultureInfo.InvariantCulture));
			Field(sb, "secondsSinceProgress", SecondsSinceProgress.ToString("0.0", CultureInfo.InvariantCulture));
			Field(sb, "secondsSinceStart", SecondsSinceStart.ToString("0.0", CultureInfo.InvariantCulture));
			Field(sb, "detail", detail ?? "");
			Field(sb, "build", BuildWatermark.GetBuildText());
			Field(sb, "time", System.DateTime.Now.ToString("s", CultureInfo.InvariantCulture));
			sb.Append("\"errors\": [");
			lock (Errors)
			{
				for (int i = 0; i < Errors.Count; i++)
					sb.Append(i > 0 ? ", " : "").Append('"').Append(Escape(Errors[i])).Append('"');
			}
			sb.Append("]}\n");
			try
			{
				Directory.CreateDirectory(Path.GetDirectoryName(OutFile));
				File.AppendAllText(OutFile, sb.ToString(), new UTF8Encoding(false));
			}
			catch (Exception e)
			{
				Debug.LogWarning(Tag + "could not write " + OutFile + ": " + e.Message);
			}
		}

		private static void Field(StringBuilder sb, string name, string value)
		{
			sb.Append('"').Append(name).Append("\": \"").Append(Escape(value ?? "")).Append("\", ");
		}

		private static string Escape(string s)
		{
			var sb = new StringBuilder(s.Length + 16);
			foreach (char c in s)
			{
				switch (c)
				{
					case '\\': sb.Append("\\\\"); break;
					case '\"': sb.Append("\\\""); break;
					case '\n': sb.Append("\\n"); break;
					case '\r': sb.Append("\\r"); break;
					case '\t': sb.Append("\\t"); break;
					default:
						if (c < ' ' || c == '\u2028' || c == '\u2029' || c == '\u0085')
							sb.Append("\\u").Append(((int)c).ToString("x4"));
						else
							sb.Append(c);
						break;
				}
			}
			return sb.ToString();
		}
	}
}
