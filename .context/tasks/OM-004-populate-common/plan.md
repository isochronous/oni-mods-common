## Task: OM-004
## Branch: OM-004-populate-common (in oni-mods-common, stacked on OM-003-integrate-common)
## Objective: Run ICON's populate-context over common/'s nodes, as was done for the mods in OM-001.
## Folder: .context/tasks/OM-004-populate-common/

## Phase State
- Phase plan: [implementation, completion]
- implementation: in progress
- completion: pending

## Decisions
- common/ is part of the ONI-Mods node graph (owner's rule), so documents the ONI-Mods root already holds are not seeded again here: task-plan templates, branching, commit conventions, CI/CD, unit and integration testing.
- Standards (code style, error handling, naming) and domains (entities, glossary) land at the six tool leaves: ModCheck, SaveRunner, TestRunner, Translations/StringsTemplate, WorkshopUpload, WorldgenDriver.
- common/ and tools/ are aggregators: no standards; no domains, matching the mods/ aggregator in OM-001.
- Architecture and styling skipped, as in OM-001.
- Units draft one change-set per node and prove it with `write plan --at common`; the manager applies them one at a time.

## Progress
- [ ] Units 01-02 implement: change-sets for three tools each ← IN PROGRESS
- [ ] Apply sequentially; `ks check --store common`
- [ ] Review unit
- [ ] Retrospective, commit, push, pointer bump in ONI-Mods

## Open Questions / Blockers
- None yet.

## Constraints
- Every store change through the write path; TODO(owner) where the repo gives no answer; no hedging language.
- Commit messages: one terse line.
