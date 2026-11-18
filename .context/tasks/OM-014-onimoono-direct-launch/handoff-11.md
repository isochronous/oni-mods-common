# Handoff — OM-014-onimoono-direct-launch

**Scope:** `tools/TestRunner/GameProcess.ps1`, `tools/TestRunner/Invoke-OniTests.ps1`, `tools/TestRunner/Watch-OniGame.ps1`, `tools/ModTest/Test-OniMods.ps1`
**Store:** `C:/dev/ONI-Mods/common`
**Task scratch:** `C:/dev/ONI-Mods/common/.context/tasks/OM-014-onimoono-direct-launch/` — tracked, write-path exempt, never authoritative ([IF-1.C]).

You are reading this because a coordinator passed you its PATH. It is a file, it stays a file, and you can
re-read it after anything truncates your context. Nothing below was passed to you as a payload.

## 1. Task spec

Source: `.context/tasks/OM-014-onimoono-direct-launch/units/11-implement.md`

```
# Unit 11 — implement: a stray stock game after a failed Moono launch goes to the warden

## Objective
Fix unit 10's finding (units/10-review-report.md): after a failed Moono leg, a stock OxygenNotIncluded that appeared during the launch is left running, so Invoke-OniTests skips `Restore-LeftoverConfig` / the prefs restore (Invoke-OniTests.ps1 ~:630-634) and nothing ever restores them.

## Decision (Jeremy, 2026-11-12: "Hand it to the warden")
- Stop such a stock game only when it provably belongs to the launch: it descends from the launcher/OniMoono processes `Stop-MoonoLaunch` already ends, or its command line carries this run's `--onitest-` arguments (e.g. this run's `--onitest-dir`).
- Otherwise leave it running and hand the restore (config, prefs; mods.json if not already back) to the warden (`Watch-OniGame.ps1`, started via `Start-Warden`), which restores once that game exits. Nothing the player opened is ever killed.

## Scope
- IN: tools/TestRunner/GameProcess.ps1 (`Stop-MoonoLaunch`), tools/TestRunner/Invoke-OniTests.ps1 (the Moono failure path in `finally`), tools/TestRunner/Watch-OniGame.ps1 only if it needs to follow a given pid, tools/ModTest/Test-OniMods.ps1 if ModTest has the same gap (say whether it does; ModTest has no warden — use its own restore-on-exit pattern), READMEs.
- OUT: stock-leg behaviour unchanged; no game or OniMoono launch; never touch running OniMoono/OxygenNotIncluded processes.
- Acceptance: three scripts (plus Watch-OniGame.ps1 if changed) parse with 0 errors; `-BuildOnly -Runtime Moono` exits 0; offline stand-ins (cmd.exe copies named OxygenNotIncluded.exe / OniMoono.exe, as in the manager's scratch test …/scratchpad/stop-test.ps1): (a) a stock stand-in started by the fake launcher is stopped; (b) one carrying this run's --onitest-dir is stopped; (c) an unrelated one survives and a warden is started that restores after it exits — show the restore happening against a scratch copy of the files, never Jeremy's real mods.json/prefs. Your report must include actual command output.

## Context the packet does not carry
- units/09-review-report.md, units/10-review-report.md; units/08-implement-report.md (Stop-MoonoLaunch introduced); manager commit 38fdbf1 (Stop-MoonoLaunch by descent/log path).
- Also answer from the code, if you can: is Moono's `--crash-watch` OniMoono a descendant of the launcher or of the game process (unit 10 left it unverified)?

## Report
- File: .context/tasks/OM-014-onimoono-direct-launch/units/11-implement-report.md
- Must contain: changes (file:line), offline checks with output, what only an in-game run can prove, commit hash.
- Model tier: default
```

## 2. What applies here

The next hop is `ks read`: read the pointer, not the file it sits in.

POINTERS, not contents — ids, one line each, and why each matched. Open what the task needs; the budget was
spent selecting these so yours can be spent reading them.

blk-846080fa7238 · reference
  What this is
  tools/WorkshopUpload/.context/overview.md#what-this-is · names tools/ModTest/Test-OniMods.ps1 → tools/ModTest/Test-OniMods.ps1
