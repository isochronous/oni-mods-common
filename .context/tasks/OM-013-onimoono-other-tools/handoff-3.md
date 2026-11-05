# Handoff — OM-013-onimoono-other-tools

**Scope:** `tools/SaveRunner`, `tools/WorldgenDriver`
**Store:** `C:/dev/ONI-Mods/common`
**Task scratch:** `C:/dev/ONI-Mods/common/.context/tasks/OM-013-onimoono-other-tools/` — tracked, write-path exempt, never authoritative ([IF-1.C]).

You are reading this because a coordinator passed you its PATH. It is a file, it stays a file, and you can
re-read it after anything truncates your context. Nothing below was passed to you as a payload.

## 1. Task spec

Source: `.context/tasks/OM-013-onimoono-other-tools/units/03-test.md`

```
Task: OM-013 — in-game checks through oni-moono for SaveRunner and WorldgenDriver
Lens: test. Repo C:\dev\ONI-Mods\common, branch OM-013-onimoono-other-tools. Do not commit, do not edit code, never edit Steam settings (launch options start the game through oni-moono; leave them).
Jeremy approved these checks 2026-11-04 and is asleep: there is no one to ask. Do NOT run ModTest (it needs a human to quit the game).

Rules (Jeremy's, hard):
- Sequential, one tool at a time. Exactly ONE run per tool. A second run of a tool only if the first clearly failed for an environmental reason you can name (not a harness bug) — at most one, and never a loop. A tool whose run fails for a reason you can't name: stop that tool, collect logs/screenshots, report.
- SaveRunner launches have been closed from outside before: if a SaveRunner run is closed externally, do not relaunch.
- Never foreground or poll the game window. Never kill a game you did not start; if a game is already open before a run, stop and report.
- After each run, before the next: confirm no OxygenNotIncluded/OniMoono process is left, and the player's mods.json hash equals its pre-run hash (record the pre-run hash first; the runner's backup is the reference). If mods.json differs and the tool did not restore it, STOP all runs and report; do not try to repair it yourself beyond what the tool's documented leftover-backup restore does.
- Kill nothing mid-run; a PowerShell call is capped at 10 min, so start each run with run_in_background and redirect output to a file in C:\Users\thegr\AppData\Local\Temp\claude\C--dev-ONI-Mods\7b932c4a-6569-4783-a8c2-8a995ac90316\scratchpad, then wait for it to finish.

Runs (from common/.context/tasks/OM-013-onimoono-other-tools/units/01-implement-report.md, "The in-game check that proves each tool"); read each tool's README for exact parameters first:
1. SaveRunner: `tools/SaveRunner/Invoke-SaveRun.ps1 <a small save> -Seconds 10 -Without 2854869130,3281716506 -Label moono`. Pick a small save: prefer a fixture save in common/tools/TestRunner/fixtures (never one of Jeremy's colony saves unless no fixture works; the tool copies the save, never modifies the original). Proof: no "did not start within 120 s"; closing line "Game exited ...; status done" with probe rows > 0 (killed-after-status also proves following); mods.json restored.
2. WorldgenDriver: build it first if needed (`dotnet build tools/WorldgenDriver -c Release`), then `tools/WorldgenDriver/Invoke-Worldgen.ps1 SNDST-A-100-0-0-0 -PerSession 1 -Label moono`. Proof: "session 1 exited: 1 result(s)", not "no-process"; config and mods.json restored.
For each: confirm from the copied Player.log that line 1 is "Moono starting ...".

Report: C:\dev\ONI-Mods\common\.context\tasks\OM-013-onimoono-other-tools\units\03-test-report.md — per tool: exact command, verbatim runner output tail, Moono log line, mods.json hashes before/after, leftover processes, verdict. Reply with a 3-line summary.
Model tier: default
```

## 2. What applies here

The next hop is `ks read`: read the pointer, not the file it sits in.

POINTERS, not contents — ids, one line each, and why each matched. Open what the task needs; the budget was
spent selecting these so yours can be spent reading them.

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
tools/WorldgenDriver:domains/glossary.md · reference
  WorldgenDriver glossary: abbreviations (ONI, DLC, JSONL, cluster prefixes), terms to avoid with…
  tools/WorldgenDriver/.context/domains/glossary.md · linked from domains/entities.md · linked from standards/naming-conventions.md
blk-6c2bbf052519 · reference
  Where things are
  tools/.context/overview.md#where-things-are · names tools/SaveRunner/Invoke-SaveRun.ps1 → tools/SaveRunner · names tools/WorldgenDriver/Invoke-Worldgen.ps1 → tools/WorldgenDriver
tags: this brief answered the path question only. A record about a subject rather than a place is reached by `--tag <term>`; the terms are listed by: ks tags --store C:/dev/ONI-Mods/common

⚠ 2 record(s) in these 2 nodes declare no coverage and no tags and name no source path, so a brief reaches them only when a target or a matched record cites them: tools/SaveRunner:tags.md, tools/WorldgenDriver:tags.md. Give them `applies_to` or tags through the write path.

**Report back, don't write.** You have no write path here — what follows goes back to the coordinator in your report, not into `.context/`. Once you have opened what this work needed: did anything above turn out not to apply once you opened it (nothing above, if nothing was shown)? Did anything that actually governs this work fail to appear here at all — the miss only you can see? Did any pointer's stated one-line reason not match what its record actually says (nothing above, if nothing was shown)?

## 3. Checkpoint contract

**No checkpoints.** The coordinator sized this work at zero deliberately: this unit owes no progress file, because its only deliverable is its report. Zero is a number that was chosen, not a flag that was forgotten.

Write no checkpoint file, and do not invent one under another name — a file this contract does not name is scratch that nothing will read.

Do the work to its end and report. If you cannot finish, your report is what survives this lane: say in it what is done, what is not, and what you would do next.

## 4. Read-beyond licence

This is a strong default, not a cage — read beyond it whenever the task needs it.
