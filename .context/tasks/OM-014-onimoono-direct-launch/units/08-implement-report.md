# Unit 08 implement report: unit 07 findings fixed

Files: tools/TestRunner/GameProcess.ps1, Invoke-OniTests.ps1, README.md; tools/ModTest/Test-OniMods.ps1.
(tools/ModTest/README.md needed no change: it already describes the crash watcher being left out.)
No game or OniMoono was launched. Stand-ins used: a copy of cmd.exe named OniMoono.exe (fake game / fake
crash watcher), plain cmd.exe, whoami.exe.

## Finding -> fix

- M1 (live Moono blocks the next run). GameProcess.ps1 `Get-GameProcess -Any` now skips an OniMoono whose
  command line holds `--crash-watch` (unreadable line still counts, so the check errs toward refusing).
  One place; Invoke-OniTests.ps1 lines 212/217/229 and ModTest's Test-OniRunning get it without edits.
  ModTest's `Get-AnyGame` is removed, its callers use `Get-GameProcess -Any`.
- M2 (failed Moono launch leaves OniMoono running). New `Stop-MoonoLaunch $Launcher $StockBefore $Since` in
  GameProcess.ps1: ends the launcher, every OniMoono started since the launch (game and watcher; none was
  open before) and any stock OxygenNotIncluded not in StockBefore. Called first in the `finally` of
  Invoke-OniTests.ps1 (`$leg -eq 'Moono' -and -not $proc`) and in ModTest's `finally`
  (`$leg -eq 'Moono' -and -not $game`), before mods.json and prefs are restored. `$launcher`/`$stockBefore`
  are reset per leg in both scripts (they were undefined on a stock leg under strict mode).
- m1 (exit code unknown). Already fixed by c8d11ae; checked it covers the finding (below: a Get-Process
  object of an exited process gives no code, the log line gives it). Only change: Wait-MoonoGame's throw now
  passes `-Launcher` explicitly to `Get-MoonoExitText`.
- m2 (Moono failure in Both aborts). Invoke-OniTests.ps1: for a Moono leg, Start-Process + Wait-MoonoGame are
  in a try/catch; the message goes to `$legFailure`, a warning, the leg's summary ("FAILED: ...", session line
  "Leg failed before the game was found: ..."), `$legExit = 2`, and Both continues to the table. The post-launch
  wait/copy block is skipped when `$legFailure` is set. Stock-leg throws are unchanged. A single `-Runtime Moono`
  failure now also ends as a summary and exit 2 (it was an uncaught throw, exit 1).
- m3 (stale Player.log on reuse). Invoke-OniTests.ps1, reuse path: Moono leg uses `Copy-FreshLog ... $t0`;
  Stock keeps the main-branch unconditional copy.
- m4. Summary line is now `Runtime: <leg> asked; no game log written by this launch` when nothing ran-log is
  available; README says `no game log written by this launch`.
- m5. (a) GameProcess.ps1 header now names Get-MoonoArgumentLine and Wait-MoonoGame; (b) README options table
  says "above"; (c) README exit-code 2 says `Player.log`, or `moono.log` on a Moono leg. Also updated the
  GameProcess.ps1 `-Any` header comment, ModTest's header comment, and README notes for the watcher and for
  the failed-leg behaviour.

## Offline output

```
TestRunner\Invoke-OniTests.ps1 parse errors: 0
TestRunner\GameProcess.ps1 parse errors: 0
ModTest\Test-OniMods.ps1 parse errors: 0
fake game pid 21352, fake watcher pid 18444
Get-GameProcess -Any ids among fakes: 21352 (expect only game pid 21352)
Get-GameProcess (marker) ids: 21352
Ending cmd (pid 10032) left by the failed Moono launch.
Ending OniMoono (pid 18444) left by the failed Moono launch.
Ending OniMoono (pid 21352) left by the failed Moono launch.
after Stop-MoonoLaunch: launcher exited=True game exited=True watcher exited=True
older process spared: running=True
exit code from launcher (same process): 7
exit code with nothing: null -> unknown
Get-Process object after exit, with log fallback: 3        (cmd exited 5; the Get-Process object has no code, the log's 3 is used)
Moono exit text:
OniMoono exited with code 3 before the game started.
End of Moono's log (...om014-fake.log):
    [engine] Moono starting: commit=abc
    [engine] Exiting after 120 frames with code 3
stale Player.log copied: False (expect False)
```
Against the real processes of Jeremy's own Moono session (game 24528 + `--crash-watch` 17060):
`Get-GameProcess -Any (...): 24528` (watcher excluded). Stop-MoonoLaunch was only run with a Since after
Jeremy's game started, so it never touched it.

```
Invoke-OniTests.ps1 -Mods MotionSensorRange -BuildOnly -Runtime Moono
Built and checked against the game assemblies (-BuildOnly: no game session).   exit 0
Invoke-OniTests.ps1 -Mods MotionSensorRange -BuildOnly -Runtime Both            exit 0 (same line)
Test-OniMods.ps1 -Only MotionSensorRange -Runtime Both -DryRun                  exit 0
```
An attempt to run a fake-Moono leg end to end (`-MoonoExe whoami.exe`, to see the FAILED summary and the
mods.json restore) was refused by the script at once: "The game is already running; close it first."
(Jeremy's Moono session was up). mods.json hash was unchanged afterwards.

## Left for an in-game run (or a run with no game open)
- Moono twice within 15 s: second run reuses the live game (M1 end to end), then a different loadout restarts it.
- A Moono leg failing in Wait-MoonoGame inside `-Runtime Both`: FAILED summary, exit 2, table written, mods.json
  restored. Offline, this can be tried with `-MoonoExe C:\Windows\System32\whoami.exe` when no game is open
  (exits at once, so "OniMoono exited ... before the game started" after 3 s).
- Stop-MoonoLaunch on a real stock-appeared failure (a real stock OxygenNotIncluded ended before restore).
- Real moono.log containing the `[engine] Exiting after N frames with code X` line in the shape the regex expects.

## ICON tooling friction
None hit. I read the handoff file and the review report directly and did not open the `ks read` pointers
(TestRunner style/error-handling records); nothing was missing that I could tell.

Commit: see the hash in the sub-agent's reply.
