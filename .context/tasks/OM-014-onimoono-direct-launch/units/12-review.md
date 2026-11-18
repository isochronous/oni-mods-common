# Unit 12 — review: stray stock game handed to the warden (039e519)

## Objective
Confirm commit 039e519 fixes unit 10's finding (a stock game left running after a failed Moono launch skipped the config/prefs restore) per Jeremy's decision, and adds nothing new.

## Scope
- IN: `git show 039e519` in C:\dev\ONI-Mods\common (Stop-MoonoLaunch `-RunMarker`, returns a stray pid; Invoke-OniTests starts a warden `-GamePid` and skips its own restore; ModTest waits for the stray pid before restoring).
- Decision being checked (Jeremy, 2026-11-12): stop a stock game only when it provably belongs to the launch (descends from the launcher/OniMoono processes or carries this run's `--onitest-` arguments); otherwise leave it running and let the warden restore once it exits; never kill a game the player opened.
- Focus: every restore (mods.json, config, prefs) still happens exactly once on every path; the warden's `-GamePid` path; ModTest's wait cannot hang forever unnoticed; stock legs unchanged.
- OUT: launching anything; touching running processes (in-game runs may be live while you review — do not interfere); changing code.
- Acceptance: finding fixed / not fixed with file:line; new findings with severity and expected fix; or "no new findings" with the passes run.

## Context the packet does not carry
- units/10-review-report.md, units/11-implement-report.md.

## Report
- File: .context/tasks/OM-014-onimoono-direct-launch/units/12-review-report.md
- Model tier: default
