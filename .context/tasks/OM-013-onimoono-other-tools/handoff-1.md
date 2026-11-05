# Handoff — OM-013-onimoono-other-tools

**Scope:** `tools/SaveRunner`, `tools/WorldgenDriver`, `tools/ModTest`, `tools/TestRunner/GameProcess.ps1`
**Store:** `C:/dev/ONI-Mods/common`
**Task scratch:** `C:/dev/ONI-Mods/common/.context/tasks/OM-013-onimoono-other-tools/` — tracked, write-path exempt, never authoritative ([IF-1.C]).

You are reading this because a coordinator passed you its PATH. It is a file, it stays a file, and you can
re-read it after anything truncates your context. Nothing below was passed to you as a payload.

## 1. Task spec

Source: `.context/tasks/OM-013-onimoono-other-tools/units/01-implement.md`

```
Task: OM-013 — SaveRunner, WorldgenDriver and ModTest follow a game started through oni-moono
Lens: implement. Repo: C:\dev\ONI-Mods\common, branch OM-013-onimoono-other-tools (has OM-012's tools/TestRunner/GameProcess.ps1). Do not commit. Do not launch the game. Do not touch Steam settings.

Read: common/.context/tasks/OM-013-onimoono-other-tools/plan.md; OM-012's change as the pattern: `git diff 5008da9..1d45792 -- tools/TestRunner` and common/.context/tasks/OM-012-onimoono-harness/units/01-implement-report.md.

Do:
1. In tools/SaveRunner/Invoke-SaveRun.ps1, tools/WorldgenDriver/Invoke-Worldgen.ps1 and tools/ModTest/Test-OniMods.ps1 (and any helper they dot-source or warden they start), find every place the game is found, waited for, killed or checked by process name or window. Replace each with the OM-012 rule: the launched game is `OxygenNotIncluded`, or `OniMoono` whose command line carries that tool's own launch arguments (find what each passes); pre-launch "game already open" refusals count any `OniMoono`.
2. Share the code: prefer making TestRunner's Get-GameProcess usable by all tools (e.g. a parameter for the argument marker, keeping its current default and behaviour for TestRunner) and dot-sourcing it by a path relative to $PSScriptRoot. If a shared location under tools/ is cleaner (moving GameProcess.ps1 and updating TestRunner's dot-source), say so and do it only if it stays a small diff; TestRunner's behaviour must stay identical.
3. Confirm tools/Kanim/kanim.py and tools/Translations/translations.py only use OxygenNotIncluded as a path (no change), and say so.
4. Update each tool's README where it describes launching, briefly, matching its style.
5. Verify without the game: parse check of every changed .ps1; stand-in checks like OM-012's (renamed cmd.exe copies with and without each tool's arguments) for the shared helper's new parameter; TestRunner's earlier stand-in cases still pass.

Report: C:\dev\ONI-Mods\common\.context\tasks\OM-013-onimoono-other-tools\units\01-implement-report.md — every call site changed (file:line), each tool's argument marker, verification output, which in-game check would prove each tool (narrowest invocation), and text for each tool's code-style.md if its Layout/Indentation needs it. Reply with the report path and a 3-line summary.
Model tier: default
```

## 2. What applies here

The next hop is `ks read`: read the pointer, not the file it sits in.

POINTERS, not contents — ids, one line each, and why each matched. Open what the task needs; the budget was
spent selecting these so yours can be spent reading them.

blk-846080fa7238 · reference
  What this is
  tools/WorkshopUpload/.context/overview.md#what-this-is · names tools/ModTest/Test-OniMods.ps1 → tools/ModTest
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
  tools/TestRunner/.context/domains/entities.md · *.ps1 → tools/TestRunner/GameProcess.ps1
tools/TestRunner:standards/code-style.md · reference
  TestRunner's C# and PowerShell style: indentation and braces, C# 9 on net48 through TestRunner.…
  tools/TestRunner/.context/standards/code-style.md · *.ps1 → tools/TestRunner/GameProcess.ps1 · linked from standards/error-handling.md · linked from standards/naming-conventions.md
tools/TestRunner:standards/error-handling.md · reference
  How TestRunner turns problems into results: the five test statuses, session statuses from Sessi…
  tools/TestRunner/.context/standards/error-handling.md · *.ps1 → tools/TestRunner/GameProcess.ps1 · linked from domains/entities.md · linked from standards/code-style.md
