# Unit 03 — implement: fix review findings F1–F4

## Objective
Fix the review findings in units/02-review-report.md before the PR: F1 and F2 (moderate), F3 and F4 (minor). F5 (store docs) is the manager's, out of scope.

## Scope
- IN: tools/SaveRunner/MemoryProbe.cs, Mod.cs, README.md (and MemorySnapshot.cs only if the same fail-safe gap is there) in the worktree C:\dev\ONI-Mods-wt\common-OM-016, branch OM-016-saverunner-memory-probe.
- OUT: everything else; no behaviour change for `fridges` or probe-less runs; do not launch the game; never touch C:\dev\ONI-Mods\common.
- Goal of F1/F2: the memory probe fails safe — an exception in a reading, a hook or Close is logged once (`[SaveRunner]` warning) and the probe stops or skips, never breaking mod loading, SaveRunner's OnLoad, or Runner.Finish writing the status file and quitting. Follow the node's error-handling standard (in the packet).
- Acceptance: build 0 errors and ModCheck passes; show the actual command output — do not claim success without showing evidence. The build deploys to mods\local\SaveRunner; that is expected.

## Context the packet does not carry
- Review report: units/02-review-report.md (findings with locations and expected fixes).
- Unit 01 report: units/01-implement-report.md (build and ModCheck commands).

## Report
- File: .context/tasks/OM-016-saverunner-memory-probe/units/03-implement-report.md
- Must contain: each finding and how it was fixed (file:line), build + ModCheck output, commit hash.
- Model tier: default
