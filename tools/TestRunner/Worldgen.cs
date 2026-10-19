using System;
using System.Collections.Generic;
using HarmonyLib;
using Klei.CustomSettings;
using ProcGen;
using UnityEngine;

namespace Isochronous.TestRunner
{
	/// <summary>
	/// Tests in a freshly generated world ([OniTest(Worldgen = "COORD")]): from the main menu the
	/// coordinate is applied and the game's own worldgen screen opened (the path of its
	/// devAutoWorldGen setting, as tools/WorldgenDriver does); when generation completes the game
	/// loads the colony and the driver resumes at the test. Consecutive tests with one coordinate
	/// share the generation. A failed generation fails those tests and the run goes on.
	/// </summary>
	public static class Worldgen
	{
		/// <summary>Real seconds without a progress report from the worldgen thread that count as a hang.</summary>
		public const double HangSeconds = 300;

		/// <summary>The coordinate to generate next, set when a test needs it; generated once the main menu is up.</summary>
		public static string Pending;
		/// <summary>The test whose world Pending is: its Prepare runs, and its world key is kept, when generation begins.</summary>
		public static TestCase PendingTest;
		/// <summary>True from the worldgen screen's opening until the colony spawns or generation fails.</summary>
		public static bool Generating;
		/// <summary>Why the last generation (or its Prepare) failed, as its tests' message, until they are written off at the main menu.</summary>
		public static string Failure;
		/// <summary>The colony now loaded, when it was generated rather than loaded from the fixture.</summary>
		public static GeneratedWorld Current;

		private static GeneratedWorld underway;
		private static float lastProgress;

		/// <summary>"SNDST-C-100-0-0-0" is a Spaced Out cluster, "SNDST-A-100-0-0-0" a base-game one: the letter before the seed.</summary>
		public static bool IsSpacedOut(string coordinate)
		{
			string[] parts = coordinate.Split('-');
			return parts.Length >= 5 && parts[parts.Length - 5] == "C";
		}

		/// <summary>Null when the game's mode can generate the coordinate, else why not.</summary>
		public static string ModeMismatch(string coordinate)
		{
			bool wanted = IsSpacedOut(coordinate);
			if (DlcManager.IsExpansion1Active() == wanted)
				return null;
			return coordinate + " is a " + (wanted ? "Spaced Out" : "base game") + " cluster and the game is in " + (wanted ? "base game" : "Spaced Out") + " mode";
		}

		/// <summary>
		/// Called at the settled main menu: undoes the last world's Prepare, runs this world's, applies
		/// the coordinate and opens the worldgen screen. A Prepare that throws fails the world's tests
		/// as a failed generation does, without leaving the menu.
		/// </summary>
		public static void Begin(MainMenu menu)
		{
			string coordinate = Pending;
			TestCase test = PendingTest;
			Session.RunUndo();
			if (test?.Prepare != null)
			{
				try
				{
					Debug.Log(Session.Tag + "preparing " + test.WorldKey);
					Session.SetUndo(test.WorldKey, test.RunPrepare());
				}
				catch (Exception e)
				{
					Debug.LogWarning(Session.Tag + "the Prepare of " + test.WorldKey + " threw: " + e);
					Failure = "Prepare threw: " + e.GetType().Name + ": " + e.Message;
					return;
				}
			}
			underway = new GeneratedWorld(coordinate, test?.WorldKey);
			Generating = true;
			lastProgress = Time.realtimeSinceStartup;
			try
			{
				ProcGenGame.WorldGen.WaitForPendingLoadSettings();
				CustomGameSettings settings = CustomGameSettings.Instance;
				settings.SetSurvivalDefaults();
				settings.LoadClusters();
				string[] parts = CustomGameSettings.ParseSettingCoordinate(coordinate);
				if (parts.Length < 4 || !int.TryParse(parts[2], out _))
					throw new ArgumentException("cannot parse the coordinate " + coordinate);
				ClusterLayout layout = null;
				foreach (string name in SettingsCache.GetClusterNames())
				{
					ClusterLayout data = SettingsCache.clusterLayouts.GetClusterData(name);
					if (data != null && data.coordinatePrefix == parts[1])
						layout = data;
				}
				if (layout == null)
					throw new ArgumentException("no cluster with the coordinate prefix " + parts[1] + " in this mode");
				settings.SetQualitySetting(CustomGameSettingConfigs.ClusterLayout, layout.filePath);
				settings.SetQualitySetting(CustomGameSettingConfigs.WorldgenSeed, parts[2]);
				settings.ParseAndApplySettingsCode(parts[3]);
				settings.ParseAndApplyStoryTraitSettingsCode(parts.Length >= 5 ? parts[4] : "0");
				settings.ParseAndApplyMixingSettingsCode(parts.Length >= 6 ? parts[5] : "0");
				underway.Cluster = layout.filePath;
				underway.Applied = settings.GetSettingsCoordinate();
				Debug.Log(Session.Tag + "generating " + underway.Applied + " (" + layout.filePath + ")");
				Util.KInstantiateUI(ScreenPrefabs.Instance.WorldGenScreen.gameObject, menu.gameObject, force_active: true);
			}
			catch (Exception e)
			{
				Fail(e.Message);
			}
		}

