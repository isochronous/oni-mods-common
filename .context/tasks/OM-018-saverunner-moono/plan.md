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
- [ ] Harness: reused game quits loading a test-written save (found in OM-017, run-20261126-152950). Unit 02 stopped by Jeremy; the manager took it over directly. Findings: the warden did not kill the game (warden.log "game exited" only); App.Quit, Application.Quit() and App.Restart traces stayed silent, so the quit came through App.QuitCode/Application.Quit(int) or a window close. The one difference from passing reused runs: a BatteryTracker.UpdateData NRE during the reused reload (App.isLoading, so KCrashReporter defers it to the next SceneInitializerLoader.Awake). Refuted in game with probes: a crash dialog opened then a fresh load (run-20261126-154713), and a planted deferred error then a fresh load (run-20261126-154824) — both survive. A reused repeat of the failing pair (run-20261126-154527) did not quit and had no NRE. Not reproducible on demand. Kept: 87a6ba0 traces App.QuitCode and logs crash dialogs during a run, so the next occurrence names its caller. Dropped: unit 02's unproven Session.Quit IdleSince change and the probe test.
- [x] Review (units/03-review-report.md): 0 critical, 2 moderate, 5 minor. Fixed: M1 Moono crash watcher stopped after a killed game; M2 failed Moono launch exits 1; m1 game matched by --saverun-out=<csv> and --crash-watch excluded; m4 Moono run reads moono.log first; m5 README synopsis lists -MoonoExe. Left: m2 (Ctrl+C advice during the 900 s stray wait, same text in ModTest, untestable without a console) and m3 (empty stock snapshot if Set-Mods throws; narrow). Verified: parse 0 errors, dry run, real Moono run om018-review-20261126-155554 (exit 0, status done, mods.json restored).
- [ ] Completion: decision note on OM-014, retro, PR ← IN PROGRESS

## Open Questions / Blockers
- Jeremy's game must be closed before any run.

## Constraints
- Test runs hold Jeremy's desktop: one run, ask before long or chained runs; never leave the game open; never stop a runner mid-run.
- Disable Mod Profile Manager for runs on his loadout (-Without 2854869130,3281716506).
