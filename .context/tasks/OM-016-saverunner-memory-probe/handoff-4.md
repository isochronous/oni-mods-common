# Handoff — OM-016-saverunner-memory-probe

**Scope:** `tools/SaveRunner/MemoryProbe.cs`, `tools/SaveRunner/README.md`
**Store:** `C:/dev/ONI-Mods-wt/common-OM-016`
**Task scratch:** `C:/dev/ONI-Mods-wt/common-OM-016/.context/tasks/OM-016-saverunner-memory-probe/` — tracked, write-path exempt, never authoritative ([IF-1.C]).

You are reading this because a coordinator passed you its PATH. It is a file, it stays a file, and you can
re-read it after anything truncates your context. Nothing below was passed to you as a payload.

## 1. Task spec

Source: `.context/tasks/OM-016-saverunner-memory-probe/units/04-review.md`

```
# Unit 04 — review: the review fixes (54dcd5a..1fbb20a)

## Objective
Confirm commit 1fbb20a fixes review findings F1–F4 of units/02-review-report.md and introduces nothing new, so the review checkpoint covers the final diff.

## Scope
- IN: `git diff 54dcd5a..1fbb20a` in C:\dev\ONI-Mods-wt\common-OM-016 (tools/SaveRunner/MemoryProbe.cs, README.md).
- OUT: the rest of the change (reviewed in unit 02); docs under .context (manager's). Do not launch the game; change no code.
- Acceptance: per finding F1–F4, fixed / not fixed with file:line; any new finding with severity and expected fix; or "no new findings" with the passes run.

## Context the packet does not carry
- units/02-review-report.md, units/03-implement-report.md.

## Report
- File: .context/tasks/OM-016-saverunner-memory-probe/units/04-review-report.md
- Model tier: default
```

## 2. What applies here

The next hop is `ks read`: read the pointer, not the file it sits in.

POINTERS, not contents — ids, one line each, and why each matched. Open what the task needs; the budget was
spent selecting these so yours can be spent reading them.

tools/SaveRunner:domains/entities.md · reference
  SaveRunner's entities: SaveRunnerMod, Runner and its members, each Harmony patch in Patches.cs,…
  tools/SaveRunner/.context/domains/entities.md · *.cs → tools/SaveRunner/MemoryProbe.cs
tools/SaveRunner:standards/code-style.md · reference
  SaveRunner's C# and PowerShell style: indentation and braces, C# 9 on net48 through SaveRunner.…
  tools/SaveRunner/.context/standards/code-style.md · *.cs → tools/SaveRunner/MemoryProbe.cs · linked from standards/error-handling.md · linked from standards/naming-conventions.md
tools/SaveRunner:standards/error-handling.md · reference
  How SaveRunner ends and reports a run: the four status values Runner.Finish writes, the RunBeha…
  tools/SaveRunner/.context/standards/error-handling.md · *.cs → tools/SaveRunner/MemoryProbe.cs · linked from standards/code-style.md
tools/SaveRunner:standards/naming-conventions.md · reference
  Naming in SaveRunner: C# file and type names, the <Type>_<Method>_Patch classes, Runner state f…
  tools/SaveRunner/.context/standards/naming-conventions.md · *.cs → tools/SaveRunner/MemoryProbe.cs · linked from domains/entities.md · linked from standards/code-style.md
tools/SaveRunner:domains/glossary.md · reference
  SaveRunner glossary: the abbreviations ONI, SMI, STT, SO and NMF used in probe columns and code…
  tools/SaveRunner/.context/domains/glossary.md · linked from domains/entities.md
tags: this brief answered the path question only. A record about a subject rather than a place is reached by `--tag <term>`; the terms are listed by: ks tags --store C:/dev/ONI-Mods-wt/common-OM-016

⚠ 1 record(s) in this node declare no coverage and no tags and name no source path, so a brief reaches them only when a target or a matched record cites them: tools/SaveRunner:tags.md. Give them `applies_to` or tags through the write path.

**Report back, don't write.** You have no write path here — what follows goes back to the coordinator in your report, not into `.context/`. Once you have opened what this work needed: did anything above turn out not to apply once you opened it (nothing above, if nothing was shown)? Did anything that actually governs this work fail to appear here at all — the miss only you can see? Did any pointer's stated one-line reason not match what its record actually says (nothing above, if nothing was shown)?

## 3. Checkpoint contract

**No checkpoints.** The coordinator sized this work at zero deliberately: this unit owes no progress file, because its only deliverable is its report. Zero is a number that was chosen, not a flag that was forgotten.

Write no checkpoint file, and do not invent one under another name — a file this contract does not name is scratch that nothing will read.

Do the work to its end and report. If you cannot finish, your report is what survives this lane: say in it what is done, what is not, and what you would do next.

## 4. Read-beyond licence

This is a strong default, not a cage — read beyond it whenever the task needs it.
