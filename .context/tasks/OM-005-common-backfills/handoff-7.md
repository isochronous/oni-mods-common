# Handoff — OM-005-common-backfills

**Scope:** `tools`, `tools/ModCheck`, `tools/SaveRunner`, `tools/TestRunner`, `tools/Translations/StringsTemplate`, `tools/WorkshopUpload`, `tools/WorldgenDriver`
**Store:** `C:/dev/ONI-Mods/common`
**Task scratch:** `C:/dev/ONI-Mods/common/.context/tasks/OM-005-common-backfills/` — tracked, write-path exempt, never authoritative ([IF-1.C]).

You are reading this because a coordinator passed you its PATH. It is a file, it stays a file, and you can
re-read it after anything truncates your context. Nothing below was passed to you as a payload.

## 1. Task spec

Source: `.context/tasks/OM-005-common-backfills/units/07-implement-review-fixes.md`

```
# Unit 07 — implement: change-sets for the unit 06 review findings

## Objective
Draft change-sets fixing the findings of `units/06-review-report.md` listed below. The manager applies them. Never run `write apply`, never commit.

## Scope
Read `C:/dev/ONI-Mods/common/.context/tasks/OM-005-common-backfills/units/06-review-report.md` in full first; finding ids below refer to it. All nodes are in the common store (`C:/dev/ONI-Mods/common`).

- IN, fix findings:
  - **1.1** Write the paths in `tools/.context/overview.md` ("What this is" and "Where things are") store-relative (`tools/Release/release.py`, `tools/Kanim/kanim.py`, ...), so source-mention reaches it.
  - **2.1** Rewrite the description of `tools::overview.md` so its folder list is true; keep the code names true and inside the node (store-relative, under `tools/`).
  - **3.1** Preferred option: mint `in-game-testing` in `tools/TestRunner/.context/tags.md` with the gloss from `C:/dev/ONI-Mods/.context/tags.md` word for word, in a change-set of its own (`fix07-mint-TestRunner.json`, nothing else in it). Then put each whole-tool term at document level on the same four documents of its node: overview.md, domains/README.md, domains/entities.md, domains/glossary.md — `in-game-testing` (TestRunner), `steam-workshop` (WorkshopUpload), `translations` (StringsTemplate), `generated-worlds` (WorldgenDriver), `game-update-compatibility` (ModCheck). Where a section beneath one of those documents carries the same term, remove it from the section in the same change-set.
  - **4.1** TestRunner: take `screenshots` and `player-log` off their carriers and remove both terms from its tags.md. For `mod-entry-point` in TestRunner: add it to the records it genuinely fits by its gloss (the entry-point class sections) if that brings it to at least 3 records across 2 documents; otherwise take it off its carrier and remove the term. Say which in the report.
  - **4.3** Remove every tag from the five "Where things are" file-map sections listed (blk-1d0e91d4e985, blk-3dee2f4998e2, blk-04261b4be94e, blk-6dcf139ddfa3, blk-ebff91fa90b7).
  - **4.4** StringsTemplate and WorkshopUpload `standards/error-handling.md`: document tags become `exception-handling` only; put `exit-codes` and (StringsTemplate) `console-output` on the sections they describe (the exit-code and report-keyword sections, and any other section they fit). Their `standards/README.md` document tags become `code-style`, `exception-handling`, as in the other four nodes.
  - **4.5** Move `translation-template` (StringsTemplate) and `legacy-workshop-item` (WorkshopUpload) from overview document level to the overview's "What this is" section.
  - **4.6** Covered by 3.1.
  - **4.7** Remove `mod-entry-point` from SaveRunner blk-545208d8763a.
- IN, nits: 1.2, 1.3, 1.4, 1.5, 1.6, 1.7, 1.8 (overview/README text, checked against the code), 2.2, 2.3, 2.4 (descriptions; 2.3 rewrites the six index descriptions to name each node's own topics), 4.8, 4.9, 4.10 (tags).
- OUT: 3.2 (the manager keeps the mod-strings gloss), 3.3, 4.2, 4.11; any other change; applying; committing.

## Rules
- Shapes: `node c:/dev/ai/icon/bin/ks.mjs write schema` and `--row narrative --mode section|describe`, `--row tags --mode tag|create|remove`. Tag ops address sections by block address as the schema says; a term removal from tags.md is a `remove` op on `tags.md` with the term's section and a reason.
- One change-set per node, `C:/dev/ONI-Mods/common/.context/tasks/OM-005-common-backfills/changesets/fix07-<name>.json` (`fix07-tools.json`, `fix07-ModCheck.json`, ...), plus `fix07-mint-TestRunner.json`. If a node needs tags removed from records before its term can be removed, put the term removal in a second file `fix07-rmterm-<name>.json`. Envelope `task: "OM-005"`, a `why`, `node` relative to `common/`.
- Read every document you change in full; every fact you write is checked against the code. Plain wording, no hedges; descriptions follow the description-backfill rules (what the file contains, one line, names the tool, true in-node code names).
- Build JSON with a script in a uniquely named scratchpad subfolder or the Write tool, never a heredoc.

## Verification you owe
From `C:/dev/ONI-Mods`: `node c:/dev/ai/icon/bin/ks.mjs write plan --changeset <file> --at common` for each file until accepted. The TestRunner change-set using `in-game-testing` cannot plan until the mint is applied: plan it and report the refusal if that is the only reason, and say the order to apply.

## Report
- File: C:/dev/ONI-Mods/common/.context/tasks/OM-005-common-backfills/units/07-implement-review-fixes-report.md
- Must contain: per finding, what changed and where; the apply order; plan summaries; friction met.
- Model tier: default
```

