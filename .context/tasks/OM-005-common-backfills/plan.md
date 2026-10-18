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
- [x] Phase A: 8 change-sets applied (8 overviews, 12 README indexes), no TODO(owner) left; strict check 0 block, 1 advise (META.md aggregator); committed 0b0f3c2
- [x] Phase B: 50 descriptions authored by units 03-04, merged into work/desc-answers.json, applied (status complete, 0 declined); 50 distinct; strict check 0 block; committed f9771e2
- [x] Phase C: coverage surveys at all 8 nodes exit 0, nothing owed: 0 proposed, 18 no candidate (README indexes and glossaries: no resolved token; OM-004 documents already declare applies_to); nothing written (work/cov/)
- [x] Phase D vocabulary: unit 05 proposed 98 terms over six nodes (11/16/26/12/16/17), 24 reusing ONI-Mods spellings; checked against the floor, the 85% bar, location and shape bars; kept TestRunner's 26 (76 records, each term meets the floor); code-style and exception-handling glossed "the tool's" in the three non-mod tools. Minted, committed
- [ ] Phase D tagging: 10 chunks dispatched (passes opened as .partial)
- [ ] Review, fixes, retrospective, commit, push, pointer bump

## Open Questions / Blockers

## Constraints
- Every store change through the write path; TODO(owner) where the repo gives no answer; no hedging language.
- Commit messages: one terse line.
