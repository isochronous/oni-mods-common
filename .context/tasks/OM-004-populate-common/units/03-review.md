# Unit 03 — review: the 30 documents OM-004 landed in common/

## Objective
Review `tools/*/.context/standards/{code-style,error-handling,naming-conventions}.md` and `tools/*/.context/domains/{entities,glossary}.md` (six tools: ModCheck, SaveRunner, TestRunner, Translations/StringsTemplate, WorkshopUpload, WorldgenDriver, under C:/dev/ONI-Mods/common) for accuracy against the tools' code.

## Passes (name each in the report)
1. Every file path cited exists (script). 2. Concrete claims (classes, methods, commands, options, exit codes, log formats) match the code, checked for every tool. 3. Invented claims that should be TODO(owner). 4. Template leftovers: placeholders, "Template" in titles, web examples. 5. Hedges, disclaimers. 6. `applies_to` globs match real files.

## Scope
- Read only. OUT: fixing; writing the store; committing.

## Report
- File: C:/dev/ONI-Mods/common/.context/tasks/OM-004-populate-common/units/03-review-report.md
- Must contain: findings with severity (block / fix / nit), document and heading, expected fix; the passes run and their coverage.
- Model tier: default
