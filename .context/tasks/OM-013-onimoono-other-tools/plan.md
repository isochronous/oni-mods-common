## Task: OM-013
## Branch: OM-013-onimoono-other-tools (in common/, stacked on OM-012-onimoono-harness, which adds tools/TestRunner/GameProcess.ps1)
## Objective: SaveRunner, WorldgenDriver and ModTest find the game they launched when it runs through oni-moono (process `OniMoono`), the way TestRunner does after OM-012. Today they look only for `OxygenNotIncluded`, so on Jeremy's Moono launch options they report the game as not started, or stop watching it while it runs.
## Folder: common/.context/tasks/OM-013-onimoono-other-tools/

## Phase State
- **Phase plan**: implementation → testing → completion
- **Completed**: (none)
- **Current**: implementation   (status: in-progress)
- **Next**: testing
- **Loaded skill**: task-plan-phase-implementation
- **Branch**: OM-013-onimoono-other-tools
- **Attempts (current phase)**: 1

## Decisions
- Same rule as OM-012 (Jeremy, 2026-11-04): the launched game is `OxygenNotIncluded`, or `OniMoono` whose command line carries the tool's own launch arguments; any open `OniMoono` blocks a launch the way an open game does. Jeremy asked for the other common tools to be updated 2026-11-04.
- Reuse TestRunner's `GameProcess.ps1` rather than copy it, if each tool can reach it without a fragile path; otherwise the unit proposes where a shared copy should live.

## Key Files
- common/tools/SaveRunner/Invoke-SaveRun.ps1
- common/tools/WorldgenDriver/Invoke-Worldgen.ps1
- common/tools/ModTest/Test-OniMods.ps1
- common/tools/TestRunner/GameProcess.ps1 (from OM-012)
- Also name OxygenNotIncluded but are likely paths, not process lookups: tools/Kanim/kanim.py, tools/Translations/translations.py (to confirm)

## Phase Handoff Log

## Progress
- [ ] Implement in the three tools ← IN PROGRESS
- [ ] Review
- [ ] In-game check through Moono — ask Jeremy (one run per tool, narrowest)
- [ ] Store: each tool's code-style/README records; parent note "Launching with oni-moono" drops the caveat
- [ ] Completion: retro, commit, push onto PR #8's branch (one PR per repo), parent pointer bump onto PR #15's branch

## Open Questions / Blockers
- What launch arguments each tool passes (needed for the command-line filter).

## Constraints
- Never foreground or poll the game window. Never edit Steam settings.
- Commit inside common first, then bump the pointer in the parent.
