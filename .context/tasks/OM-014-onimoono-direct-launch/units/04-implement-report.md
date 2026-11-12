# Unit 04 report — implement: launch Moono in standalone mode

No game, OniMoono or Steam was launched; no commits. ModTest shares the launch, so it is changed too.

## Changes (all under tools/, uncommitted)

- `TestRunner/GameProcess.ps1:65-70` `Get-MoonoArgumentLine([string[]]$GameArgs)`: no `--game`, no GameFolder parameter; only the quoted game arguments.
- `TestRunner/GameProcess.ps1:72-77` new `Get-StockGameIds`: pids of OxygenNotIncluded before the launch.
- `TestRunner/GameProcess.ps1:79-105` new `Wait-MoonoGame $Launcher $StockBefore $TimeoutSec {Find}`: polls every 0.5 s; returns the game from `{Find}`; throws at once if an OxygenNotIncluded not in `$StockBefore` appears ("The stock game ... started during the Moono launch ... The leg is failed."); throws "OniMoono exited with code N before the game started." when the launcher (Start-Process -PassThru) has exited and no game showed within 3 s (grace in case the launcher hands off to another OniMoono process); else "did not start within N s".
- `TestRunner/Invoke-OniTests.ps1:559-562` Moono leg: `Get-MoonoArgumentLine $gameArgs`, `$stockBefore`, `Start-Process ... -PassThru` into `$launcher`. `:568-575` the 120 s wait loop is now `Wait-MoonoGame` (Moono only, finds `OniMoono` by the --onitest- marker); Stock keeps its loop. A throw lands in the existing `finally` (restore), as the old 120 s throw did.
- `TestRunner/Invoke-OniTests.ps1:46-48` `.PARAMETER Runtime` help refreshed.
- `ModTest/Test-OniMods.ps1:35-36, 85-88, 245-246, 252` help/comments; launch is `OniMoono.exe` with no arguments (`Get-MoonoArgumentLine` with none). `:299-303` Moono launch uses `-PassThru` and `$stockBefore`; `:305-317` Moono leg waits with `Wait-MoonoGame` (180 s), Stock unchanged.
- `TestRunner/README.md` ("Stock or Moono") and `ModTest/README.md` (-Runtime Moono bullet): `--game` removed, standalone mode and the fail-fast behaviours described.
- `grep -- --game` over both tools now matches only the "no --game" statements.

## Checks

Parse (`Parser::ParseFile`): Invoke-OniTests.ps1 errors: 0; GameProcess.ps1 errors: 0; Test-OniMods.ps1 errors: 0.

`pwsh tools/TestRunner/Invoke-OniTests.ps1 -Mods MotionSensorRange -Filter "*Defaults*" -Runtime Moono -BuildOnly`:
```
Build succeeded.   3 Warning(s) (CS0618 FindObjectOfType, pre-existing)   0 Error(s)
Checking MotionSensorRange.Tests.dll against the game assemblies
Built and checked against the game assemblies (-BuildOnly: no game session).
exit 0
```
-BuildOnly does not reach the launch, so the line below comes from calling the functions with the sample arguments.

Launch command the Moono leg now uses (TestRunner):
```
F:\modding\oni-moono\player\OniMoono.exe --onitest-dir=<runDir> --onitest-save=<runDir>\fixture.sav --onitest-timeout=<s> --onitest-test-timeout=<s> [--onitest-filter=*Defaults*] [--onitest-config=..] [--onitest-trace=1] --onitest-server=<liveDir>
```
(sample output: `... --onitest-dir=C:\runs\x-moono --onitest-save=C:\runs\x-moono\fixture.sav --onitest-timeout=600 --onitest-test-timeout=120 --onitest-filter=*Defaults* --onitest-server=C:\live`). ModTest: `ModTest -Runtime Moono -DryRun` prints `Would launch (Moono): F:\modding\oni-moono\player\OniMoono.exe` (no arguments).

Exit-code path exercised with a harmless stand-in launcher (`cmd /c exit 7`, Find returning nothing):
`launcher-exit test: OniMoono exited with code 7 before the game started.`
The stock-game branch (OxygenNotIncluded appearing) was not exercised: it needs the game running.

## Proposed wording for testing/integration-testing.md

"`-Runtime Moono` starts `OniMoono.exe` directly with the `--onitest-*` arguments only. There is no `--game`: in standalone mode Moono gets the game folder from Steam, starts Steam if needed and sets SteamAppId itself. The runner records the OxygenNotIncluded processes running before the launch; if a new one appears during the Moono leg, or OniMoono exits before the game is found, the leg fails at once with that message (and OniMoono's exit code) instead of waiting out the 120 s timeout."

## Open points

- `Get-GameFolder` is still used by Invoke-OniTests.ps1 (managed folder, workshop acf); only the Moono launch stopped using it. ModTest no longer calls it.
- The 3 s exit grace is my choice; Moono's Restarter/crash-watch may hand off processes.
- Other callers of `Get-MoonoArgumentLine` in tools/ (SaveRunner, WorldgenDriver): none found by grep of tools/TestRunner and tools/ModTest only; other tools were not searched.

## ICON tooling friction

- A `grep -rn "--game" .` over tools/ ran past the 120 s tool timeout (large trees); the scoped grep afterwards was instant.
- Bash heredoc with a long python body failed to parse (the known ~8k cut); a Write-then-run script worked.
- I did not need `ks read` or gather-context; the handoff plus the scoped source files were enough. Nothing in the pointer list was wrong; the TestRunner README (not listed) is the document that governs the help text.
