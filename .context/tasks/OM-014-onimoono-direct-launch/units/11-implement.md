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
