# Handoff — OM-012-onimoono-harness

**Scope:** `tools/TestRunner`
**Store:** `C:/dev/ONI-Mods/common`
**Task scratch:** `C:/dev/ONI-Mods/common/.context/tasks/OM-012-onimoono-harness/` — tracked, write-path exempt, never authoritative ([IF-1.C]).

You are reading this because a coordinator passed you its PATH. It is a file, it stays a file, and you can
re-read it after anything truncates your context. Nothing below was passed to you as a payload.

## 1. Task spec

Source: `.context/tasks/OM-012-onimoono-harness/units/02-review.md`

```
Feature: TestRunner follows a game started through oni-moono, and an open Moono game blocks a run
Ticket: OM-012
Lens: review (read-only; write only your report). Do not launch the game.
Diff: in C:\dev\ONI-Mods\common, `git diff 5008da9..HEAD -- tools/`
Changed files:
  - tools/TestRunner/GameProcess.ps1 (new): Get-GameProcess (OxygenNotIncluded, or OniMoono with --onitest- in its command line; -Id; -Any = any OniMoono too)
  - tools/TestRunner/Invoke-OniTests.ps1: every game lookup uses it; -Any at the pre-launch "already running" checks (:216 :219 :221 :233) and :577
  - tools/TestRunner/Watch-OniGame.ps1: Find-Game uses it
  - tools/TestRunner/README.md: launching/warden sentences
Context: plan common/.context/tasks/OM-012-onimoono-harness/plan.md; implement report units/01-implement-report.md; evidence C:\dev\ONI-Mods\.context\tasks\OM-011-fridge-thresholds-patch\units\04-test-report.md "Run 5 (oni-moono)".
Review focus:
  - Each call site uses the right mode: pre-launch refusal and leftover-file decisions use -Any; the start wait, live-game lookups and warden follow only the test game. Check :577 specifically: what decision is it, and is -Any right there (e.g. would it make the warden or end-of-run wait on a hand-started Moono)?
  - A plain OxygenNotIncluded launch behaves exactly as before (same objects, same timing).
  - The warden restores mods.json after whichever process exits; no path where it stops while the test game is still running.
  - WMI CommandLine access: failure modes (access denied, process gone between list and query, null CommandLine) handled without throwing or misclassifying; cost if called in a tight loop.
  - Nothing foregrounds or polls the game window.
Report: C:\dev\ONI-Mods\common\.context\tasks\OM-012-onimoono-harness\units\02-review-report.md — findings with severity and file:line, each with the fix expected; "no findings" only with the passes run.
Model tier: default
```

## 2. What applies here

The next hop is `ks read`: read the pointer, not the file it sits in.

POINTERS, not contents — ids, one line each, and why each matched. Open what the task needs; the budget was
spent selecting these so yours can be spent reading them.

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
  tools/.context/overview.md#what-this-is · names tools/TestRunner/Invoke-OniTests.ps1 → tools/TestRunner
blk-6c2bbf052519 · reference
  Where things are
  tools/.context/overview.md#where-things-are · names tools/TestRunner/Invoke-OniTests.ps1 → tools/TestRunner
tags: this brief answered the path question only. A record about a subject rather than a place is reached by `--tag <term>`; the terms are listed by: ks tags --store C:/dev/ONI-Mods/common

⚠ 1 record(s) in this node declare no coverage and no tags and name no source path, so a brief reaches them only when a target or a matched record cites them: tools/TestRunner:tags.md. Give them `applies_to` or tags through the write path.

**Report back, don't write.** You have no write path here — what follows goes back to the coordinator in your report, not into `.context/`. Once you have opened what this work needed: did anything above turn out not to apply once you opened it (nothing above, if nothing was shown)? Did anything that actually governs this work fail to appear here at all — the miss only you can see? Did any pointer's stated one-line reason not match what its record actually says (nothing above, if nothing was shown)?

## 3. Checkpoint contract

**No checkpoints.** The coordinator sized this work at zero deliberately: this unit owes no progress file, because its only deliverable is its report. Zero is a number that was chosen, not a flag that was forgotten.

Write no checkpoint file, and do not invent one under another name — a file this contract does not name is scratch that nothing will read.

Do the work to its end and report. If you cannot finish, your report is what survives this lane: say in it what is done, what is not, and what you would do next.

## 4. Read-beyond licence

This is a strong default, not a cage — read beyond it whenever the task needs it.
