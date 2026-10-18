## Task: OM-005
## Branch: OM-005-common-backfills (in oni-mods-common, stacked on OM-004-populate-common)
## Objective: Bring common/ from scaffold to described, covered and tagged, as OM-002 did for the root and the mods: write the placeholder overviews and README indexes, then run the description, coverage and tag backfills over common's nodes.
## Folder: .context/tasks/OM-005-common-backfills/

## Phase State
- Phase plan: [implementation, completion]
- implementation: in progress
- completion: pending

## Decisions
- Order as in OM-002: overviews and README indexes first, then descriptions, then coverage, then tags.
- Phase A covers 8 overviews (common, tools, six tool leaves) and the 12 domains/standards README indexes of the six leaves. Units draft change-sets; the manager plans and applies them with `--at common`.
- META.md placeholders stay, as in OM-002.
- Tags: one dictionary per node. common/ and tools/ hold one document each (overview), so they cannot meet the two-document floor and stay untagged, like mods/ in OM-002.
- The tools without a node (Kanim, MakePreview, ModTest, Release, translations.py) are named in the tools/ overview's folder list; giving them nodes is not part of this task.

## Key Files
- units/: specs and reports; changesets/: drafted change-sets; work/: survey, answers and journal files

## Progress
- [ ] Phase A: overviews and README indexes
- [ ] Phase B: descriptions
- [ ] Phase C: coverage
- [ ] Phase D: tags
- [ ] Review, fixes, retrospective, commit, push, pointer bump

## Open Questions / Blockers

## Constraints
- Every store change through the write path; TODO(owner) where the repo gives no answer; no hedging language.
- Commit messages: one terse line.
