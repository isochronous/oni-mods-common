Task: OM-012 — one confirming in-game run of the patched TestRunner through oni-moono
Lens: test. Repos: C:\dev\ONI-Mods (branch OM-012-onimoono-harness, has mods/fridge-thresholds-extended) with common/ on branch OM-012-onimoono-harness (the harness change). Do not commit, do not edit code, do not touch Steam settings.
Jeremy approved ONE run (2026-11-04), launch options set to start the game through oni-moono.

Before: confirm no OxygenNotIncluded or OniMoono process is running (if one is, stop and report; do not kill it).
Run once: `pwsh common/tools/TestRunner/Invoke-OniTests.ps1 -Mods FridgeThresholdsExtended -Steam 2959340381,2618339179,3557584850`. No -KeepGame, never stop the runner mid-run, no foregrounding/polling the window, no rerun whatever the result.

Prove, with copied evidence:
1. The launcher found the game (no "did not start within 120 s"), exit code 0 if all pass.
2. Pass/fail counts (expect 5/5, as run 5 under Moono).
3. Player.log line 1 is "Moono starting ...", and Player.log was copied into the run folder.
4. The warden restored mods.json after the game exited: hash of the player's mods.json vs mods.json.real in the run folder, and the warden's log/timing showing it waited for the OniMoono process to exit.
5. No game process left afterwards.
Compare with OM-011 run 5 (C:\dev\ONI-Mods\.context\tasks\OM-011-fridge-thresholds-patch\units\04-test-report.md "Run 5 (oni-moono)").

Report: C:\dev\ONI-Mods\common\.context\tasks\OM-012-onimoono-harness\units\03-test-report.md with the verbatim runner output and the evidence above. Reply with a 3-line summary.
Model tier: default
