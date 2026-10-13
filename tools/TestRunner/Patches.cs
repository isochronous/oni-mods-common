using System.IO;
using HarmonyLib;
using UnityEngine;

namespace Isochronous.TestRunner
{
	/// <summary>
	/// At the main menu: note the time. The load itself waits a few seconds (see the watchdog below)
	/// so the menu's popups have finished laying out before the scene is torn down.
	/// </summary>
	[HarmonyPatch(typeof(MainMenu), "OnSpawn")]
	public static class MainMenu_OnSpawn_Patch
	{
		public static void Postfix()
		{
			if (Session.Active && !Session.Started && Session.MenuSeenAt < 0f)
				Session.MenuSeenAt = Time.realtimeSinceStartup;
		}
	}

	/// <summary>The colony is up: attach the driver to the Game object.</summary>
	[HarmonyPatch(typeof(Game), "OnSpawn")]
	public static class Game_OnSpawn_Patch
	{
		public static void Postfix(Game __instance)
		{
			if (!Session.Active || !Session.Started || Session.Finished)
				return;
			Debug.Log(Session.Tag + "colony loaded after " + (Time.realtimeSinceStartup - Session.StartedAt).ToString("F0") + " s real time");
			__instance.gameObject.AddComponent<TestBehaviour>();
		}
	}

	/// <summary>Whoever quits the game during a run says so in the log, with a stack.</summary>
	[HarmonyPatch(typeof(App), nameof(App.Quit))]
	public static class App_Quit_Patch
	{
		public static void Prefix()
		{
			if (Session.Active)
				Debug.Log(Session.Tag + "App.Quit called" + (Session.Finished ? " by the runner" : " by someone else:\n" + System.Environment.StackTrace));
		}
	}

	/// <summary>
	/// Nothing written to disk while a run is on (no autosave, no save on quit, no colony summary)
	/// except under the run directory, where a round-trip test may save.
	/// </summary>
	[HarmonyPatch(typeof(SaveLoader), nameof(SaveLoader.Save), typeof(string), typeof(bool), typeof(bool))]
	public static class SaveLoader_Save_Patch
	{
		public static bool Prefix(string filename, ref string __result)
		{
			if (!Session.Active)
				return true;
			if (filename != null && Path.GetFullPath(filename).StartsWith(Path.GetFullPath(Session.Dir), System.StringComparison.OrdinalIgnoreCase))
				return true;
			Debug.Log(Session.Tag + "blocked a save to " + filename);
			__result = filename;
			return false;
		}
	}

	/// <summary>
	/// Runs in every scene: starts the load once the main menu has settled, the way the Load screen
	/// does it minus its dialogs, and calls a session that takes too long in real time a hang.
	/// </summary>
	[HarmonyPatch(typeof(App), "LateUpdate")]
	public static class App_LateUpdate_Patch
	{
		private const float MenuSettleSeconds = 4f;

		public static void Postfix()
		{
			if (!Session.Active || Session.Finished)
				return;
			if (!Session.Started && Session.MenuSeenAt >= 0f && Time.realtimeSinceStartup - Session.MenuSeenAt >= MenuSettleSeconds)
			{
				Session.Started = true;
				if (!File.Exists(Session.SavePath))
				{
					Session.Finish("no-save", "file not found: " + Session.SavePath);
					return;
				}
				Debug.Log(Session.Tag + "loading " + Session.SavePath);
				LoadScreen.DoLoad(Session.SavePath);
				return;
			}
			if (Time.realtimeSinceStartup - Session.StartedAt > Session.TimeoutSeconds)
				Session.Finish("timeout", "no end of session after " + Session.TimeoutSeconds + " s real time");
		}
	}

	/// <summary>
	/// The fixture was saved with the player's own loadout, and a run's is always different, so
	/// the game would list the differences in a "mod differences" dialog over the HUD for the
	/// whole session (in the way of any screenshot). The comparison only records events for
	/// that dialog, and the loadout is the launcher's own, so it is skipped during a run.
	/// </summary>
	[HarmonyPatch(typeof(KMod.Manager), nameof(KMod.Manager.MatchFootprint))]
	public static class Manager_MatchFootprint_Patch
	{
		public static bool Prefix(ref bool __result)
		{
			if (!Session.Active)
				return true;
			__result = true;
			return false;
		}
	}

	/// <summary>
	/// The main menu reports what the mod manager noticed at boot (a Workshop mod installed,
	/// removed or updated, a mod deactivated, a restart requested) in a "mod events" dialog, with
	/// a restart button in dev mode. During a run they go to the log instead, under the session
	/// tag, and the dialog never opens.
	/// </summary>
	[HarmonyPatch(typeof(KMod.Manager), nameof(KMod.Manager.Report))]
	public static class Manager_Report_Patch
	{
		public static bool Prefix(KMod.Manager __instance)
		{
			if (!Session.Active)
				return true;
			foreach (KMod.Event e in __instance.events)
			{
				string line = Session.Tag + "mod event: " + e.event_type + " for " + e.mod.title + " (" + e.mod.id + ")"
					+ (string.IsNullOrEmpty(e.details) ? "" : ": " + e.details);
				if (e.event_type == KMod.EventType.LoadError || e.event_type == KMod.EventType.ActiveDuringCrash)
					Debug.LogWarning(line);
				else
					Debug.Log(line);
			}
			__instance.events.Clear();
			return false;
		}
	}

	/// <summary>
	/// A fixture with no Duplicants is a lost colony to the game: GameFlowManager's running state
	/// checks every tick and shows the "colony lost" screen about half a minute after the load,
	/// which would sit over the HUD for the rest of the session. No game over during a run.
	/// </summary>
	[HarmonyPatch(typeof(GameFlowManager.StatesInstance), nameof(GameFlowManager.StatesInstance.CheckForGameOver))]
	public static class GameFlowManager_CheckForGameOver_Patch
	{
		public static bool Prefix()
		{
			return !Session.Active;
		}
	}

	/// <summary>
	/// The game drops every click and key while its window is not focused (KInputManager.isFocused
	/// gates KButton clicks, queued events and the whole input update), and an unattended run has
	/// the window wherever the desktop left it. Unity reports focus changes here; the run forces the
	/// flag back on after each one.
	/// </summary>
	[HarmonyPatch(typeof(KInputManager), nameof(KInputManager.OnApplicationFocus))]
	public static class KInputManager_OnApplicationFocus_Patch
	{
		public static void Postfix()
		{
			if (Session.Active && Session.Started)
				Harness.ForceFocus();
		}
	}

	/// <summary>Same for the per-tool flag that gates InterfaceTool.OnMouseMove's visualizer move and the hover update.</summary>
	[HarmonyPatch(typeof(InterfaceTool), "OnApplicationFocus")]
	public static class InterfaceTool_OnApplicationFocus_Patch
	{
		public static void Postfix(InterfaceTool __instance)
		{
			if (Session.Active && Session.Started)
				Reflect.SetAppFocused(__instance, true);
		}
	}
}
