# Unit 13 implement report

Commit: 7587e6b (tools only: 4 scripts + 2 READMEs; plan.md and this report left uncommitted).

## Defects -> fixes
1. Reused Moono session, no game log. Choice: the launcher records the live game's log path in the live dir, the reusing run reads it back and names it; nothing is copied into the new run folder (README says so).
   - Invoke-OniTests.ps1:559 writes `$liveDir\moono-log.txt` (= the launching run's moono.log) next to key.txt for a Moono leg.
   - Invoke-OniTests.ps1:512 on reuse sets `$moonoLog` from `Get-LiveMoonoLog $liveDir`; the existing summary code then reads the runtime from it and prints "Moono's log: <path>".
   - GameProcess.ps1:224 `Get-LiveMoonoLog` (returns '' if the record or the file is missing). Stock reuse untouched (still Player.log fresh-copy path).
2. Unit 12 findings:
   - L1 ModTest wait had no timeout: Test-OniMods.ps1:~349-356, `Wait-Process -Timeout` over a shared 900 s budget; if a stray is still up, `$strayHeld` makes the finally block skip the restore and print the by-hand Copy-Item command (:358). Not restoring under a running game keeps Jeremy's decision. Not run offline (needs a stray stock game); parse-checked only.
   - L2 warden followed only `$stray[0]`: Watch-OniGame.ps1 gets `-AlsoPids` (:25, wait loop :92-96); Invoke-OniTests.ps1:631 passes the rest.
   - L3 empty `$stockBefore`: Invoke-OniTests.ps1:578 now takes `Get-StockGameIds` before `Get-MoonoArgumentLine`/`Get-MoonoStreamPaths` can throw (ModTest already did it first, :299).

## Offline output
```
TestRunner/Invoke-OniTests.ps1 parse errors: 0
TestRunner/GameProcess.ps1 parse errors: 0
TestRunner/Watch-OniGame.ps1 parse errors: 0
ModTest/Test-OniMods.ps1 parse errors: 0
no file ->  []
with file -> [C:\dev\ONI-Mods\common\tools\TestRunner\runs\om014-reuse1-20261112-122449-moono\moono.log]
Runtime: Moono asked; moono.log shows Moono (commit 2fb86e60140d4656409ff4e8e0804bbb03dd9144)
Moono's log: C:\dev\ONI-Mods\common\tools\TestRunner\runs\om014-reuse1-20261112-122449-moono\moono.log
stale path -> []
--- warden AlsoPids (two scratch ping processes, scratch mods.json)
warden returned after 7 s; b exited: True; mods.json: y
... [24676] game exited / also waiting for pid 9932 / pid 9932 exited / restored mods.json / live session closed
```
`-BuildOnly -Mods MotionSensorRange -Runtime Moono`: "Built and checked against the game assemblies (-BuildOnly: no game session)", exit 0.
(The summary lines were reproduced with the same expressions as Invoke-OniTests.ps1:~690-710 against a fake live dir; the full script path was not run.)

## For an in-game run
Two Moono runs in a row with the same loadout: run 2 should print "Runtime: Moono asked; moono.log shows Moono (commit ...)" and "Moono's log: <run 1's moono.log>". The ModTest timeout and two simultaneous strays are untested.

## ICON tooling friction
None. I read the handoff file only, not the `ks read` pointers (the spec was sufficient). Note: Invoke-OniTests.ps1 is LF in the working tree while the others are CRLF (git warns LF->CRLF); left as is.
