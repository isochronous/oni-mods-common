using System;
using HarmonyLib;
using Klei.CustomSettings;
using ProcGen;

namespace WorldgenDriver
{
	/// <summary>
	/// At the main menu (first boot and after each coordinate): check the mode, apply the next
	/// coordinate and open the worldgen screen, the path the game's own devAutoWorldGen setting takes.
	/// </summary>
	[HarmonyPatch(typeof(MainMenu), "OnSpawn")]
	public static class MainMenu_OnSpawn_Patch
	{
		public static void Postfix(MainMenu __instance)
		{
			if (!Driver.Active || Driver.Started || Driver.Coordinate == null)
				return;
			Driver.BeginCoordinate();
			try
			{
				if (DlcManager.IsExpansion1Active() != Driver.WantExpansion1)
				{
					Driver.Finish("wrong-mode", "game is in " + (DlcManager.IsExpansion1Active() ? "Spaced Out" : "base game") + " mode; set EXPANSION1_ID.ENABLED in kplayerprefs.yaml and relaunch");
					return;
				}
				ProcGenGame.WorldGen.WaitForPendingLoadSettings();
				CustomGameSettings settings = CustomGameSettings.Instance;
				settings.SetSurvivalDefaults();
				settings.LoadClusters();
				string[] parts = CustomGameSettings.ParseSettingCoordinate(Driver.Coordinate);
				if (parts.Length < 4 || !int.TryParse(parts[2], out _))
				{
					Driver.Finish("bad-args", "cannot parse coordinate " + Driver.Coordinate);
					return;
				}
				ClusterLayout layout = null;
				foreach (string name in SettingsCache.GetClusterNames())
				{
					ClusterLayout data = SettingsCache.clusterLayouts.GetClusterData(name);
					if (data != null && data.coordinatePrefix == parts[1])
						layout = data;
				}
				if (layout == null)
				{
					Driver.Finish("bad-args", "no cluster with coordinate prefix " + parts[1] + " in this mode");
					return;
				}
				settings.SetQualitySetting(CustomGameSettingConfigs.ClusterLayout, layout.filePath);
				settings.SetQualitySetting(CustomGameSettingConfigs.WorldgenSeed, parts[2]);
				settings.ParseAndApplySettingsCode(parts[3]);
				settings.ParseAndApplyStoryTraitSettingsCode(parts.Length >= 5 ? parts[4] : "0");
				settings.ParseAndApplyMixingSettingsCode(parts.Length >= 6 ? parts[5] : "0");
				if (Driver.Stories == "all")
					foreach (SettingConfig story in settings.StorySettings.Values)
						settings.SetStorySetting(story, true);
				if (Driver.Mixing == "all" || Driver.Mixing == "guarantee")
					foreach (SettingConfig mixing in settings.MixingSettings.Values)
					{
						if (mixing is DlcMixingSettingConfig)
							settings.SetMixingSetting(mixing, DlcMixingSettingConfig.EnabledLevelId);
						else if (mixing is MixingSettingConfig)
							settings.SetMixingSetting(mixing, Driver.Mixing == "guarantee" ? "GuranteeMixing" : "TryMixing");
					}
				Driver.ClusterId = layout.filePath;
				Driver.Seed = parts[2];
				Driver.Applied = settings.GetSettingsCoordinate();
				Debug.Log(Driver.Tag + "generating " + Driver.Applied + " (" + layout.filePath + ", stories=" + (Driver.Stories == "" ? "coordinate" : Driver.Stories) + ", mixing=" + (Driver.Mixing == "" ? "coordinate" : Driver.Mixing) + ")");
				Util.KInstantiateUI(ScreenPrefabs.Instance.WorldGenScreen.gameObject, __instance.gameObject, force_active: true);
			}
			catch (Exception e)
			{
				Debug.LogWarning(Driver.Tag + e);
				Driver.Finish("driver-error", e.ToString());
			}
		}
	}

	/// <summary>Runs on the worldgen thread for every progress report.</summary>
	[HarmonyPatch(typeof(OfflineWorldGen), "UpdateProgress")]
	public static class OfflineWorldGen_UpdateProgress_Patch
	{
		public static void Postfix(StringKey stringKeyRoot, float completePercent)
		{
			if (Driver.Active)
				Driver.Progress(stringKeyRoot.String, completePercent);
		}
	}

	/// <summary>
	/// Main-thread watchdog on the one per-frame hook that exists in every scene: a crash screen or a
	/// stall (including never getting past the main menu, e.g. a mod restart dialog) ends the session.
	/// </summary>
	[HarmonyPatch(typeof(App), "LateUpdate")]
	public static class App_LateUpdate_Patch
	{
		public static void Postfix()
		{
			if (!Driver.Active || Driver.Finished)
				return;
			if (KCrashReporter.hasCrash)
				Driver.Finish("crash", "the game's crash reporter was triggered; see Player.log");
			else if (Driver.SecondsSinceProgress > Driver.TimeoutSeconds)
				Driver.Finish(Driver.Started ? "hang" : "stuck-before-start", "no progress for " + Driver.TimeoutSeconds + " s");
		}
	}

	[HarmonyPatch(typeof(OfflineWorldGen), "OnError")]
	public static class OfflineWorldGen_OnError_Patch
	{
		public static void Postfix(OfflineWorldGen.ErrorInfo error)
		{
			if (!Driver.Active)
				return;
			string text = error.errorDesc + (error.exception != null ? " | " + error.exception : "");
			lock (Driver.Errors) Driver.Errors.Add(text);
			Debug.LogWarning(Driver.Tag + "worldgen error: " + text);
		}
	}

	/// <summary>
	/// The game's failure path (shut the sim down, show the dialog, back to the menu on OK) becomes a
	/// "failed" result; the driver does the shutdown and the return to the menu itself.
	/// </summary>
	[HarmonyPatch(typeof(OfflineWorldGen), "DoExitFlow")]
	public static class OfflineWorldGen_DoExitFlow_Patch
	{
		public static bool Prefix()
		{
			if (!Driver.Active)
				return true;
			if (!Driver.Finished)
			{
				Sim.Shutdown();
				Driver.Finish("failed");
			}
			return false;
		}
	}

	/// <summary>Generation complete: the game would load the colony. Record it and move on unless asked to load.</summary>
	[HarmonyPatch(typeof(App), nameof(App.LoadScene))]
	public static class App_LoadScene_Patch
	{
		public static bool Prefix(string scene_name)
		{
			if (!Driver.Active || Driver.Finished || scene_name != "backend")
				return true;
			if (Driver.LoadColony)
			{
				Debug.Log(Driver.Tag + "generation complete, loading the colony");
				return true;
			}
			Driver.Finish("complete");
			return false;
		}
	}

	[HarmonyPatch(typeof(Game), "OnSpawn")]
	public static class Game_OnSpawn_Patch
	{
		public static void Postfix()
		{
			if (Driver.Active && Driver.LoadColony)
				Driver.Finish("loaded");
		}
	}
}
