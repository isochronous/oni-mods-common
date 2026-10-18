---
doc_id: doc-706c134736e2
description: "SaveRunner's entities: SaveRunnerMod, Runner and its members, each Harmony patch in Patches.cs, RunBehaviour and FridgeProbe, Invoke-SaveRun.ps1 and its parameters, files read and written, business rules, code paths and run state transitions"
applies_to:
  - "*.cs"
  - "*.ps1"
  - "*.csproj"
  - "*.yaml"
---

# SaveRunner domain

## Overview
**Id:** blk-a0e41250aa34  
**Tags:** activation-guard, save-files, mod-loadout, probe

SaveRunner loads a save in the real game with nobody at the keyboard, runs the colony for a number of game seconds with all saving blocked, records readings once per game second and quits. It exists to reproduce an in-game observation from a player's save, with and without a mod, under real sim behaviour and the player's own mod loadout. Two parts: a development mod (`SaveRunner`, inert unless the game starts with `--saverun=FILE`) and a launcher (`Invoke-SaveRun.ps1`). It is not published to the Workshop.

## Key Entities
**Id:** blk-6e0fb1c00ac7

### SaveRunnerMod
**Id:** blk-ca4d28981335  
**Tags:** mod-entry-point, command-line, crash-dialog, harmony-patching  
**Description**: the mod's entry point (`UserMod2`). Reads the command line at load; when a run is on it sets the crash-reporter flags and installs quit tracing.  
**Location**: `Mod.cs`  
**Relationships**: calls `Runner.ReadCommandLine`; traces `App.Restart` and `Application.Quit` through `TracePatch`/`TraceQuit`; the Harmony patches in `Patches.cs` are applied by `base.OnLoad`.

### Runner
**Id:** blk-b5fe6721e0d5  
**Tags:** command-line, run-result  
**Description**: the command line and state of one run, and the way it ends.  
**Location**: `Runner.cs`

| Member | Description |
|--------|-------------|
| `Active` | true when `--saverun=FILE` was given |
| `SavePath`, `Seconds`, `Speed`, `Probe`, `OutFile`, `TimeoutSeconds` | the options |
| `Started`, `Finished` | load requested; run ended |
| `StartedAt`, `MenuSeenAt` | real-time marks (`MenuSeenAt` is -1 until the main menu appears) |
| `Finish(status, detail)` | writes the status line, quits |

### Patches
**Id:** blk-4905afd4b039  
**Tags:** harmony-patching, activation-guard, watchdog, save-files  
**Description**: the Harmony patches, each a no-op unless `Runner.Active`.  
**Location**: `Patches.cs`

| Patch | Target | Does |
|-------|--------|------|
| `MainMenu_OnSpawn_Patch` | `MainMenu.OnSpawn` | notes when the menu appeared |
| `App_LateUpdate_Patch` | `App.LateUpdate` | four seconds after the menu: checks the file exists and calls `LoadScreen.DoLoad(path)`; also the real-time timeout watchdog |
| `Game_OnSpawn_Patch` | `Game.OnSpawn` | adds `RunBehaviour` to the Game object once the colony is up |
| `SaveLoader_Save_Patch` | `SaveLoader.Save(string, bool, bool)` | skips the save and returns the filename |
| `App_Quit_Patch` | `App.Quit` | logs who quit, with a stack when it was not the runner |

### RunBehaviour
**Id:** blk-1f9288a97c7a  
**Tags:** probe  
**Description**: a `MonoBehaviour` on the Game object for one run. Unpauses and sets the speed, accumulates scaled frame time as game seconds, samples the probe once per game second, ends the run at `Runner.Seconds`.  
**Location**: `RunBehaviour.cs`  
**Relationships**: owns the CSV `StreamWriter`; calls `FridgeProbe.Sample`; ends through `Runner.Finish`.

### FridgeProbe
**Id:** blk-043bf2a0945a  
**Tags:** probe  
**Description**: the one probe, `fridges`. For every building with a `RefrigeratorController.Def` and every item in its `Storage`, writes controller state, powered state, whether No Magic Fridges' `FridgeThermostat` is on the building, the def's setpoint, the cell's temperature, element and mass, and per item its element, mass, temperature, `SimTemperatureTransfer` surface area and ground-transfer scale, whether that component is enabled, its sim handle and `deltaKJ`.  
**Location**: `RunBehaviour.cs`

