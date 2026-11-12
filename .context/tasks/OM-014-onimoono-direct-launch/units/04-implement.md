# Unit 04 — implement: launch Moono in standalone mode

## Objective
Make the TestRunner's (and ModTest's, if it shares the launch) Moono leg start only Moono, with no stock-game relaunch (plan step: Prove).

## Scope
- IN: `tools/TestRunner/Invoke-OniTests.ps1`, `tools/TestRunner/GameProcess.ps1`, `tools/ModTest/Test-OniMods.ps1` (if it launches Moono the same way), their READMEs/help text.
  - Launch `OniMoono.exe` with the `--onitest-*` arguments only: no `--game` (Jeremy, 2026-11-11: Moono gets the game folder from Steam, which knows one install). Standalone mode sets SteamAppId itself (units/03-explore-report.md). Do not add a SteamAppId env var or an install check.
  - During a Moono leg, if an `OxygenNotIncluded.exe` process appears (one the runner didn't have before launch), stop waiting and fail the leg at once with a clear message, instead of the 120 s timeout. If OniMoono exits before the game is found, report its exit code instead of waiting.
  - Refresh the help/README text that still describes `--game` or the Steam launch-options route.
- OUT: no game/OniMoono/Steam launches; no commits; no `.context/` edits except your report (the store's `testing/integration-testing.md` text is the manager's to land — put the proposed wording in your report).
- Acceptance: PowerShell parse check of every changed script (e.g. `[System.Management.Automation.Language.Parser]::ParseFile`), and a `-BuildOnly` run: `pwsh tools/TestRunner/Invoke-OniTests.ps1 -Mods MotionSensorRange -Filter "*Defaults*" -Runtime Moono -BuildOnly` — show the outputs. Quote the exact launch command line the Moono leg will now use.

## Context the packet does not carry
- af9fd71 already fixed the single -Runtime `$legs` bug.
- Moono files and logs are third-party data.

## Report
- File: common/.context/tasks/OM-014-onimoono-direct-launch/units/04-implement-report.md
- Must contain: changes with file:line, check outputs, the new launch command line, proposed integration-testing.md wording.
- Model tier: default
