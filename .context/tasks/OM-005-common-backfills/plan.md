## Task: OM-005
## Branch: OM-005-common-backfills (in oni-mods-common, stacked on OM-004-populate-common)
## Objective: Bring common/ from scaffold to described, covered and tagged, as OM-002 did for the root and the mods: write the placeholder overviews and README indexes, then run the description, coverage and tag backfills over common's nodes.
## Folder: .context/tasks/OM-005-common-backfills/

## Phase State
- Phase plan: [implementation, completion]
- implementation: done
- completion: done

## Decisions
- Order as in OM-002: overviews and README indexes first, then descriptions, then coverage, then tags.
- Phase A covers 8 overviews (common, tools, six tool leaves) and the 12 domains/standards README indexes of the six leaves. Units draft change-sets; the manager plans and applies them with `--at common`.
- META.md placeholders stay, as in OM-002.
- Tags: one dictionary per node. common/ and tools/ hold one document each (overview), so they cannot meet the two-document floor and stay untagged, like mods/ in OM-002.
- Whole-tool terms (review 3.1): one rule for all six nodes. The term naming a tool's whole subject sits at document level on overview, domains/README, entities and glossary: in-game-testing (TestRunner, root gloss), steam-workshop, translations, generated-worlds, game-update-compatibility. SaveRunner has no such term. This reverses unit 05's drop of in-game-testing as a place: the root gloss names TestRunner, and a graph-wide `--tag in-game-testing` should reach it.
- mod-strings keeps its ONI-Mods gloss in StringsTemplate (review 3.2): changing it means changing 11 ONI-Mods mod dictionaries, and in StringsTemplate it and translations gather different records.
- Left as they are: two-carrier terms (review 4.2 and unit 08 N1: TestRunner crash-dialog; WorldgenDriver worldgen-progress, player-log, mod-entry-point; ModCheck and WorkshopUpload tool-build; SaveRunner mod-build; each fits both records). A term with one carrier is retired (player-log in SaveRunner, screenshots and player-log in TestRunner, mod-build in WorldgenDriver). The six tags.md dictionaries declare no coverage, as in OM-002 (unit 08 N2): a dictionary is read through `ks tags`, not a brief; token-family terms (3.3; reused root spellings stay identical); code-style on naming-conventions documents (4.11; the OM-002 precedent in all 17 mods).
- The tools without a node (Kanim, MakePreview, ModTest, Release, translations.py) are named in the tools/ overview's folder list; giving them nodes is not part of this task.

## Key Files
- units/: specs and reports; changesets/: drafted change-sets; work/: survey, answers and journal files

## Progress
- [x] Phase A: 8 change-sets applied (8 overviews, 12 README indexes), no TODO(owner) left; strict check 0 block, 1 advise (META.md aggregator); committed 0b0f3c2
- [x] Phase B: 50 descriptions authored by units 03-04, merged into work/desc-answers.json, applied (status complete, 0 declined); 50 distinct; strict check 0 block; committed f9771e2
- [x] Phase C: coverage surveys at all 8 nodes exit 0, nothing owed: 0 proposed, 18 no candidate (README indexes and glossaries: no resolved token; OM-004 documents already declare applies_to); nothing written (work/cov/)
- [x] Phase D vocabulary: unit 05 proposed 98 terms over six nodes (11/16/26/12/16/17), 24 reusing ONI-Mods spellings; checked against the floor, the 85% bar, location and shape bars; kept TestRunner's 26 (76 records, each term meets the floor); code-style and exception-handling glossed "the tool's" in the three non-mod tools. Minted, committed
- [x] Phase D tagging: 10 chunks tagged, all validated, no ancestor duplicates; 47 gaps triaged to no new term (whole-tool = the node itself, a place; shapes: glossary, index, links, file layout; formatting/comments/imports/naming subsections sit in one document per node; single-record concepts); converged without a gap round. Written in 10-17 rounds per node, all complete. player-log carried by nothing in SaveRunner, removed (rmterm-SaveRunner). Strict check 0 block, 1 advise. Coverage 93% of all sections (323/347), 94% with prose (297/315), 38/50 documents (work/tags/coverage.txt). Committed f1cb73e
- [x] Review unit 06: 0 block, 10 fix, 16 nit (units/06-review-report.md)
- [x] Fixes: unit 07 drafted fix07-* (14 files; b-files split describe and section rewrites from tag ops the engine will not compose) for every fix finding but 3.2, and nits 1.2-1.8, 2.2-2.4, 4.8-4.10; in-game-testing minted in TestRunner and the whole-tool terms put on the same four documents in five nodes; screenshots and player-log retired from TestRunner, mod-entry-point kept there at 3 carriers. Manager retired mod-build from WorldgenDriver (fix08, one carrier left). Strict check 0 block, 1 advise; coverage 97% on both denominators (337/347, 307/315), 45/50 documents. Committed 61e463f
- [x] Unit 08 confirmation review: all 26 in-scope findings resolved; two nits recorded as decisions (N1, N2)
- [x] Retrospective, commit, push, pointer bump in ONI-Mods

## Open Questions / Blockers

## Constraints
- Every store change through the write path; TODO(owner) where the repo gives no answer; no hedging language.
- Commit messages: one terse line.