### Invoke-SaveRun.ps1
**Id:** blk-3eb75b734c83  
**Tags:** game-launch, mod-loadout, command-line  
**Description**: the launcher. Copies the save into the run folder, rewrites `mods.json` for the run, starts the game through Steam, waits for it to quit, restores `mods.json`, prints the status.  
**Location**: `Invoke-SaveRun.ps1`

| Parameter | Description |
|-----------|-------------|
| `-Save` | the `.sav` to load (required) |
| `-Seconds`, `-Speed` | game seconds (default 60) and speed 1 to 3 |
| `-Probe` | `fridges` or none |
| `-With`, `-Without` | mods to add or remove for the run |
| `-TimeoutSec`, `-Label`, `-OutDir`, `-SteamExe`, `-AppId` | real-time limit (900), run-folder prefix, output root, Steam path, game app id (457140) |

### Files it reads and writes
**Id:** blk-2c721000393a  
**Tags:** save-files, mod-loadout, probe, run-result

| File | Direction | Purpose |
|------|-----------|---------|
| the save | read | copied to `runs/<Label>-<stamp>/colony.sav`; the original is never opened by the game |
| `Documents\Klei\OxygenNotIncluded\mods\mods.json` | read, write, restored | the loadout: current mods, plus SaveRunner and `-With`, minus `-Without`; backup at `mods.json.saverun-backup` |
| `kplayerprefs.yaml` | read | tells whether the game is in Spaced Out mode so `enabledForDlc` is set to match |
| `probe.csv` | written by the game | one row per item per game second, or one row for an empty fridge; written only when `--saverun-probe` is given (the launcher always passes `--saverun-out`) |
| `probe.csv.status` | written by the game | one JSON line when the run ends |
| `Player.log` | copied | into the run folder after the game exits |

## Business Rules
**Id:** blk-ae74a2604bba  
**Tags:** activation-guard, save-files, mod-loadout, game-launch

- Without `--saverun` the mod changes nothing.
- Nothing is saved while a run is on: `SaveLoader.Save` is skipped, and the game opens a copy of the save.
- The game's mode (base game or Spaced Out) is left as it is; the save has to be from the current mode.
- Mod Profile Manager (Workshop 2854869130) and its Light addon (3281716506) restart the game when a save loads with a different mod list, which a run always has; disable them with `-Without 2854869130,3281716506`.
- The save path travels through Steam's command line, so the copy is named `colony.sav` in a folder without spaces.
- The launcher refuses to start when the game is already running.

## Key Code Paths
**Id:** blk-d8b4ef45566a  
**Tags:** game-launch, harmony-patching, probe, run-result

- **Start**: `Invoke-SaveRun.ps1` builds the arguments and runs `steam.exe -applaunch 457140 --saverun=...`; the game loads the mod, `SaveRunnerMod.OnLoad` reads the arguments. See `Mod.cs`, `Runner.cs`.
- **Load**: `MainMenu.OnSpawn` postfix notes the time; the `App.LateUpdate` postfix calls `LoadScreen.DoLoad` four seconds later. See `Patches.cs`.
- **Run**: `Game.OnSpawn` postfix adds `RunBehaviour`; it samples each game second and finishes at the time limit. See `RunBehaviour.cs`.
- **End**: `Runner.Finish` writes the status line and quits; the launcher sees the process exit, copies `Player.log`, restores `mods.json`. See `Runner.cs`, `Invoke-SaveRun.ps1`.

## State Transitions
**Id:** blk-bafa30d4781e  
**Tags:** run-result, watchdog

| State | Transitions To | Trigger |
|-------|---------------|---------|
| inactive | waiting for menu | `--saverun` given |
| waiting for menu | loading | four seconds after the menu appeared (`Runner.Started`) |
| waiting for menu | finished (`no-save`) | file not found |
| loading | running | `Game.OnSpawn`, `RunBehaviour` added |
| running | finished (`done`) | game seconds reached |
| running | finished (`runner-error`) | exception in `RunBehaviour.Update` |
| waiting for menu, loading or running | finished (`timeout`) | real time since the command line was read passes `TimeoutSeconds` before the run finishes |

## Related
**Id:** blk-bf1ccef2e930

- See also: [glossary](glossary.md)
- Governed by: [naming conventions](../standards/naming-conventions.md)