blk-3ca89d57dfde · reference
  What this is
  tools/ModCheck/.context/overview.md#what-this-is · names tools/TestRunner/Invoke-OniTests.ps1 → tools/TestRunner/Invoke-OniTests.ps1
tools/TestRunner:domains/entities.md · reference
  TestRunner's entities: TestRunnerMod, Session members, TestCase and TestResult, TestBehaviour,…
  tools/TestRunner/.context/domains/entities.md · *.ps1 → tools/TestRunner/GameProcess.ps1 · *.ps1 → tools/TestRunner/Invoke-OniTests.ps1 · *.ps1 → tools/TestRunner/Watch-OniGame.ps1
tools/TestRunner:standards/code-style.md · reference
  TestRunner's C# and PowerShell style: indentation and braces, C# 9 on net48 through TestRunner.…
  tools/TestRunner/.context/standards/code-style.md · *.ps1 → tools/TestRunner/GameProcess.ps1 · *.ps1 → tools/TestRunner/Invoke-OniTests.ps1 · *.ps1 → tools/TestRunner/Watch-OniGame.ps1 · linked from standards/error-handling.md · linked from standards/naming-conventions.md
tools/TestRunner:standards/error-handling.md · reference
  How TestRunner turns problems into results: the five test statuses, session statuses from Sessi…
  tools/TestRunner/.context/standards/error-handling.md · *.ps1 → tools/TestRunner/GameProcess.ps1 · *.ps1 → tools/TestRunner/Invoke-OniTests.ps1 · *.ps1 → tools/TestRunner/Watch-OniGame.ps1 · linked from domains/entities.md · linked from standards/code-style.md
tools/TestRunner:standards/naming-conventions.md · reference
  Naming in TestRunner: files and run folders, the two namespaces, patch classes, Reflect handles…
  tools/TestRunner/.context/standards/naming-conventions.md · *.ps1 → tools/TestRunner/GameProcess.ps1 · *.ps1 → tools/TestRunner/Invoke-OniTests.ps1 · *.ps1 → tools/TestRunner/Watch-OniGame.ps1 · linked from domains/entities.md · linked from standards/code-style.md
tools/TestRunner:domains/glossary.md · reference
  TestRunner glossary: the abbreviations ONI, SO, HUD and DLL, and definitions of harness, kit, f…
  tools/TestRunner/.context/domains/glossary.md · linked from domains/entities.md
blk-647210c6a78a · reference
  What this is
  tools/.context/overview.md#what-this-is · names tools/ModTest/Test-OniMods.ps1 → tools/ModTest/Test-OniMods.ps1 · names tools/TestRunner/Invoke-OniTests.ps1 → tools/TestRunner/Invoke-OniTests.ps1
blk-6c2bbf052519 · reference
  Where things are
  tools/.context/overview.md#where-things-are · names tools/ModTest/Test-OniMods.ps1 → tools/ModTest/Test-OniMods.ps1 · names tools/TestRunner/Invoke-OniTests.ps1 → tools/TestRunner/Invoke-OniTests.ps1
tags: this brief answered the path question only. A record about a subject rather than a place is reached by `--tag <term>`; the terms are listed by: ks tags --store C:/dev/ONI-Mods/common

⚠ 1 record(s) in this node declare no coverage and no tags and name no source path, so a brief reaches them only when a target or a matched record cites them: tools/TestRunner:tags.md. Give them `applies_to` or tags through the write path.

**Report back, don't write.** You have no write path here — what follows goes back to the coordinator in your report, not into `.context/`. Once you have opened what this work needed: did anything above turn out not to apply once you opened it (nothing above, if nothing was shown)? Did anything that actually governs this work fail to appear here at all — the miss only you can see? Did any pointer's stated one-line reason not match what its record actually says (nothing above, if nothing was shown)?

## 3. Checkpoint contract

**No checkpoints.** The coordinator sized this work at zero deliberately: this unit owes no progress file, because its only deliverable is its report. Zero is a number that was chosen, not a flag that was forgotten.

Write no checkpoint file, and do not invent one under another name — a file this contract does not name is scratch that nothing will read.

Do the work to its end and report. If you cannot finish, your report is what survives this lane: say in it what is done, what is not, and what you would do next.

## 4. Read-beyond licence

This is a strong default, not a cage — read beyond it whenever the task needs it.
