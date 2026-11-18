# Unit 07 review report: OM-014 `-Runtime Stock|Moono|Both`

Verdict: approve with fixes. No critical findings. 2 moderate, 5 minor. Read-only review of
`git diff origin/main...HEAD -- tools` (5 files) plus the evidence folders runs/om014-20261112-115436-moono, -115715-stock/-moono. Game not launched, no code changed.

## Passes run
1. mods.json restore on every path (Moono leg fails early, Wait throws, Get-MoonoExe throws, Start-Process fails, Both, ModTest legs).
2. Stock vs main (diff -w of Invoke-OniTests.ps1: arguments, launch, wait loop, Player.log copy, exit codes).
3. Live-game reuse across runtimes (loadout key, pre-checks, Stop-LiveSession between legs).
4. Quoting through `--game-arg` (ConvertTo-CommandLineArgument, Get-MoonoArgumentLine, single-string -ArgumentList).
5. Truth of failure/summary messages against the evidence summaries and moono.log.
6. README/header-comment vs code.
7. The known minor (exit code unknown).
8. Other tools dot-sourcing GameProcess.ps1 (SaveRunner, WorldgenDriver, Watch-OniGame): only additive functions, no name clash.

## Findings

### Moderate

M1. A live Moono game blocks the next run instead of being reused. Invoke-OniTests.ps1:229-231.
`$running = Get-GameProcess -Any` counts every OniMoono, including Moono's crash watcher (a second `OniMoono.exe --crash-watch`, seen in 02-implement-report and 01-explore-report). The reuse test is `@($running).Count -eq 1 -and Id -eq $live.Id`, so a live Moono game (game + watcher = 2) throws "The game is already running; close it first." during the 15 s keep-alive window. The "runtime is part of the loadout, a live game is reused only by the same runtime" feature therefore cannot work for Moono, and a Both run started within 15 s of a Moono run fails the same way. Not exercised by the evidence (each evidence run started with no game). Expected fix: filter the watcher out in one place, GameProcess.ps1 (move ModTest's `Get-AnyGame` there, or make `Get-GameProcess -Any` skip command lines holding `--crash-watch`), and use it at 212, 217, 229 and in ModTest's Test-OniRunning. Add a Moono-twice-within-15 s check (reuse, then a different loadout restart).

M2. A Moono leg that fails in Wait-MoonoGame leaves the OniMoono it started running. Invoke-OniTests.ps1:574-576 with the finally at 595-617; GameProcess.ps1:~140-160 (stock-appeared throw) and the launcher-exit throw.
On the "stock game started" throw, `$proc` is null, so the finally kills the warden and restores mods.json, but `$launcher` (and any game it started) is still alive, now holding the test loadout in memory with nothing to restore the kplayerprefs mode switch (the restore at 614 is skipped because a game process exists). The stock game that triggered the throw is also left booting under a mods.json that was just swapped back. Same in ModTest (`$game` is null, `$launcher` stays alive). Expected fix: in the failure path stop `$launcher` (and the OniMoono children carrying `--onitest-`) before the restore, or, if a game of ours is up, hand the restore to the warden instead of killing it.

### Minor

m1. Known minor: "OniMoono exit code unknown". Test-OniMods.ps1 (the `Get-ExitCodeOf $game` line in the follow loop); Invoke-OniTests.ps1:~655-657 has the same reading. It reads the followed game process; that Process object came from Get-Process and its ExitCode is not readable after exit. Expected fix, in order: (a) take the code from Moono's own log, the last `[engine] Exiting after N frames with code C` line (authoritative, and it is already the file the tools point at), (b) fall back to `$launcher.ExitCode` (touch `$launcher.Handle` right after `Start-Process -PassThru` so PowerShell caches it), (c) only then "unknown". Put (a) in a `Get-MoonoLogExitCode` in GameProcess.ps1 and use it in both tools. Also the TestRunner summary prints no exit line at all on a normal pass because the game is still alive at report time (summary says "Game live after N s"); that is consistent, but README says the exit code is "reported" only for failures, which matches.

