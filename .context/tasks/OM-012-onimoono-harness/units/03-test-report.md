# OM-012 test report: one run of the patched TestRunner through oni-moono

**Result: pass.** The launcher found the game, 5 of 5 tests passed, exit code 0, mods.json restored after the game exited, no game process left. One run, no -KeepGame, no rerun. Nothing committed or edited.

Before the run: no OxygenNotIncluded or OniMoono process. mods.json SHA-256 98490930...A690B.

## Runner output (verbatim, 21:15:49 to 21:17:00)

```
pwsh -NoProfile -File common/tools/TestRunner/Invoke-OniTests.ps1 -Mods FridgeThresholdsExtended -Steam 2959340381,2618339179,3557584850

Building ...FridgeThresholdsExtended.Tests.csproj
(3 CS0618 obsolete-API warnings in TestRunner Worldgen.cs and TestBehaviour.cs)
Build succeeded.
    3 Warning(s)
    0 Error(s)
Time Elapsed 00:00:02.38
Checking FridgeThresholdsExtended.Tests.dll against the game assemblies
Loadout: TestRunner + FridgeThresholdsExtended + Workshop 2959340381, 2618339179, 3557584850; mode Spaced Out; fixture C:\dev\ONI-Mods\common\tools\TestRunner\fixtures\testbed-so.sav
Launching: -applaunch 457140 --onitest-dir=C:\dev\ONI-Mods\common\tools\TestRunner\runs\run-20261104-211549 --onitest-save=...\fixture.sav --onitest-timeout=600 --onitest-test-timeout=60 --onitest-config=FridgeThresholdsExtended --onitest-server=C:\Users\thegr\AppData\Local\IsochronousOniTests\live
The game stays up 15 s for another run with this loadout; its warden puts mods.json back when it exits.
PASS     FridgeThresholdsExtended.Tests/Fridges.FreezerFollowsThresholds (6.5 s)
PASS     FridgeThresholdsExtended.Tests/Fridges.IndustrialRevolutionFridge (4.2 s)
PASS     FridgeThresholdsExtended.Tests/Fridges.FreezerCopySettings (2.5 s)
PASS     FridgeThresholdsExtended.Tests/Fridges.WriteRoundTrip (4.4 s)
PASS     FridgeThresholdsExtended.Tests/Fridges.LoadKeepsOrderFillAndGreen (3.6 s)
Session done: 5 passed, 0 failed, 0 errors, 0 timeouts, 0 skipped; 5 passed, 0 failed, 0 errors, 0 timeouts, 0 skipped of 5; 49.4 s real time; fixture loaded again 1 time(s)
Player's folders: nothing new in save_files, cloud_save_files, RetiredColonies
Game live after 66 s; results in C:\dev\ONI-Mods\common\tools\TestRunner\runs\run-20261104-211549
EXIT 0
```

Console capture: C:\Users\thegr\AppData\Local\Temp\claude\C--dev-ONI-Mods\7b932c4a-6569-4783-a8c2-8a995ac90316\scratchpad\om012-run.txt
Run folder: C:\dev\ONI-Mods\common\tools\TestRunner\runs\run-20261104-211549 (Player.log, results.jsonl, summary.txt, artifacts, mods.json.real, warden.log copy).

## Evidence

1. **Launcher found the game.** No "did not start within 120 s"; the line "The game stays up 15 s for another run..." appeared and the results were printed. Exit code 0. (Run 5 under Moono: exit 1 with that error.)
2. **Counts.** 5 passed, 0 failed, 0 errors, 0 timeouts, 0 skipped. Same as OM-011 run 5 (timings within 0.1 to 0.5 s of it).
3. **Moono and Player.log.** Line 1 of Player.log, both the live file and the copy in the run folder: `[engine] Moono starting: commit=07cbcfcb8417afb90313e034a3d8456f888d2bb6 exe=%USERPROFILE%\AppData\Local\oni-moono\player\OniMoono.exe game=F:\SteamLibrary\steamapps\common\OxygenNotIncluded .NET=10.0.13`. The launcher copied it (209952 bytes, 21:16:59). It holds 0 lines matching Exception or [ERROR]. Final engine lines: `Application quitting`, `driver destroyed`, `Exiting after 4084 frames with code 0` (Run 5 had 7028 frames; the live game had quit at 21:17:15).
4. **Warden restored mods.json after the game exited.** Warden log (the file's timestamps are local time):
   ```
   21:15:55 [0] waiting up to 150 s for the game
   21:15:57 [56816] watching
   21:17:15 [56816] game exited
   21:17:15 [56816] restored mods.json
   21:17:15 [56816] put the player's last-save pointer back (SaveFilenameKey/)
   21:17:15 [56816] live session closed
   ```
   The warden found the game 2 s after launch, kept watching through the run, and restored at the moment OniMoono exited (Player.log's last line 21:17:15). The launcher printed no "Restored mods.json" of its own this time. Hashes after: player mods.json 98490930F3A42F0F757D77A095C6ECD22D994E574957FF1C74CEED24631A690B, mods.json.real in the run folder the same, and equal to the hash before the run. Player mods.json LastWriteTime 21:17:15. In Run 5 the restore came from the launcher's failure path, 25 s after the game exit, with no warden left.
5. **No game process left.** After the game exited, no OniMoono or OxygenNotIncluded process remained (checked at exit and again about 25 s later). The run's pid was OniMoono, which the harness now follows.

## Compared with OM-011 run 5

| | Run 5 (before) | This run |
|---|---|---|
| Launcher finds game | no, threw after 120 s, exit 1 | yes, exit 0 |
| Results printed | no (read from results.jsonl) | yes |
| Player.log copied by launcher | no | yes |
| mods.json restored by | launcher failure path | warden, on OniMoono exit |
| Warden | stopped by launcher | watched and exited on its own |
| Tests | 5/5 | 5/5 |

## Brief feedback (for the coordinator)

- The `common` store's TestRunner pointers were reached only through this handoff; nothing in them mentions oni-moono. The testing note `.context/testing/integration-testing.md` ("Launching with oni-moono") should now say the launcher and warden follow the OniMoono process. I did not check whether it already does.
- No ICON friction.
