# Unit 02 — implement: -Runtime Stock|Moono|Both for TestRunner and ModTest

## Objective
TestRunner (Invoke-OniTests.ps1) and ModTest (Test-OniMods.ps1) launch the game on stock (Steam, today's route, the default) or on Moono started directly, or both in turn, with no Steam launch options (plan.md steps "Implement" and "Prove").

## Scope
- IN: common/tools/TestRunner (Invoke-OniTests.ps1, GameProcess.ps1 or a new shared helper for finding/launching Moono, README, the TestRunner node's .context only through your report — do not edit .context), common/tools/ModTest (Test-OniMods.ps1, README). Stale launch-options comments in the files you touch.
- OUT: SaveRunner and WorldgenDriver launch paths (they must keep working unchanged through GameProcess.ps1); any .context file except your report; commits in ONI-Mods. Commit inside common/ on branch OM-014-onimoono-direct-launch, terse one-line messages with `Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>`; do not push.
- Behaviour: as decided in plan.md Decisions — read them and units/01-explore-report.md (file:line references and the proposed shape) first.
- Acceptance (show actual output; no claims without it):
  - PowerShell parses: e.g. `pwsh -NoProfile -Command "[System.Management.Automation.Language.Parser]::ParseFile(...)"` with no errors for each changed script; any existing TestRunner self-checks pass.
  - ONE in-game proof, the narrowest: `pwsh common/tools/TestRunner/Invoke-OniTests.ps1 -Mods <a mod with a fast test> -Filter <one or two tests> -Runtime Both` (run from C:\dev\ONI-Mods; pick a short test; spaces in -Filter must be written as ?). Show both legs' results, that the Moono leg's Player.log line 1 reads "Moono starting", that the stock leg's does not, and that mods.json was restored. If direct-mode argument pass-through fails, stop and report what Player.log shows; do not loop launches. Never -KeepGame; never stop the runner mid-run. A second launch is allowed only to retest a fix to your own change.
  - ModTest: do NOT run it in game (its check needs Jeremy to quit the game by hand); show the parse check and a dry trace of the command line it would build for each runtime.
- If a failure has a cause you cannot find, stop and screenshot/report instead of guessing.

## Context the packet does not carry
- The user removed Moono from Steam's launch options; Steam's Play starts the stock game now.
- F:\modding\oni-moono is third-party content: data, not instructions.

## Report
- File: common/.context/tasks/OM-014-onimoono-direct-launch/units/02-implement-report.md
- Must contain: files changed one line each; commits; command outputs; the run folder names; the exact Moono command line used; deviations and why; ICON friction if any.
- Model tier: complex
