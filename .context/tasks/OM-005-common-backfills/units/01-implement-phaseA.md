# Unit 01 — implement: overviews and README indexes for `.`, `tools`, `tools/ModCheck`, `tools/SaveRunner`

## Objective
Draft one change-set per listed node replacing its scaffold placeholders.

## Scope
- IN, in the common store (`C:/dev/ONI-Mods/common/<node>/.context/`):
  - `.` (common root): `overview.md`. common is the shared repository (oni-mods-common) included in ONI-Mods as the `common/` submodule; read `common/README.md`, and list its real top-level folders and files.
  - `tools` (aggregator): `overview.md`. Its "Where things are" names every folder under `common/tools/`, including the ones with no `.context/` node (Kanim, MakePreview, ModTest, Release, Translations/translations.py), one line each from what the code says, and says which six folders are nodes.
  - `tools/ModCheck` and `tools/SaveRunner`: `overview.md`, `domains/README.md`, `standards/README.md`.
- OUT: other nodes and documents; applying; committing.
- Acceptance: four change-set files, each accepted by `write plan --at common` (output shown).

## Context the packet does not carry
- Read `C:/dev/ONI-Mods/common/.context/tasks/OM-005-common-backfills/units/common-rules.md` first; it binds this unit.

## Report
- File: C:/dev/ONI-Mods/common/.context/tasks/OM-005-common-backfills/units/01-implement-phaseA-report.md
- Must contain: per node, one line on what went in; TODO(owner)s left; plan summaries; friction met.
- Model tier: default
