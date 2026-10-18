---
doc_id: doc-8cec0a95808d
description: WorldgenDriver's mod class in Mod.cs, the Driver state and --worldgen options in Driver.cs, the seven patches in Patches.cs, the Invoke-Worldgen.ps1 functions, files read and written, business rules, code paths and per-coordinate states
applies_to:
  - "*.cs"
  - "*.ps1"
  - "*.csproj"
  - "*.yaml"
---

# WorldgenDriver domain

## Overview
**Id:** blk-b6ed29aae2e0

WorldgenDriver generates clusters in the real game with nobody at the keyboard. It has two halves. The development-only mod (`Mod.cs`, `Patches.cs`, `Driver.cs`) reads `--worldgen*` options from the game's command line and, at the main menu, applies each coordinate and opens the worldgen screen; it watches progress, writes one JSON line per coordinate and returns to the menu or quits. The launcher (`Invoke-Worldgen.ps1`) prepares the game's configuration, starts the game through Steam, collects results and restarts the game after a hang or crash. Without a `--worldgen` argument the mod does nothing, so it can stay installed.

It is used to reproduce worldgen bug reports and to check a mod build against a list of worlds and seeds with the game's real worldgen and sim. Run it with `./Invoke-Worldgen.ps1 <coordinate> -Mods <mod>`; the options are in `README.md`.

## Key Entities
**Id:** blk-2b3516654971

### WorldgenDriverMod
**Id:** blk-1e7cd48f4614  
**Description**: The `UserMod2` entry point. `OnLoad` reads the command line and applies the Harmony patches only when a coordinate was requested; otherwise it logs `no --worldgen argument; idle`.  
**Location**: `tools/WorldgenDriver/Mod.cs`

**Relationships**: calls `Driver.ReadCommandLine`, then `base.OnLoad(harmony)` which patches every class in `Patches.cs`.

### Driver
**Id:** blk-cb417a1e3041  
**Description**: Static session state and the result writer: the coordinate list and the current index, the timeout, the wanted mode, the story and mixing overrides, per-coordinate state (cluster id, seed, applied coordinate, stage, percent, errors) and the `Started`/`Finished` flags.  
**Location**: `tools/WorldgenDriver/Driver.cs`

| Command-line option | Description |
|---------------------|-------------|
| `--worldgen=COORD[;COORD...]` | coordinates for this session, in order |
| `--worldgen-list=FILE` | the same, one per line; blank lines and `#` lines are ignored |
| `--worldgen-out=FILE` | JSON Lines result file (default `<Klei folder>\worldgen-driver\results.jsonl`) |
| `--worldgen-timeout=SECONDS` | no progress for this long is a hang (default 300) |
| `--worldgen-expansion1=0\|1` | mode this session needs (default: Spaced Out when the first coordinate is one) |
| `--worldgen-stories=all` | turn every story trait on after the coordinate is applied |
| `--worldgen-mixing=all\|guarantee` | turn every DLC mixing toggle on and set every world and subworld mixing to try (or guarantee) |
| `--worldgen-load=1` | also load each generated colony before moving on |

**Relationships**: `Patches.cs` calls `BeginCoordinate`, `Progress`, `Finish` and reads the settings; `Finish` calls `App.LoadScene("frontend")` for the next coordinate or `App.Quit()`.

### Patches (`Patches.cs`)
**Id:** blk-84d2affac2b8

| Patch | Hook | Role |
|-------|------|------|
| `MainMenu_OnSpawn_Patch` | postfix `MainMenu.OnSpawn` | checks the mode, applies the coordinate through `CustomGameSettings` (cluster by coordinate prefix, seed, settings, story and mixing codes), opens `ScreenPrefabs.Instance.WorldGenScreen` |
| `OfflineWorldGen_UpdateProgress_Patch` | postfix `OfflineWorldGen.UpdateProgress` | records stage and percent (runs on the worldgen thread) |
| `App_LateUpdate_Patch` | postfix `App.LateUpdate` | watchdog in every scene: crash reporter, and no progress past the timeout |
| `OfflineWorldGen_OnError_Patch` | postfix `OfflineWorldGen.OnError` | collects error text for the result |
| `OfflineWorldGen_DoExitFlow_Patch` | prefix `OfflineWorldGen.DoExitFlow` | the failure path becomes a `failed` result; the driver shuts the sim down itself |
| `App_LoadScene_Patch` | prefix `App.LoadScene` | scene `backend` means generation complete: `complete`, unless `--worldgen-load` lets the colony load |
| `Game_OnSpawn_Patch` | postfix `Game.OnSpawn` | with `--worldgen-load`, a loaded colony is `loaded` |

