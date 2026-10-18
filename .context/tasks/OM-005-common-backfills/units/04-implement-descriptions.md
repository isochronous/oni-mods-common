# Unit 04 — implement: author descriptions for 24 documents

## Objective
Write one description line per listed document for the description backfill (Phase B). The manager merges and applies.

## Scope
- IN: exactly these documents of the common store, each read in full from disk (paths are relative to `C:/dev/ONI-Mods/common`):
- `tools/Translations/StringsTemplate::domains/README.md`  (file: `tools/Translations/StringsTemplate/.context/domains/README.md`)
- `tools/Translations/StringsTemplate::domains/entities.md`  (file: `tools/Translations/StringsTemplate/.context/domains/entities.md`)
- `tools/Translations/StringsTemplate::domains/glossary.md`  (file: `tools/Translations/StringsTemplate/.context/domains/glossary.md`)
- `tools/Translations/StringsTemplate::overview.md`  (file: `tools/Translations/StringsTemplate/.context/overview.md`)
- `tools/Translations/StringsTemplate::standards/README.md`  (file: `tools/Translations/StringsTemplate/.context/standards/README.md`)
- `tools/Translations/StringsTemplate::standards/code-style.md`  (file: `tools/Translations/StringsTemplate/.context/standards/code-style.md`)
- `tools/Translations/StringsTemplate::standards/error-handling.md`  (file: `tools/Translations/StringsTemplate/.context/standards/error-handling.md`)
- `tools/Translations/StringsTemplate::standards/naming-conventions.md`  (file: `tools/Translations/StringsTemplate/.context/standards/naming-conventions.md`)
- `tools/WorkshopUpload::domains/README.md`  (file: `tools/WorkshopUpload/.context/domains/README.md`)
- `tools/WorkshopUpload::domains/entities.md`  (file: `tools/WorkshopUpload/.context/domains/entities.md`)
- `tools/WorkshopUpload::domains/glossary.md`  (file: `tools/WorkshopUpload/.context/domains/glossary.md`)
- `tools/WorkshopUpload::overview.md`  (file: `tools/WorkshopUpload/.context/overview.md`)
- `tools/WorkshopUpload::standards/README.md`  (file: `tools/WorkshopUpload/.context/standards/README.md`)
- `tools/WorkshopUpload::standards/code-style.md`  (file: `tools/WorkshopUpload/.context/standards/code-style.md`)
- `tools/WorkshopUpload::standards/error-handling.md`  (file: `tools/WorkshopUpload/.context/standards/error-handling.md`)
- `tools/WorkshopUpload::standards/naming-conventions.md`  (file: `tools/WorkshopUpload/.context/standards/naming-conventions.md`)
- `tools/WorldgenDriver::domains/README.md`  (file: `tools/WorldgenDriver/.context/domains/README.md`)
- `tools/WorldgenDriver::domains/entities.md`  (file: `tools/WorldgenDriver/.context/domains/entities.md`)
- `tools/WorldgenDriver::domains/glossary.md`  (file: `tools/WorldgenDriver/.context/domains/glossary.md`)
- `tools/WorldgenDriver::overview.md`  (file: `tools/WorldgenDriver/.context/overview.md`)
- `tools/WorldgenDriver::standards/README.md`  (file: `tools/WorldgenDriver/.context/standards/README.md`)
- `tools/WorldgenDriver::standards/code-style.md`  (file: `tools/WorldgenDriver/.context/standards/code-style.md`)
- `tools/WorldgenDriver::standards/error-handling.md`  (file: `tools/WorldgenDriver/.context/standards/error-handling.md`)
- `tools/WorldgenDriver::standards/naming-conventions.md`  (file: `tools/WorldgenDriver/.context/standards/naming-conventions.md`)
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
- `C:/dev/ONI-Mods/common/.context/tasks/OM-005-common-backfills/work/desc-answers-04.json`: one JSON object, keys exactly as listed above (`<node>::<path>`), values the description strings. Every listed key exactly once, no others. Write it with a script or the Write tool, then validate it parses and has 24 keys.

## Report
- File: C:/dev/ONI-Mods/common/.context/tasks/OM-005-common-backfills/units/04-implement-descriptions-report.md
- Must contain: the key count check output, any null with its reason, anything in these rules you found to be wrong, friction met.
- Model tier: default
