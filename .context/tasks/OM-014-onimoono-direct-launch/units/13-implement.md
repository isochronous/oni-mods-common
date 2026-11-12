# Unit 13 — implement: reused Moono session's runtime line, and unit 12's three findings

## Objective
Fix the last defects found in OM-014's own diff before the PR (Jeremy: every bug in the task's diff is fixed in the task).

## Defects
1. **Reused Moono session reports no game log.** In-game, 2026-11-12 12:25 (runs/om014-reuse2-20261112-122519-moono): "The running game took the run (same loadout); no restart." then "Runtime: Moono asked; no game log written by this launch" and "Moono's log: none written". The live game was started by run 1 with `--log runs/om014-reuse1-…/moono.log` and keeps writing there. On a reused Moono session, read the runtime from (and name) the live session's moono.log — e.g. record its path in the live dir when the session is launched and read it back on reuse. Copy or point to it as fits the run folder conventions; say what you chose.
2. units/12-review-report.md, all three low findings: ModTest's wait for a stray stock game has no timeout; the warden follows only the first stray; an empty `$stockBefore` only defers the restore. Fix each (or show from the code why one is not a defect, with file:line).

## Scope
- IN: tools/TestRunner/Invoke-OniTests.ps1, tools/TestRunner/GameProcess.ps1, tools/TestRunner/Watch-OniGame.ps1, tools/ModTest/Test-OniMods.ps1, READMEs, in C:\dev\ONI-Mods\common (branch OM-014-onimoono-direct-launch).
- OUT: stock behaviour unchanged (a reused stock session must still read its fresh Player.log); no game or OniMoono launch; never touch running processes; tests on scratch copies only, never Jeremy's real mods.json/prefs/config.
- Acceptance: scripts parse with 0 errors; `-BuildOnly -Mods MotionSensorRange -Runtime Moono` exits 0; offline checks for each fix (for 1, exercise the reuse summary with a fake live dir pointing at an existing moono.log such as runs/om014-reuse1-20261112-122449-moono/moono.log). Your report must include actual command output.
- Do not create git commits until all edits are done (each commit changes the SHA stamped into TestRunner.dll); then one commit.

## Context the packet does not carry
- units/11-implement-report.md, units/12-review-report.md.

## Report
- File: .context/tasks/OM-014-onimoono-direct-launch/units/13-implement-report.md
- Must contain: each defect → fix (file:line), offline checks with output, what remains for an in-game run, commit hash.
- Model tier: default
