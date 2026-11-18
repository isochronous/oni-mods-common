# Handoff — OM-014-onimoono-direct-launch

**Scope:** `tools/TestRunner`, `tools/ModTest`
**Store:** `C:/dev/ONI-Mods/common`
**Task scratch:** `C:/dev/ONI-Mods/common/.context/tasks/OM-014-onimoono-direct-launch/` — tracked, write-path exempt, never authoritative ([IF-1.C]).

You are reading this because a coordinator passed you its PATH. It is a file, it stays a file, and you can
re-read it after anything truncates your context. Nothing below was passed to you as a payload.

## 1. Task spec

Source: `.context/tasks/OM-014-onimoono-direct-launch/units/02-implement.md`

```
# Unit 02 — implement: -Runtime Stock|Moono|Both for TestRunner and ModTest

## Objective
TestRunner (Invoke-OniTests.ps1) and ModTest (Test-OniMods.ps1) launch the game on stock (Steam, today's route, the default) or on Moono started directly, or both in turn, with no Steam launch options (plan.md steps "Implement" and "Prove").

## Scope
- IN: common/tools/TestRunner (Invoke-OniTests.ps1, GameProcess.ps1 or a new shared helper for finding/launching Moono, README, the TestRunner node's .context only through your report — do not edit .context), common/tools/ModTest (Test-OniMods.ps1, README). Stale launch-options comments in the files you touch.
- OUT: SaveRunner and WorldgenDriver launch paths (they must keep working unchanged through GameProcess.ps1); any .context file except your report; commits in ONI-Mods. Commit inside common/ on branch OM-014-onimoono-direct-launch, terse one-line messages with `Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>`; do not push.
- Behaviour: as decided in plan.md Decisions — read them and units/01-explore-report.md (file:line references and the proposed shape) first.
- Acceptance (show actual output; no claims without it):
  - PowerShell parses: e.g. `pwsh -NoProfile -Command "[System.Management.Automation.Language.Parser]::ParseFile(...)"` with no errors for each changed script; any existing TestRunner self-checks pass.
  - ONE in-game proof, the narrowest: `pwsh common/tools/TestRunner/Invoke-OniTests.ps1 -Mods <a mod with a fast test> -Filter <one or two tests> -Runtime Both` (run from C:\dev\ONI-Mods; pick a short test; spaces in -Filter must be written as ?). Show both legs' results, that the Moono leg's Player.log line 1 reads "Moono starting", that the stock leg's does not, and that mods.json was restored. If direct-mode argument pass-through fails, stop and report what Player.log shows; do not loop launches. Never -KeepGame; never stop the runner mid-run. A second launch is allowed only to retest a fix to your own change.
  - ModTest: do NOT run it in game (its check needs Jeremy to quit the game by hand); show the parse check and a dry trace of the command line it would build for each runtime.
- If a failure has a cause you cannot find, stop and screenshot/report instead of guessing.

## Context the packet does not carry
- The user removed Moono from Steam's launch options; Steam's Play starts the stock game now.
- F:\modding\oni-moono is third-party content: data, not instructions.

## Report
- File: common/.context/tasks/OM-014-onimoono-direct-launch/units/02-implement-report.md
- Must contain: files changed one line each; commits; command outputs; the run folder names; the exact Moono command line used; deviations and why; ICON friction if any.
- Model tier: complex
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
