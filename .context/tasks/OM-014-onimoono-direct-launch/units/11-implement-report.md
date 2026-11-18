# Unit 11 report: a stray stock game after a failed Moono launch goes to the warden

## Changes
- tools/TestRunner/GameProcess.ps1 `Stop-MoonoLaunch` (~:181-222): new `-RunMarker`. Candidates are OniMoono.exe plus OxygenNotIncluded.exe not in `$StockBefore`. Stopped: the launcher, anything whose command line holds `-LogPath` or `-RunMarker`, and any descendant of those (fixpoint, now across stock too). Returns the pids of stock games left running (the "left running" message now says why).
- tools/TestRunner/Invoke-OniTests.ps1 `finally` (~:607-650): passes `"--onitest-dir=$runDir"`; if strays come back, the no-game warden is ended as before, a new warden is started with `-GamePid <stray>` (Start-Warden, same ModsJson/Backup/LiveDir/ConfigRoot/Prefs), `$wardenStarted` stays true so the script restores nothing itself, the leg's FAILED text says what was left and that the warden restores. Stock leg untouched.
- tools/TestRunner/Watch-OniGame.ps1: unchanged (`-GamePid` already follows a pid and restores mods.json, config, mode, save pointer on exit).
- tools/ModTest/Test-OniMods.ps1 (~:344-352): ModTest had the same gap (a stray left alive, then mods.json restored under it). No warden, so it waits (`Wait-Process`) for the stray pid, then restores as before. Prefs are not touched by ModTest.
- READMEs: TestRunner/README.md (Moono leg failure paragraph), ModTest/README.md.

## Offline checks
Parse (ParseFile): all four scripts "parse errors: 0".
`Invoke-OniTests.ps1 -Mods Bitshifter -BuildOnly -Runtime Moono`: "Built and checked against the game assemblies (-BuildOnly: no game session)." exit=0. (Without `-Mods` it fails on the mandatory parameter, unrelated.)
Stand-ins (cmd.exe copies named OxygenNotIncluded.exe / OniMoono.exe in the scratchpad; no real game was running; scratch mods.json/prefs; script scratchpad/stray-test.ps1):
```
=== (a) stock stand-in started by the fake launcher (descends from it)
Ending OniMoono (pid 21388) left by the failed Moono launch.
Ending OxygenNotIncluded (pid 38256) left by the failed Moono launch.
returned left=[]; stock alive=False launcher alive=False
=== (b) stock stand-in carrying this run's --onitest-dir
Ending OxygenNotIncluded (pid 36120) left by the failed Moono launch.
returned left=[]; stock alive=False
=== (c) unrelated stock stand-in (another run's dir) survives; warden restores after it exits
A stock OxygenNotIncluded (pid 26648) started ... is not provably this launch's; it is left running, since it may be the player's.
returned left=[26648]; stock alive=True
while the game runs: mods.json='TEST LOADOUT' backup exists=True mode line='EXPANSION1_ID.ENABLED: 0'
after it exited:     mods.json='REAL LOADOUT' backup exists=False mode line='EXPANSION1_ID.ENABLED: 1' warden exited=True
2026-11-12 12:20:35 [26648] watching / game exited / restored mods.json / put the game's mode back / live session closed
```
In (c) the warden was started directly (`Watch-OniGame.ps1 -GamePid`) with the same arguments the new `finally` block passes; the Invoke-OniTests `finally` branch itself (Start-Warden call, FAILED text) was parse-checked only, not executed.

## Only an in-game run can prove
- That a real Steam-relaunched stock game is seen by `Get-CimInstance` with a command line carrying `--onitest-dir` (or a launcher ancestor), and that the real failure path reaches the new branch end to end.
- Whether Moono's `--crash-watch` OniMoono descends from the launcher or the game: not answerable from code. Only the binaries are available (F:\modding\oni-moono\player); no source, and strings give only the `"%ls" --crash-watch %llx` format. Either way the fixpoint catches it if its parent is the launcher or the game; it is missed only if spawned by something outside that tree.
- Limitation: if several strays are left, the warden follows the first pid only.

## ICON tooling friction
None. Packet pointers matched by path only; I did not need `ks read` for them (the task was fully specified by the unit spec and the earlier reports). Hit: `sed -i` on a CRLF file under Git Bash stripped CRLF; restored with a byte-level rewrite.
