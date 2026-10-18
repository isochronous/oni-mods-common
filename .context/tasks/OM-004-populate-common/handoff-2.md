# Handoff — OM-004-populate-common

**Scope:** `common/tools/Translations/StringsTemplate`, `common/tools/WorkshopUpload`, `common/tools/WorldgenDriver`
**Store:** `C:/dev/ONI-Mods/common`
**Task scratch:** `C:/dev/ONI-Mods/common/.context/tasks/OM-004-populate-common/` — tracked, write-path exempt, never authoritative ([IF-1.C]).

You are reading this because a coordinator passed you its PATH. It is a file, it stays a file, and you can
re-read it after anything truncates your context. Nothing below was passed to you as a payload.

## 1. Task spec

Source: `common/.context/tasks/OM-004-populate-common/units/02-implement.md`

```
# Unit 02 — implement: populate-context change-sets for `tools/Translations/StringsTemplate`, `tools/WorkshopUpload`, `tools/WorldgenDriver`

## Objective
Draft one change-set per listed node creating five documents adapted from the ICON templates: `standards/code-style.md`, `standards/error-handling.md`, `standards/naming-conventions.md` (templates under `standards/`), and `domains/entities.md`, `domains/glossary.md` (templates under `domains/`).

## Scope
- IN: nodes `tools/Translations/StringsTemplate`, `tools/WorkshopUpload`, `tools/WorldgenDriver` in the common store (`C:/dev/ONI-Mods/common/<node>`).
- entities.md: the tool's real things (entry points, commands and options, classes, files it reads and writes, game hooks if any), each with its file location and relationships; rules the tool enforces; key code paths. Title it after the tool (e.g. "ModCheck domain"). Cut sections that do not apply.
- glossary.md: terms a newcomer to that tool needs.
- Standards: the conventions the tool's code actually follows (language/runtime, layout, logging and exit codes, error handling, naming), with file references.
- Each node already has `standards/README.md` and `domains/README.md`; do not touch them.
- OUT: other nodes; applying; committing.
- Acceptance: three change-set files, five create ops each, each accepted by `write plan --at common` (output shown).

## Context the packet does not carry
- Read `C:/dev/ONI-Mods/common/.context/tasks/OM-004-populate-common/units/common-rules.md` first; it binds this unit.

## Report
- File: C:/dev/ONI-Mods/common/.context/tasks/OM-004-populate-common/units/02-implement-report.md
- Must contain: per node, one line on what the tool is and what went in; TODO(owner)s left; plan summaries; friction met.
- Model tier: default
```

## 2. What applies here

The next hop is `ks read`: read the pointer, not the file it sits in.

POINTERS, not contents — ids, one line each, and why each matched. Open what the task needs; the budget was
spent selecting these so yours can be spent reading them.

Nothing matched this scope. That is the store's answer, not a failure to look: every candidate class was
collected and each found nothing.

tags: this brief answered the path question only. A record about a subject rather than a place is reached by `--tag <term>`; the terms are listed by: ks tags --store C:/dev/ONI-Mods/common

⚠ 9 record(s) in these 3 nodes declare no coverage and no tags and name no source path, so a brief reaches them only when a target or a matched record cites them: tools/Translations/StringsTemplate:domains/README.md, tools/Translations/StringsTemplate:overview.md, tools/Translations/StringsTemplate:standards/README.md, tools/WorkshopUpload:domains/README.md, tools/WorkshopUpload:overview.md and 4 more. Give them `applies_to` or tags through the write path; `--json` lists them all.

## 3. Checkpoint contract

**No checkpoints.** The coordinator sized this work at zero deliberately: this unit owes no progress file, because its only deliverable is its report. Zero is a number that was chosen, not a flag that was forgotten.

Write no checkpoint file, and do not invent one under another name — a file this contract does not name is scratch that nothing will read.

Do the work to its end and report. If you cannot finish, your report is what survives this lane: say in it what is done, what is not, and what you would do next.

## 4. Read-beyond licence

This is a strong default, not a cage — read beyond it whenever the task needs it.
