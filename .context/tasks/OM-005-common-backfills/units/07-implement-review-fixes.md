# Unit 07 — implement: change-sets for the unit 06 review findings

## Objective
Draft change-sets fixing the findings of `units/06-review-report.md` listed below. The manager applies them. Never run `write apply`, never commit.

## Scope
Read `C:/dev/ONI-Mods/common/.context/tasks/OM-005-common-backfills/units/06-review-report.md` in full first; finding ids below refer to it. All nodes are in the common store (`C:/dev/ONI-Mods/common`).

- IN, fix findings:
  - **1.1** Write the paths in `tools/.context/overview.md` ("What this is" and "Where things are") store-relative (`tools/Release/release.py`, `tools/Kanim/kanim.py`, ...), so source-mention reaches it.
  - **2.1** Rewrite the description of `tools::overview.md` so its folder list is true; keep the code names true and inside the node (store-relative, under `tools/`).
  - **3.1** Preferred option: mint `in-game-testing` in `tools/TestRunner/.context/tags.md` with the gloss from `C:/dev/ONI-Mods/.context/tags.md` word for word, in a change-set of its own (`fix07-mint-TestRunner.json`, nothing else in it). Then put each whole-tool term at document level on the same four documents of its node: overview.md, domains/README.md, domains/entities.md, domains/glossary.md — `in-game-testing` (TestRunner), `steam-workshop` (WorkshopUpload), `translations` (StringsTemplate), `generated-worlds` (WorldgenDriver), `game-update-compatibility` (ModCheck). Where a section beneath one of those documents carries the same term, remove it from the section in the same change-set.
  - **4.1** TestRunner: take `screenshots` and `player-log` off their carriers and remove both terms from its tags.md. For `mod-entry-point` in TestRunner: add it to the records it genuinely fits by its gloss (the entry-point class sections) if that brings it to at least 3 records across 2 documents; otherwise take it off its carrier and remove the term. Say which in the report.
  - **4.3** Remove every tag from the five "Where things are" file-map sections listed (blk-1d0e91d4e985, blk-3dee2f4998e2, blk-04261b4be94e, blk-6dcf139ddfa3, blk-ebff91fa90b7).
  - **4.4** StringsTemplate and WorkshopUpload `standards/error-handling.md`: document tags become `exception-handling` only; put `exit-codes` and (StringsTemplate) `console-output` on the sections they describe (the exit-code and report-keyword sections, and any other section they fit). Their `standards/README.md` document tags become `code-style`, `exception-handling`, as in the other four nodes.
  - **4.5** Move `translation-template` (StringsTemplate) and `legacy-workshop-item` (WorkshopUpload) from overview document level to the overview's "What this is" section.
  - **4.6** Covered by 3.1.
  - **4.7** Remove `mod-entry-point` from SaveRunner blk-545208d8763a.
- IN, nits: 1.2, 1.3, 1.4, 1.5, 1.6, 1.7, 1.8 (overview/README text, checked against the code), 2.2, 2.3, 2.4 (descriptions; 2.3 rewrites the six index descriptions to name each node's own topics), 4.8, 4.9, 4.10 (tags).
- OUT: 3.2 (the manager keeps the mod-strings gloss), 3.3, 4.2, 4.11; any other change; applying; committing.

## Rules
- Shapes: `node c:/dev/ai/icon/bin/ks.mjs write schema` and `--row narrative --mode section|describe`, `--row tags --mode tag|create|remove`. Tag ops address sections by block address as the schema says; a term removal from tags.md is a `remove` op on `tags.md` with the term's section and a reason.
- One change-set per node, `C:/dev/ONI-Mods/common/.context/tasks/OM-005-common-backfills/changesets/fix07-<name>.json` (`fix07-tools.json`, `fix07-ModCheck.json`, ...), plus `fix07-mint-TestRunner.json`. If a node needs tags removed from records before its term can be removed, put the term removal in a second file `fix07-rmterm-<name>.json`. Envelope `task: "OM-005"`, a `why`, `node` relative to `common/`.
- Read every document you change in full; every fact you write is checked against the code. Plain wording, no hedges; descriptions follow the description-backfill rules (what the file contains, one line, names the tool, true in-node code names).
- Build JSON with a script in a uniquely named scratchpad subfolder or the Write tool, never a heredoc.

## Verification you owe
From `C:/dev/ONI-Mods`: `node c:/dev/ai/icon/bin/ks.mjs write plan --changeset <file> --at common` for each file until accepted. The TestRunner change-set using `in-game-testing` cannot plan until the mint is applied: plan it and report the refusal if that is the only reason, and say the order to apply.

## Report
- File: C:/dev/ONI-Mods/common/.context/tasks/OM-005-common-backfills/units/07-implement-review-fixes-report.md
- Must contain: per finding, what changed and where; the apply order; plan summaries; friction met.
- Model tier: default
