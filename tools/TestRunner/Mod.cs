using HarmonyLib;
using KMod;

namespace Isochronous.TestRunner
{
	/// <summary>
	/// Development tool. Started with --onitest-dir=DIR and --onitest-save=FILE the game loads that
	/// save from the main menu, runs every [OniTest] coroutine in DIR/tests/*.dll against it with
	/// saving to disk blocked, writes one JSON line per test to DIR/results.jsonl, a status line to
	/// DIR/results.jsonl.status, and quits. Without the arguments the mod does nothing, so it can
	/// stay installed. See README.md and Kit/ for what a test can do.
	/// </summary>
	public sealed class TestRunnerMod : UserMod2
	{
		public override void OnLoad(Harmony harmony)
		{
			base.OnLoad(harmony);
			Session.ReadCommandLine();
			if (!Session.Active)
				return;
			Debug.Log(Session.Tag + "will load " + Session.SavePath + " and run tests from " + Session.Dir
				+ (Session.Filters.Count > 0 ? " matching " + string.Join(";", Session.Filters) : "")
				+ ", session timeout " + Session.TimeoutSeconds + " s, test timeout " + Session.TestTimeoutSeconds + " s");
			// No crash dialog during a run: its ReportErrorDialog calls App.QuitCode(1) when the scene
			// change destroys it, which ended runs without a word. Exceptions still go to the log.
			KCrashReporter.ignoreAll = true;
			KCrashReporter.terminateOnError = false;
			// Test assemblies reference the mods under test and this harness by name; the game loads
			// every mod DLL with LoadFrom, so a name lookup against what is already loaded settles it.
			System.AppDomain.CurrentDomain.AssemblyResolve += (_, e) =>
			{
				string name = new System.Reflection.AssemblyName(e.Name).Name;
				foreach (System.Reflection.Assembly loaded in System.AppDomain.CurrentDomain.GetAssemblies())
					if (loaded.GetName().Name == name)
						return loaded;
				return null;
			};
			// Whoever ends the process during a run leaves a trace: a managed caller of Application.Quit
			// shows up in the stack here, a window close or an external kill does not.
			UnityEngine.Application.wantsToQuit += () =>
			{
				Debug.Log(Session.Tag + "Application wants to quit" + (Session.Finished ? " (runner)" : ":\n" + System.Environment.StackTrace));
				return true;
			};
			UnityEngine.Application.quitting += () => Debug.Log(Session.Tag + "Application quitting");
			TracePatch(harmony, AccessTools.Method(typeof(App), "Restart"), "App.Restart");
			TracePatch(harmony, AccessTools.Method(typeof(UnityEngine.Application), "Quit", new System.Type[0]), "Application.Quit");
		}

		private static void TracePatch(Harmony harmony, System.Reflection.MethodInfo target, string name)
		{
			if (target == null)
			{
				Debug.LogWarning(Session.Tag + name + " not found; quits through it go untraced");
				return;
			}
			try
			{
				harmony.Patch(target, prefix: new HarmonyMethod(typeof(TestRunnerMod), nameof(TraceQuit)));
			}
			catch (System.Exception e)
			{
				Debug.LogWarning(Session.Tag + "cannot trace " + name + ": " + e.Message);
			}
		}

		public static void TraceQuit(System.Reflection.MethodBase __originalMethod)
		{
			if (Session.Active)
				Debug.Log(Session.Tag + __originalMethod.DeclaringType?.Name + "." + __originalMethod.Name + " called" + (Session.Finished ? " by the runner" : " by:\n" + System.Environment.StackTrace));
		}
	}
}
