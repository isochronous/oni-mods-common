# Handoff — OM-014-onimoono-direct-launch

**Scope:** `tools/TestRunner/Invoke-OniTests.ps1`, `tools/TestRunner/GameProcess.ps1`, `tools/TestRunner/Watch-OniGame.ps1`, `tools/ModTest/Test-OniMods.ps1`
**Store:** `C:/dev/ONI-Mods/common`
**Task scratch:** `C:/dev/ONI-Mods/common/.context/tasks/OM-014-onimoono-direct-launch/` — tracked, write-path exempt, never authoritative ([IF-1.C]).

You are reading this because a coordinator passed you its PATH. It is a file, it stays a file, and you can
re-read it after anything truncates your context. Nothing below was passed to you as a payload.

## 1. Task spec

Source: `.context/tasks/OM-014-onimoono-direct-launch/units/13-implement.md`

```
# Unit 13 — implement: reused Moono session's runtime line, and unit 12's three findings

## Objective
Fix the last defects found in OM-014's own diff before the PR (Jeremy: every bug in the task's diff is fixed in the task).

## Defects
1. **Reused Moono session reports no game log.** In-game, 2026-11-12 12:25 (runs/om014-reuse2-20261112-122519-moono): "The running game took the run (same loadout); no restart." then "Runtime: Moono asked; no game log written by this launch" and "Moono's log: none written". The live game was started by run 1 with `--log runs/om014-reuse1-…/moono.log` and keeps writing there. On a reused Moono session, read the runtime from (and name) the live session's moono.log — e.g. record its path in the live dir when the session is launched and read it back on reuse. Copy or point to it as fits the run folder conventions; say what you chose.
2. units/12-review-report.md, all three low findings: ModTest's wait for a stray stock game has no timeout; the warden follows only the first stray; an empty `$stockBefore` only defers the restore. Fix each (or show from the code why one is not a defect, with file:line).

## Scope
- IN: tools/TestRunner/Invoke-OniTests.ps1, tools/TestRunner/GameProcess.ps1, tools/TestRunner/Watch-OniGame.ps1, tools/ModTest/Test-OniMods.ps1, READMEs, in C:\dev\ONI-Mods\common (branch OM-014-onimoono-direct-launch).
- OUT: stock behaviour unchanged (a reused stock session must still read its fresh Player.log); no game or OniMoono launch; never touch running processes; tests on scratch copies only, never Jeremy's real mods.json/prefs/config.
- Acceptance: scripts parse with 0 errors; `-BuildOnly -Mods MotionSensorRange -Runtime Moono` exits 0; offline checks for each fix (for 1, exercise the reuse summary with a fake live dir pointing at an existing moono.log such as runs/om014-reuse1-20261112-122449-moono/moono.log). Your report must include actual command output.
- Do not create git commits until all edits are done (each commit changes the SHA stamped into TestRunner.dll); then one commit.

## Context the packet does not carry
- units/11-implement-report.md, units/12-review-report.md.

## Report
- File: .context/tasks/OM-014-onimoono-direct-launch/units/13-implement-report.md
- Must contain: each defect → fix (file:line), offline checks with output, what remains for an in-game run, commit hash.
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
