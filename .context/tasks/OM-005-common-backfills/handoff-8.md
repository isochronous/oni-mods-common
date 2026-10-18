# Handoff — OM-005-common-backfills

**Scope:** `.`, `tools`, `tools/ModCheck`, `tools/SaveRunner`, `tools/TestRunner`, `tools/Translations/StringsTemplate`, `tools/WorkshopUpload`, `tools/WorldgenDriver`
**Store:** `C:/dev/ONI-Mods/common`
**Task scratch:** `C:/dev/ONI-Mods/common/.context/tasks/OM-005-common-backfills/` — tracked, write-path exempt, never authoritative ([IF-1.C]).

You are reading this because a coordinator passed you its PATH. It is a file, it stays a file, and you can
re-read it after anything truncates your context. Nothing below was passed to you as a payload.

## 1. Task spec

Source: `.context/tasks/OM-005-common-backfills/units/08-review-fixes.md`

```
# Unit 08 — review: confirm the unit 06 findings are fixed

## Objective
Confirm that every finding of `units/06-review-report.md` taken into scope is fixed in the common store, and that the fixes broke nothing.

## Scope (read only)
- `C:/dev/ONI-Mods/common`: `git diff f1cb73e..HEAD -- . ':!.context/tasks'`, plus `units/06-review-report.md`, `units/07-implement-review-fixes-report.md` and `plan.md` (its Decisions record what was left on purpose: 3.2, 3.3, 4.2, 4.11, and the whole-tool term rule).
- For each of 1.1, 2.1, 3.1, 4.1, 4.3, 4.4, 4.5, 4.6, 4.7 and nits 1.2–1.8, 2.2–2.4, 4.8–4.10: resolved, partly resolved, or not, with evidence. Check every new fact in a changed overview, README or description against the code.
- Check the fixes broke nothing: every link in the common store resolves; no section repeats a tag its document or an enclosing heading carries; every term in each `tags.md` has at least one carrier (count `**Tags:**` lines and frontmatter `tags:` yourself; `ks tags` counts records matched through a shared word); `ks check --all --strict --store C:/dev/ONI-Mods/common` is 0 block; `ks brief` over the eight nodes no longer lists `tools:overview.md` as unreached.
- Manager change after unit 07: `mod-build` taken off WorldgenDriver's code-style "Language and Toolchain" and retired from its tags.md (one carrier left after 4.3).
- OUT: fixing anything; writing the store.

## Report
- File: C:/dev/ONI-Mods/common/.context/tasks/OM-005-common-backfills/units/08-review-fixes-report.md
- Must contain: a line per finding with its state and evidence; any new finding with severity (block / fix / nit), location and expected fix; friction met.
- Model tier: default
```

## 2. What applies here

The next hop is `ks read`: read the pointer, not the file it sits in.

POINTERS, not contents — ids, one line each, and why each matched. Open what the task needs; the budget was
spent selecting these so yours can be spent reading them.

blk-77fa8ff1839f · reference
  Overview
  tools/Translations/StringsTemplate/.context/domains/entities.md#overview · names tools/Translations/StringsTemplate/Program.cs, tools/Translations/translations.py → . · names tools/Translations/StringsTemplate/Program.cs, tools/Translations/translations.py → tools · names tools/Translations/StringsTemplate/Program.cs → tools/Translations/StringsTemplate
blk-b1319f3547fd · reference
  Entry point. Validates the three arguments, installs the `AssemblyResolve` handler, loads the D…
  tools/Translations/StringsTemplate/.context/domains/entities.md#key-entities/program-main · names tools/Translations/StringsTemplate/Program.cs → . · names tools/Translations/StringsTemplate/Program.cs → tools · names tools/Translations/StringsTemplate/Program.cs → tools/Translations/StringsTemplate
