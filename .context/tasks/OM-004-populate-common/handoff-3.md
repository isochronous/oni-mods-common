# Handoff — OM-004-populate-common

**Scope:** `common/tools`
**Store:** `C:/dev/ONI-Mods/common`
**Task scratch:** `C:/dev/ONI-Mods/common/.context/tasks/OM-004-populate-common/` — tracked, write-path exempt, never authoritative ([IF-1.C]).

You are reading this because a coordinator passed you its PATH. It is a file, it stays a file, and you can
re-read it after anything truncates your context. Nothing below was passed to you as a payload.

## 1. Task spec

Source: `common/.context/tasks/OM-004-populate-common/units/03-review.md`

```
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
```

## 2. What applies here

The next hop is `ks read`: read the pointer, not the file it sits in.

POINTERS, not contents — ids, one line each, and why each matched. Open what the task needs; the budget was
spent selecting these so yours can be spent reading them.

blk-77fa8ff1839f · reference
  Overview
  tools/Translations/StringsTemplate/.context/domains/entities.md#overview · names tools/Translations/StringsTemplate/Program.cs, tools/Translations/translations.py → tools
blk-b1319f3547fd · reference
  Entry point. Validates the three arguments, installs the `AssemblyResolve` handler, loads the D…
  tools/Translations/StringsTemplate/.context/domains/entities.md#key-entities/program-main · names tools/Translations/StringsTemplate/Program.cs → tools
tools/Translations/StringsTemplate:standards/code-style.md · reference
  StringsTemplate (the translation template writer and checker): tabs, Allman braces, C# 9 on .NE…
  tools/Translations/StringsTemplate/.context/standards/code-style.md · *.cs, *.csproj → tools · names tools/Translations/StringsTemplate/Program.cs → tools · linked from standards/error-handling.md · linked from standards/naming-conventions.md
blk-0c006bb6c374 · reference
  Language and Toolchain
  tools/Translations/StringsTemplate/.context/standards/code-style.md#language-and-toolchain · names tools/Translations/translations.py → tools
blk-aa442e0b3e88 · reference
  Key Code Paths
  tools/Translations/StringsTemplate/.context/domains/entities.md#key-code-paths · names tools/Translations/translations.py → tools
tools/Translations/StringsTemplate:domains/entities.md · reference
  StringsTemplate (the translation template writer and checker): Program.Main and its helpers, th…
  tools/Translations/StringsTemplate/.context/domains/entities.md · *.cs, *.csproj → tools · linked from standards/error-handling.md
tools/Translations/StringsTemplate:standards/error-handling.md · reference
  StringsTemplate (the translation template writer and checker): exit codes 0/1/2, the FAIL, MISS…
  tools/Translations/StringsTemplate/.context/standards/error-handling.md · *.cs, *.csproj → tools · linked from standards/code-style.md
tools/Translations/StringsTemplate:standards/naming-conventions.md · reference
  StringsTemplate (the translation template writer and checker): file, method, variable and trans…
  tools/Translations/StringsTemplate/.context/standards/naming-conventions.md · *.cs, *.csproj → tools · linked from domains/entities.md · linked from standards/code-style.md
tools/Translations/StringsTemplate:domains/glossary.md · reference
  StringsTemplate (the translation template writer and checker): terms a newcomer needs: ModStrin…
  tools/Translations/StringsTemplate/.context/domains/glossary.md · linked from domains/entities.md · linked from standards/naming-conventions.md
tools/ModCheck:domains/entities.md · reference
  ModCheck's real things: Program and its helpers, the type-load and JIT checks, inputs, output l…
  tools/ModCheck/.context/domains/entities.md · Program.cs, ModCheck.csproj → tools
tools/ModCheck:standards/code-style.md · reference
  C# code style for the ModCheck console tool: formatting, toolchain, structure and comments, wit…
  tools/ModCheck/.context/standards/code-style.md · Program.cs, ModCheck.csproj → tools · linked from standards/error-handling.md · linked from standards/naming-conventions.md
tools/ModCheck:standards/error-handling.md · reference
  How ModCheck reports failures: exit codes 0, 1 and 2, the OK, FAIL and WARN output lines, per-t…
  tools/ModCheck/.context/standards/error-handling.md · Program.cs, ModCheck.csproj → tools · linked from domains/entities.md · linked from standards/code-style.md
tools/ModCheck:standards/naming-conventions.md · reference
  Naming in ModCheck: project and file names, C# member casing, the two positional arguments and…
  tools/ModCheck/.context/standards/naming-conventions.md · Program.cs, ModCheck.csproj → tools · linked from domains/entities.md · linked from standards/code-style.md
blk-cc023dbf02c0 · reference
  Key Code Paths
  tools/WorkshopUpload/.context/domains/entities.md#key-code-paths · names tools/Release/release.py → tools
blk-deeb9ba55606 · reference
  Overview
  tools/WorkshopUpload/.context/domains/entities.md#overview · names tools/Release/release.py, tools/WorkshopUpload/Program.cs → tools
tools/WorkshopUpload:standards/error-handling.md · reference
  WorkshopUpload (the legacy Steam Workshop publisher): exit codes 0/1/2, Steam result checks, Aw…
  tools/WorkshopUpload/.context/standards/error-handling.md · *.cs, *.csproj → tools · names tools/Release/release.py → tools · linked from standards/code-style.md
