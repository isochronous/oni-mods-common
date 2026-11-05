## Task: OM-013
## Branch: OM-013-onimoono-other-tools (in common/, stacked on OM-012-onimoono-harness, which adds tools/TestRunner/GameProcess.ps1)
## Objective: SaveRunner, WorldgenDriver and ModTest find the game they launched when it runs through oni-moono (process `OniMoono`), the way TestRunner does after OM-012. Today they look only for `OxygenNotIncluded`, so on Jeremy's Moono launch options they report the game as not started, or stop watching it while it runs.
## Folder: common/.context/tasks/OM-013-onimoono-other-tools/

## Phase State
- **Phase plan**: implementation → testing → completion
- **Completed**: implementation, testing, completion
- **Current**: completion   (status: done)
- **Next**: none — task complete
- **Loaded skill**: task-plan-phase-completion
- **Branch**: OM-013-onimoono-other-tools
- **Attempts (current phase)**: 1

## Decisions
- Same rule as OM-012 (Jeremy, 2026-11-04): the launched game is `OxygenNotIncluded`, or `OniMoono` whose command line carries the tool's own launch arguments; any open `OniMoono` blocks a launch the way an open game does. Jeremy asked for the other common tools to be updated 2026-11-04.
- Reuse TestRunner's `GameProcess.ps1` rather than copy it, if each tool can reach it without a fragile path; otherwise the unit proposes where a shared copy should live.

## Key Files
- common/tools/SaveRunner/Invoke-SaveRun.ps1
- common/tools/WorldgenDriver/Invoke-Worldgen.ps1
- common/tools/ModTest/Test-OniMods.ps1
- common/tools/TestRunner/GameProcess.ps1 (from OM-012)
- Also name OxygenNotIncluded but are likely paths, not process lookups: tools/Kanim/kanim.py, tools/Translations/translations.py (to confirm)

## Phase Handoff Log

## Progress
- [x] Implement (units/01-implement-report.md, 326871b): Get-GameProcess -Marker (default `--onitest-`); SaveRunner `--saverun`, WorldgenDriver `--worldgen-`; ModTest (no launch args) follows the first game started after launch; any OniMoono refuses a launch; READMEs. Kanim/Translations use the name only as a path, unchanged. Parse 0 errors; stand-ins pass, TestRunner's unchanged.
- [x] Review (units/02-review-report.md): moderate ModTest restart race fixed (78f9ef3: follow by old Id + StartTime); helper comments generalised. Left: WorldgenDriver no open-game check before sessions 2..n (pre-existing); ModTest own launch argument (out of scope).
- [x] In-game through Moono, unattended with Jeremy's approval (units/03-test-report.md): WorldgenDriver passed (session 1 exited: 1 result, complete; mods.json/kplayerprefs/config hashes restored; no processes left). SaveRunner followed OniMoono from 2 s to its exit at 968 s and restored mods.json, but game time ran at ~1/100 speed and the run timed out at 900 s (9 of 10 game seconds; spec omitted -Probe, so 0 rows was expected). Cause unknown; not rerun. ModTest not run (needs the game quit by hand).
- [x] Store: code-style.md in tools/TestRunner, SaveRunner, WorldgenDriver; retro in common retrospectives/icon.md; parent testing/integration-testing.md "Launching with oni-moono"; `ks check` 0 block in both stores.
- [x] Completion: commits pushed onto PR #8's branch; parent pointer bump onto PR #15's branch

## Open Questions / Blockers
- Resolved: markers `--saverun`, `--worldgen-`; ModTest passes none.
- Resolved 2026-11-05: the SaveRunner timeout was a pause, not Moono. A loaded save starts paused (Game.OnSpawn), RunBehaviour unpaused only on its first update, and something paused the game again 8.4 game seconds in. RunBehaviour now unpauses whenever it finds the game paused (851e330); rerun on Moono with -TimeoutSec 180: status done, 11 samples, 71 s real time, mods.json hash unchanged. The second pause was the "colony lost" screen (the fixture has no Duplicants; Jeremy), and the mod events dialog pauses the same way: SaveRunner now carries TestRunner's CheckForGameOver, MatchFootprint and Report patches; rerun: done, no pause logged, 10 game s in 10 real s. Launcher `-Only` added (only SaveRunner and -With) for quick fixture runs, at Jeremy's request.
- For Jeremy: ModTest's Moono path needs one in-game check with the game quit by hand (`Test-OniMods.ps1 -NoLocal`), ideally with one mod-change restart.
- The SaveRunner leftover restore removed an empty worldgen-driver\config-backup folder dated 2026-10-20 (documented behaviour).

## Completion Summary
**Accomplished:** SaveRunner, WorldgenDriver and ModTest find the game they launched when it runs through oni-moono, through TestRunner's shared Get-GameProcess.
**Files changed:** tools/TestRunner/GameProcess.ps1, tools/SaveRunner/Invoke-SaveRun.ps1, tools/WorldgenDriver/Invoke-Worldgen.ps1, tools/ModTest/Test-OniMods.ps1, their READMEs; three code-style records here, one note in the parent.
**Tests:** stand-in process checks; WorldgenDriver passed in game on Moono; SaveRunner followed the game but timed out on slow game time; ModTest unchecked in game.
**Follow-up work:** see Open Questions.

## Constraints
- Never foreground or poll the game window. Never edit Steam settings.
- Commit inside common first, then bump the pointer in the parent.