tools/Translations/StringsTemplate:standards/code-style.md · reference
  StringsTemplate's code style for Program.cs and StringsTemplate.csproj: indentation, line lengt…
  tools/Translations/StringsTemplate/.context/standards/code-style.md · *.cs, *.csproj → . · *.cs, *.csproj → tools · *.cs, *.csproj → tools/Translations/StringsTemplate · names tools/Translations/StringsTemplate/Program.cs → . · names tools/Translations/StringsTemplate/Program.cs → tools · names tools/Translations/StringsTemplate/Program.cs → tools/Translations/StringsTemplate · linked from standards/error-handling.md · linked from standards/naming-conventions.md
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
  StringsTemplate's Program.Main and its helpers in Program.cs (Entry, Collect, WritePot, CheckOp…
  tools/Translations/StringsTemplate/.context/domains/entities.md · *.cs, *.csproj → . · *.cs, *.csproj → tools · *.cs, *.csproj → tools/Translations/StringsTemplate · linked from standards/error-handling.md
tools/Translations/StringsTemplate:standards/error-handling.md · reference
  StringsTemplate's error handling: its principles, the exit code table (0, 1, 2), the report key…
  tools/Translations/StringsTemplate/.context/standards/error-handling.md · *.cs, *.csproj → . · *.cs, *.csproj → tools · *.cs, *.csproj → tools/Translations/StringsTemplate · linked from standards/code-style.md
tools/Translations/StringsTemplate:standards/naming-conventions.md · reference
  StringsTemplate's naming rules: folder, project and output file names, type, method, variable a…
  tools/Translations/StringsTemplate/.context/standards/naming-conventions.md · *.cs, *.csproj → . · *.cs, *.csproj → tools · *.cs, *.csproj → tools/Translations/StringsTemplate · linked from domains/entities.md · linked from standards/code-style.md
tools/Translations/StringsTemplate:domains/glossary.md · reference
  StringsTemplate glossary: abbreviations (pot, po, LocString, PLib), terms to avoid with their r…
  tools/Translations/StringsTemplate/.context/domains/glossary.md · linked from domains/entities.md · linked from standards/naming-conventions.md
tools/ModCheck:domains/entities.md · reference
  ModCheck's entities: the Program class in Program.cs and its members, the type-load and JIT che…
  tools/ModCheck/.context/domains/entities.md · Program.cs, ModCheck.csproj → . · Program.cs, ModCheck.csproj → tools · Program.cs, ModCheck.csproj → tools/ModCheck
tools/ModCheck:standards/code-style.md · reference
  ModCheck's C# code style: tabs and Allman braces, C# 9 on .NET 8 with no packages in ModCheck.c…
  tools/ModCheck/.context/standards/code-style.md · Program.cs, ModCheck.csproj → . · Program.cs, ModCheck.csproj → tools · Program.cs, ModCheck.csproj → tools/ModCheck · linked from standards/error-handling.md · linked from standards/naming-conventions.md
tools/ModCheck:standards/error-handling.md · reference
  How ModCheck handles and reports errors: principles, the exit code table, every OK, FAIL and WA…
  tools/ModCheck/.context/standards/error-handling.md · Program.cs, ModCheck.csproj → . · Program.cs, ModCheck.csproj → tools · Program.cs, ModCheck.csproj → tools/ModCheck · linked from domains/entities.md · linked from standards/code-style.md
tools/ModCheck:standards/naming-conventions.md · reference
  Naming in ModCheck: the ModCheck.csproj and Program.cs file names, C# casing for types, methods…
  tools/ModCheck/.context/standards/naming-conventions.md · Program.cs, ModCheck.csproj → . · Program.cs, ModCheck.csproj → tools · Program.cs, ModCheck.csproj → tools/ModCheck · linked from domains/entities.md · linked from standards/code-style.md
blk-846080fa7238 · reference
  What this is
  tools/WorkshopUpload/.context/overview.md#what-this-is · names tools/ModTest/Test-OniMods.ps1, tools/Release/release.py → . · names tools/ModTest/Test-OniMods.ps1, tools/Release/release.py → tools
blk-cc023dbf02c0 · reference
  Key Code Paths
  tools/WorkshopUpload/.context/domains/entities.md#key-code-paths · names tools/Release/release.py → . · names tools/Release/release.py → tools