## 2. What applies here

The next hop is `ks read`: read the pointer, not the file it sits in.

POINTERS, not contents — ids, one line each, and why each matched. Open what the task needs; the budget was
spent selecting these so yours can be spent reading them.

blk-77fa8ff1839f · reference
  Overview
  tools/Translations/StringsTemplate/.context/domains/entities.md#overview · names tools/Translations/StringsTemplate/Program.cs, tools/Translations/translations.py → tools · names tools/Translations/StringsTemplate/Program.cs → tools/Translations/StringsTemplate
blk-b1319f3547fd · reference
  Entry point. Validates the three arguments, installs the `AssemblyResolve` handler, loads the D…
  tools/Translations/StringsTemplate/.context/domains/entities.md#key-entities/program-main · names tools/Translations/StringsTemplate/Program.cs → tools · names tools/Translations/StringsTemplate/Program.cs → tools/Translations/StringsTemplate
tools/Translations/StringsTemplate:standards/code-style.md · reference
  StringsTemplate's code style for Program.cs and StringsTemplate.csproj: indentation, line lengt…
  tools/Translations/StringsTemplate/.context/standards/code-style.md · *.cs, *.csproj → tools · *.cs, *.csproj → tools/Translations/StringsTemplate · names tools/Translations/StringsTemplate/Program.cs → tools · names tools/Translations/StringsTemplate/Program.cs → tools/Translations/StringsTemplate · linked from standards/error-handling.md · linked from standards/naming-conventions.md
blk-04261b4be94e · reference
  Where things are
  tools/Translations/StringsTemplate/.context/overview.md#where-things-are · names tools/Translations/translations.py → tools
blk-0c006bb6c374 · reference
  Language and Toolchain
  tools/Translations/StringsTemplate/.context/standards/code-style.md#language-and-toolchain · names tools/Translations/translations.py → tools
blk-aa442e0b3e88 · reference
  Key Code Paths
  tools/Translations/StringsTemplate/.context/domains/entities.md#key-code-paths · names tools/Translations/translations.py → tools
blk-e125ed2b68c6 · reference
  What this is
  tools/Translations/StringsTemplate/.context/overview.md#what-this-is · names tools/Translations/translations.py → tools
tools/Translations/StringsTemplate:domains/entities.md · reference
  StringsTemplate's Program.Main and its helpers in Program.cs (Entry, Collect, WritePot, CheckOp…
  tools/Translations/StringsTemplate/.context/domains/entities.md · *.cs, *.csproj → tools · *.cs, *.csproj → tools/Translations/StringsTemplate · linked from standards/error-handling.md
