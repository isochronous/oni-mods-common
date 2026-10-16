using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Isochronous.TestKit;
using UnityEngine;

namespace Isochronous.TestRunner
{
	/// <summary>
	/// Command line and state of one test session.
	///   --onitest-dir=DIR              run directory (required): DIR/tests/*.dll are the test assemblies,
	///                                  DIR/results.jsonl gets one JSON object per test as it ends and
	///                                  DIR/results.jsonl.status one JSON line when the session ends
	///   --onitest-save=FILE            fixture save to load from the main menu (required)
	///   --onitest-filter=GLOB[;GLOB]   run only tests whose "Assembly/Class.Method" or display name matches
	///   --onitest-timeout=SECONDS      real time allowed for the whole session before it is called a hang (default 600)
	///   --onitest-test-timeout=SECONDS default real time per test (default 60; [OniTest(Timeout = n)] overrides)
	///   --onitest-keep=1               leave the colony open when the tests are done instead of quitting
	///   --onitest-server=DIR           stay up after the run and take further runs from DIR/queue
	///                                  (see Server below); quit after IdleSeconds with none
	/// </summary>
	public static class Session
	{
		public const string Tag = "[TestRunner] ";

		public static bool Active;
		public static bool Started;
		public static bool Finished;
		public static string Dir;
		public static string SavePath;
		public static readonly List<string> Filters = new List<string>();
		public static double TimeoutSeconds = 600;
		public static double TestTimeoutSeconds = 60;
		public static bool Keep;
		/// <summary>--onitest-trace=1: log the grid marks the game makes during tests (Diagnostics.cs).</summary>
		public static bool Trace;
		public static float StartedAt;
		/// <summary>Real time the main menu appeared, or -1 before that.</summary>
		public static float MenuSeenAt = -1f;

		/// <summary>The live-session folder (--onitest-server), or null for a one-run session.</summary>
		public static string ServerDir;
		/// <summary>Real seconds a live session waits for the next run before it quits.</summary>
		public const float IdleSeconds = 15f;
		/// <summary>Real time the last run finished, while the session waits for the next.</summary>
		public static float IdleSince = -1f;
		/// <summary>The test assembly of the last test run on the current load: the next assembly gets a fresh load.</summary>
		public static string LastAssembly;

		public static readonly List<TestCase> Tests = new List<TestCase>();
		/// <summary>Index of the next test to run; survives a scene change, so a reload resumes here.</summary>
		public static int NextIndex;
		/// <summary>True from each load of the fixture until a test runs on it.</summary>
		public static bool SaveIsFresh;
		/// <summary>How many times the fixture was loaded again for a [OniTest(FreshSave = true)] test.</summary>
		public static int Reloads;
		public static int Passed, Failed, Errors, Timeouts, Skipped;

		private static StreamWriter results;

		public static string ResultsPath => Path.Combine(Dir, "results.jsonl");

