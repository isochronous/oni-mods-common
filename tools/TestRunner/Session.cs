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

		public static readonly List<TestCase> Tests = new List<TestCase>();
		/// <summary>Index of the next test to run; survives a scene change, so a reload resumes here.</summary>
		public static int NextIndex;
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
				}
			}
			Active = !string.IsNullOrEmpty(Dir) && !string.IsNullOrEmpty(SavePath);
			StartedAt = Time.realtimeSinceStartup;
		}

		/// <summary>
		/// Loads every DLL under DIR/tests and lists its [OniTest] methods: public static, returning
		/// IEnumerator, taking one TestContext. Order: assembly name, class, declaration order. Called
		/// once the colony is up, so every mod a test references has been loaded by the game.
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
				Assembly asm = Assembly.LoadFrom(dll);
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
				if (regexes.Count == 0 || regexes.Exists(r => r.IsMatch(test.Id) || r.IsMatch(test.Name)))
					Tests.Add(test);
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
				+ ",\"timeouts\":" + Timeouts + ",\"skipped\":" + Skipped
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
			else
				App.Quit();
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

		public TestCase(string assemblyName, Type type, MethodInfo method, OniTestAttribute attr)
		{
			AssemblyName = assemblyName;
			Type = type;
			Method = method;
			Name = attr.Name ?? method.Name;
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
