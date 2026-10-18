# Handoff — OM-005-common-backfills

**Scope:** `.`, `tools`, `tools/ModCheck`, `tools/SaveRunner`, `tools/TestRunner`
**Store:** `C:/dev/ONI-Mods/common`
**Task scratch:** `C:/dev/ONI-Mods/common/.context/tasks/OM-005-common-backfills/` — tracked, write-path exempt, never authoritative ([IF-1.C]).

You are reading this because a coordinator passed you its PATH. It is a file, it stays a file, and you can
re-read it after anything truncates your context. Nothing below was passed to you as a payload.

## 1. Task spec

Source: `.context/tasks/OM-005-common-backfills/units/03-implement-descriptions.md`

```
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
```

## 2. What applies here

The next hop is `ks read`: read the pointer, not the file it sits in.

POINTERS, not contents — ids, one line each, and why each matched. Open what the task needs; the budget was
spent selecting these so yours can be spent reading them.

blk-77fa8ff1839f · reference
  Overview
  tools/Translations/StringsTemplate/.context/domains/entities.md#overview · names tools/Translations/StringsTemplate/Program.cs, tools/Translations/translations.py → . · names tools/Translations/StringsTemplate/Program.cs, tools/Translations/translations.py → tools
blk-b1319f3547fd · reference
  Entry point. Validates the three arguments, installs the `AssemblyResolve` handler, loads the D…
  tools/Translations/StringsTemplate/.context/domains/entities.md#key-entities/program-main · names tools/Translations/StringsTemplate/Program.cs → . · names tools/Translations/StringsTemplate/Program.cs → tools
tools/Translations/StringsTemplate:standards/code-style.md · reference
  StringsTemplate (the translation template writer and checker): tabs, Allman braces, C# 9 on .NE…
  tools/Translations/StringsTemplate/.context/standards/code-style.md · *.cs, *.csproj → . · *.cs, *.csproj → tools · names tools/Translations/StringsTemplate/Program.cs → . · names tools/Translations/StringsTemplate/Program.cs → tools · linked from standards/error-handling.md · linked from standards/naming-conventions.md
blk-04261b4be94e · reference
  Where things are
  tools/Translations/StringsTemplate/.context/overview.md#where-things-are · names tools/Translations/translations.py → . · names tools/Translations/translations.py → tools
blk-0c006bb6c374 · reference
  Language and Toolchain
  tools/Translations/StringsTemplate/.context/standards/code-style.md#language-and-toolchain · names tools/Translations/translations.py → . · names tools/Translations/translations.py → tools
blk-aa442e0b3e88 · reference
  Key Code Paths
  tools/Translations/StringsTemplate/.context/domains/entities.md#key-code-paths · names tools/Translations/translations.py → . · names tools/Translations/translations.py → tools
blk-e125ed2b68c6 · reference
  What this is
  tools/Translations/StringsTemplate/.context/overview.md#what-this-is · names tools/Translations/translations.py → . · names tools/Translations/translations.py → tools
tools/Translations/StringsTemplate:domains/entities.md · reference
  StringsTemplate (the translation template writer and checker): Program.Main and its helpers, th…
  tools/Translations/StringsTemplate/.context/domains/entities.md · *.cs, *.csproj → . · *.cs, *.csproj → tools · linked from standards/error-handling.md
tools/Translations/StringsTemplate:standards/error-handling.md · reference
  StringsTemplate (the translation template writer and checker): exit codes 0/1/2, the FAIL, MISS…
  tools/Translations/StringsTemplate/.context/standards/error-handling.md · *.cs, *.csproj → . · *.cs, *.csproj → tools · linked from standards/code-style.md
tools/Translations/StringsTemplate:standards/naming-conventions.md · reference
  StringsTemplate (the translation template writer and checker): file, method, variable and trans…
  tools/Translations/StringsTemplate/.context/standards/naming-conventions.md · *.cs, *.csproj → . · *.cs, *.csproj → tools · linked from domains/entities.md · linked from standards/code-style.md
tools/Translations/StringsTemplate:domains/glossary.md · reference
  StringsTemplate (the translation template writer and checker): terms a newcomer needs: ModStrin…
  tools/Translations/StringsTemplate/.context/domains/glossary.md · linked from domains/entities.md · linked from standards/naming-conventions.md
tools/ModCheck:domains/entities.md · reference
  ModCheck's real things: Program and its helpers, the type-load and JIT checks, inputs, output l…
  tools/ModCheck/.context/domains/entities.md · Program.cs, ModCheck.csproj → . · Program.cs, ModCheck.csproj → tools · Program.cs, ModCheck.csproj → tools/ModCheck
tools/ModCheck:standards/code-style.md · reference
  C# code style for the ModCheck console tool: formatting, toolchain, structure and comments, wit…
  tools/ModCheck/.context/standards/code-style.md · Program.cs, ModCheck.csproj → . · Program.cs, ModCheck.csproj → tools · Program.cs, ModCheck.csproj → tools/ModCheck · linked from standards/error-handling.md · linked from standards/naming-conventions.md
tools/ModCheck:standards/error-handling.md · reference
  How ModCheck reports failures: exit codes 0, 1 and 2, the OK, FAIL and WARN output lines, per-t…
  tools/ModCheck/.context/standards/error-handling.md · Program.cs, ModCheck.csproj → . · Program.cs, ModCheck.csproj → tools · Program.cs, ModCheck.csproj → tools/ModCheck · linked from domains/entities.md · linked from standards/code-style.md