tools/Translations/StringsTemplate:standards/error-handling.md · reference
  StringsTemplate's error handling: its principles, the exit code table (0, 1, 2), the report key…
  tools/Translations/StringsTemplate/.context/standards/error-handling.md · *.cs, *.csproj → tools · *.cs, *.csproj → tools/Translations/StringsTemplate · linked from standards/code-style.md
tools/Translations/StringsTemplate:standards/naming-conventions.md · reference
  StringsTemplate's naming rules: folder, project and output file names, type, method, variable a…
  tools/Translations/StringsTemplate/.context/standards/naming-conventions.md · *.cs, *.csproj → tools · *.cs, *.csproj → tools/Translations/StringsTemplate · linked from domains/entities.md · linked from standards/code-style.md
tools/Translations/StringsTemplate:domains/glossary.md · reference
  StringsTemplate glossary: abbreviations (pot, po, LocString, PLib), terms to avoid with their r…
  tools/Translations/StringsTemplate/.context/domains/glossary.md · linked from domains/entities.md · linked from standards/naming-conventions.md
tools/ModCheck:domains/entities.md · reference
  ModCheck's entities: the Program class in Program.cs and its members, the type-load and JIT che…
  tools/ModCheck/.context/domains/entities.md · Program.cs, ModCheck.csproj → tools · Program.cs, ModCheck.csproj → tools/ModCheck
tools/ModCheck:standards/code-style.md · reference
  ModCheck's C# code style: tabs and Allman braces, C# 9 on .NET 8 with no packages in ModCheck.c…
  tools/ModCheck/.context/standards/code-style.md · Program.cs, ModCheck.csproj → tools · Program.cs, ModCheck.csproj → tools/ModCheck · linked from standards/error-handling.md · linked from standards/naming-conventions.md
tools/ModCheck:standards/error-handling.md · reference
  How ModCheck handles and reports errors: principles, the exit code table, every OK, FAIL and WA…
  tools/ModCheck/.context/standards/error-handling.md · Program.cs, ModCheck.csproj → tools · Program.cs, ModCheck.csproj → tools/ModCheck · linked from domains/entities.md · linked from standards/code-style.md
tools/ModCheck:standards/naming-conventions.md · reference
  Naming in ModCheck: the ModCheck.csproj and Program.cs file names, C# casing for types, methods…
  tools/ModCheck/.context/standards/naming-conventions.md · Program.cs, ModCheck.csproj → tools · Program.cs, ModCheck.csproj → tools/ModCheck · linked from domains/entities.md · linked from standards/code-style.md
blk-846080fa7238 · reference
  What this is
  tools/WorkshopUpload/.context/overview.md#what-this-is · names tools/ModTest/Test-OniMods.ps1, tools/Release/release.py → tools
blk-cc023dbf02c0 · reference
  Key Code Paths
  tools/WorkshopUpload/.context/domains/entities.md#key-code-paths · names tools/Release/release.py → tools
blk-deeb9ba55606 · reference
  Overview
  tools/WorkshopUpload/.context/domains/entities.md#overview · names tools/Release/release.py, tools/WorkshopUpload/Program.cs → tools · names tools/WorkshopUpload/Program.cs → tools/WorkshopUpload
tools/WorkshopUpload:standards/error-handling.md · reference
  WorkshopUpload's error handling: principles, the exit code table (0, 1, 2), stdout and stderr r…
  tools/WorkshopUpload/.context/standards/error-handling.md · *.cs, *.csproj → tools · *.cs, *.csproj → tools/WorkshopUpload · names tools/Release/release.py → tools · linked from standards/code-style.md
blk-3ca89d57dfde · reference
  What this is
  tools/ModCheck/.context/overview.md#what-this-is · names tools/TestRunner/Invoke-OniTests.ps1 → tools · names tools/TestRunner/Invoke-OniTests.ps1 → tools/TestRunner
