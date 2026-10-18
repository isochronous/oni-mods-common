# Unit 05 — research: propose the tag vocabularies for common's six tool nodes

## Objective
Propose the tag vocabulary for each of the six tool nodes of the common store: `tools/ModCheck`, `tools/SaveRunner`, `tools/TestRunner`, `tools/Translations/StringsTemplate`, `tools/WorkshopUpload`, `tools/WorldgenDriver` (8 documents each: overview, two README indexes, code-style, error-handling, naming-conventions, entities, glossary). Phase D, tag-backfill step 2. The manager checks it and mints the dictionaries; you write nothing to the store.

## Scope
- IN: `work/tags/lexicon-<tool>.json` for each tool (6 files; paths relative to `C:/dev/ONI-Mods/common/.context/tasks/OM-005-common-backfills/`) and the documents they list (under `C:/dev/ONI-Mods/common/<node>/.context/`).
- Read for spelling only: the ONI-Mods dictionaries `C:/dev/ONI-Mods/.context/tags.md` and `C:/dev/ONI-Mods/mods/*/.context/tags.md`.
- OUT: writing anything except your output and report; committing.

## Rules (ICON tag-backfill step 2; follow exactly)
- Propose from the lexicon files (each document's title, description and section titles), then extend from the bodies of a sample of the documents (read them from disk) for terms that live only in bodies.
- **Admission floor:** a term must plausibly gather at least 3 records (documents or sections) across at least 2 distinct documents of THE SAME NODE. Count it per node; state the estimate.
- **Bar** a term that would reach more than about 85% of that node's records: it selects nothing.
- **Bar a location rather than a subject.** A term whose honest meaning is "records about this directory" is a path, not a subject. A folder-like word is fine when it names a shared concern.
- **Bar shape terms:** a term that would fit any document of that shape about anything (e.g. "index", "overview", "examples") is not a subject.
- **Gloss:** exactly one sentence saying what a record must be about to earn the term, starting "A record earns this term when it is about". No boundary clause, no "not X", no pointer to another term, no list of sub-topics.
- Terms: lower-case, hyphenated noun phrases, no commas. Somebody starting work would ask for it by name.
- Read every gloss you have written before adding another: no plurals, short forms or near-synonyms of an existing term.
- Report anything in these rules you find to be wrong.
- Each node answers for itself and gets its own dictionary. But the tools share document shapes, and common is part of the ONI-Mods node graph, so **a concept that recurs must be spelled identically and glossed identically in every node where it is admitted**. Where an ONI-Mods dictionary already has a term for the concept (for example `harmony-patching`, `logging`, `code-style`, `in-game-testing`, `mod-entry-point`), reuse its spelling and its gloss word for word, adapted only where the gloss names "a mod" and the tool is not one. Build a shared core first, admit each core term per node only where that node meets the floor, then add each tool's own subject terms (its commands, game systems, files, mechanics) where they meet the floor in that node.
- Expect roughly 6–15 terms per node. Fewer real terms beat padded ones.

## Output
- `work/tags/vocab-tools.json`: `{"<node e.g. tools/ModCheck>": [{"term": "...", "gloss": "...", "est_records": N, "est_documents": N, "evidence": ["<record ids>"]}], ...}` with all six nodes as keys. Write with a script or the Write tool; validate it parses.

## Report
- File: C:/dev/ONI-Mods/common/.context/tasks/OM-005-common-backfills/units/05-research-vocab-report.md
- Must contain: term counts per node, which terms reuse an ONI-Mods spelling, the closest calls you dropped and why, anything in the rules you found wrong, friction met.
- Model tier: complex
