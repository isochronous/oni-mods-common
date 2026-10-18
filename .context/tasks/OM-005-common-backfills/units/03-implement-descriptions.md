# Unit 03 — implement: author descriptions for 26 documents

## Objective
Write one description line per listed document for the description backfill (Phase B). The manager merges and applies.

## Scope
- IN: exactly these documents of the common store, each read in full from disk (paths are relative to `C:/dev/ONI-Mods/common`):
- `.::overview.md`  (file: `.context/overview.md`)
- `tools::overview.md`  (file: `tools/.context/overview.md`)
- `tools/ModCheck::domains/README.md`  (file: `tools/ModCheck/.context/domains/README.md`)
- `tools/ModCheck::domains/entities.md`  (file: `tools/ModCheck/.context/domains/entities.md`)
- `tools/ModCheck::domains/glossary.md`  (file: `tools/ModCheck/.context/domains/glossary.md`)
- `tools/ModCheck::overview.md`  (file: `tools/ModCheck/.context/overview.md`)
- `tools/ModCheck::standards/README.md`  (file: `tools/ModCheck/.context/standards/README.md`)
- `tools/ModCheck::standards/code-style.md`  (file: `tools/ModCheck/.context/standards/code-style.md`)
- `tools/ModCheck::standards/error-handling.md`  (file: `tools/ModCheck/.context/standards/error-handling.md`)
- `tools/ModCheck::standards/naming-conventions.md`  (file: `tools/ModCheck/.context/standards/naming-conventions.md`)
- `tools/SaveRunner::domains/README.md`  (file: `tools/SaveRunner/.context/domains/README.md`)
- `tools/SaveRunner::domains/entities.md`  (file: `tools/SaveRunner/.context/domains/entities.md`)
- `tools/SaveRunner::domains/glossary.md`  (file: `tools/SaveRunner/.context/domains/glossary.md`)
- `tools/SaveRunner::overview.md`  (file: `tools/SaveRunner/.context/overview.md`)
- `tools/SaveRunner::standards/README.md`  (file: `tools/SaveRunner/.context/standards/README.md`)
- `tools/SaveRunner::standards/code-style.md`  (file: `tools/SaveRunner/.context/standards/code-style.md`)
- `tools/SaveRunner::standards/error-handling.md`  (file: `tools/SaveRunner/.context/standards/error-handling.md`)
- `tools/SaveRunner::standards/naming-conventions.md`  (file: `tools/SaveRunner/.context/standards/naming-conventions.md`)
- `tools/TestRunner::domains/README.md`  (file: `tools/TestRunner/.context/domains/README.md`)
- `tools/TestRunner::domains/entities.md`  (file: `tools/TestRunner/.context/domains/entities.md`)
- `tools/TestRunner::domains/glossary.md`  (file: `tools/TestRunner/.context/domains/glossary.md`)
- `tools/TestRunner::overview.md`  (file: `tools/TestRunner/.context/overview.md`)
- `tools/TestRunner::standards/README.md`  (file: `tools/TestRunner/.context/standards/README.md`)
- `tools/TestRunner::standards/code-style.md`  (file: `tools/TestRunner/.context/standards/code-style.md`)
- `tools/TestRunner::standards/error-handling.md`  (file: `tools/TestRunner/.context/standards/error-handling.md`)
- `tools/TestRunner::standards/naming-conventions.md`  (file: `tools/TestRunner/.context/standards/naming-conventions.md`)
- OUT: any other document; writing the store; committing.

## Rules (from the ICON description-backfill skill; follow them exactly)
- A description says **what the file contains**, not what it says in compressed form and not why it matters. Read the whole document; name what a reader finds in it (e.g. "The commands, options, Steam call results and files of WorkshopUpload, with the rules it enforces and where each lives in Program.cs").
- Never lift or paraphrase the document's first paragraph; never describe from the headings alone.
- One line, no line break, no trailing period required; aim for 15–35 words.
- Name the tool in every tool document's description: the six tools share document shapes, so a description that does not name its tool is indistinguishable.
- Code names in a description (paths, file names, identifiers) must be true and must sit inside the document's own node: a later coverage pass reads them to decide which directory a document is about, and a code name outside the node, or one that does not exist, stops it. Name a file only where the document is about that code.
- Plain wording, no hedges.
- If a document genuinely cannot be described, give the value `null` and say why in the report; that stops the whole run, so use it only when true.

## Output
- `C:/dev/ONI-Mods/common/.context/tasks/OM-005-common-backfills/work/desc-answers-03.json`: one JSON object, keys exactly as listed above (`<node>::<path>`), values the description strings. Every listed key exactly once, no others. Write it with a script or the Write tool, then validate it parses and has 26 keys.

## Report
- File: C:/dev/ONI-Mods/common/.context/tasks/OM-005-common-backfills/units/03-implement-descriptions-report.md
- Must contain: the key count check output, any null with its reason, anything in these rules you found to be wrong, friction met.
- Model tier: default
