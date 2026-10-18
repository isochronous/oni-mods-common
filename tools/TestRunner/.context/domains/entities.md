---
doc_id: doc-78306051e4cf
description: "TestRunner's real things: the harness mod, Session, TestBehaviour, patches, FixtureArea, Worldgen, Reflect, the test kit, the launcher and warden, files, rules and states"
applies_to:
  - "*.cs"
  - "Kit/**/*.cs"
  - "*.ps1"
  - "*.csproj"
  - "*.yaml"
---
# TestRunner domain

## Overview
**Id:** blk-70f176589bcc

TestRunner runs a mod's in-game tests in the real game with nobody at the keyboard. A launcher builds the mod's test project, checks it with ModCheck, starts the game with `--onitest-dir=DIR --onitest-save=FILE`, and the harness mod loads a fixture save, runs every `[OniTest]` coroutine found in `DIR/tests/*.dll`, writes one JSON line per test and a status line, and quits (or stays up for the next run). Tests drive the game the way a player does: the real `InterfaceTool` fed the calls the player controller makes (layer 1), with the game's own API for setup and oracles (layer 0). It is a development tool, not for the Workshop, and inert without its arguments. The user-facing guide is `README.md`.

## Key Entities
**Id:** blk-978685bb1f6f

### TestRunnerMod
**Id:** blk-12154230c41b  
**Description**: the entry point (`UserMod2`). Reads the command line, sets the crash-reporter flags, adds the assembly resolver that lets test assemblies find the mods under test and this harness, and traces quits.  
**Location**: `Mod.cs`  
**Relationships**: calls `Session.ReadCommandLine`; the patches in `Patches.cs`, `Worldgen.cs` and `Diagnostics.cs` are applied by `base.OnLoad`.

### Session
**Id:** blk-7f9302c43486  
**Description**: command line, state and results of a session.  
**Location**: `Session.cs`

| Member | Description |
|--------|-------------|
| `Active`, `Started`, `Finished` | session flags; `Active` needs `--onitest-dir` and `--onitest-save` |
| `Dir`, `SavePath`, `Filters`, `TimeoutSeconds`, `TestTimeoutSeconds`, `Keep`, `Trace`, `ServerDir` | the options |
| `Tests`, `NextIndex` | the selected tests and the next to run; survive a scene change |
| `Passed`, `Failed`, `Errors`, `Timeouts`, `Skipped`, `Reloads` | counters for the status line |
| `Discover()` | loads every DLL in `DIR/tests` from its bytes and lists `[OniTest]` methods, sorted by assembly, class and declaration order |
| `Start()`, `Continue()`, `WriteOff()` | begin a run; move to the next test's world or fixture; record tests as not run |
| `Write(result)`, `Finish(status, detail)` | append a result; write the status and quit or go idle |
| `PollQueue()`, `WriteState()` | live-session requests and state file |

### TestCase and TestResult
**Id:** blk-18187e3cb8c6  
**Description**: a discovered test method (id, name, timeout, layer and the attribute flags) and what one test produced (one `results.jsonl` line).  
**Location**: `Session.cs`

### TestBehaviour
**Id:** blk-49587b759cd2  
**Description**: a `MonoBehaviour` on the Game object (or on a driver object for a no-colony run). Waits for the HUD, throws the harness switches, finds and clears the fixture area, then runs each test as a hand-stepped coroutine with a real-time deadline, captures logged errors, runs cleanups and writes the result.  
**Location**: `TestBehaviour.cs` (`LogCapture` in the same file)

### Patches
**Id:** blk-6e02b4b02569  
**Description**: Harmony patches, each inert unless `Session.Active`.  
**Location**: `Patches.cs`

| Patch | Does |
|-------|------|
| `MainMenu_OnSpawn_Patch` | notes when the menu appeared |
| `App_LateUpdate_Patch` | four seconds after the menu: starts the session; drives worldgen; session timeout; polls the live queue |
| `Game_OnSpawn_Patch` | adds `TestBehaviour` when the colony is up |
| `SaveLoader_Save_Patch`, `Game_StartDelayedSave_Patch` | block saves outside the run folder; skip autosave |
| `App_Quit_Patch` | logs who quit |
| `Manager_MatchFootprint_Patch`, `Manager_Report_Patch` | no "mod differences" or "mod events" dialogs |
| `GameFlowManager_CheckForGameOver_Patch` | no "colony lost" screen on a fixture with no Duplicants |
| `KInputManager_OnApplicationFocus_Patch`, `InterfaceTool_OnApplicationFocus_Patch` | force the focus flags back on |