blk-deeb9ba55606 · reference
  Overview
  tools/WorkshopUpload/.context/domains/entities.md#overview · names tools/Release/release.py, tools/WorkshopUpload/Program.cs → . · names tools/Release/release.py, tools/WorkshopUpload/Program.cs → tools · names tools/WorkshopUpload/Program.cs → tools/WorkshopUpload
tools/WorkshopUpload:standards/error-handling.md · reference
  WorkshopUpload's error handling: principles, the exit code table (0, 1, 2), stdout and stderr r…
  tools/WorkshopUpload/.context/standards/error-handling.md · *.cs, *.csproj → . · *.cs, *.csproj → tools · *.cs, *.csproj → tools/WorkshopUpload · names tools/Release/release.py → . · names tools/Release/release.py → tools · linked from standards/code-style.md
blk-3ca89d57dfde · reference
  What this is
  tools/ModCheck/.context/overview.md#what-this-is · names tools/TestRunner/Invoke-OniTests.ps1 → . · names tools/TestRunner/Invoke-OniTests.ps1 → tools · names tools/TestRunner/Invoke-OniTests.ps1 → tools/TestRunner
blk-c95b82024244 · reference
  Entry point. Prints usage (return 2) when there are no arguments, calls `SteamAPI.Init`, prints…
  tools/WorkshopUpload/.context/domains/entities.md#key-entities/program-main · names tools/WorkshopUpload/Program.cs → . · names tools/WorkshopUpload/Program.cs → tools · names tools/WorkshopUpload/Program.cs → tools/WorkshopUpload
tools/WorkshopUpload:standards/code-style.md · reference
  WorkshopUpload's code style for Program.cs and WorkshopUpload.csproj: indentation, line length,…
  tools/WorkshopUpload/.context/standards/code-style.md · *.cs, *.csproj → . · *.cs, *.csproj → tools · *.cs, *.csproj → tools/WorkshopUpload · names tools/WorkshopUpload/Program.cs → . · names tools/WorkshopUpload/Program.cs → tools · names tools/WorkshopUpload/Program.cs → tools/WorkshopUpload · linked from standards/error-handling.md · linked from standards/naming-conventions.md
tags: this brief answered the path question only. A record about a subject rather than a place is reached by `--tag <term>`; the terms are listed by: ks tags --store C:/dev/ONI-Mods/common

**Truncated.** 26 further candidate(s) matched and are not listed above (7983 of 8192 bytes; the whole selection is 17325). See the rest with:

    ks brief --at C:/dev/ONI-Mods/common --for . tools tools/ModCheck tools/SaveRunner tools/TestRunner tools/Translations/StringsTemplate tools/WorkshopUpload tools/WorldgenDriver --budget 17325

⚠ 6 record(s) in these 6 nodes declare no coverage and no tags and name no source path, so a brief reaches them only when a target or a matched record cites them: tools/ModCheck:tags.md, tools/SaveRunner:tags.md, tools/TestRunner:tags.md, tools/Translations/StringsTemplate:tags.md, tools/WorkshopUpload:tags.md and 1 more. Give them `applies_to` or tags through the write path; `--json` lists them all.

**Report back, don't write.** You have no write path here — what follows goes back to the coordinator in your report, not into `.context/`. Once you have opened what this work needed: did anything above turn out not to apply once you opened it (nothing above, if nothing was shown)? Did anything that actually governs this work fail to appear here at all — the miss only you can see? Did any pointer's stated one-line reason not match what its record actually says (nothing above, if nothing was shown)?

## 3. Checkpoint contract

**No checkpoints.** The coordinator sized this work at zero deliberately: this unit owes no progress file, because its only deliverable is its report. Zero is a number that was chosen, not a flag that was forgotten.

Write no checkpoint file, and do not invent one under another name — a file this contract does not name is scratch that nothing will read.

Do the work to its end and report. If you cannot finish, your report is what survives this lane: say in it what is done, what is not, and what you would do next.

## 4. Read-beyond licence

This is a strong default, not a cage — read beyond it whenever the task needs it.