blk-c95b82024244 · reference
  Entry point. Prints usage (return 2) when there are no arguments, calls `SteamAPI.Init`, prints…
  tools/WorkshopUpload/.context/domains/entities.md#key-entities/program-main · names tools/WorkshopUpload/Program.cs → tools
tools/WorkshopUpload:standards/code-style.md · reference
  WorkshopUpload (the legacy Steam Workshop publisher): tabs, Allman braces, .NET 8 with Steamwor…
  tools/WorkshopUpload/.context/standards/code-style.md · *.cs, *.csproj → tools · names tools/WorkshopUpload/Program.cs → tools · linked from standards/error-handling.md · linked from standards/naming-conventions.md
blk-cb417a1e3041 · reference
  Static session state and the result writer: the coordinate list and the current index, the time…
  tools/WorldgenDriver/.context/domains/entities.md#key-entities/driver · names tools/WorldgenDriver/Driver.cs → tools
blk-2c4e50d4b9fc · reference
  The launcher. Groups coordinates by mode, runs them in sessions of up to `-PerSession` (default…
  tools/WorldgenDriver/.context/domains/entities.md#key-entities/invoke-worldgen-ps1 · names tools/WorldgenDriver/Invoke-Worldgen.ps1 → tools
blk-1e7cd48f4614 · reference
  The `UserMod2` entry point. `OnLoad` reads the command line and applies the Harmony patches onl…
  tools/WorldgenDriver/.context/domains/entities.md#key-entities/worldgendrivermod · names tools/WorldgenDriver/Mod.cs → tools
tools/SaveRunner:domains/entities.md · reference
  SaveRunner's real things: the mod, Runner, patches, RunBehaviour, FridgeProbe, the launcher wit…
  tools/SaveRunner/.context/domains/entities.md · *.cs, *.ps1, *.csproj, *.yaml → tools
tools/SaveRunner:standards/code-style.md · reference
  C# and PowerShell style for the SaveRunner dev mod: formatting, toolchain, file layout, inert-w…
  tools/SaveRunner/.context/standards/code-style.md · *.cs, *.ps1, *.csproj → tools · linked from standards/error-handling.md · linked from standards/naming-conventions.md
tools/SaveRunner:standards/error-handling.md · reference
  How SaveRunner ends and reports a run: the four status values, the driver try/catch, optional h…
  tools/SaveRunner/.context/standards/error-handling.md · *.cs, *.ps1, *.csproj, *.yaml → tools · linked from standards/code-style.md
tools/SaveRunner:standards/naming-conventions.md · reference
  Naming in SaveRunner: files, patch classes, Runner state, probes, the --saverun-* options, laun…
  tools/SaveRunner/.context/standards/naming-conventions.md · *.cs, *.ps1, *.csproj, *.yaml → tools · linked from domains/entities.md · linked from standards/code-style.md
tools/TestRunner:domains/entities.md · reference
  TestRunner's real things: the harness mod, Session, TestBehaviour, patches, FixtureArea, Worldg…
  tools/TestRunner/.context/domains/entities.md · *.cs, Kit/**/*.cs, *.ps1, *.csproj, *.yaml → tools
tools/TestRunner:standards/code-style.md · reference
  C# and PowerShell style for the TestRunner harness mod and test kit: formatting, toolchain, nam…
  tools/TestRunner/.context/standards/code-style.md · *.cs, Kit/**/*.cs, *.ps1, *.csproj → tools · linked from standards/error-handling.md · linked from standards/naming-conventions.md
tools/TestRunner:standards/error-handling.md · reference
  How TestRunner turns problems into results: test statuses, session statuses, launcher exit code…
  tools/TestRunner/.context/standards/error-handling.md · *.cs, Kit/**/*.cs, *.ps1, *.csproj, *.yaml → tools · linked from domains/entities.md · linked from standards/code-style.md
tags: this brief answered the path question only. A record about a subject rather than a place is reached by `--tag <term>`; the terms are listed by: ks tags --store C:/dev/ONI-Mods/common

**Truncated.** 12 further candidate(s) matched and are not listed above (8107 of 8192 bytes; the whole selection is 11639). See the rest with:

    ks brief --at C:/dev/ONI-Mods/common --for tools --decisions --budget 11639

⚠ 1 record(s) in this node declare no coverage and no tags and name no source path, so a brief reaches them only when a target or a matched record cites them: tools:overview.md. Give them `applies_to` or tags through the write path.

**Report back, don't write.** You have no write path here — what follows goes back to the coordinator in your report, not into `.context/`. Once you have opened what this work needed: did anything above turn out not to apply once you opened it (nothing above, if nothing was shown)? Did anything that actually governs this work fail to appear here at all — the miss only you can see? Did any pointer's stated one-line reason not match what its record actually says (nothing above, if nothing was shown)?

## 3. Checkpoint contract

**No checkpoints.** The coordinator sized this work at zero deliberately: this unit owes no progress file, because its only deliverable is its report. Zero is a number that was chosen, not a flag that was forgotten.

Write no checkpoint file, and do not invent one under another name — a file this contract does not name is scratch that nothing will read.

Do the work to its end and report. If you cannot finish, your report is what survives this lane: say in it what is done, what is not, and what you would do next.

## 4. Read-beyond licence

This is a strong default, not a cage — read beyond it whenever the task needs it.
