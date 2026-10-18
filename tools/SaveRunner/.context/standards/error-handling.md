---
doc_id: doc-f968aa1e0e91
description: "How SaveRunner ends and reports a run: the four status values, the driver try/catch, optional hooks, crash dialog, launcher throws, mods.json restore and logging"
applies_to:
  - "*.cs"
  - "*.ps1"
  - "*.csproj"
  - "*.yaml"
---
# Error Handling Conventions

## Principles
**Id:** blk-225bad67d9a6

1. **A run always ends with a status.** Whatever goes wrong inside the game, `Runner.Finish` writes one JSON line to `<out>.status` and quits, so the launcher never waits on a game that has silently given up.
2. **Never write to the player's data.** The save is copied first and every `SaveLoader.Save` is blocked, so a failed run leaves the colony as it was.
3. **Leave a trace.** Anything that ends the process during a run logs its stack; every failure goes to `Player.log` under the `[SaveRunner] ` tag.
4. **Put the machine back.** The launcher restores `mods.json` in a `finally` block, whatever happened.

## Run Status
**Id:** blk-a1742b4823d2

`Runner.Finish(status, detail)` writes `{"status":..., "save":..., "detail":..., "realSeconds":...}` and calls `App.Quit()`. The first call wins (`Runner.Finished`).

| Status | Written by | Meaning |
|--------|-----------|---------|
| `done` | `RunBehaviour.Update` | the game seconds were reached; `detail` is the sample count |
| `timeout` | `App_LateUpdate_Patch` | no end after `--saverun-timeout` real seconds (default 900) |
| `no-save` | `App_LateUpdate_Patch` | the file named by `--saverun` does not exist |
| `runner-error` | `RunBehaviour.Update` | an exception in the per-frame driver; `detail` is the exception |

The launcher prints the status line, the number of probe rows and the run folder when the game has gone. A missing status file prints `(no status file: the run never finished)`.

## Handling Patterns
**Id:** blk-4db51d308caa

### Driver
**Id:** blk-dbdec23d78e3

`RunBehaviour.Update` runs its whole body in one `try`. The `catch` logs a warning, closes the CSV writer and finishes with `runner-error`. The broad catch is deliberate: an unattended run must end, with the reason recorded.

### Fire-and-forget file writes
**Id:** blk-8cfa00557d4c

Writing the status file is wrapped in its own try/catch that logs a warning (`Runner.Finish`); a failed write must not stop the quit.

### Optional hooks
**Id:** blk-c881d85c0855

`SaveRunnerMod.TracePatch` returns with a warning when the target method is missing and catches an exception from `harmony.Patch`. A game update that moves `App.Restart` costs a warning, not the mod.

### Crash dialog
**Id:** blk-07e31aa20eae

`OnLoad` sets `KCrashReporter.ignoreAll = true` and `terminateOnError = false` during a run. The game's crash dialog quits the process when the scene change destroys it, which ended runs without a word; exceptions still reach the log.

### Command line
**Id:** blk-862fa77a8e5f

`Runner.ReadCommandLine` ignores a number it cannot parse or that is not positive and keeps the default; `--saverun-speed` is clamped to 1 to 3. An unknown `--saverun-*` key is ignored.

### The launcher
**Id:** blk-aa64035a160f

`Invoke-SaveRun.ps1` sets `$ErrorActionPreference = 'Stop'` and checks its preconditions with `throw` and a message that names the fix: save not found, game already running, Steam not found, `SaveRunner is not deployed; run: dotnet build tools/SaveRunner -c Release`, local mod folder not found, game not started within 120 s.

- A leftover `mods.json.saverun-backup` means an earlier launcher died: it is copied back over `mods.json` first, with a warning, so the real loadout is never overwritten by a modified one.
- The game process is stopped (`Stop-Process -Force`) 60 s after the status file stops changing and `TimeoutSec + 180` s after launch, and the launcher says which (`killed-after-status`, `killed`).
- The launcher sets no exit code of its own; a `throw` ends it with PowerShell's failure exit code, and a finished run returns 0 whatever the status, so read the printed status line or `probe.csv.status`.

## Logging
**Id:** blk-f7c3dad5aad4

- All lines go through `Debug.Log` or `Debug.LogWarning` with the `Runner.Tag` prefix (`[SaveRunner] `).
- Warnings for problems the run survives; plain log lines for milestones (loading, colony loaded, running, finished, every quit call).
- Probe rows are copied to the log every tenth game second (`t % 10 == 0`) so the log shows the run progressing.

## Anti-Patterns to Avoid
**Id:** blk-1cec34c97303

- Calling `App.Quit()` directly instead of `Runner.Finish`: no status file is written.
- Loading the save from `MainMenu.OnSpawn`: the menu was torn down under a popup still laying out, which threw and opened the crash dialog. The load waits four seconds after the menu appears.
- Patching without the `Runner.Active` guard.
- Pointing the game at the original save; the launcher always copies it to the run folder.

## Related
**Id:** blk-bb6114284811

- See also: [code style](code-style.md)
