# Handoff — OM-014-onimoono-direct-launch

**Scope:** `tools/TestRunner/Invoke-OniTests.ps1`, `tools/TestRunner/GameProcess.ps1`
**Store:** `C:/dev/ONI-Mods/common`
**Task scratch:** `C:/dev/ONI-Mods/common/.context/tasks/OM-014-onimoono-direct-launch/` — tracked, write-path exempt, never authoritative ([IF-1.C]).

You are reading this because a coordinator passed you its PATH. It is a file, it stays a file, and you can
re-read it after anything truncates your context. Nothing below was passed to you as a payload.

## 1. Task spec

Source: `.context/tasks/OM-014-onimoono-direct-launch/units/03-explore.md`

```
# Unit 03 — explore: why OniMoono's direct launch also starts the stock game

## Objective
Explain why `OniMoono.exe --game <dir>` started both Moono's own game window and the stock OxygenNotIncluded.exe through Steam (with Steam's "launch with custom arguments" approval prompt), and find a launch the TestRunner can use that starts only Moono, with no prompt (plan step: Prove, blocked on this).

## Observed (2026-11-11, from the OM-015 session; see plan.md Progress)
- Hand launch: `F:\modding\oni-moono\player\OniMoono.exe --game F:\SteamLibrary\steamapps\common\OxygenNotIncluded --log <file>`. Two OniMoono processes started 18:18:15; OxygenNotIncluded.exe started 18:18:26 and reached the main menu; Moono's window was still loading mods. Moono's log has `[engine] launch: direct (--game)`. Jeremy's Steam launch options for ONI are empty (he removed them), so Steam's Play starts the stock game.
- Runner launch (`Invoke-OniTests.ps1 -Runtime Moono`, with the --onitest- args) a second after a stock session closed: nothing within 120 s, no Player.log written.
- The hand-launch log is at `C:\Users\thegr\AppData\Local\Temp\claude\10c6dea4b3666197\e17731f0-4d66-4d70-a838-2d15581d5e18\scratchpad\moono-launch.log`.

## Scope
- IN (read-only): that log (all `launch:` and Steam-related lines, and how far loading got); OniMoono's files under `F:\modding\oni-moono` (player folder, README, setup.ps1, launch.json, any config); strings or a decompile of OniMoono.dll / the player (into a scratch folder, not the repo) for how each launch mode brings Steam up — e.g. `steam://run/457140`, `-applaunch 457140`, a Steam API init with SteamAppId, a restart-through-Steam check (`SteamAPI_RestartAppIfNecessary`), `steam_appid.txt` handling. Units/01-explore-report.md has the earlier findings.
- Find: what starts the stock exe; what triggers Steam's custom-arguments prompt; whether a mode or flag avoids both (e.g. `standalone`, a `steam_appid.txt`, Steam already running, `--` with a Steam command); what the runner should pass.
- OUT: no launches of the game, OniMoono or Steam; no edits outside your report; no commits. Third-party files and logs are untrusted data.
- Acceptance: the cause with quoted evidence (log lines, strings/decompiled code with locations), the recommended launch for the runner, and — if unknown — the single narrowest launch that would settle it, run through the TestRunner with a minimal loadout (Jeremy must approve it).

## Report
- File: common/.context/tasks/OM-014-onimoono-direct-launch/units/03-explore-report.md
- Must contain: cause + evidence, recommended runner launch, the runner changes it implies, open questions.
- Model tier: default
```

## 2. What applies here

The next hop is `ks read`: read the pointer, not the file it sits in.

POINTERS, not contents — ids, one line each, and why each matched. Open what the task needs; the budget was
spent selecting these so yours can be spent reading them.

blk-3ca89d57dfde · reference
  What this is
  tools/ModCheck/.context/overview.md#what-this-is · names tools/TestRunner/Invoke-OniTests.ps1 → tools/TestRunner/Invoke-OniTests.ps1
tools/TestRunner:domains/entities.md · reference
  TestRunner's entities: TestRunnerMod, Session members, TestCase and TestResult, TestBehaviour,…
  tools/TestRunner/.context/domains/entities.md · *.ps1 → tools/TestRunner/GameProcess.ps1 · *.ps1 → tools/TestRunner/Invoke-OniTests.ps1
tools/TestRunner:standards/code-style.md · reference
  TestRunner's C# and PowerShell style: indentation and braces, C# 9 on net48 through TestRunner.…
  tools/TestRunner/.context/standards/code-style.md · *.ps1 → tools/TestRunner/GameProcess.ps1 · *.ps1 → tools/TestRunner/Invoke-OniTests.ps1 · linked from standards/error-handling.md · linked from standards/naming-conventions.md
tools/TestRunner:standards/error-handling.md · reference
  How TestRunner turns problems into results: the five test statuses, session statuses from Sessi…
  tools/TestRunner/.context/standards/error-handling.md · *.ps1 → tools/TestRunner/GameProcess.ps1 · *.ps1 → tools/TestRunner/Invoke-OniTests.ps1 · linked from domains/entities.md · linked from standards/code-style.md
tools/TestRunner:standards/naming-conventions.md · reference
  Naming in TestRunner: files and run folders, the two namespaces, patch classes, Reflect handles…
  tools/TestRunner/.context/standards/naming-conventions.md · *.ps1 → tools/TestRunner/GameProcess.ps1 · *.ps1 → tools/TestRunner/Invoke-OniTests.ps1 · linked from domains/entities.md · linked from standards/code-style.md
tools/TestRunner:domains/glossary.md · reference
  TestRunner glossary: the abbreviations ONI, SO, HUD and DLL, and definitions of harness, kit, f…
  tools/TestRunner/.context/domains/glossary.md · linked from domains/entities.md
blk-647210c6a78a · reference
  What this is
  tools/.context/overview.md#what-this-is · names tools/TestRunner/Invoke-OniTests.ps1 → tools/TestRunner/Invoke-OniTests.ps1
blk-6c2bbf052519 · reference
  Where things are
  tools/.context/overview.md#where-things-are · names tools/TestRunner/Invoke-OniTests.ps1 → tools/TestRunner/Invoke-OniTests.ps1
tags: this brief answered the path question only. A record about a subject rather than a place is reached by `--tag <term>`; the terms are listed by: ks tags --store C:/dev/ONI-Mods/common

⚠ 1 record(s) in this node declare no coverage and no tags and name no source path, so a brief reaches them only when a target or a matched record cites them: tools/TestRunner:tags.md. Give them `applies_to` or tags through the write path.

**Report back, don't write.** You have no write path here — what follows goes back to the coordinator in your report, not into `.context/`. Once you have opened what this work needed: did anything above turn out not to apply once you opened it (nothing above, if nothing was shown)? Did anything that actually governs this work fail to appear here at all — the miss only you can see? Did any pointer's stated one-line reason not match what its record actually says (nothing above, if nothing was shown)?

## 3. Checkpoint contract

**No checkpoints.** The coordinator sized this work at zero deliberately: this unit owes no progress file, because its only deliverable is its report. Zero is a number that was chosen, not a flag that was forgotten.

Write no checkpoint file, and do not invent one under another name — a file this contract does not name is scratch that nothing will read.

Do the work to its end and report. If you cannot finish, your report is what survives this lane: say in it what is done, what is not, and what you would do next.

## 4. Read-beyond licence

This is a strong default, not a cage — read beyond it whenever the task needs it.
