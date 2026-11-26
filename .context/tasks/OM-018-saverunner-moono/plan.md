## Task: OM-018
## Branch: OM-018-saverunner-moono (in common/, the oni-mods-common submodule)
## Objective: Give Save Runner a `-Runtime Stock|Moono` choice like TestRunner and ModTest, so a save can be loaded unattended on oni-moono, the runtime Jeremy plays on. Needed now for OM-017 (Smart Storage threshold signal wrong after load, diagnostic must run in his save on Moono), and wanted in general (Jeremy, 2026-11-26: "need to add moono support to save runner anyway").
## Folder: common/.context/tasks/OM-018-saverunner-moono/

## Phase State
- **Phase plan**: implementation → testing → completion
- **Completed**: implementation, testing
- **Current**: completion   (status: pending)
- **Next**: none
- **Loaded skill**: task-plan-phase-implementation
- **Branch**: OM-018-saverunner-moono
- **Attempts (current phase)**: 1

## Decisions
- Reverses OM-014's "SaveRunner and WorldgenDriver keep launching through Steam" for SaveRunner only (Jeremy, 2026-11-26). WorldgenDriver unchanged. The OM-014 decision record gets a superseding note at close (adr-discipline).
- Reuse TestRunner's Moono launch helpers in the shared GameProcess.ps1 rather than a second launcher (OM-014: SaveRunner/WorldgenDriver must keep working through the shared GameProcess.ps1).
- Default runtime stays Stock, so existing callers are unchanged.

## Key Files
- tools/SaveRunner/ (Invoke-SaveRun.ps1, dev mod): the launcher being extended
- tools/TestRunner/GameProcess.ps1: shared process helpers, Moono launch (Wait-MoonoGame etc.)
- .context/tasks/OM-014-onimoono-direct-launch/plan.md: how -Runtime was added to TestRunner/ModTest and its pitfalls (Moono needs --game, game args as --game-arg, unknown -- options exit 2)

## Phase Handoff Log

## Progress
- [x] Implement -Runtime for Save Runner — e27db56 (units/01-implement-report.md): -Runtime/-MoonoExe/-DryRun, standalone launch (no --game, per OM-014), waits on the launched OniMoono, Player.log copied only when fresh, runtime read from moono.log; Get-LogRuntime fix (launch line moved to line 4 in Moono f005cd6c, now scans first 10 lines). Dry runs and parse checks pass; no real run yet (game open).
- [x] Test: real Moono run 2026-11-26 15:01 (runs/om017-moono-20261126-150108), Jeremy's go: newest autosave Cycle 993, -Seconds 30, -Without MPM. Output: "Restored mods.json." / "Runtime: Moono asked; moono.log shows Moono (commit 287fc57d...; launch: standalone...)" / "OniMoono exit code 0" / "Game exited after 154 s; status done, 31 samples, realSeconds 97.3". OM-017 diagnostic lines (105) in moono.log.
- [ ] Completion: review unit, decision note on OM-014, retro, PR ← NEXT

## Open Questions / Blockers
- Jeremy's game must be closed before any run.

## Constraints
- Test runs hold Jeremy's desktop: one run, ask before long or chained runs; never leave the game open; never stop a runner mid-run.
- Disable Mod Profile Manager for runs on his loadout (-Without 2854869130,3281716506).