tools/TestRunner:standards/naming-conventions.md · reference
  Naming in TestRunner: files and run folders, the two namespaces, patch classes, Reflect handles…
  tools/TestRunner/.context/standards/naming-conventions.md · *.ps1 → tools/TestRunner/GameProcess.ps1 · linked from domains/entities.md · linked from standards/code-style.md
tools/WorldgenDriver:domains/entities.md · reference
  WorldgenDriver's mod class in Mod.cs, the Driver state and --worldgen options in Driver.cs, the…
  tools/WorldgenDriver/.context/domains/entities.md · *.cs, *.ps1, *.csproj, *.yaml → tools/WorldgenDriver · linked from standards/error-handling.md
tools/WorldgenDriver:standards/code-style.md · reference
  WorldgenDriver's code style for the C# mod and Invoke-Worldgen.ps1: indentation and braces per…
  tools/WorldgenDriver/.context/standards/code-style.md · *.cs, *.ps1, *.csproj, *.yaml → tools/WorldgenDriver · linked from standards/error-handling.md · linked from standards/naming-conventions.md
tools/WorldgenDriver:standards/error-handling.md · reference
  WorldgenDriver's error handling: principles, the result status table with where each is raised,…
  tools/WorldgenDriver/.context/standards/error-handling.md · *.cs, *.ps1, *.csproj, *.yaml → tools/WorldgenDriver · linked from standards/code-style.md
tools/WorldgenDriver:standards/naming-conventions.md · reference
  WorldgenDriver's naming rules: file, run output and Klei-folder names, class, patch class and m…
  tools/WorldgenDriver/.context/standards/naming-conventions.md · *.cs, *.ps1, *.csproj, *.yaml → tools/WorldgenDriver · linked from domains/entities.md · linked from standards/code-style.md
tools/SaveRunner:domains/glossary.md · reference
  SaveRunner glossary: the abbreviations ONI, SMI, STT, SO and NMF used in probe columns and code…
  tools/SaveRunner/.context/domains/glossary.md · linked from domains/entities.md
tools/TestRunner:domains/glossary.md · reference
  TestRunner glossary: the abbreviations ONI, SO, HUD and DLL, and definitions of harness, kit, f…
  tools/TestRunner/.context/domains/glossary.md · linked from domains/entities.md
tools/WorldgenDriver:domains/glossary.md · reference
  WorldgenDriver glossary: abbreviations (ONI, DLC, JSONL, cluster prefixes), terms to avoid with…
  tools/WorldgenDriver/.context/domains/glossary.md · linked from domains/entities.md · linked from standards/naming-conventions.md
blk-647210c6a78a · reference
  What this is
  tools/.context/overview.md#what-this-is · names tools/ModTest/Test-OniMods.ps1 → tools/ModTest
blk-6c2bbf052519 · reference
  Where things are
  tools/.context/overview.md#where-things-are · names tools/ModTest/Test-OniMods.ps1, tools/ModTest/disabled.txt, tools/ModTest/included.txt → tools/ModTest · names tools/SaveRunner/Invoke-SaveRun.ps1 → tools/SaveRunner · names tools/WorldgenDriver/Invoke-Worldgen.ps1 → tools/WorldgenDriver
tags: this brief answered the path question only. A record about a subject rather than a place is reached by `--tag <term>`; the terms are listed by: ks tags --store C:/dev/ONI-Mods/common

⚠ 3 record(s) in these 3 nodes declare no coverage and no tags and name no source path, so a brief reaches them only when a target or a matched record cites them: tools/SaveRunner:tags.md, tools/TestRunner:tags.md, tools/WorldgenDriver:tags.md. Give them `applies_to` or tags through the write path.

**Report back, don't write.** You have no write path here — what follows goes back to the coordinator in your report, not into `.context/`. Once you have opened what this work needed: did anything above turn out not to apply once you opened it (nothing above, if nothing was shown)? Did anything that actually governs this work fail to appear here at all — the miss only you can see? Did any pointer's stated one-line reason not match what its record actually says (nothing above, if nothing was shown)?

## 3. Checkpoint contract

**No checkpoints.** The coordinator sized this work at zero deliberately: this unit owes no progress file, because its only deliverable is its report. Zero is a number that was chosen, not a flag that was forgotten.

Write no checkpoint file, and do not invent one under another name — a file this contract does not name is scratch that nothing will read.

Do the work to its end and report. If you cannot finish, your report is what survives this lane: say in it what is done, what is not, and what you would do next.

## 4. Read-beyond licence

This is a strong default, not a cage — read beyond it whenever the task needs it.
