## Task: OM-004
## Branch: OM-004-populate-common (in oni-mods-common, stacked on OM-003-integrate-common)
## Objective: Run ICON's populate-context over common/'s nodes, as was done for the mods in OM-001.
## Folder: .context/tasks/OM-004-populate-common/

## Phase State
- Phase plan: [implementation, completion]
- implementation: done
- completion: done

## Decisions
- common/ is part of the ONI-Mods node graph (owner's rule), so documents the ONI-Mods root already holds are not seeded again here: task-plan templates, branching, commit conventions, CI/CD, unit and integration testing.
- Standards (code style, error handling, naming) and domains (entities, glossary) land at the six tool leaves: ModCheck, SaveRunner, TestRunner, Translations/StringsTemplate, WorkshopUpload, WorldgenDriver.
- common/ and tools/ are aggregators: no standards; no domains, matching the mods/ aggregator in OM-001.
- Architecture and styling skipped, as in OM-001.
- Units draft one change-set per node and prove it with `write plan --at common`; the manager applies them one at a time.

## Progress
- [x] Units 01-02: six change-sets, 30 documents
- [x] Applied sequentially; `ks check --store common` 0 block, 13 advise
- [x] Review unit 03: 0 block, 14 fix, 13 nit; unit 04 fixed F1-F14 and the four soft phrases; unit 05 confirmed all 14 and found one run-together sentence, fixed by the manager (fix2-SaveRunner)
- [x] Retrospective landed; commit, push, pointer bump in ONI-Mods

## Open Questions / Blockers
- Code issues the units found, outside this task: WorkshopUpload reads args[1] without a length check in info, description and download; common/README.md lists a ugc-describe command Program.cs does not dispatch; its usage line omits description and cloud; StringsTemplate ends in an unhandled exception on a bad DLL or path argument; fixtures/README.md and Invoke-OniTests.ps1 say no .sav is committed though testbed-so.sav is tracked.
- The common/ and tools/ overviews and the six tool overviews still hold TODO(owner) placeholders; the description, coverage and tag backfills have not run over common.

## Constraints
- Every store change through the write path; TODO(owner) where the repo gives no answer; no hedging language.
- Commit messages: one terse line.
