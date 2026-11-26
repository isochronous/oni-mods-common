## Task: OM-018
## Branch: OM-018-saverunner-moono (in common/, the oni-mods-common submodule)
## Objective: Give Save Runner a `-Runtime Stock|Moono` choice like TestRunner and ModTest, so a save can be loaded unattended on oni-moono, the runtime Jeremy plays on. Needed now for OM-017 (Smart Storage threshold signal wrong after load, diagnostic must run in his save on Moono), and wanted in general (Jeremy, 2026-11-26: "need to add moono support to save runner anyway").
## Folder: common/.context/tasks/OM-018-saverunner-moono/

## Phase State
- **Phase plan**: implementation → testing → completion
- **Completed**: (none)
- **Current**: implementation   (status: in-progress)
- **Next**: testing
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
- [ ] Implement -Runtime for Save Runner (and any GameProcess.ps1 changes), README updated ← IN PROGRESS
- [ ] Test: one short Moono run on a small save (or Jeremy's newest save for OM-017) with his game closed; confirm Player.log line 1 is "Moono starting", probe output written, game closed, mods.json restored
- [ ] Completion: review, decision note on OM-014, retro, PR in oni-mods-common

## Open Questions / Blockers
- Jeremy's game must be closed before any run.

## Constraints
- Test runs hold Jeremy's desktop: one run, ask before long or chained runs; never leave the game open; never stop a runner mid-run.
- Disable Mod Profile Manager for runs on his loadout (-Without 2854869130,3281716506).
