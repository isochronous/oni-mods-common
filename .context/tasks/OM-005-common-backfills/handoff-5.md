# Handoff — OM-005-common-backfills

**Scope:** `tools/ModCheck`, `tools/SaveRunner`, `tools/TestRunner`, `tools/Translations/StringsTemplate`, `tools/WorkshopUpload`, `tools/WorldgenDriver`
**Store:** `C:/dev/ONI-Mods/common`
**Task scratch:** `C:/dev/ONI-Mods/common/.context/tasks/OM-005-common-backfills/` — tracked, write-path exempt, never authoritative ([IF-1.C]).

You are reading this because a coordinator passed you its PATH. It is a file, it stays a file, and you can
re-read it after anything truncates your context. Nothing below was passed to you as a payload.

## 1. Task spec

Source: `.context/tasks/OM-005-common-backfills/units/05-research-vocab.md`

```
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
  StringsTemplate's code style for Program.cs and StringsTemplate.csproj: indentation, line lengt…
  tools/Translations/StringsTemplate/.context/standards/code-style.md · *.cs, *.csproj → tools/Translations/StringsTemplate · names tools/Translations/StringsTemplate/Program.cs → tools/Translations/StringsTemplate · linked from standards/error-handling.md · linked from standards/naming-conventions.md
tools/Translations/StringsTemplate:domains/entities.md · reference
  StringsTemplate's Program.Main and its helpers in Program.cs (Entry, Collect, WritePot, CheckOp…
  tools/Translations/StringsTemplate/.context/domains/entities.md · *.cs, *.csproj → tools/Translations/StringsTemplate · linked from standards/error-handling.md
tools/Translations/StringsTemplate:standards/error-handling.md · reference
  StringsTemplate's error handling: its principles, the exit code table (0, 1, 2), the report key…
  tools/Translations/StringsTemplate/.context/standards/error-handling.md · *.cs, *.csproj → tools/Translations/StringsTemplate · linked from standards/code-style.md
tools/Translations/StringsTemplate:standards/naming-conventions.md · reference
  StringsTemplate's naming rules: folder, project and output file names, type, method, variable a…
  tools/Translations/StringsTemplate/.context/standards/naming-conventions.md · *.cs, *.csproj → tools/Translations/StringsTemplate · linked from domains/entities.md · linked from standards/code-style.md
tools/Translations/StringsTemplate:domains/glossary.md · reference
  StringsTemplate glossary: abbreviations (pot, po, LocString, PLib), terms to avoid with their r…
  tools/Translations/StringsTemplate/.context/domains/glossary.md · linked from domains/entities.md · linked from standards/naming-conventions.md
tools/ModCheck:domains/entities.md · reference
  ModCheck's entities: the Program class in Program.cs and its members, the type-load and JIT che…
  tools/ModCheck/.context/domains/entities.md · Program.cs, ModCheck.csproj → tools/ModCheck
tools/ModCheck:standards/code-style.md · reference
  ModCheck's C# code style: tabs and Allman braces, C# 9 on .NET 8 with no packages in ModCheck.c…
  tools/ModCheck/.context/standards/code-style.md · Program.cs, ModCheck.csproj → tools/ModCheck · linked from standards/error-handling.md · linked from standards/naming-conventions.md
tools/ModCheck:standards/error-handling.md · reference
  How ModCheck handles and reports errors: principles, the exit code table, every OK, FAIL and WA…
  tools/ModCheck/.context/standards/error-handling.md · Program.cs, ModCheck.csproj → tools/ModCheck · linked from domains/entities.md · linked from standards/code-style.md
tools/ModCheck:standards/naming-conventions.md · reference
  Naming in ModCheck: the ModCheck.csproj and Program.cs file names, C# casing for types, methods…
  tools/ModCheck/.context/standards/naming-conventions.md · Program.cs, ModCheck.csproj → tools/ModCheck · linked from domains/entities.md · linked from standards/code-style.md
blk-3ca89d57dfde · reference
  What this is
  tools/ModCheck/.context/overview.md#what-this-is · names tools/TestRunner/Invoke-OniTests.ps1 → tools/TestRunner
blk-c95b82024244 · reference
  Entry point. Prints usage (return 2) when there are no arguments, calls `SteamAPI.Init`, prints…
  tools/WorkshopUpload/.context/domains/entities.md#key-entities/program-main · names tools/WorkshopUpload/Program.cs → tools/WorkshopUpload
blk-deeb9ba55606 · reference
  Overview
  tools/WorkshopUpload/.context/domains/entities.md#overview · names tools/WorkshopUpload/Program.cs → tools/WorkshopUpload
tools/WorkshopUpload:standards/code-style.md · reference
  WorkshopUpload's code style for Program.cs and WorkshopUpload.csproj: indentation, line length,…
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
tools/SaveRunner:domains/entities.md · reference
  SaveRunner's entities: SaveRunnerMod, Runner and its members, each Harmony patch in Patches.cs,…
  tools/SaveRunner/.context/domains/entities.md · *.cs, *.ps1, *.csproj, *.yaml → tools/SaveRunner
tools/SaveRunner:standards/code-style.md · reference
  SaveRunner's C# and PowerShell style: indentation and braces, C# 9 on net48 through SaveRunner.…
  tools/SaveRunner/.context/standards/code-style.md · *.cs, *.ps1, *.csproj → tools/SaveRunner · linked from standards/error-handling.md · linked from standards/naming-conventions.md
tools/SaveRunner:standards/error-handling.md · reference
  How SaveRunner ends and reports a run: the four status values Runner.Finish writes, the RunBeha…
  tools/SaveRunner/.context/standards/error-handling.md · *.cs, *.ps1, *.csproj, *.yaml → tools/SaveRunner · linked from standards/code-style.md
tools/SaveRunner:standards/naming-conventions.md · reference
  Naming in SaveRunner: C# file and type names, the <Type>_<Method>_Patch classes, Runner state f…
  tools/SaveRunner/.context/standards/naming-conventions.md · *.cs, *.ps1, *.csproj, *.yaml → tools/SaveRunner · linked from domains/entities.md · linked from standards/code-style.md
tools/TestRunner:domains/entities.md · reference
  TestRunner's entities: TestRunnerMod, Session members, TestCase and TestResult, TestBehaviour,…
  tools/TestRunner/.context/domains/entities.md · *.cs, Kit/**/*.cs, *.ps1, *.csproj, *.yaml → tools/TestRunner
tools/TestRunner:standards/code-style.md · reference
  TestRunner's C# and PowerShell style: indentation and braces, C# 9 on net48 through TestRunner.…
  tools/TestRunner/.context/standards/code-style.md · *.cs, Kit/**/*.cs, *.ps1, *.csproj → tools/TestRunner · linked from standards/error-handling.md · linked from standards/naming-conventions.md
tools/TestRunner:standards/error-handling.md · reference
  How TestRunner turns problems into results: the five test statuses, session statuses from Sessi…
  tools/TestRunner/.context/standards/error-handling.md · *.cs, Kit/**/*.cs, *.ps1, *.csproj, *.yaml → tools/TestRunner · linked from domains/entities.md · linked from standards/code-style.md
tools/TestRunner:standards/naming-conventions.md · reference
  Naming in TestRunner: files and run folders, the two namespaces, patch classes, Reflect handles…
  tools/TestRunner/.context/standards/naming-conventions.md · *.cs, Kit/**/*.cs, *.ps1, *.csproj, *.yaml → tools/TestRunner · linked from domains/entities.md · linked from standards/code-style.md
tags: this brief answered the path question only. A record about a subject rather than a place is reached by `--tag <term>`; the terms are listed by: ks tags --store C:/dev/ONI-Mods/common

**Truncated.** 12 further candidate(s) matched and are not listed above (8117 of 8192 bytes; the whole selection is 11692). See the rest with:

    ks brief --at C:/dev/ONI-Mods/common --for tools/ModCheck tools/SaveRunner tools/TestRunner tools/Translations/StringsTemplate tools/WorkshopUpload tools/WorldgenDriver --budget 11692

⚠ 21 record(s) in these 6 nodes declare no coverage and no tags and name no source path, so a brief reaches them only when a target or a matched record cites them: tools/ModCheck:domains/README.md, tools/ModCheck:domains/glossary.md, tools/ModCheck:standards/README.md, tools/SaveRunner:domains/README.md, tools/SaveRunner:domains/glossary.md and 16 more. Give them `applies_to` or tags through the write path; `--json` lists them all.

**Report back, don't write.** You have no write path here — what follows goes back to the coordinator in your report, not into `.context/`. Once you have opened what this work needed: did anything above turn out not to apply once you opened it (nothing above, if nothing was shown)? Did anything that actually governs this work fail to appear here at all — the miss only you can see? Did any pointer's stated one-line reason not match what its record actually says (nothing above, if nothing was shown)?

## 3. Checkpoint contract

**No checkpoints.** The coordinator sized this work at zero deliberately: this unit owes no progress file, because its only deliverable is its report. Zero is a number that was chosen, not a flag that was forgotten.

Write no checkpoint file, and do not invent one under another name — a file this contract does not name is scratch that nothing will read.

Do the work to its end and report. If you cannot finish, your report is what survives this lane: say in it what is done, what is not, and what you would do next.

## 4. Read-beyond licence

This is a strong default, not a cage — read beyond it whenever the task needs it.
