# Unit 09 — review: the fixes after unit 07 (f4656d4^..4c7fad1, i.e. c8d11ae and 4c7fad1)

## Objective
Confirm the commits after the unit 07 review fix every unit 07 finding (M1, M2, m1–m5) and add nothing new, so the review checkpoint covers the final diff.

## Scope
- IN: `git diff 5c65587..4c7fad1 -- tools` in C:\dev\ONI-Mods\common (c8d11ae: `Get-MoonoExitCode`; 4c7fad1: crash watcher excluded from `Get-GameProcess -Any`, `Stop-MoonoLaunch` before the restore, failed Moono leg recorded per leg in Both, `Copy-FreshLog` on the Moono reuse path, wording and cross-references).
- OUT: the rest of the diff (unit 07 covered it); no game or OniMoono launch — Jeremy has OniMoono open; do not touch his processes; change no code.
- Focus beyond the findings: `Stop-MoonoLaunch` must only ever stop processes this launch started (never a Moono Jeremy started by hand); `Get-GameProcess -Any` must still refuse a launch while Jeremy's own game is open.
- Acceptance: per finding fixed / not fixed with file:line; any new finding with severity and expected fix; or "no new findings" with the passes run.

## Context the packet does not carry
- units/07-review-report.md, units/08-implement-report.md.

## Report
- File: .context/tasks/OM-014-onimoono-direct-launch/units/09-review-report.md
- Model tier: default
