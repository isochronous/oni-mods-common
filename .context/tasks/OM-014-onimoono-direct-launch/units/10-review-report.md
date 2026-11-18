# Unit 10 review: N1 fix (38fdbf1)

Verdict: N1 fixed. One new finding (medium) from leaving the stock game running; one low note. No code changed, nothing launched; the only process query was a read-only CIM listing (no OniMoono/OxygenNotIncluded was running at that moment).

## N1: fixed
`Stop-MoonoLaunch` (tools/TestRunner/GameProcess.ps1:185-212) no longer selects by start time. Victims are the launcher id (:189), OniMoono processes whose command line holds the launch's `--log` path (:190-192, case-insensitive substring), and OniMoono processes whose `ParentProcessId` is already in the set, repeated to a fixpoint (:193-198). A player's hand-started Moono has neither the log path nor a parent in the set. Stock `OxygenNotIncluded` is only named (:207-209), never stopped.
Callers pass the same string that built the argument line: Invoke-OniTests.ps1:611 (`$moonoLog`, also used at 573/579) and Test-OniMods.ps1:344 (`$launch.Log`, the `Log` that built `$moonoArgs`, :252-254). Both parse with 0 errors. READMEs match the code.

## Can it still miss what the launch started
- Game: carries `--log` (Moono's own option, first on the line, 06-explore-report) and/or descends from the launcher. Covered.
- Crash watcher: its line is `"<exe>" --crash-watch <hex ids> ...` (02-implement-report) and holds no `--log`, so it is caught only by descent from the game or launcher. The game spawns it, so it descends. Parent ids are read from the CIM snapshot field, so the chain survives a launcher that has already exited (the dead id stays in `$ids`). Miss only if the watcher is spawned by a process that is neither the launcher nor a descendant, e.g. handed off through Steam or the Restarter; not observable offline. Residual and unverified, same as before the fix. The offline test used cmd.exe stand-ins, so it did not exercise a real watcher line.
- Launcher already exited: `$Launcher.HasExited` is true, so it is not stopped (no reuse hazard on it); its id still seeds the descent.

## Can it hit anything else
- Pid reuse: a dead launcher's id being reused as the parent of an unrelated OniMoono, or an id reused between the CIM snapshot and `Stop-Process`, needs a new process to take the id and spawn an OniMoono within the run. Negligible. Non-launcher ids are re-fetched with `Get-Process -Id` just before the kill. No fix needed.
- Log path match is a substring; the paths are unique per run (runDir `moono.log`, timestamped temp file), so no collision with a player's game.
- Launcher null (Start-Process threw): `$ids` holds only log-path matches, which is correct.

## New finding F1 (medium): a stock game left running skips the config and prefs restore
Invoke-OniTests.ps1:635-638 only runs `Restore-LeftoverConfig` and `Write-PrefsRestore (Restore-GameMode ...) (Restore-SaveKey ...)` when `-not (Get-GameProcess -Any)`. `-Any` counts every stock `OxygenNotIncluded`. The main way a Moono launch starts a stock game is Steam relaunching it (Wait-MoonoGame throws for exactly that), and that game is ours. Before this commit it was stopped; now it is left alive, so the config and the kplayerprefs game-mode/save-key stay in test state after the leg. A stock game still running with the test loadout may also write mods.json when it closes, over the restored copy (mod-loadout recovery history). The console message says "Close it before playing", but nothing restores prefs afterwards, and `-Runtime Both` goes on: the next leg's open-game pre-check will refuse because that game is open. ModTest has the same exposure for its mods.json restore (no prefs restore there).
Expected fix, smallest: tell a stock game of ours from the player's by evidence, and stop it only then. For example, stop a stock process that is not in `$StockBefore` and whose parent chain contains the launcher or an id in `$ids`, or whose command line holds `--onitest-`; keep the "named, left running" path for everything else. If it is left running, say in the message that prefs and config were not restored and how, and have the leg's summary record it.

## Low note L1
The new Write-Host in the stock case is console-only; the leg's FAILED summary does not carry it. If F1 is not fixed, put the same sentence in the failure text so the summary file shows it.

## Passes run
1. `git show 38fdbf1`, every hunk against the unit 09 N1 description.
2. Victim selection: launcher, log-path match, parent fixpoint, stock handling.
3. Callers' log-path string against the one that built the argument line (both scripts).
4. Watcher lineage from units 01, 02, 06, 08 (its command line has no `--log`).
5. Pid reuse and exited-launcher paths.
6. Restore path after a stock game is left alive (Invoke 610-640): found F1.
7. Parse check of the three scripts (0 errors); README text against code.

## ICON tooling friction
None blocking. The packet's pointer ids were not opened with `ks read`: the task needed only the code and the earlier unit reports, and none of the listed records (WorkshopUpload/ModCheck overviews, TestRunner standards) bore on a process-selection review. The pointers matched by path only (`*.ps1` globs); nothing governing this work was missing. A PowerShell one-liner passed through Bash was mangled by `$` expansion, so I wrote it to a scratchpad .ps1 and ran that.