		/// <summary>The colony that just spawned: the generated one when a generation was under way, else the fixture.</summary>
		public static void OnColonySpawned()
		{
			if (Generating && underway != null)
			{
				underway.Seconds = Time.realtimeSinceStartup - underway.StartedAt;
				Current = underway;
				Debug.Log(Session.Tag + "generated " + Current.Applied + " in " + Current.Seconds.ToString("F0") + " s" + (Current.Errors.Count > 0 ? " with " + Current.Errors.Count + " error(s)" : ""));
			}
			else
				Current = null;
			Generating = false;
			Pending = null;
			PendingTest = null;
			underway = null;
		}

		/// <summary>Generation failed: back to the main menu, where its tests are written off.</summary>
		public static void Fail(string why)
		{
			Debug.LogWarning(Session.Tag + "generation of " + Pending + " failed: " + why);
			Failure = "the world could not be generated: " + why + (underway != null && underway.Errors.Count > 0 ? " (" + string.Join(" | ", underway.Errors) + ")" : "");
			Generating = false;
			underway = null;
			App.LoadScene("frontend");
		}

		/// <summary>Per frame while generating: a worldgen thread with no progress report for HangSeconds is a hang, which ends the session.</summary>
		public static void Watch()
		{
			if (Generating && Time.realtimeSinceStartup - lastProgress > HangSeconds)
				Session.Finish("worldgen-hang", "no progress generating " + Pending + " for " + HangSeconds + " s");
		}

		internal static void Progress()
		{
			lastProgress = Time.realtimeSinceStartup;
		}

		internal static void Error(string text)
		{
			underway?.Errors.Add(text);
		}

		private static readonly System.Reflection.FieldInfo ProceedButton = AccessTools.Field(typeof(CharacterSelectionController), "proceedButton");
		private static readonly System.Reflection.FieldInfo WattsonButton = AccessTools.Field(typeof(WattsonMessage), "button");