### Harness
**Id:** blk-052ae0295b17  
**Description**: the global switches of a run: focus forced on, mouse position locked (parked in the bottom right corner between tests), physical keyboard and mouse ignored, instant build on, speed 1, tool reset, camera look-at.  
**Location**: `Harness.cs`

### FixtureArea
**Id:** blk-633dd6387bca  
**Description**: the 40 by 10 rectangle of the active world that tests build in: fully revealed and buildable, with the most empty cells (cells underground count double). Cleared before the first test and after every test (objects, loose items, solid cells; Duplicants are the test's to remove). `Dupes.Box` walls it for Duplicant tests.  
**Location**: `FixtureArea.cs`

### Worldgen and GeneratedWorld
**Id:** blk-9e3323c536e4  
**Description**: tests marked `[OniTest(Worldgen = "COORD")]` run in a world freshly generated from that coordinate: the harness goes to the main menu, applies the coordinate, runs the game's worldgen screen, embarks, then runs the test. Consecutive tests with one coordinate share the world. `GeneratedWorld` records the coordinate, the applied coordinate, the cluster, worldgen errors and seconds.  
**Location**: `Worldgen.cs`

### Reflect and Diagnostics
**Id:** blk-741c81a8a7c1  
**Description**: `Reflect` is the list of private game members used, with `SelfCheck`. `Diagnostics` traces the grid marks the game makes during tests (`--onitest-trace=1`).  
**Location**: `Reflect.cs`, `Diagnostics.cs`

### The kit (`Isochronous.TestKit`)
**Id:** blk-c6e1619a47bb  
**Description**: what a test author uses.  
**Location**: `Kit/`

| Type | File | Purpose |
|------|------|---------|
| `OniTestAttribute`, `LayerAttribute` | `Attributes.cs` | mark tests and set their options (`Timeout`, `FreshSave`, `Explicit`, `Boxed`, `NoColony`, `Worldgen`) |
| `OniAssertException`, `SkipException` | `Attributes.cs` | thrown by `t.Assert` and `t.Skip` |
| `TestContext` | `TestContext.cs` | `Assert`, `World`, `Tools`, `Dupes`, waits (`Frames`, `WaitGameSeconds`, `WaitRealSeconds`, `WaitUntil`), `Rect`, `Area`, `Log`, `Cleanup`, `AllowLogError`, `Skip`, `Speed`, `InstantBuild`, `ResearchAll`, `Screenshot`, `Press`, `PointAt`, `LookAt`, `Generated` |
| `Wait` and its subclasses, `CellRect` | `TestContext.cs` | what a test yields; a rectangle of cells |
| `OniAssert` | `OniAssert.cs` | `True`, `False`, `Eq`, `NotEq`, `Null`, `NotNull`, `Has`, `Lacks`, `Fail` |
| `World`, `Cells` | `World.cs` | `Build`, `At`, `Destroy`, `LogicNetwork`, `LogicConnections` at layer 0; cell descriptions |
| `Tools`, `UtilityDragSession` | `Tools.cs` | the real utility build tool at layer 1: `Path`, `Sweep`, `PreviewTint`, `PreviewValid` |
| `Dupes` | `Dupes.cs` | `Box`, `ClearBox`, `Spawn`, `Grant`, `GrantAll`, `Drop`, `Remove`, `RemoveAll` |
| `SaveFile` | `SaveFile.cs` | `Capture` and `Restore` a building through the game's own save and load |

### Invoke-OniTests.ps1
**Id:** blk-5b0c52216ccc  
**Description**: the launcher. For each mod in `-Mods`, finds `src/<Mod>.Tests/<Mod>.Tests.csproj` (in a repo or under `mods/`), builds it, checks the test DLL with `tools/ModCheck`, copies it into the run folder, rewrites `mods.json` so only TestRunner, the mods under test, `-With` and `-Steam` are enabled, launches the game through Steam (or hands the run to a live game), waits for the status, prints a line per test and exits with its code.  
**Location**: `Invoke-OniTests.ps1`  
**Options**: `-Mods` (required), `-Filter`, `-Fixture`, `-With`, `-Steam`, `-TimeoutSec`, `-TestTimeoutSec`, `-KeepGame`, `-BuildOnly`, `-NoBuild`, `-NoCheck`, `-BuildArgs`, `-Label`, `-Trace`, `-OutDir`, `-SteamExe`, `-AppId`.

### Watch-OniGame.ps1
**Id:** blk-5f4f98f50aec  
**Description**: the warden of a live session, started detached next to the game. Waits for the game to exit, restores `mods.json` from the backup, removes the live-session folder; ends a game that stops answering.  
**Location**: `Watch-OniGame.ps1`

### Files
**Id:** blk-aabf5834e2e4

| File | Direction | Purpose |
|------|-----------|---------|
| `DIR/tests/*.dll` | read | the test assemblies |
| `DIR/results.jsonl`, `results.jsonl.status` | written by the game | per-test results; the session status |
| `DIR/artifacts/*.png` | written by the game | `t.Screenshot` output |
| fixture save | read | copied to the run folder as `fixture.sav`; never written back |
| `Documents\Klei\OxygenNotIncluded\mods\mods.json` | rewritten, restored | the run's loadout; backup `mods.json.onitest-backup` |
| `%LOCALAPPDATA%\IsochronousOniTests\live` | read, written | live-session queue, state and pid |

## Business Rules
**Id:** blk-ad9c4a3b39af

- Without `--onitest-dir` and `--onitest-save` the mod changes nothing.
- No save goes to disk outside the run folder, and the game's autosave is skipped.
- A test is `public static IEnumerator M(TestContext t)` with `[OniTest]`; any other signature with the attribute fails the session at discovery.
- Tests run in order of assembly name, class, declaration. An `Explicit` test runs only when `-Filter` selects it.
- Any `Error`, `Exception` or `Assert` the game logs during a test fails it unless `t.AllowLogError` allows it.
- Each mod's tests start on a fresh load of the fixture; `[OniTest(FreshSave = true)]` loads it again before a test, after a settle of five game seconds.
- A run is handed to a live game only when the loadout matches: the mode, the mods, `-With`, `-Steam`, `-Trace` and every file of each deployed local mod, the harness and their shared configs. Anything else ends the game and launches a new one.
- The game stays up 15 s after a run for another; one run at a time per machine.
- A world coordinate of the other game mode skips its tests.
- A run needs Steam running and the game not running (apart from its own live session).

## Key Code Paths
**Id:** blk-8a2f716a53ff

- **Launch**: `Invoke-OniTests.ps1` builds, runs ModCheck, writes `mods.json`, starts or hands over to the game. See `Invoke-OniTests.ps1`.
- **Start**: `MainMenu.OnSpawn` postfix, then `App.LateUpdate` postfix calls `Session.Start`: `Discover`, then `Continue` loads the fixture (`LoadScreen.DoLoad`) or asks for a world. See `Patches.cs`, `Session.cs`.
- **Run**: `Game.OnSpawn` postfix adds `TestBehaviour`; `RunAll` picks the area, clears it and steps each test with `RunOne`. See `TestBehaviour.cs`.
- **Generated world**: `Worldgen.Begin` at the main menu, `Worldgen.Embark` after the colony spawns. See `Worldgen.cs`.
- **End**: `Session.Finish` writes the status and quits or goes idle; the launcher reads `results.jsonl` and the status and exits. See `Session.cs`, `Invoke-OniTests.ps1`.
- **Live session**: `Session.PollQueue` takes `<run>.req`, `Session.Begin` resets and starts again. See `Session.cs`.

## State Transitions
**Id:** blk-27f068ae61ee

| State | Transitions To | Trigger |
|-------|---------------|---------|
| test pending | pass, fail, error, timeout, skipped | the driver steps it (see [error handling](../standards/error-handling.md)) |
| session busy | idle | `Session.Finish` in a live session |
| session idle | busy | a request in `queue` |
| session idle | quitting | `quit.req`, or 15 s with no request |

## Related
**Id:** blk-ac2e0e4164d8

- See also: [glossary](glossary.md)
- Governed by: [naming conventions](../standards/naming-conventions.md)