		public static void ReadCommandLine()
		{
			foreach (string arg in Environment.GetCommandLineArgs())
			{
				if (!arg.StartsWith("--onitest", StringComparison.Ordinal))
					continue;
				int eq = arg.IndexOf('=');
				string key = eq < 0 ? arg : arg.Substring(0, eq);
				string value = eq < 0 ? "" : arg.Substring(eq + 1);
				switch (key)
				{
					case "--onitest-dir": Dir = value; break;
					case "--onitest-save": SavePath = value; break;
					case "--onitest-filter":
						foreach (string f in value.Split(';'))
							if (f.Trim().Length > 0)
								Filters.Add(f.Trim());
						break;
					case "--onitest-timeout": if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double t) && t > 0) TimeoutSeconds = t; break;
					case "--onitest-test-timeout": if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double tt) && tt > 0) TestTimeoutSeconds = tt; break;
					case "--onitest-keep": Keep = value == "1" || value.Equals("true", StringComparison.OrdinalIgnoreCase); break;
					case "--onitest-trace": Trace = value == "1"; break;
					case "--onitest-server": ServerDir = value; break;
				}
			}
			Active = !string.IsNullOrEmpty(Dir) && !string.IsNullOrEmpty(SavePath);
			StartedAt = Time.realtimeSinceStartup;
		}

		/// <summary>True when every selected test is [OniTest(NoColony = true)]: the run loads no save.</summary>
		public static bool AllNoColony => Tests.Count > 0 && Tests.TrueForAll(test => test.NoColony);

		/// <summary>
		/// Loads every DLL under DIR/tests and lists its [OniTest] methods: public static, returning
		/// IEnumerator, taking one TestContext. Order: assembly name, class, declaration order. Called
		/// at the main menu or once the colony is up; the game loads every mod at boot, so whatever a
		/// test references is there. The DLLs are loaded from their bytes, so a live session takes a
		/// rebuilt test assembly of the same name as a new one.
		/// </summary>
		public static void Discover()
		{
			if (Tests.Count > 0)
				return;
			string testsDir = Path.Combine(Dir, "tests");
			if (!Directory.Exists(testsDir))
				throw new DirectoryNotFoundException("no tests folder at " + testsDir);
			var regexes = new List<Regex>();
			foreach (string glob in Filters)
				regexes.Add(new Regex("^" + Regex.Escape(glob).Replace("\\*", ".*").Replace("\\?", ".") + "$", RegexOptions.IgnoreCase));
			var found = new List<TestCase>();
			string[] dlls = Directory.GetFiles(testsDir, "*.dll");
			Array.Sort(dlls, string.CompareOrdinal);
			foreach (string dll in dlls)
			{
				Assembly asm = Assembly.Load(File.ReadAllBytes(dll));
				Type[] types;
				try
				{
					types = asm.GetTypes();
				}
				catch (ReflectionTypeLoadException e)
				{
					var sb = new StringBuilder("cannot load the types of " + Path.GetFileName(dll) + ":");
					foreach (Exception inner in e.LoaderExceptions)
						sb.Append("\n  ").Append(inner.Message);
					throw new InvalidOperationException(sb.ToString());
				}
				var ofAssembly = new List<TestCase>();
				foreach (Type type in types)
				{
					foreach (MethodInfo method in type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly))
					{
						OniTestAttribute attr = method.GetCustomAttribute<OniTestAttribute>();
						if (attr == null)
							continue;
						ParameterInfo[] parameters = method.GetParameters();
						if (method.ReturnType != typeof(System.Collections.IEnumerator) || parameters.Length != 1 || parameters[0].ParameterType != typeof(TestContext))
							throw new InvalidOperationException(type.FullName + "." + method.Name + " has [OniTest] but is not 'public static IEnumerator M(TestContext t)'");
						ofAssembly.Add(new TestCase(asm.GetName().Name, type, method, attr));
					}
				}
				ofAssembly.Sort((a, b) =>
				{
					int c = string.CompareOrdinal(a.Type.FullName, b.Type.FullName);
					return c != 0 ? c : a.Method.MetadataToken.CompareTo(b.Method.MetadataToken);
				});
				found.AddRange(ofAssembly);
			}
			foreach (TestCase test in found)
			{
				if (regexes.Count == 0 ? !test.Explicit : regexes.Exists(r => r.IsMatch(test.Id) || r.IsMatch(test.Name)))
					Tests.Add(test);
				else if (regexes.Count == 0)
					Debug.Log(Tag + "  " + test.Id + ": left out (runs only when a filter selects it)");
			}
			Debug.Log(Tag + Tests.Count + " of " + found.Count + " test(s) selected from " + dlls.Length + " assembly(ies)");
			foreach (TestCase test in Tests)
				Debug.Log(Tag + "  " + test.Id + ": " + test.Name);
		}

		public static void Write(TestResult result)
		{
			try
			{
				if (results == null)
					results = new StreamWriter(ResultsPath, append: true, new UTF8Encoding(false));
				results.WriteLine(result.ToJson());
				results.Flush();
			}
			catch (Exception e)
			{
				Debug.LogWarning(Tag + "cannot write a result: " + e.Message);
			}
			switch (result.Status)
			{
				case "pass": Passed++; break;
				case "fail": Failed++; break;
				case "error": Errors++; break;
				case "timeout": Timeouts++; break;
				case "skipped": Skipped++; break;
			}
			Debug.Log(Tag + result.Status.ToUpperInvariant() + " " + result.Id + (result.Message != null ? ": " + result.Message : ""));
		}

		/// <summary>Records how the session ended and quits the game (unless asked to keep it open).</summary>
		public static void Finish(string status, string detail = null)
		{
			if (Finished)
				return;
			Finished = true;
			string line = "{\"status\":" + Json.Str(status) + ",\"detail\":" + Json.Str(detail ?? "")
				+ ",\"save\":" + Json.Str(SavePath)
				+ ",\"total\":" + Tests.Count + ",\"passed\":" + Passed + ",\"failed\":" + Failed + ",\"errors\":" + Errors
				+ ",\"timeouts\":" + Timeouts + ",\"skipped\":" + Skipped + ",\"reloads\":" + Reloads
				+ ",\"realSeconds\":" + (Time.realtimeSinceStartup - StartedAt).ToString("F1", CultureInfo.InvariantCulture) + "}";
			Debug.Log(Tag + "finished: " + line);
			try
			{
				results?.Flush();
				results?.Dispose();
				results = null;
				File.WriteAllText(ResultsPath + ".status", line + "\n");
			}
			catch (Exception e)
			{
				Debug.LogWarning(Tag + "cannot write the status file: " + e.Message);
			}
			if (Keep)
				Debug.Log(Tag + "leaving the game open (--onitest-keep)");
			else if (ServerDir != null)
			{
				IdleSince = Time.realtimeSinceStartup;
				WriteState("idle");
				Debug.Log(Tag + "waiting up to " + IdleSeconds + " s for the next run in " + ServerDir);
			}
			else
				App.Quit();
		}

		// --- live session ------------------------------------------------------------------------
		//
		// A launcher whose loadout matches the running game's drops a request file in DIR/queue
		// instead of starting the game again; the game takes it (renames it .taken), loads the
		// fixture fresh and runs it like the first. DIR/state says what the game is doing, for the
		// launchers and the warden that restores the player's mods.json once the game exits. A
		// request named quit.req ends the session at once; no request for IdleSeconds ends it too.

		private static string QueueDir => Path.Combine(ServerDir, "queue");

		/// <summary>"busy &lt;deadline&gt;", "idle &lt;since&gt;" or "quitting", with Unix seconds, for the launcher and the warden.</summary>
		public static void WriteState(string state)
		{
			if (ServerDir == null)
				return;
			try
			{
				long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
				string line = state == "busy" ? "busy " + (now + (long)TimeoutSeconds) : state == "idle" ? "idle " + now : state;
				File.WriteAllText(Path.Combine(ServerDir, "state"), line + "\n");
			}
			catch (Exception e)
			{
				Debug.LogWarning(Tag + "cannot write the session state: " + e.Message);
			}
		}

		/// <summary>
		/// Called every frame while the session is idle: takes the next request, or quits after
		/// IdleSeconds with none. Returns true when a run has started.
		/// </summary>
		public static bool PollQueue()
		{
			string[] requests;
			try
			{
				requests = Directory.Exists(QueueDir) ? Directory.GetFiles(QueueDir, "*.req") : new string[0];
			}
			catch (Exception e)
			{
				Debug.LogWarning(Tag + "cannot read the queue: " + e.Message);
				requests = new string[0];
			}
			Array.Sort(requests, string.CompareOrdinal);
			foreach (string request in requests)
			{
				if (Path.GetFileName(request).Equals("quit.req", StringComparison.OrdinalIgnoreCase))
				{
					TryDelete(request);
					Quit("asked to quit");
					return false;
				}
				string taken = Path.ChangeExtension(request, ".taken");
				Dictionary<string, string> fields;
				try
				{
					File.Move(request, taken);
					fields = ReadRequest(taken);
				}
				catch (Exception e)
				{
					Debug.LogWarning(Tag + "cannot take " + request + ": " + e.Message);
					continue;
				}
				Begin(fields);
				return true;
			}
			if (Time.realtimeSinceStartup - IdleSince > IdleSeconds)
				Quit("no run for " + IdleSeconds + " s");
			return false;
		}

		private static Dictionary<string, string> ReadRequest(string path)
		{
			var fields = new Dictionary<string, string>();
			foreach (string line in File.ReadAllLines(path))
			{
				int eq = line.IndexOf('=');
				if (eq > 0)
					fields[line.Substring(0, eq).Trim()] = line.Substring(eq + 1).Trim();
			}
			return fields;
		}

		/// <summary>A fresh run in the live game: the run's settings, every count back to zero, the tests found again.</summary>
		private static void Begin(Dictionary<string, string> fields)
		{
			fields.TryGetValue("dir", out Dir);
			fields.TryGetValue("save", out SavePath);
			Filters.Clear();
			if (fields.TryGetValue("filter", out string filter))
				foreach (string f in filter.Split(';'))
					if (f.Trim().Length > 0)
						Filters.Add(f.Trim());
			if (fields.TryGetValue("timeout", out string timeout) && double.TryParse(timeout, NumberStyles.Float, CultureInfo.InvariantCulture, out double t) && t > 0)
				TimeoutSeconds = t;
			if (fields.TryGetValue("test-timeout", out string testTimeout) && double.TryParse(testTimeout, NumberStyles.Float, CultureInfo.InvariantCulture, out double tt) && tt > 0)
				TestTimeoutSeconds = tt;
			Trace = fields.TryGetValue("trace", out string trace) && trace == "1";
			Tests.Clear();
			NextIndex = 0;
			Passed = Failed = Errors = Timeouts = Skipped = 0;
			Reloads = 0;
			LastAssembly = null;
			SaveIsFresh = false;
			IdleSince = -1f;
			StartedAt = Time.realtimeSinceStartup;
			Finished = false;
			WriteState("busy");
			Debug.Log(Tag + "next run: tests from " + Dir + (Filters.Count > 0 ? " matching " + string.Join(";", Filters) : "") + ", fixture " + SavePath);
			Start();
		}

		/// <summary>
		/// Starts a run where the game is: the tests found, then either the no-colony driver (a run
		/// of NoColony tests only, which loads no save) or the fixture loaded (the colony's spawn
		/// attaches the driver).
		/// </summary>
		public static void Start()
		{
			Started = true;
			try
			{
				Discover();
			}
			catch (Exception e)
			{
				Debug.LogWarning(Tag + e);
				Finish("runner-error", e.ToString());
				return;
			}
			if (AllNoColony)
			{
				Debug.Log(Tag + "every selected test needs no colony: running them without loading a save");
				var driver = new GameObject("OniTestNoColonyDriver");
				UnityEngine.Object.DontDestroyOnLoad(driver);
				driver.AddComponent<TestBehaviour>().NoColony = true;
				return;
			}
			if (!File.Exists(SavePath))
			{
				Finish("no-save", "file not found: " + SavePath);
				return;
			}
			Debug.Log(Tag + "loading " + SavePath);
			LoadScreen.DoLoad(SavePath);
		}

		private static void Quit(string why)
		{
			Debug.Log(Tag + "ending the live session: " + why);
			WriteState("quitting");
			App.Quit();
		}

		private static void TryDelete(string path)
		{
			try
			{
				File.Delete(path);
			}
			catch (Exception)
			{
			}
		}
	}

	/// <summary>One discovered test method.</summary>
	public sealed class TestCase
	{
		public readonly string AssemblyName;
		public readonly Type Type;
		public readonly MethodInfo Method;
		public readonly string Name;
		public readonly double TimeoutSeconds;
		public readonly int Layer;
		public readonly bool FreshSave;
		public readonly bool Explicit;
		public readonly bool Boxed;
		public readonly bool NoColony;

		public TestCase(string assemblyName, Type type, MethodInfo method, OniTestAttribute attr)
		{
			AssemblyName = assemblyName;
			Type = type;
			Method = method;
			Name = attr.Name ?? method.Name;
			FreshSave = attr.FreshSave;
			Explicit = attr.Explicit;
			Boxed = attr.Boxed;
			NoColony = attr.NoColony;
			TimeoutSeconds = attr.Timeout > 0 ? attr.Timeout : Session.TestTimeoutSeconds;
			LayerAttribute layer = method.GetCustomAttribute<LayerAttribute>() ?? type.GetCustomAttribute<LayerAttribute>();
			Layer = layer != null ? layer.Layer : -1;
		}

		public string Id => AssemblyName + "/" + Type.Name + "." + Method.Name;

		public System.Collections.IEnumerator Invoke(TestContext t)
		{
			return (System.Collections.IEnumerator)Method.Invoke(null, new object[] { t });
		}
	}

	/// <summary>What one test produced; one line of results.jsonl.</summary>
	public sealed class TestResult
	{
		public string Id;
		public string AssemblyName, ClassName, MethodName, Name;
		public int Layer;
		public string Status = "pass";
		public string Message;
		public string Stack;
		public double DurationSeconds;
		public double GameSeconds;
		public readonly List<string> LogErrors = new List<string>();
		public readonly List<string> Notes = new List<string>();

		public TestResult(TestCase test)
		{
			Id = test.Id;
			AssemblyName = test.AssemblyName;
			ClassName = test.Type.Name;
			MethodName = test.Method.Name;
			Name = test.Name;
			Layer = test.Layer;
		}

		public string ToJson()
		{
			return "{\"id\":" + Json.Str(Id) + ",\"assembly\":" + Json.Str(AssemblyName) + ",\"class\":" + Json.Str(ClassName)
				+ ",\"method\":" + Json.Str(MethodName) + ",\"name\":" + Json.Str(Name) + ",\"layer\":" + Layer
				+ ",\"status\":" + Json.Str(Status) + ",\"message\":" + Json.Str(Message) + ",\"stack\":" + Json.Str(Stack)
				+ ",\"durationSeconds\":" + DurationSeconds.ToString("F2", CultureInfo.InvariantCulture)
				+ ",\"gameSeconds\":" + GameSeconds.ToString("F2", CultureInfo.InvariantCulture)
				+ ",\"logErrors\":" + Json.Arr(LogErrors) + ",\"notes\":" + Json.Arr(Notes) + "}";
		}
	}

	internal static class Json
	{
		public static string Str(string s)
		{
			if (s == null)
				return "null";
			var sb = new StringBuilder("\"");
			foreach (char c in s)
			{
				switch (c)
				{
					case '"': sb.Append("\\\""); break;
					case '\\': sb.Append("\\\\"); break;
					case '\n': sb.Append("\\n"); break;
					case '\r': sb.Append("\\r"); break;
					case '\t': sb.Append("\\t"); break;
					default:
						if (c < ' ')
							sb.Append("\\u").Append(((int)c).ToString("x4"));
						else
							sb.Append(c);
						break;
				}
			}
			return sb.Append('"').ToString();
		}

		public static string Arr(IEnumerable<string> items)
		{
			var sb = new StringBuilder("[");
			bool first = true;
			foreach (string item in items)
			{
				if (!first)
					sb.Append(',');
				first = false;
				sb.Append(Str(item));
			}
			return sb.Append(']').ToString();
		}
	}
}