		/// <summary>
		/// A new colony opens on the "choose three Duplicants" screen over the map, then, once embarked,
		/// on the printing pod's opening message ("ALERT", Begin), which pauses the game. The driver
		/// embarks with the Duplicants offered and begins, through each screen's own button, the way a
		/// player clicks them, so tests and screenshots see the colony as a player would. Nothing here
		/// throws (an exception would end the driver's coroutine and leave the game idle): a screen
		/// that does not come or does not go is logged and the run goes on.
		/// </summary>
		public static System.Collections.IEnumerator Embark()
		{
			MinionSelectScreen select = null;
			float deadline = Time.realtimeSinceStartup + 30f;
			while (Time.realtimeSinceStartup < deadline)
			{
				select = UnityEngine.Object.FindObjectOfType<MinionSelectScreen>();
				if (select != null && select.isActiveAndEnabled)
					break;
				select = null;
				yield return null;
			}
			if (select == null)
			{
				Debug.LogWarning(Session.Tag + "no Duplicant selection screen came up in the generated colony");
				yield break;
			}
			// The screen fills its three containers over its first frames.
			for (int i = 0; i < 5; i++)
				yield return null;
			KButton embark = ProceedButton?.GetValue(select) as KButton;
			if (embark == null)
			{
				Debug.LogWarning(Session.Tag + "the Duplicant selection screen's Embark button was not found (the game changed)");
				yield break;
			}
			Debug.Log(Session.Tag + "embarking with the Duplicants offered");
			embark.SignalClick(KKeyCode.Mouse0);
			// The screen is destroyed once it has done its work; Unity's == null sees that.
			deadline = Time.realtimeSinceStartup + 30f;
			while (Time.realtimeSinceStartup < deadline && select != null && select.isActiveAndEnabled)
				yield return null;

			// The opening message comes with the new base and lets its button be pressed once shown.
			WattsonMessage message = null;
			KButton begin = null;
			deadline = Time.realtimeSinceStartup + 30f;
			while (Time.realtimeSinceStartup < deadline)
			{
				message = UnityEngine.Object.FindObjectOfType<WattsonMessage>();
				begin = message != null && message.isActiveAndEnabled ? WattsonButton?.GetValue(message) as KButton : null;
				if (begin != null && begin.isInteractable)
					break;
				begin = null;
				yield return null;
			}
			if (begin == null)
				Debug.LogWarning(Session.Tag + "the opening message's Begin button did not come up");
			else
			{
				Debug.Log(Session.Tag + "beginning (the opening message's button)");
				begin.SignalClick(KKeyCode.Mouse0);
				deadline = Time.realtimeSinceStartup + 15f;
				while (Time.realtimeSinceStartup < deadline && message != null && message.isActiveAndEnabled)
					yield return null;
			}
			for (int i = 0; i < 30; i++)
				yield return null;
			Debug.Log(Session.Tag + "embarked; " + Components.LiveMinionIdentities.Count + " Duplicant(s) in the colony");
		}

		/// <summary>Leaves the colony for the main menu the way the game's quit does, to generate there.</summary>
		public static void LeaveColony()
		{
			LoadScreen.ForceStopGame();
			App.LoadScene("frontend");
		}
	}

	/// <summary>What a test in a generated world can ask about it (t.Generated).</summary>
	public sealed class GeneratedWorld
	{
		/// <summary>The coordinate the test asked for, and the one the game reports after applying it.</summary>
		public readonly string Coordinate;
		/// <summary>The coordinate and the Prepare method of the tests it was generated for (TestCase.WorldKey).</summary>
		public readonly string Key;
		public string Applied;
		public string Cluster;
		/// <summary>Errors the worldgen reported (OfflineWorldGen.OnError), in order.</summary>
		public readonly List<string> Errors = new List<string>();
		/// <summary>Real seconds from the worldgen screen's opening to the colony's spawn.</summary>
		public float Seconds;
		internal readonly float StartedAt = Time.realtimeSinceStartup;

		public GeneratedWorld(string coordinate, string key)
		{
			Coordinate = coordinate;
			Key = key ?? coordinate + "|";
		}
	}

	[HarmonyPatch(typeof(OfflineWorldGen), "UpdateProgress")]
	public static class OfflineWorldGen_UpdateProgress_Patch
	{
		public static void Postfix()
		{
			if (Session.Active && Worldgen.Generating)
				Worldgen.Progress();
		}
	}

	[HarmonyPatch(typeof(OfflineWorldGen), "OnError")]
	public static class OfflineWorldGen_OnError_Patch
	{
		public static void Postfix(OfflineWorldGen.ErrorInfo error)
		{
			if (!Session.Active || !Worldgen.Generating)
				return;
			string text = error.errorDesc + (error.exception != null ? " | " + error.exception.Message : "");
			Worldgen.Error(text);
			Debug.LogWarning(Session.Tag + "worldgen error: " + text);
		}
	}

	/// <summary>The game's failure path (shut the sim down, a dialog, back to the menu on OK) without the dialog.</summary>
	[HarmonyPatch(typeof(OfflineWorldGen), "DoExitFlow")]
	public static class OfflineWorldGen_DoExitFlow_Patch
	{
		public static bool Prefix()
		{
			if (!Session.Active || !Worldgen.Generating)
				return true;
			Sim.Shutdown();
			Worldgen.Fail("the worldgen gave up");
			return false;
		}
	}
}
