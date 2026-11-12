# Unit 15 — review: b01ae19 (failure reason said once; stray-game wording)

## Objective
Confirm commit b01ae19 (manager edit after unit 14) is correct and complete, so the review checkpoint covers the final diff.

## Scope
- IN: `git show b01ae19` in C:\dev\ONI-Mods\common: Invoke-OniTests no longer warns at the catch; the summary prints the failure once (first line in the session line when no status, the rest after it; all lines as "FAILED:" when a status exists); "Moono's log: none written" not printed after a failure that already said so; plain wording in GameProcess.ps1 (Stop-MoonoLaunch stray message), Invoke-OniTests (stray text), Test-OniMods.ps1 (stray wait and 900 s warning: says mods.json holds the test loadout).
- Evidence: a Moono-only run with a stand-in OniMoono that exits 2 printed: "Restored mods.json." / "Runtime: Moono asked; no game log written by this launch" / "Leg failed before the game was found: OniMoono exited with code 2 before the game started." / "Moono wrote no log at …" / exit 2; mods.json unchanged.
- Focus: no failure path now prints nothing (e.g. a failure with a status file, a failure text of one line, the Both side-by-side footer); messages state facts plainly with no hedging.
- OUT: launching anything; touching processes; code changes; commits.
- Acceptance: correct / not, with file:line; findings with severity and expected fix; or "no findings" with the passes run.

## Report
- File: .context/tasks/OM-014-onimoono-direct-launch/units/15-review-report.md
- Model tier: default
