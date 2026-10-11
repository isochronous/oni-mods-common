using System.IO;
using HarmonyLib;
using UnityEngine;

namespace SaveRunner
{
	/// <summary>
	/// At the main menu: note the time. The load itself waits a few seconds (see the watchdog
	/// below): loading in OnSpawn tore the menu down while its popups were still laying out,
	/// one of them threw in KModalScreen.ResizeBackground, and the crash dialog that opened
	/// quit the game as soon as the scene change destroyed it.
	/// </summary>
	[HarmonyPatch(typeof(MainMenu), "OnSpawn")]
	public static class MainMenu_OnSpawn_Patch
	{
		public static void Postfix()
		{
			if (Runner.Active && !Runner.Started && Runner.MenuSeenAt < 0f)
				Runner.MenuSeenAt = Time.realtimeSinceStartup;
		}
	}

	/// <summary>The colony is up: start the clock and the probe.</summary>
	[HarmonyPatch(typeof(Game), "OnSpawn")]
	public static class Game_OnSpawn_Patch
	{
		public static void Postfix(Game __instance)
		{
			if (!Runner.Active || !Runner.Started || Runner.Finished)
				return;
			Debug.Log(Runner.Tag + "colony loaded after " + (Time.realtimeSinceStartup - Runner.StartedAt).ToString("F0") + " s real time");
			RunBehaviour run = __instance.gameObject.AddComponent<RunBehaviour>();
			Debug.Log(Runner.Tag + "runner attached to " + __instance.gameObject.name + " (active " + __instance.gameObject.activeInHierarchy + ", component " + (run != null) + ")");
		}
	}

	/// <summary>Whoever quits the game during a run says so in the log, with a stack.</summary>
	[HarmonyPatch(typeof(App), nameof(App.Quit))]
	public static class App_Quit_Patch
	{
		public static void Prefix()
		{
			if (Runner.Active)
				Debug.Log(Runner.Tag + "App.Quit called" + (Runner.Finished ? " by the runner" : " by someone else:\n" + System.Environment.StackTrace));
		}
	}

	/// <summary>Nothing written to disk while a run is on: no autosave, no save on quit, no colony summary.</summary>
	[HarmonyPatch(typeof(SaveLoader), nameof(SaveLoader.Save), typeof(string), typeof(bool), typeof(bool))]
	public static class SaveLoader_Save_Patch
	{
		public static bool Prefix(string filename, ref string __result)
		{
			if (!Runner.Active)
				return true;
			Debug.Log(Runner.Tag + "blocked a save to " + filename);
			__result = filename;
			return false;
		}
	}

	/// <summary>
	/// Runs in every scene: starts the load once the main menu has settled, the way the Load
	/// screen does it minus its dialogs, and calls a run that takes too long in real time a hang.
	/// </summary>
	[HarmonyPatch(typeof(App), "LateUpdate")]
	public static class App_LateUpdate_Patch
	{
		private const float MenuSettleSeconds = 4f;

		public static void Postfix()
		{
			if (!Runner.Active || Runner.Finished)
				return;
			if (!Runner.Started && Runner.MenuSeenAt >= 0f && Time.realtimeSinceStartup - Runner.MenuSeenAt >= MenuSettleSeconds)
			{
				Runner.Started = true;
				if (!File.Exists(Runner.SavePath))
				{
					Runner.Finish("no-save", "file not found: " + Runner.SavePath);
					return;
				}
				Debug.Log(Runner.Tag + "loading " + Runner.SavePath);
				LoadScreen.DoLoad(Runner.SavePath);
				return;
			}
			if (Time.realtimeSinceStartup - Runner.StartedAt > Runner.TimeoutSeconds)
				Runner.Finish("timeout", "no end of run after " + Runner.TimeoutSeconds + " s real time");
		}
	}
}
