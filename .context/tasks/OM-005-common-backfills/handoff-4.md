# Handoff — OM-005-common-backfills

**Scope:** `tools/Translations/StringsTemplate`, `tools/WorkshopUpload`, `tools/WorldgenDriver`
**Store:** `C:/dev/ONI-Mods/common`
**Task scratch:** `C:/dev/ONI-Mods/common/.context/tasks/OM-005-common-backfills/` — tracked, write-path exempt, never authoritative ([IF-1.C]).

You are reading this because a coordinator passed you its PATH. It is a file, it stays a file, and you can
re-read it after anything truncates your context. Nothing below was passed to you as a payload.

## 1. Task spec

Source: `.context/tasks/OM-005-common-backfills/units/04-implement-descriptions.md`

```
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
```

## 2. What applies here

The next hop is `ks read`: read the pointer, not the file it sits in.

POINTERS, not contents — ids, one line each, and why each matched. Open what the task needs; the budget was
spent selecting these so yours can be spent reading them.

blk-77fa8ff1839f · reference
  Overview
  tools/Translations/StringsTemplate/.context/domains/entities.md#overview · names tools/Translations/StringsTemplate/Program.cs → tools/Translations/StringsTemplate
blk-b1319f3547fd · reference
  Entry point. Validates the three arguments, installs the `AssemblyResolve` handler, loads the D…
  tools/Translations/StringsTemplate/.context/domains/entities.md#key-entities/program-main · names tools/Translations/StringsTemplate/Program.cs → tools/Translations/StringsTemplate
tools/Translations/StringsTemplate:standards/code-style.md · reference
  StringsTemplate (the translation template writer and checker): tabs, Allman braces, C# 9 on .NE…
  tools/Translations/StringsTemplate/.context/standards/code-style.md · *.cs, *.csproj → tools/Translations/StringsTemplate · names tools/Translations/StringsTemplate/Program.cs → tools/Translations/StringsTemplate · linked from standards/error-handling.md · linked from standards/naming-conventions.md
tools/Translations/StringsTemplate:domains/entities.md · reference
  StringsTemplate (the translation template writer and checker): Program.Main and its helpers, th…
  tools/Translations/StringsTemplate/.context/domains/entities.md · *.cs, *.csproj → tools/Translations/StringsTemplate · linked from standards/error-handling.md
tools/Translations/StringsTemplate:standards/error-handling.md · reference
  StringsTemplate (the translation template writer and checker): exit codes 0/1/2, the FAIL, MISS…
  tools/Translations/StringsTemplate/.context/standards/error-handling.md · *.cs, *.csproj → tools/Translations/StringsTemplate · linked from standards/code-style.md
tools/Translations/StringsTemplate:standards/naming-conventions.md · reference
  StringsTemplate (the translation template writer and checker): file, method, variable and trans…
  tools/Translations/StringsTemplate/.context/standards/naming-conventions.md · *.cs, *.csproj → tools/Translations/StringsTemplate · linked from domains/entities.md · linked from standards/code-style.md
tools/Translations/StringsTemplate:domains/glossary.md · reference
  StringsTemplate (the translation template writer and checker): terms a newcomer needs: ModStrin…
  tools/Translations/StringsTemplate/.context/domains/glossary.md · linked from domains/entities.md · linked from standards/naming-conventions.md
blk-c95b82024244 · reference
  Entry point. Prints usage (return 2) when there are no arguments, calls `SteamAPI.Init`, prints…
  tools/WorkshopUpload/.context/domains/entities.md#key-entities/program-main · names tools/WorkshopUpload/Program.cs → tools/WorkshopUpload
blk-deeb9ba55606 · reference
  Overview
  tools/WorkshopUpload/.context/domains/entities.md#overview · names tools/WorkshopUpload/Program.cs → tools/WorkshopUpload
tools/WorkshopUpload:standards/code-style.md · reference
  WorkshopUpload (the legacy Steam Workshop publisher): tabs, Allman braces, .NET 8 with Steamwor…
  tools/WorkshopUpload/.context/standards/code-style.md · *.cs, *.csproj → tools/WorkshopUpload · names tools/WorkshopUpload/Program.cs → tools/WorkshopUpload · linked from standards/error-handling.md · linked from standards/naming-conventions.md
blk-cb417a1e3041 · reference
  Static session state and the result writer: the coordinate list and the current index, the time…
  tools/WorldgenDriver/.context/domains/entities.md#key-entities/driver · names tools/WorldgenDriver/Driver.cs → tools/WorldgenDriver
