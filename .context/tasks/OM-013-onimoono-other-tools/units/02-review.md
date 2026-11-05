Feature: SaveRunner, WorldgenDriver and ModTest follow a game started through oni-moono, reusing TestRunner's Get-GameProcess (new -Marker parameter)
Ticket: OM-013
Lens: review (read-only; write only your report). Do not launch the game or start game-named processes.
Diff: in C:\dev\ONI-Mods\common, `git diff 1d45792..326871b -- tools/`
Context: common/.context/tasks/OM-013-onimoono-other-tools/plan.md and units/01-implement-report.md; OM-012's review for the established rules: common/.context/tasks/OM-012-onimoono-harness/units/02-review-report.md.
Review focus:
  - TestRunner's behaviour is identical (default marker; every existing call).
  - Each tool's marker matches the arguments it really passes (read where each builds its launch command).
  - Every game lookup in each tool (and any warden/helper it starts) uses the helper; pre-launch refusals count any OniMoono; waits and exit-follow only the launched game; mods.json/config restore still happens after the right process exits on every path (success, timeout, kill, restart).
  - ModTest's start-time-based detection (no launch argument): races (a game starting just before the launch timestamp, clock resolution, a Moono parent/child pair, the restart path) and whether "refuse while any game is open" makes it safe enough.
  - The relative dot-source path to ..\TestRunner\GameProcess.ps1 from each tool: works from any working directory, and fails loudly if missing.
  - Nothing foregrounds or polls the game window.
Report: C:\dev\ONI-Mods\common\.context\tasks\OM-013-onimoono-other-tools\units\02-review-report.md — findings with severity and file:line, each with the fix expected; "no findings" only with the passes run. Reply with the path and a 3-line summary.
Model tier: default
