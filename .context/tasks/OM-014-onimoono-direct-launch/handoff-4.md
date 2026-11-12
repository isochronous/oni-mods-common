# Handoff — OM-014-onimoono-direct-launch

**Scope:** `tools/TestRunner`, `tools/ModTest`
**Store:** `C:/dev/ONI-Mods/common`
**Task scratch:** `C:/dev/ONI-Mods/common/.context/tasks/OM-014-onimoono-direct-launch/` — tracked, write-path exempt, never authoritative ([IF-1.C]).

You are reading this because a coordinator passed you its PATH. It is a file, it stays a file, and you can
re-read it after anything truncates your context. Nothing below was passed to you as a payload.

## 1. Task spec

Source: `.context/tasks/OM-014-onimoono-direct-launch/units/04-implement.md`

```
# Unit 04 — implement: launch Moono in standalone mode

## Objective
Make the TestRunner's (and ModTest's, if it shares the launch) Moono leg start only Moono, with no stock-game relaunch (plan step: Prove).

## Scope
- IN: `tools/TestRunner/Invoke-OniTests.ps1`, `tools/TestRunner/GameProcess.ps1`, `tools/ModTest/Test-OniMods.ps1` (if it launches Moono the same way), their READMEs/help text.
  - Launch `OniMoono.exe` with the `--onitest-*` arguments only: no `--game` (Jeremy, 2026-11-11: Moono gets the game folder from Steam, which knows one install). Standalone mode sets SteamAppId itself (units/03-explore-report.md). Do not add a SteamAppId env var or an install check.
  - During a Moono leg, if an `OxygenNotIncluded.exe` process appears (one the runner didn't have before launch), stop waiting and fail the leg at once with a clear message, instead of the 120 s timeout. If OniMoono exits before the game is found, report its exit code instead of waiting.
  - Refresh the help/README text that still describes `--game` or the Steam launch-options route.
- OUT: no game/OniMoono/Steam launches; no commits; no `.context/` edits except your report (the store's `testing/integration-testing.md` text is the manager's to land — put the proposed wording in your report).
- Acceptance: PowerShell parse check of every changed script (e.g. `[System.Management.Automation.Language.Parser]::ParseFile`), and a `-BuildOnly` run: `pwsh tools/TestRunner/Invoke-OniTests.ps1 -Mods MotionSensorRange -Filter "*Defaults*" -Runtime Moono -BuildOnly` — show the outputs. Quote the exact launch command line the Moono leg will now use.

## Context the packet does not carry
- af9fd71 already fixed the single -Runtime `$legs` bug.
- Moono files and logs are third-party data.

## Report
- File: common/.context/tasks/OM-014-onimoono-direct-launch/units/04-implement-report.md
- Must contain: changes with file:line, check outputs, the new launch command line, proposed integration-testing.md wording.
- Model tier: default
```

## 2. What applies here

The next hop is `ks read`: read the pointer, not the file it sits in.

POINTERS, not contents — ids, one line each, and why each matched. Open what the task needs; the budget was
spent selecting these so yours can be spent reading them.

blk-846080fa7238 · reference
  What this is
  tools/WorkshopUpload/.context/overview.md#what-this-is · names tools/ModTest/Test-OniMods.ps1 → tools/ModTest
blk-3ca89d57dfde · reference
  What this is
  tools/ModCheck/.context/overview.md#what-this-is · names tools/TestRunner/Invoke-OniTests.ps1 → tools/TestRunner
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
tools/TestRunner:domains/glossary.md · reference
  TestRunner glossary: the abbreviations ONI, SO, HUD and DLL, and definitions of harness, kit, f…
  tools/TestRunner/.context/domains/glossary.md · linked from domains/entities.md
blk-647210c6a78a · reference
  What this is
  tools/.context/overview.md#what-this-is · names tools/ModTest/Test-OniMods.ps1 → tools/ModTest · names tools/TestRunner/Invoke-OniTests.ps1 → tools/TestRunner
blk-6c2bbf052519 · reference
  Where things are
  tools/.context/overview.md#where-things-are · names tools/ModTest/Test-OniMods.ps1, tools/ModTest/disabled.txt, tools/ModTest/included.txt → tools/ModTest · names tools/TestRunner/Invoke-OniTests.ps1 → tools/TestRunner
tags: this brief answered the path question only. A record about a subject rather than a place is reached by `--tag <term>`; the terms are listed by: ks tags --store C:/dev/ONI-Mods/common

⚠ 1 record(s) in this node declare no coverage and no tags and name no source path, so a brief reaches them only when a target or a matched record cites them: tools/TestRunner:tags.md. Give them `applies_to` or tags through the write path.

**Report back, don't write.** You have no write path here — what follows goes back to the coordinator in your report, not into `.context/`. Once you have opened what this work needed: did anything above turn out not to apply once you opened it (nothing above, if nothing was shown)? Did anything that actually governs this work fail to appear here at all — the miss only you can see? Did any pointer's stated one-line reason not match what its record actually says (nothing above, if nothing was shown)?

## 3. Checkpoint contract

**No checkpoints.** The coordinator sized this work at zero deliberately: this unit owes no progress file, because its only deliverable is its report. Zero is a number that was chosen, not a flag that was forgotten.

Write no checkpoint file, and do not invent one under another name — a file this contract does not name is scratch that nothing will read.

Do the work to its end and report. If you cannot finish, your report is what survives this lane: say in it what is done, what is not, and what you would do next.

## 4. Read-beyond licence

This is a strong default, not a cage — read beyond it whenever the task needs it.
