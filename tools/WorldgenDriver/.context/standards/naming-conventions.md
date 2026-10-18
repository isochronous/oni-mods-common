---
tags: code-style
doc_id: doc-ed918e244212
description: "WorldgenDriver's naming rules: file, run output and Klei-folder names, class, patch class and member naming, PowerShell parameters, functions and variables, --worldgen options, result statuses, result JSON fields and coordinate format"
applies_to:
  - "*.cs"
  - "*.ps1"
  - "*.csproj"
  - "*.yaml"
---

# WorldgenDriver naming conventions

## Files and Directories
**Id:** blk-28ae24c5737d  
**Tags:** run-result

- Folder, project, assembly and namespace are `WorldgenDriver`; the mod's `staticID` is `Isochronous.WorldgenDriver`.
- C# files by role: `Mod.cs` (the `UserMod2` entry point), `Patches.cs` (every Harmony patch), `Driver.cs` (state, command line, result writing).
- The launcher is `Invoke-Worldgen.ps1`, a PowerShell Verb-Noun name.
- Run output goes under `tools/WorldgenDriver/runs/` (git-ignored): `<Label>-<stamp>.jsonl` for results and `<Label>-<stamp>-session<N>.log` for each session's `Player.log`; `<stamp>` is `yyyyMMdd-HHmmss`.
- In the Klei folder (`Documents\Klei\OxygenNotIncluded`): `worldgen-driver\results.jsonl` and `worldgen-driver\list.txt`; backups are the original name plus `.worldgen-backup`.

## Code Naming
**Id:** blk-7321225beeb8

### Classes
**Id:** blk-eec69329743c  
**Tags:** harmony-patching, mod-entry-point

- PascalCase. The mod class is `WorldgenDriverMod`; the state class is `Driver`.
- A Harmony patch class is `<GameType>_<Method>_Patch`: `MainMenu_OnSpawn_Patch`, `OfflineWorldGen_UpdateProgress_Patch`, `App_LateUpdate_Patch`, `OfflineWorldGen_OnError_Patch`, `OfflineWorldGen_DoExitFlow_Patch`, `App_LoadScene_Patch`, `Game_OnSpawn_Patch`. Each is `public static` with a `Prefix` or `Postfix` method.

### Members
**Id:** blk-a4fb60b0a844  
**Tags:** logging

- `Driver` exposes PascalCase static properties and fields (`Active`, `Coordinate`, `TimeoutSeconds`, `WantExpansion1`, `Applied`); private state is camelCase (`expansionGiven`, `quitting`).
- Methods are verbs: `ReadCommandLine`, `BeginCoordinate`, `Finish`, `Progress`, `WriteResult`.
- Log prefix constant: `Driver.Tag`.

### PowerShell
**Id:** blk-b2d0f975cc8f  
**Tags:** game-launch

- Parameters PascalCase (`-Coordinate`, `-ListFile`, `-Mods`, `-PerSession`, `-TimeoutSec`, `-Label`), switches for flags (`-Load`, `-NoGraphics`), `ValidateSet` for enumerations (`-Mode auto|base|spacedout`, `-Stories`, `-Mixing`).
- Functions Verb-Noun: `Set-Mode`, `Set-Mods`, `Read-Results`.
- Script variables camelCase (`$modsJson`, `$resultFile`, `$listPath`, `$sessionLog`).

## Project-Specific Conventions
**Id:** blk-12a3adabf260

### Command-line options
**Id:** blk-aa65fc8f3058  
**Tags:** command-line

- `--worldgen` and `--worldgen-<name>` in kebab-case, each with `=value`: `--worldgen`, `--worldgen-list`, `--worldgen-out`, `--worldgen-timeout`, `--worldgen-expansion1`, `--worldgen-stories`, `--worldgen-mixing`, `--worldgen-load`. Booleans take `1` or `true`.

### Result statuses
**Id:** blk-f9b58681c447  
**Tags:** run-result

- Lower-case, hyphenated words: `complete`, `loaded`, `failed`, `bad-args`, `wrong-mode`, `driver-error`, `crash`, `hang`, `stuck-before-start`. The launcher groups and filters by these strings, so a new status is added in `Driver.Finish` and in the launcher's summary together.

### Result JSON
**Id:** blk-cecebc5b1133  
**Tags:** run-result

- One line per coordinate, field names camelCase: `status`, `coordinate`, `index`, `cluster`, `seed`, `applied`, `expansion1`, `stage`, `percent`, `secondsSinceProgress`, `secondsSinceStart`, `detail`, `build`, `time`, `errors`.
- The launcher adds `session` and `log` to its in-memory copies.

### Coordinates
**Id:** blk-c03f0112aec4  
**Tags:** world-coordinate

- A coordinate is the game's settings coordinate, upper-case parts joined by `-`, such as `CERS-A-100-0-0-0` (base game) or `M-FLIP-C-100-0-0-0` (Spaced Out).

## Related
**Id:** blk-a26aafb78ff9

- See also: [code style](code-style.md), [glossary](../domains/glossary.md)