### Invoke-Worldgen.ps1
**Id:** blk-2c4e50d4b9fc  
**Description**: The launcher. Groups coordinates by mode, runs them in sessions of up to `-PerSession` (default 25), one game start per session, and prints a summary.  
**Location**: `tools/WorldgenDriver/Invoke-Worldgen.ps1`

| Function | Description |
|----------|-------------|
| `Set-Mode` | sets `EXPANSION1_ID.ENABLED` in `kplayerprefs.yaml` to 1 (Spaced Out) or 0 |
| `Set-Mods` | rewrites `mods.json` so only the driver and the mods in `-Mods` (local folder names or Workshop ids) are enabled for the right DLC, local mods reset to installed |
| `Read-Results` | parses the result file, skipping incomplete or unreadable lines |

**Relationships**: starts the game with `steam.exe -applaunch 457140` and the `--worldgen-*` arguments, polls the result file, copies `Player.log` after each session.

## Files read and written
**Id:** blk-2d4957e55f9e

| File | Direction | Description |
|------|-----------|-------------|
| `<Klei folder>\mods\mods.json` | launcher rewrites | backed up to `.worldgen-backup`, restored at the end |
| `<Klei folder>\kplayerprefs.yaml` | launcher rewrites | same |
| `<Klei folder>\worldgen-driver\list.txt` | launcher writes, mod reads | the coordinates of one session |
| `<Klei folder>\worldgen-driver\results.jsonl` | mod appends, launcher reads | one JSON line per coordinate; deleted before each session |
| `runs\<Label>-<stamp>.jsonl`, `...-session<N>.log` | launcher writes | the collected results and each session's `Player.log` |

`<Klei folder>` is `Documents\Klei\OxygenNotIncluded`.

The README mentions `runs/coverage-*.txt` coordinate lists. `runs/` is gitignored, so a checkout has none. TODO(owner): decide whether to commit the coverage lists.

## Business Rules
**Id:** blk-016ad7dc25b9

- The mod is idle without `--worldgen`; it must stay safe to leave installed.
- A coordinate is applied the way the destination screen's coordinate box does, and the result's `applied` field carries the coordinate the game actually used (it differs when `-Stories` or `-Mixing` override it).
- The mode is decided by the coordinate: `C` as the fifth part from the end, as in `M-FLIP-C-100-0-0-0`, is Spaced Out. `-Mode` overrides it. A game in the wrong mode ends the coordinate as `wrong-mode`.
- Mods other than the driver and `-Mods` are disabled during a run and restored afterwards, even on failure.
- A hang or crash ends the session; the launcher starts another for what is left, and gives up on a mode group after two sessions in a row with no result.

## Key Code Paths
**Id:** blk-8d4b33a1d147

- **One coordinate**: `MainMenu_OnSpawn_Patch` -> `Driver.BeginCoordinate` -> apply settings -> open the worldgen screen -> progress reports -> `App_LoadScene_Patch` (`complete`) or `DoExitFlow` (`failed`) -> `Driver.Finish` -> `WriteResult` -> `App.LoadScene("frontend")` or `App.Quit()` -- `Patches.cs`, `Driver.cs`.
- **One session**: `Set-Mode`, `Set-Mods`, write `list.txt`, `Start-Process steam.exe -applaunch`, poll `Read-Results` until the game exits -- `Invoke-Worldgen.ps1`.

## State Transitions
**Id:** blk-98bc32d0b35e

Per coordinate, in the mod:

| State | Transitions To | Trigger |
|-------|---------------|---------|
| menu (`Started` false) | started | main menu spawns and `BeginCoordinate` runs |
| started | finished | `Finish(status)` on completion, failure or watchdog |
| finished | menu | status can continue and coordinates remain: `App.LoadScene("frontend")` |
| finished | game quits | list done, or status is `wrong-mode`, `driver-error`, `crash`, `hang` or `stuck-before-start` |

## Related
**Id:** blk-6c989c0a9f46

- See also: [glossary](glossary.md)
- Governed by: [naming conventions](../standards/naming-conventions.md)