blk-2c4e50d4b9fc · reference
  The launcher. Groups coordinates by mode, runs them in sessions of up to `-PerSession` (default…
  tools/WorldgenDriver/.context/domains/entities.md#key-entities/invoke-worldgen-ps1 · names tools/WorldgenDriver/Invoke-Worldgen.ps1 → tools/WorldgenDriver
blk-1e7cd48f4614 · reference
  The `UserMod2` entry point. `OnLoad` reads the command line and applies the Harmony patches onl…
  tools/WorldgenDriver/.context/domains/entities.md#key-entities/worldgendrivermod · names tools/WorldgenDriver/Mod.cs → tools/WorldgenDriver
tools/WorkshopUpload:domains/entities.md · reference
  WorkshopUpload (the legacy Steam Workshop publisher): the seven commands, helpers, app ids, fil…
  tools/WorkshopUpload/.context/domains/entities.md · *.cs, *.csproj → tools/WorkshopUpload · linked from standards/error-handling.md
tools/WorkshopUpload:standards/error-handling.md · reference
  WorkshopUpload (the legacy Steam Workshop publisher): exit codes 0/1/2, Steam result checks, Aw…
  tools/WorkshopUpload/.context/standards/error-handling.md · *.cs, *.csproj → tools/WorkshopUpload · linked from standards/code-style.md
tools/WorkshopUpload:standards/naming-conventions.md · reference
  WorkshopUpload (the legacy Steam Workshop publisher): command, option, method, constant and Ste…
  tools/WorkshopUpload/.context/standards/naming-conventions.md · *.cs, *.csproj → tools/WorkshopUpload · linked from domains/entities.md · linked from standards/code-style.md
tools/WorldgenDriver:domains/entities.md · reference
  WorldgenDriver (the unattended worldgen mod and its PowerShell launcher): the mod class, Driver…
  tools/WorldgenDriver/.context/domains/entities.md · *.cs, *.ps1, *.csproj, *.yaml → tools/WorldgenDriver · linked from standards/error-handling.md
tools/WorldgenDriver:standards/code-style.md · reference
  WorldgenDriver (the unattended worldgen mod and its PowerShell launcher): C# 9 on net48 with ta…
  tools/WorldgenDriver/.context/standards/code-style.md · *.cs, *.ps1, *.csproj, *.yaml → tools/WorldgenDriver · linked from standards/error-handling.md · linked from standards/naming-conventions.md
tools/WorldgenDriver:standards/error-handling.md · reference
  WorldgenDriver (the unattended worldgen mod and its PowerShell launcher): the result statuses,…
  tools/WorldgenDriver/.context/standards/error-handling.md · *.cs, *.ps1, *.csproj, *.yaml → tools/WorldgenDriver · linked from standards/code-style.md
tools/WorldgenDriver:standards/naming-conventions.md · reference
  WorldgenDriver (the unattended worldgen mod and its PowerShell launcher): file, patch class, op…
  tools/WorldgenDriver/.context/standards/naming-conventions.md · *.cs, *.ps1, *.csproj, *.yaml → tools/WorldgenDriver · linked from domains/entities.md · linked from standards/code-style.md
tools/WorkshopUpload:domains/glossary.md · reference
  WorkshopUpload (the legacy Steam Workshop publisher): terms a newcomer needs: legacy item, uplo…
  tools/WorkshopUpload/.context/domains/glossary.md · linked from domains/entities.md · linked from standards/naming-conventions.md
tools/WorldgenDriver:domains/glossary.md · reference
  WorldgenDriver (the unattended worldgen mod and its PowerShell launcher): terms a newcomer need…
  tools/WorldgenDriver/.context/domains/glossary.md · linked from domains/entities.md · linked from standards/naming-conventions.md
tags: this brief answered the path question only. A record about a subject rather than a place is reached by `--tag <term>`; the terms are listed by: ks tags --store C:/dev/ONI-Mods/common

⚠ 10 record(s) in these 3 nodes declare no coverage and no tags and name no source path, so a brief reaches them only when a target or a matched record cites them: tools/Translations/StringsTemplate:domains/README.md, tools/Translations/StringsTemplate:domains/glossary.md, tools/Translations/StringsTemplate:standards/README.md, tools/WorkshopUpload:domains/README.md, tools/WorkshopUpload:domains/glossary.md and 5 more. Give them `applies_to` or tags through the write path; `--json` lists them all.

**Report back, don't write.** You have no write path here — what follows goes back to the coordinator in your report, not into `.context/`. Once you have opened what this work needed: did anything above turn out not to apply once you opened it (nothing above, if nothing was shown)? Did anything that actually governs this work fail to appear here at all — the miss only you can see? Did any pointer's stated one-line reason not match what its record actually says (nothing above, if nothing was shown)?

## 3. Checkpoint contract

**No checkpoints.** The coordinator sized this work at zero deliberately: this unit owes no progress file, because its only deliverable is its report. Zero is a number that was chosen, not a flag that was forgotten.

Write no checkpoint file, and do not invent one under another name — a file this contract does not name is scratch that nothing will read.

Do the work to its end and report. If you cannot finish, your report is what survives this lane: say in it what is done, what is not, and what you would do next.

## 4. Read-beyond licence

This is a strong default, not a cage — read beyond it whenever the task needs it.
