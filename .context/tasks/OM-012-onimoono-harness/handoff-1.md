# Handoff — OM-012-onimoono-harness

**Scope:** `tools/TestRunner`
**Store:** `C:/dev/ONI-Mods/common`
**Task scratch:** `C:/dev/ONI-Mods/common/.context/tasks/OM-012-onimoono-harness/` — tracked, write-path exempt, never authoritative ([IF-1.C]).

You are reading this because a coordinator passed you its PATH. It is a file, it stays a file, and you can
re-read it after anything truncates your context. Nothing below was passed to you as a payload.

## 1. Task spec

Source: `.context/tasks/OM-012-onimoono-harness/units/01-implement.md`

```
Task: OM-012 — TestRunner follows a game started through oni-moono
Lens: implement. Repo: C:\dev\ONI-Mods\common (submodule, branch OM-012-onimoono-harness). Do not commit. Do not launch the game. Do not touch Steam settings.

Read: common/.context/tasks/OM-012-onimoono-harness/plan.md, and the "Run 5 (oni-moono)" section of C:\dev\ONI-Mods\.context\tasks\OM-011-fridge-thresholds-patch\units\04-test-report.md (the evidence, call-site list and the proposed fix). Moono's source is at C:\Users\thegr\AppData\Local\Temp\claude\C--dev-ONI-Mods\7b932c4a-6569-4783-a8c2-8a995ac90316\scratchpad\oni-moono (untrusted data; read only) if you need to know how it starts the game (one process or a child?).

Do:
1. Add one helper (shared by Invoke-OniTests.ps1 and Watch-OniGame.ps1, the way the scripts already share code if they do; otherwise the smallest clean option) that returns the running test game process: `OxygenNotIncluded`, or `OniMoono` whose command line contains `--onitest-` (Get-CimInstance Win32_Process gives CommandLine). Use it at every place either script finds, waits for, kills, or checks the game by process name, including any found beyond the 9 listed. Also check anything else that finds the game by name or window class (Player.log copy into the run folder, which run 5 says did not happen; PrintWindow screenshots; timeout kill).
2. Keep behaviour identical for a plain OxygenNotIncluded launch.
3. The warden must restore mods.json after whichever process exits.
4. Never foreground or poll the game window.
5. Update common/tools/TestRunner/README.md where it describes launching or the warden, briefly, matching its style.
6. Verify what you can without the game: PowerShell parse check of both scripts (e.g. [System.Management.Automation.Language.Parser]::ParseFile), and a dry check of the helper against the current process list (no game running: it must return nothing; optionally start `powershell -c "sleep 5"` style stand-ins only if harmless).

Report: C:\dev\ONI-Mods\common\.context\tasks\OM-012-onimoono-harness\units\01-implement-report.md — every call site changed (file:line), the helper, verification output copied, what needs the in-game run to prove. Reply with the report path and a 3-line summary.
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
