using HarmonyLib;
using KMod;

namespace SaveRunner
{
	/// <summary>
	/// Development tool. Started with --saverun=FILE the game loads that save from the main menu,
	/// lets it run for --saverun-seconds of game time (default 60) at --saverun-speed (1 to 3)
	/// with every save to disk blocked, writes the readings of --saverun-probe to --saverun-out
	/// and quits. Without the argument the mod does nothing, so it can stay installed.
	/// </summary>
	public sealed class SaveRunnerMod : UserMod2
	{
		public override void OnLoad(Harmony harmony)
		{
			base.OnLoad(harmony);
			Runner.ReadCommandLine();
			if (!Runner.Active)
				return;
			Debug.Log(Runner.Tag + "will load " + Runner.SavePath + ", run " + Runner.Seconds + " s at speed " + Runner.Speed
				+ (Runner.Probe != null ? " with probe " + Runner.Probe : "") + (Runner.OutFile != null ? ", writing " + Runner.OutFile : ""));
			// No crash dialog during a run: its ReportErrorDialog calls App.QuitCode(1) when the scene
			// change destroys it, which ended runs without a word. Exceptions still go to the log.
			KCrashReporter.ignoreAll = true;
			KCrashReporter.terminateOnError = false;
			// Whoever ends the process during a run leaves a trace: a managed caller of Application.Quit
			// shows up in the stack here, a window close or an external kill does not.
			UnityEngine.Application.wantsToQuit += () =>
			{
				Debug.Log(Runner.Tag + "Application wants to quit" + (Runner.Finished ? " (runner)" : ":\n" + System.Environment.StackTrace));
				return true;
			};
			UnityEngine.Application.quitting += () => Debug.Log(Runner.Tag + "Application quitting");
			// Unity defers Application.Quit to the end of the frame, so the caller is gone by the time
			// wantsToQuit fires; catch it at the call instead. Done by hand so a missing method on some
			// game build costs a warning, not the mod.
			TracePatch(harmony, AccessTools.Method(typeof(App), "Restart"), "App.Restart");
			TracePatch(harmony, AccessTools.Method(typeof(UnityEngine.Application), "Quit", new System.Type[0]), "Application.Quit");
		}

		private static void TracePatch(Harmony harmony, System.Reflection.MethodInfo target, string name)
		{
			if (target == null)
			{
				Debug.LogWarning(Runner.Tag + name + " not found; quits through it go untraced");
				return;
			}
			try
			{
				harmony.Patch(target, prefix: new HarmonyMethod(typeof(SaveRunnerMod), nameof(TraceQuit)));
			}
			catch (System.Exception e)
			{
				Debug.LogWarning(Runner.Tag + "cannot trace " + name + ": " + e.Message);
			}
		}

		public static void TraceQuit(System.Reflection.MethodBase __originalMethod)
		{
			if (Runner.Active)
				Debug.Log(Runner.Tag + __originalMethod.DeclaringType?.Name + "." + __originalMethod.Name + " called" + (Runner.Finished ? " by the runner" : " by:\n" + System.Environment.StackTrace));
		}
	}
}
