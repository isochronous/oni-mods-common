# Unit 12 review: stray stock game handed to the warden (039e519)

Verdict: unit 10's F1 is fixed per Jeremy's decision; nothing outside it was added. No new finding above low. Nothing launched, no process touched, no code changed. The four scripts parse with 0 errors.

## F1: fixed
- Selection (tools/TestRunner/GameProcess.ps1:189-206). A stock `OxygenNotIncluded` is a candidate only if it is not in `$StockBefore` (:191). It is stopped only if it descends from the launcher or an already selected pid (fixpoint :201-206), or its command line holds the launch's `--log` path or the run's `--onitest-dir=<runDir>` (:196-199). A game the player opened before the launch is never a candidate. A game opened during the launch that matches none of these is not stopped.
- Return value (:215-219). The pids of the stock games that are neither in `$StockBefore` nor in `$ids` come back as `$left`. The only other statements in the function that could emit output are `[void]`-guarded or `Write-Host`, so the caller gets a clean int array.
- Invoke-OniTests.ps1:608-630. The stray pids come back (:614), the entry warden is stopped and `$wardenStarted` cleared (:616-621), then a `Watch-OniGame.ps1 -GamePid <pid>` warden is started with `-ModsJson -Backup -LiveDir -ConfigRoot -Prefs` (:622-627), `$wardenStarted` is set (:627) and the inline restore (:630) is skipped. The leg failure text and the console line say what was left and who restores.
- Watch-OniGame.ps1 `-GamePid` (:51-53, :79-90): it takes the process by id; if it has already exited the loop is skipped and the restore runs at once. Otherwise it waits, then does mods.json, config backups, mode and save pointer once, then removes the live dir. The idle/busy kills (:80-87) read `$LiveDir\state`, which only a game started with `--onitest-` arguments writes (Session.cs:269). A stray stock game has none, so the warden never ends it.

## Every restore happens exactly once
- Moono, game found: unchanged (`$stray` is empty, the entry warden follows the game).
- Moono, no game, nothing stray: entry warden stopped, inline restore of mods.json, config and prefs (:631-646), unchanged.
- Moono, no game, stray: entry warden stopped, one new warden restores everything once the stray exits. The inline path is skipped by `$wardenStarted`. There is no window with two wardens: the entry warden is stopped before the new one starts.
- Stock leg: `$stray` stays empty and the new block is skipped; code unchanged.
- ModTest (Test-OniMods.ps1:344-352): waits for each stray pid, then falls to the existing else branch (:353-), which restores mods.json once. If the pid is already gone, `Wait-Process` returns at once.
- `-Runtime Both` runs Stock then Moono, so the Moono leg is last and a stray never blocks a following leg.

## Findings
- L1 (low), tools/ModTest/Test-OniMods.ps1:349: `Wait-Process -Id $id` has no timeout. The wait is announced on the console with the by-hand restore command, so it is not silent, but an unattended ModTest run (nobody reading the console) blocks until the game is closed. Expected fix, only if wanted: write the same line to a log or return a distinct exit status; a timeout would restore mods.json under a running game, which Jeremy's decision rules out. No change is required.
- L2 (low), Invoke-OniTests.ps1:624: the warden follows `$stray[0]` only. If two strays appear (Steam relaunch plus one more), the restore runs when the first exits while the second still runs. The failure text lists all pids. Two stock games at once is not a case the launch produces; no fix needed.
- L3 (note): if `Get-MoonoArgumentLine` throws before `$stockBefore` is assigned (:573-575), `$StockBefore` is empty, so any stock game then running counts as "appeared". It is returned as a stray and waited on, never stopped (not descended, no marker), so no player game is killed; the warden just defers the restore. The open-game pre-check already refuses a run with a game open. No fix needed.

## Passes run
1. `git show 039e519`, every hunk against Jeremy's decision.
2. Victim selection and the returned pid list in Stop-MoonoLaunch (stock before/after, descent, log and run-folder markers, player's game safe).
3. Warden handoff in Invoke-OniTests (entry warden stop, new `-GamePid` warden, skipped inline restore, failure text, summary line).
4. Watch-OniGame `-GamePid` path, including exited-already and the state-file kills.
5. ModTest wait and restore, including Ctrl+C and the exited-already case.
6. Restore-once matrix over Moono found / not found / stray, Stock, Both.
7. Parse check of the four scripts (0 errors); README wording checked against the code.

## ICON tooling friction
None blocking. The packet's pointers matched by `*.ps1` path globs and by overview "names" lines; I did not open them with `ks read` because the work needed only the commit, the scripts and units 10 and 11. Nothing governing this review was missing from the brief.
