## Task: OM-012
## Branch: OM-012-onimoono-harness (in common/, the oni-mods-common repo; the parent ONI-Mods repo has a branch of the same name, stacked on OM-011, for the submodule pointer and the store note)
## Objective: Make TestRunner's launcher and warden follow a game started through oni-moono. Under Moono the game runs as an `OniMoono` process, but Invoke-OniTests.ps1 and Watch-OniGame.ps1 only look for `OxygenNotIncluded`, so a run on Moono finishes but reports "The game did not start within 120 s", exits 1, and stops the warden while the game is still running (mods.json is then restored under a running game).
## Folder: common/.context/tasks/OM-012-onimoono-harness/

## Phase State
- **Phase plan**: implementation → testing → completion
- **Completed**: (none)
- **Current**: implementation   (status: in-progress)
- **Next**: testing
- **Loaded skill**: task-plan-phase-implementation
- **Branch**: OM-012-onimoono-harness
- **Attempts (current phase)**: 1

## Decisions
- One helper that returns the game process: `OxygenNotIncluded`, or `OniMoono` whose command line contains `--onitest-` (so a Moono session Jeremy started by hand is never taken for the test game). Every one of the 9 call sites uses it. (Proposed by the OM-011 run-5 test unit; Jeremy asked for the fix 2026-11-04.)

- A hand-started Moono game (any OniMoono process) blocks a run, the same as an open OxygenNotIncluded (Jeremy, 2026-11-04). Jeremy approved one confirming run through Moono after review.

## Key Files
- common/tools/TestRunner/GameProcess.ps1 (new): `Get-GameProcess`, loaded like ModConfig.ps1/GameMode.ps1
- common/tools/TestRunner/Invoke-OniTests.ps1: game process checks at :154 :215 :218 :220 :232 :542 :576 (line numbers from OM-011 run 5)
- common/tools/TestRunner/Watch-OniGame.ps1: game process checks at :41 :45
- Evidence: C:\dev\ONI-Mods\.context\tasks\OM-011-fridge-thresholds-patch\units\04-test-report.md "Run 5 (oni-moono)"

## Phase Handoff Log

## Progress
- [x] Implement the helper and use it at every call site — units/01-implement-report.md, commit 389e3ad; parse 0 errors; stand-in process checks pass
- [ ] Any OniMoono blocks the "game already running" checks (:216/:221/:233) ← IN PROGRESS
- [ ] Review
- [ ] One in-game run through Moono (FridgeThresholdsExtended suite, the one that exposed it) — ask Jeremy first
- [ ] Update parent store note "Launching with oni-moono", bump the submodule pointer
- [ ] Completion: retro, commit, PRs (oni-mods-common; parent ONI-Mods via PR #15's branch stack)

## Open Questions / Blockers
- Resolved: nothing else finds the game by name or window; Player.log was not copied in run 5 only because the start wait threw first.

## Constraints
- Never foreground or poll the game window (GUI tests never steal focus).
- The warden must still restore mods.json after the game exits, for both process names.
- Commit inside common first, then bump the pointer in the parent.
