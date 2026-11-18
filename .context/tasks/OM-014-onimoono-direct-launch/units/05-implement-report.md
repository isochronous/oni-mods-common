# Unit 05 report - make a failed Moono launch explain itself

No game or OniMoono.exe was launched. Nothing committed.

## Changes
tools/TestRunner/GameProcess.ps1
- `Get-MoonoArgumentLine` takes `-LogPath`: puts `--log <file>` first, then the game args (:65-75).
- New `Get-LogTail`, `Get-ExitCodeOf`, `Get-MoonoExitText` (exit code + end of Moono log, or "wrote no log"), `Copy-FreshLog` (copies Player.log only if LastWriteTime >= launch) (:77-110).
- `Wait-MoonoGame` takes `-LogPath`; its "exited before the game started" error now carries the exit code and the log tail (:121, ~:136).

tools/TestRunner/Invoke-OniTests.ps1
- `$logCopied`, `$moonoLog = <runDir>\moono.log` per leg (:476-477); `--log` passed on the Moono launch (:564), log passed to Wait-MoonoGame (:575).
- Fresh launch copies Player.log through `Copy-FreshLog $playerLog ... $t0` (:592); the reused-game path keeps its old copy (:510).
- Report: no verdict from a missing/stale log - "Runtime: Moono asked; no Player.log written by this launch" (:634); for Moono, if OniMoono exited with no status file, the summary gets the exit code + log tail, otherwise the exit code; always a "Moono's log:" line (:648-653); the "No status file" line names moono.log. Parameter help updated.

tools/ModTest/Test-OniMods.ps1: Moono launch now `OniMoono.exe --log %TEMP%\oni-modtest.moono-<stamp>.log` (args were passed to Start-Process too, they were not before); log passed to Wait-MoonoGame; on exit prints the exit code and log path. READMEs of both tools updated.

## How `--log` is passed and why
01-explore-report.md:11 quotes OniMoono's usage line: `OniMoono [--game DIR] [--log FILE] ... [-logFile PATH] [other Unity player arguments, passed to the game]`. `--log FILE` is a Moono option (distinct from Unity's `-logFile`, which would redirect the game's Player.log), so it goes before the `--onitest-` args as a plain argument, no `--`. Unverified by launch: that it writes a file in standalone mode; the report says "none written" if not.

## Offline checks (actual output)
```
tools\TestRunner\Invoke-OniTests.ps1 parse errors: 0
tools\TestRunner\GameProcess.ps1 parse errors: 0
tools\ModTest\Test-OniMods.ps1 parse errors: 0
--- arg line
--log "C:\runs\r 1\moono.log" "--onitest-dir=C:\a b\x" --onitest-save=s
--onitest-save=s
--- exit code + log tail (cmd /c exit 7)
OniMoono exited with code 7 before the game's session wrote its status.
End of Moono's log (...\om014-offline\moono.log):
    moono line 6 ... moono line 20      (last 15 of 20)
--- Wait-MoonoGame launcher exits, no game
OniMoono exited with code 7 before the game started.
End of Moono's log (...): [same tail]
--- stale log
stale copied: False; dst exists: False
fresh copied: True; dst exists: True
Moono        (Get-LogRuntime of the fresh copy)
empty path runtime: ''               (no verdict printed)
```
`Invoke-OniTests.ps1 -Mods MotionSensorRange -BuildOnly -Runtime Moono`: "Built and checked against the game assemblies (-BuildOnly: no game session)." exit=0.
`Test-OniMods.ps1 -Runtime Moono -DryRun`: "Would launch (Moono): F:\modding\oni-moono\player\OniMoono.exe --log C:\Users\thegr\AppData\Local\Temp\oni-modtest.moono-20261112-114848.log" exit=0.

## Open points
- Cause of the 11:45 failure is still unknown; the launcher process itself carries `--onitest-`, so `Get-GameProcess` returns it as "the game" at once (that is why it was reported as a game exit). I kept that, since Moono may run the game in-process; the exit is now reported with code and log tail. The next run's moono.log decides.
- `$proc.ExitCode` on a Get-Process object may be unreadable in some cases; it then prints "unknown". When the proc is the Start-Process launcher it is reliable (shown above with cmd).
- The end-to-end summary paths (exit-after-wait branch in Invoke-OniTests) were exercised only through the helper functions, not a full run.
- Stale-log rule applies to stock fresh launches too (goal 3 is general); the reused-game path is unchanged.
- Pointer check: none of the briefed pointers failed to apply. The only miss: `ks read` of the handoff file path was refused ("names no record in this store"), so the packet was read as a plain file. ICON friction: the packet says to read the pointers with ks; the packet itself is not a record.