m2. A Moono failure in `-Runtime Both` aborts instead of recording the leg. Invoke-OniTests.ps1:574-580. The throw skips the report, so the failed leg has no summary.txt (see runs/om014-20261112-115026-moono), no side-by-side table, and the exit code is 1, not 2 as for "session never produced a status", even though README says "exit code is the worse of the two". Expected fix: catch the Wait-MoonoGame failure per leg, write its message into that leg's summary, set `$legExit = 2`, and carry on to the table.

m3. Stale Player.log on the live-reuse path. Invoke-OniTests.ps1:510. `if (Test-Path $playerLog) { Copy-Item ...; $logCopied = $true }` copies unconditionally. A Moono leg that reuses a live Moono game (once M1 is fixed) has no Player.log of its own, so an old stock Player.log is copied, `$ran.Runtime` becomes Stock, and the summary prints a false "WARNING: the run asked for Moono but the game ran on Stock." (this is exactly what runs/om014-20261112-114511-moono showed on the earlier code). Expected fix: on a Moono leg use `Copy-FreshLog ... $t0` or skip Player.log; leave the Stock reuse path as it is (main behaviour).

m4. README/summary text mismatch. TestRunner/README.md:113-114 says the summary says `no Player.log written by this launch`; the code (Invoke-OniTests.ps1:638-639) prints `Runtime: <leg> asked; the game log shows no game log written by this launch`, which reads badly. Expected fix: print `Runtime: <leg> asked; no game log written by this launch` and align the README to the same words.

m5. Stale cross-references. (a) GameProcess.ps1:5 names `Start-MoonoGame below`; no such function (the pieces are Get-MoonoArgumentLine, Wait-MoonoGame). (b) TestRunner/README.md:121 (options table) says see "Stock or Moono" "below", but that section is above the table; line 44 is right. (c) README.md:30 says "read `Player.log`" for exit code 2; on a Moono leg the file to read is `moono.log` (the code message already says so). Expected fix: edit the three strings.

## Checked, no finding
- (1) mods.json restore: Get-MoonoExe, Start-Process and Wait-MoonoGame failures all land in the finally with `$wardenStarted -and -not $proc`, which stops the warden and restores from the backup (hash-checked, backup removed, liveDir removed). Once a game is found the warden owns the restore. ModTest: the finally restores unless `$game` is still running; each Both leg takes its own backup after the previous leg's restore; Get-MoonoExe throws before the first backup. See M2 for the one residual issue.
- (2) Stock: same launch (`-applaunch 457140` + the same arguments), same 120 s wait loop, same exit codes; differences are the added `runtime $leg` loadout text, the `Runtime:` summary line, `runtime=` in the loadout key, and Player.log copied only if written since the launch.
- (3) Other-runtime reuse: `runtime=$leg` is in the key, so a Stock live game is ended for a Moono leg and the reverse; Both ends the live session between legs and waits for the warden's liveDir removal (30 s). The M1 pre-check is the only hole, and it errs on the safe side (refuses).
- (4) Quoting: every `--game-arg` and its value are quoted by ConvertTo-CommandLineArgument and passed as one string; the `--log` path is quoted. Run folders with spaces are rejected earlier anyway. The Stock `-ArgumentList` array is unchanged. Whether Moono re-quotes a value with a space when it forwards to the game is not shown by the evidence; reachable only through `-Filter` with a space, which the README forbids.
- (5) No verdict from a stale or missing log: results/status come from the new run folder; Player.log is copied only if fresh; the Moono leg reads moono.log from its own folder; the success summary in -115715-moono names the commit and standalone launch mode.
- Wait-MoonoGame's 3 s grace after a launcher exit is a judgement call; the evidence runs do not hit it.

## ICON tooling friction
None to record for this unit. The packet pointers were not opened: the review needed only the diff and the evidence, and the brief's records (TestRunner standards, entities) were not consulted. Nothing in the brief failed to appear that I could tell. The `ks read` step was skipped; this is noted rather than treated as friction.