blk-c95b82024244 · reference
  Entry point. Prints usage (return 2) when there are no arguments, calls `SteamAPI.Init`, prints…
  tools/WorkshopUpload/.context/domains/entities.md#key-entities/program-main · names tools/WorkshopUpload/Program.cs → tools · names tools/WorkshopUpload/Program.cs → tools/WorkshopUpload
tools/WorkshopUpload:standards/code-style.md · reference
  WorkshopUpload's code style for Program.cs and WorkshopUpload.csproj: indentation, line length,…
  tools/WorkshopUpload/.context/standards/code-style.md · *.cs, *.csproj → tools · *.cs, *.csproj → tools/WorkshopUpload · names tools/WorkshopUpload/Program.cs → tools · names tools/WorkshopUpload/Program.cs → tools/WorkshopUpload · linked from standards/error-handling.md · linked from standards/naming-conventions.md
blk-cb417a1e3041 · reference
  Static session state and the result writer: the coordinate list and the current index, the time…
  tools/WorldgenDriver/.context/domains/entities.md#key-entities/driver · names tools/WorldgenDriver/Driver.cs → tools · names tools/WorldgenDriver/Driver.cs → tools/WorldgenDriver
blk-2c4e50d4b9fc · reference
  The launcher. Groups coordinates by mode, runs them in sessions of up to `-PerSession` (default…
  tools/WorldgenDriver/.context/domains/entities.md#key-entities/invoke-worldgen-ps1 · names tools/WorldgenDriver/Invoke-Worldgen.ps1 → tools · names tools/WorldgenDriver/Invoke-Worldgen.ps1 → tools/WorldgenDriver
blk-1e7cd48f4614 · reference
  The `UserMod2` entry point. `OnLoad` reads the command line and applies the Harmony patches onl…
  tools/WorldgenDriver/.context/domains/entities.md#key-entities/worldgendrivermod · names tools/WorldgenDriver/Mod.cs → tools · names tools/WorldgenDriver/Mod.cs → tools/WorldgenDriver
tools/SaveRunner:domains/entities.md · reference
  SaveRunner's entities: SaveRunnerMod, Runner and its members, each Harmony patch in Patches.cs,…
  tools/SaveRunner/.context/domains/entities.md · *.cs, *.ps1, *.csproj, *.yaml → tools · *.cs, *.ps1, *.csproj, *.yaml → tools/SaveRunner
tags: this brief answered the path question only. A record about a subject rather than a place is reached by `--tag <term>`; the terms are listed by: ks tags --store C:/dev/ONI-Mods/common

**Truncated.** 18 further candidate(s) matched and are not listed above (8189 of 8192 bytes; the whole selection is 14367). See the rest with:

    ks brief --at C:/dev/ONI-Mods/common --for tools tools/ModCheck tools/SaveRunner tools/TestRunner tools/Translations/StringsTemplate tools/WorkshopUpload tools/WorldgenDriver --budget 14367

⚠ 9 record(s) in these 7 nodes declare no coverage and no tags and name no source path, so a brief reaches them only when a target or a matched record cites them: tools/ModCheck:tags.md, tools/SaveRunner:tags.md, tools/TestRunner:domains/README.md, tools/TestRunner:tags.md, tools/Translations/StringsTemplate:tags.md and 4 more. Give them `applies_to` or tags through the write path; `--json` lists them all.

**Report back, don't write.** You have no write path here — what follows goes back to the coordinator in your report, not into `.context/`. Once you have opened what this work needed: did anything above turn out not to apply once you opened it (nothing above, if nothing was shown)? Did anything that actually governs this work fail to appear here at all — the miss only you can see? Did any pointer's stated one-line reason not match what its record actually says (nothing above, if nothing was shown)?

## 3. Checkpoint contract

**No checkpoints.** The coordinator sized this work at zero deliberately: this unit owes no progress file, because its only deliverable is its report. Zero is a number that was chosen, not a flag that was forgotten.

Write no checkpoint file, and do not invent one under another name — a file this contract does not name is scratch that nothing will read.

Do the work to its end and report. If you cannot finish, your report is what survives this lane: say in it what is done, what is not, and what you would do next.

## 4. Read-beyond licence

This is a strong default, not a cage — read beyond it whenever the task needs it.
