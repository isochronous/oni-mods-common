---
doc_id: doc-bf18c260f56c
description: "WorldgenDriver (the unattended worldgen mod and its PowerShell launcher): the result statuses, how patches and the result writer handle errors, logging, and how the launcher recovers and restores backups"
applies_to:
  - "*.cs"
  - "*.ps1"
  - "*.csproj"
  - "*.yaml"
---
# WorldgenDriver error handling

WorldgenDriver runs unattended: nobody watches the game, so every outcome has to become a result line or a log line. The mod turns each way a generation can end into a status; the launcher turns each way a session can end into a restart or a stop.

## Principles
**Id:** blk-6b5e188da28e

1. **Every coordinate gets exactly one result line.** `Driver.Finish` writes it once; the `Finished` and `quitting` flags make a second call do nothing.
2. **Name the failure.** The status says which way it ended (`failed`, `hang`, `crash`), `stage` and `percent` say where, `detail` and `errors` say why.
3. **A session that cannot trust the game stops.** Hangs, crashes and driver errors quit the game; the launcher starts a new one.
4. **Never leave the player's setup changed.** The launcher backs up `mods.json` and `kplayerprefs.yaml` and restores them in `finally`.

## Result statuses
**Id:** blk-0c3aa2a70e1c

`Driver.Finish(status, detail)` is the only writer. `canContinue` is true for the first four; any other status quits the game.

| Status | Meaning | Raised in |
|--------|---------|-----------|
| `complete` | generation finished; the game would load the colony | `App.LoadScene` prefix (scene `backend`) |
| `loaded` | with `--worldgen-load=1`, the colony loaded | `Game.OnSpawn` postfix |
| `failed` | the game's own failure path (`OfflineWorldGen.DoExitFlow`) ran | `DoExitFlow` prefix, after `Sim.Shutdown()` |
| `bad-args` | the coordinate cannot be parsed, or no cluster has its prefix in this mode | `MainMenu.OnSpawn` postfix |
| `wrong-mode` | the game is in base-game mode when Spaced Out was wanted, or the reverse | `MainMenu.OnSpawn` postfix |
| `driver-error` | an exception in the driver's own code; `detail` carries it | `MainMenu.OnSpawn` postfix `catch` |
| `crash` | `KCrashReporter.hasCrash` | `App.LateUpdate` watchdog |
| `hang` | no progress for `TimeoutSeconds` after the screen opened | `App.LateUpdate` watchdog |
| `stuck-before-start` | no progress for `TimeoutSeconds` before the screen opened (for example a mod restart dialog on the main menu) | `App.LateUpdate` watchdog |

## Handling Patterns
**Id:** blk-01c055e6d4f7

### Patches
**Id:** blk-08d7268a068c

`MainMenu_OnSpawn_Patch` wraps its work in `try`/`catch (Exception e)`: it logs `Debug.LogWarning(Driver.Tag + e)` and finishes with `driver-error`. Other patches are guarded by `Driver.Active` and do not throw. The two prefixes that replace game behaviour (`DoExitFlow`, `App.LoadScene`) return `false` only while the driver is active and unfinished.

### Worldgen errors
**Id:** blk-066c3d0efe40

`OfflineWorldGen.OnError` appends the error text (description plus exception) to `Driver.Errors` under a lock and logs a warning. The list is written into the result's `errors` array whatever the final status.

### Result file
**Id:** blk-5f1243febda9

`WriteResult` creates the folder and appends one UTF-8 JSON line. A failure to write is caught, logged with `Debug.LogWarning` and does not stop the session.

### Logging
**Id:** blk-3a01efe65d43

- Every message starts with `Driver.Tag`, `[WorldgenDriver] `, through `Debug.Log` or `Debug.LogWarning`.
- The launcher copies `Player.log` next to the results after each session, so a failed or hung coordinate can be traced.

### Launcher
**Id:** blk-64bfa31b7823

- `$ErrorActionPreference = 'Stop'`; preconditions `throw` with a message that says what to do: no coordinates, the game already running, Steam not found, the driver not deployed (names the `dotnet build` command), a local mod folder that does not exist.
- A result line the game was still writing is skipped with `Write-Warning`; only complete lines count.
- A session with no result change for `TimeoutSec + 240` seconds has its game process killed and is reported `killed`; a game that never starts within 120 s is `no-process`.
- Two sessions in a row without a single result abandon the rest of that group with a warning naming the coordinates.
- A leftover `*.worldgen-backup` means an earlier run died before restoring: the script puts the backup back first rather than backing up the stripped file.
- Final output: results grouped by status and a table of every status other than `complete` and `loaded`.

## Anti-Patterns to Avoid
**Id:** blk-3ff6be6bc8ec

- Writing a result line from more than one place for one coordinate. Go through `Finish`.
- Catching an exception in a patch and doing nothing: finish with `driver-error` so the launcher sees it.
- Leaving a backup in place: a restored `mods.json` is the player's loadout.

## Related
**Id:** blk-667e1681162b

- See also: [code style](code-style.md), [entities](../domains/entities.md)