tools/ModCheck:standards/naming-conventions.md · reference
  Naming in ModCheck: project and file names, C# member casing, the two positional arguments and…
  tools/ModCheck/.context/standards/naming-conventions.md · Program.cs, ModCheck.csproj → . · Program.cs, ModCheck.csproj → tools · Program.cs, ModCheck.csproj → tools/ModCheck · linked from domains/entities.md · linked from standards/code-style.md
blk-846080fa7238 · reference
  What this is
  tools/WorkshopUpload/.context/overview.md#what-this-is · names tools/ModTest/Test-OniMods.ps1, tools/Release/release.py → . · names tools/ModTest/Test-OniMods.ps1, tools/Release/release.py → tools
blk-cc023dbf02c0 · reference
  Key Code Paths
  tools/WorkshopUpload/.context/domains/entities.md#key-code-paths · names tools/Release/release.py → . · names tools/Release/release.py → tools
blk-deeb9ba55606 · reference
  Overview
  tools/WorkshopUpload/.context/domains/entities.md#overview · names tools/Release/release.py, tools/WorkshopUpload/Program.cs → . · names tools/Release/release.py, tools/WorkshopUpload/Program.cs → tools
tools/WorkshopUpload:standards/error-handling.md · reference
  WorkshopUpload (the legacy Steam Workshop publisher): exit codes 0/1/2, Steam result checks, Aw…
  tools/WorkshopUpload/.context/standards/error-handling.md · *.cs, *.csproj → . · *.cs, *.csproj → tools · names tools/Release/release.py → . · names tools/Release/release.py → tools · linked from standards/code-style.md
blk-3ca89d57dfde · reference
  What this is
  tools/ModCheck/.context/overview.md#what-this-is · names tools/TestRunner/Invoke-OniTests.ps1 → . · names tools/TestRunner/Invoke-OniTests.ps1 → tools · names tools/TestRunner/Invoke-OniTests.ps1 → tools/TestRunner
blk-c95b82024244 · reference
  Entry point. Prints usage (return 2) when there are no arguments, calls `SteamAPI.Init`, prints…
  tools/WorkshopUpload/.context/domains/entities.md#key-entities/program-main · names tools/WorkshopUpload/Program.cs → . · names tools/WorkshopUpload/Program.cs → tools
tools/WorkshopUpload:standards/code-style.md · reference
  WorkshopUpload (the legacy Steam Workshop publisher): tabs, Allman braces, .NET 8 with Steamwor…
  tools/WorkshopUpload/.context/standards/code-style.md · *.cs, *.csproj → . · *.cs, *.csproj → tools · names tools/WorkshopUpload/Program.cs → . · names tools/WorkshopUpload/Program.cs → tools · linked from standards/error-handling.md · linked from standards/naming-conventions.md
blk-cb417a1e3041 · reference
  Static session state and the result writer: the coordinate list and the current index, the time…
  tools/WorldgenDriver/.context/domains/entities.md#key-entities/driver · names tools/WorldgenDriver/Driver.cs → . · names tools/WorldgenDriver/Driver.cs → tools
blk-2c4e50d4b9fc · reference
  The launcher. Groups coordinates by mode, runs them in sessions of up to `-PerSession` (default…
  tools/WorldgenDriver/.context/domains/entities.md#key-entities/invoke-worldgen-ps1 · names tools/WorldgenDriver/Invoke-Worldgen.ps1 → . · names tools/WorldgenDriver/Invoke-Worldgen.ps1 → tools
blk-1e7cd48f4614 · reference
  The `UserMod2` entry point. `OnLoad` reads the command line and applies the Harmony patches onl…
  tools/WorldgenDriver/.context/domains/entities.md#key-entities/worldgendrivermod · names tools/WorldgenDriver/Mod.cs → . · names tools/WorldgenDriver/Mod.cs → tools
tags: this brief answered the path question only. A record about a subject rather than a place is reached by `--tag <term>`; the terms are listed by: ks tags --store C:/dev/ONI-Mods/common

**Truncated.** 21 further candidate(s) matched and are not listed above (8123 of 8192 bytes; the whole selection is 15107). See the rest with:

    ks brief --at C:/dev/ONI-Mods/common --for . tools tools/ModCheck tools/SaveRunner tools/TestRunner --budget 15107

⚠ 12 record(s) in these 4 nodes declare no coverage and no tags and name no source path, so a brief reaches them only when a target or a matched record cites them: tools/ModCheck:domains/README.md, tools/ModCheck:domains/glossary.md, tools/ModCheck:standards/README.md, tools/SaveRunner:domains/README.md, tools/SaveRunner:domains/glossary.md and 7 more. Give them `applies_to` or tags through the write path; `--json` lists them all.

**Report back, don't write.** You have no write path here — what follows goes back to the coordinator in your report, not into `.context/`. Once you have opened what this work needed: did anything above turn out not to apply once you opened it (nothing above, if nothing was shown)? Did anything that actually governs this work fail to appear here at all — the miss only you can see? Did any pointer's stated one-line reason not match what its record actually says (nothing above, if nothing was shown)?

## 3. Checkpoint contract

**No checkpoints.** The coordinator sized this work at zero deliberately: this unit owes no progress file, because its only deliverable is its report. Zero is a number that was chosen, not a flag that was forgotten.

Write no checkpoint file, and do not invent one under another name — a file this contract does not name is scratch that nothing will read.

Do the work to its end and report. If you cannot finish, your report is what survives this lane: say in it what is done, what is not, and what you would do next.

## 4. Read-beyond licence

This is a strong default, not a cage — read beyond it whenever the task needs it.
