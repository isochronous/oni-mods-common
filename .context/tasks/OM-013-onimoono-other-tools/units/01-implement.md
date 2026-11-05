Task: OM-013 — SaveRunner, WorldgenDriver and ModTest follow a game started through oni-moono
Lens: implement. Repo: C:\dev\ONI-Mods\common, branch OM-013-onimoono-other-tools (has OM-012's tools/TestRunner/GameProcess.ps1). Do not commit. Do not launch the game. Do not touch Steam settings.

Read: common/.context/tasks/OM-013-onimoono-other-tools/plan.md; OM-012's change as the pattern: `git diff 5008da9..1d45792 -- tools/TestRunner` and common/.context/tasks/OM-012-onimoono-harness/units/01-implement-report.md.

Do:
1. In tools/SaveRunner/Invoke-SaveRun.ps1, tools/WorldgenDriver/Invoke-Worldgen.ps1 and tools/ModTest/Test-OniMods.ps1 (and any helper they dot-source or warden they start), find every place the game is found, waited for, killed or checked by process name or window. Replace each with the OM-012 rule: the launched game is `OxygenNotIncluded`, or `OniMoono` whose command line carries that tool's own launch arguments (find what each passes); pre-launch "game already open" refusals count any `OniMoono`.
2. Share the code: prefer making TestRunner's Get-GameProcess usable by all tools (e.g. a parameter for the argument marker, keeping its current default and behaviour for TestRunner) and dot-sourcing it by a path relative to $PSScriptRoot. If a shared location under tools/ is cleaner (moving GameProcess.ps1 and updating TestRunner's dot-source), say so and do it only if it stays a small diff; TestRunner's behaviour must stay identical.
3. Confirm tools/Kanim/kanim.py and tools/Translations/translations.py only use OxygenNotIncluded as a path (no change), and say so.
4. Update each tool's README where it describes launching, briefly, matching its style.
5. Verify without the game: parse check of every changed .ps1; stand-in checks like OM-012's (renamed cmd.exe copies with and without each tool's arguments) for the shared helper's new parameter; TestRunner's earlier stand-in cases still pass.

Report: C:\dev\ONI-Mods\common\.context\tasks\OM-013-onimoono-other-tools\units\01-implement-report.md — every call site changed (file:line), each tool's argument marker, verification output, which in-game check would prove each tool (narrowest invocation), and text for each tool's code-style.md if its Layout/Indentation needs it. Reply with the report path and a 3-line summary.
Model tier: default
