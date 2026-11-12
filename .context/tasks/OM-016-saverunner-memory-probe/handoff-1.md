# Handoff — OM-016-saverunner-memory-probe

**Scope:** `tools/SaveRunner`
**Store:** `C:/dev/ONI-Mods-wt/common-OM-016`
**Task scratch:** `C:/dev/ONI-Mods-wt/common-OM-016/.context/tasks/OM-016-saverunner-memory-probe/` — tracked, write-path exempt, never authoritative ([IF-1.C]).

You are reading this because a coordinator passed you its PATH. It is a file, it stays a file, and you can
re-read it after anything truncates your context. Nothing below was passed to you as a payload.

## 1. Task spec

Source: `.context/tasks/OM-016-saverunner-memory-probe/units/01-implement.md`

```
# Unit 01 — implement: SaveRunner `memory` probe

## Objective
Give SaveRunner a probe that shows where a modded ONI's memory goes, for plan step "Implement memory probe + per-mod load deltas". Jeremy's game reached 20.6 GB committed (OniMoono.exe, 156 mods, The Aftermath v009, cycle 904) and he wants to know whether one mod is responsible or the list as a whole. The 156 mods are only 624 MB on disk, so the answer is in runtime memory. Two runs will be compared — full mod list vs no mods — so every number must be comparable between them.

## Scope
- IN: tools/SaveRunner/** in the worktree C:\dev\ONI-Mods-wt\common-OM-016 (branch OM-016-saverunner-memory-probe). Its README gets the new probe and outputs.
- OUT: tools/TestRunner, tools/ModTest, GameProcess.ps1 (OM-014 owns uncommitted changes to those in the other checkout C:\dev\ONI-Mods\common — never touch that checkout). No OniMoono launch path in this unit. Do not launch the game: the in-game runs are the manager's, after this report.
- What the run must answer (you choose how to measure; these are the questions):
  1. Over game time: process private bytes / working set, managed heap (Mono used and reserved, GC.GetTotalMemory), Unity native allocated/reserved, graphics/texture memory. Sampled from main menu, after the save loads, and on through the run, so growth (a leak) is visible apart from the load-time jump. The crash session grew ~100 MB in 11 minutes on top of ~20 GB.
  2. At mod load: how much each mod adds as it loads (private bytes and managed heap before/after each mod), so a single heavy mod stands out. The probe mod has to be measuring before the others load — solve the load-order problem (e.g. Invoke-SaveRun.ps1 already rewrites mods.json) and say how.
  3. A snapshot after the save is loaded: what the memory is made of, attributable to mods where possible — e.g. largest textures and their names, loaded asset bundles, kanim counts, instance counts of component types per assembly (a mod component on every cell or building shows up here). Whatever is cheap in a release Unity player and points at a mod.
- Must not change the save or the existing probes' behaviour; saving stays blocked.
- Acceptance: the dev mod builds; Invoke-SaveRun.ps1 parses with 0 errors and accepts the new probe; the README documents it. Your report must include the actual build and parse command output — do not claim success without showing evidence. Untested-in-game parts are listed as such.

## Context the packet does not carry
- Save: C:\Users\thegr\Documents\Klei\OxygenNotIncluded\save_files\The Aftermath\The Aftermath v009.sav (11.9 MB, verified intact). Runs pass `-Without 2854869130,3281716506` (Mod Profile Manager + Light addon restart the game when a save's mod list differs).
- A full run of a ~150-mod list took ~200 s real time for 90 game seconds (Oct 2026); boot alone takes minutes. Pick a default run length that shows growth without holding Jeremy's desktop for long, and say what you chose.
- Jeremy's mods.json: C:\Users\thegr\Documents\Klei\OxygenNotIncluded\mods\mods.json — the launcher's backup/restore of it must stay exactly as it is.
- Steam now starts the stock game; runs 1–2 are on stock.

## Report
- File: .context/tasks/OM-016-saverunner-memory-probe/units/01-implement-report.md
- Must contain: what was built and where (file list), the exact command lines for run 1 (full mods) and run 2 (no mods), the output files and what each column means, how load order was solved, build + parse output, known limits (what a release player cannot measure), anything left for an in-game run to prove.
- Model tier: complex
```

## 2. What applies here

The next hop is `ks read`: read the pointer, not the file it sits in.

POINTERS, not contents — ids, one line each, and why each matched. Open what the task needs; the budget was
spent selecting these so yours can be spent reading them.

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
tools/SaveRunner:domains/glossary.md · reference
  SaveRunner glossary: the abbreviations ONI, SMI, STT, SO and NMF used in probe columns and code…
  tools/SaveRunner/.context/domains/glossary.md · linked from domains/entities.md
blk-6c2bbf052519 · reference
  Where things are
  tools/.context/overview.md#where-things-are · names tools/SaveRunner/Invoke-SaveRun.ps1 → tools/SaveRunner
tags: this brief answered the path question only. A record about a subject rather than a place is reached by `--tag <term>`; the terms are listed by: ks tags --store C:/dev/ONI-Mods-wt/common-OM-016

⚠ 1 record(s) in this node declare no coverage and no tags and name no source path, so a brief reaches them only when a target or a matched record cites them: tools/SaveRunner:tags.md. Give them `applies_to` or tags through the write path.

**Report back, don't write.** You have no write path here — what follows goes back to the coordinator in your report, not into `.context/`. Once you have opened what this work needed: did anything above turn out not to apply once you opened it (nothing above, if nothing was shown)? Did anything that actually governs this work fail to appear here at all — the miss only you can see? Did any pointer's stated one-line reason not match what its record actually says (nothing above, if nothing was shown)?

## 3. Checkpoint contract

**No checkpoints.** The coordinator sized this work at zero deliberately: this unit owes no progress file, because its only deliverable is its report. Zero is a number that was chosen, not a flag that was forgotten.

Write no checkpoint file, and do not invent one under another name — a file this contract does not name is scratch that nothing will read.

Do the work to its end and report. If you cannot finish, your report is what survives this lane: say in it what is done, what is not, and what you would do next.

## 4. Read-beyond licence

This is a strong default, not a cage — read beyond it whenever the task needs it.
