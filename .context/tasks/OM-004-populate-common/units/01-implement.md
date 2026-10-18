# Unit 01 — implement: populate-context change-sets for `tools/ModCheck`, `tools/SaveRunner`, `tools/TestRunner`

## Objective
Draft one change-set per listed node creating five documents adapted from the ICON templates: `standards/code-style.md`, `standards/error-handling.md`, `standards/naming-conventions.md` (templates under `standards/`), and `domains/entities.md`, `domains/glossary.md` (templates under `domains/`).

## Scope
- IN: nodes `tools/ModCheck`, `tools/SaveRunner`, `tools/TestRunner` in the common store (`C:/dev/ONI-Mods/common/<node>`).
- entities.md: the tool's real things (entry points, commands and options, classes, files it reads and writes, game hooks if any), each with its file location and relationships; rules the tool enforces; key code paths. Title it after the tool (e.g. "ModCheck domain"). Cut sections that do not apply.
- glossary.md: terms a newcomer to that tool needs.
- Standards: the conventions the tool's code actually follows (language/runtime, layout, logging and exit codes, error handling, naming), with file references.
- Each node already has `standards/README.md` and `domains/README.md`; do not touch them.
- OUT: other nodes; applying; committing.
- Acceptance: three change-set files, five create ops each, each accepted by `write plan --at common` (output shown).

## Context the packet does not carry
- Read `C:/dev/ONI-Mods/common/.context/tasks/OM-004-populate-common/units/common-rules.md` first; it binds this unit.

## Report
- File: C:/dev/ONI-Mods/common/.context/tasks/OM-004-populate-common/units/01-implement-report.md
- Must contain: per node, one line on what the tool is and what went in; TODO(owner)s left; plan summaries; friction met.
- Model tier: default
