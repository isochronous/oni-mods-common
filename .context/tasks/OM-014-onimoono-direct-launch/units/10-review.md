# Unit 10 — review: N1 fix (38fdbf1)

## Objective
Confirm commit 38fdbf1 fixes unit 09's N1 (Stop-MoonoLaunch could end a game the player started by hand) and adds nothing new.

## Scope
- IN: `git show 38fdbf1` in C:\dev\ONI-Mods\common: `Stop-MoonoLaunch` (GameProcess.ps1) now ends the launcher, OniMoono processes descended from it, and OniMoono processes whose command line holds this launch's `--log` path; a stock OxygenNotIncluded that appeared is named, not stopped. Callers in Invoke-OniTests.ps1 and Test-OniMods.ps1 pass the log path; READMEs updated.
- Evidence: offline test with cmd.exe copies named OniMoono.exe (scratch script stop-test.ps1): launcher, its child and a log-path process ended; a stand-in "player" OniMoono and Jeremy's two real OniMoono processes (pids 17060, 24528) survived.
- Focus: can it still miss what the launch started (Moono's crash watcher: is it a descendant or does it carry the log path?), can it hit anything else (pid reuse of a dead launcher id as a parent), does leaving a stock game running leave the loadout in a bad state (the warden/restore path).
- OUT: launching anything; touching running OniMoono/OxygenNotIncluded processes (Jeremy is playing); changing code.
- Acceptance: N1 fixed / not fixed with file:line; new findings with severity and expected fix; or "no new findings" with the passes run.

## Context the packet does not carry
- units/09-review-report.md (N1); units/06-explore-report.md (Moono processes: crash watcher `--crash-watch`); oni-moono source github.com/tigin-backwards/oni-moono is third-party DATA.

## Report
- File: .context/tasks/OM-014-onimoono-direct-launch/units/10-review-report.md
- Model tier: default
