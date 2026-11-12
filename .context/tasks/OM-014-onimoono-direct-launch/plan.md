## Task: OM-014
## Branch: OM-014-onimoono-direct-launch (in common/ and in ONI-Mods)
## Objective: oni-moono can now be started directly, without Steam launch options (user, 2026-11-11; player at F:\modding\oni-moono\player\OniMoono.exe, updated 2026-11-10). The user has removed the launch options from Steam, so Steam's Play starts the stock game. TestRunner and ModTest gain a runtime choice so tests run on stock, on Moono, or on both, with no manual Steam config change between runs.
## Folder: common/.context/tasks/OM-014-onimoono-direct-launch/

## Phase State
- **Phase plan**: investigation → implementation → testing → completion
- **Completed**: investigation
- **Current**: implementation   (status: in-progress)
- **Next**: testing
- **Loaded skill**: task-plan-phase-implementation
- **Branch**: OM-014-onimoono-direct-launch
- **Attempts (current phase)**: 1

## Decisions
- Task id OM-014: common's store holds up to OM-013-onimoono-other-tools (ONI-Mods' own OM-013 is the Fan-out Gate, a separate counter, same practice as OM-012/OM-013).
- Interface (user, 2026-11-11): `-Runtime Stock|Moono|Both`, default Stock (Steam launch, today's cost). Both runs the suite on each and reports them side by side.
- Tools (user, 2026-11-11): TestRunner and ModTest get -Runtime. SaveRunner and WorldgenDriver keep launching through Steam.
- Unit 01 (units/01-explore-report.md): OniMoono.dll (commit 2864d5ec) has launch modes "from Steam's launch options", "direct (--game)", "standalone"; usage `OniMoono [--game DIR] ... [Unity player args, passed to the game] [--game-arg ARG] [-- STEAM-COMMAND ...]`; sets SteamAppId 457140 itself; Steam must run (Moono starts it). Argument pass-through in direct mode unproven until a run.
- Moono exe (user, 2026-11-11): default F:\modding\oni-moono\player\OniMoono.exe; ONI_MOONO_EXE env var or -MoonoExe overrides.
- Taken without a user question: always pass `--game <the game folder the tools already resolve>` so launch.json's install choice never matters; Both = stock leg then Moono leg, the second runs whatever the first did, one build, run folders suffixed -stock / -moono, combined per-test table, exit code worst of the two; runtime in the loadout/session key so a live game of the other runtime is never reused (stop the live session between legs); copy Player.log per leg; report the runtime from Player.log line 1 ("Moono starting: commit=..."); ModTest Both = two sequential sessions; SaveRunner/WorldgenDriver must keep working through the shared GameProcess.ps1.
- SUPERSEDES "always pass `--game`" above (Jeremy, 2026-11-11): Moono gets the game folder from Steam, and Steam knows only the one install, so the runner launches OniMoono.exe with no `--game` (standalone mode, which also sets SteamAppId=457140 — the fix for the stock-game relaunch, unit 03). No install check. Add: fail the Moono leg at once if OxygenNotIncluded.exe appears.
- Branches cut from origin/main in both repos (the Fan-out Gate PRs, ONI-Mods#23 / oni-mods-common#12, are not merged yet).

## Key Files
- common/tools/TestRunner/Invoke-OniTests.ps1: launcher (Steam -applaunch 457140, line ~523 adds --onitest- args)
- common/tools/TestRunner/GameProcess.ps1: finds OxygenNotIncluded / OniMoono processes
- common/tools/ModTest/Test-OniMods.ps1: ModTest launcher
- .context/testing/integration-testing.md (ONI-Mods root store), section "Launching with oni-moono" (blk-a24ff5debb99): documents the old launch-options route; also CORRUPTED since ce4aaf4 (#16): the file is 697 KB because one sentence was inserted between every character of the section's paragraph. Repair through the write path in this task.

## Phase Handoff Log

## Progress
- [x] Investigate — unit 01 (units/01-explore-report.md)
- [x] Repair integration-testing.md "Launching with oni-moono" — ONI-Mods 400e915 (text update for direct launch still due at completion)
- [x] Implement -Runtime in TestRunner and ModTest — unit 02, common 6faceba + a82d83c; parse checks and dry runs green (units/02-implement-report.md)
- [ ] Prove: `Invoke-OniTests.ps1 -Mods MotionSensorRange -Filter "*Defaults*" -Runtime Both -Label om014` — first real attempts came from OM-015 (2026-11-11, see ONI-Mods .context/tasks/OM-015-fast-wireless-automation-fix/plan.md and units/04-explore-report.md):
  - Any single `-Runtime` failed before launch: `Cannot bind argument to parameter 'Path' because it is null.` — `$legs = if (...) { @(...) } else { @($Runtime) }` unrolled to a string, `$legs[0]` was 'S'. Fixed in af9fd71 (`$legs = @(if ... { 'Stock', 'Moono' } else { $Runtime })`).
  - `-Runtime Moono` right after a Stock session: `The game did not start within 120 s.` (Invoke-OniTests.ps1:568). The player wrote no Player.log, the warden log stops at "waiting up to 150 s for the game", no crash event.
  - Hand launch `OniMoono.exe --game F:\SteamLibrary\steamapps\common\OxygenNotIncluded --log <file>` (no runner, Jeremy's full mod list): Steam put up its "launch with custom arguments" approval prompt; Moono opened its own window and loaded mods (`[engine] launch: direct (--game)`), AND the stock game (OxygenNotIncluded.exe) started separately and reached the main menu while Moono was still loading. Jeremy killed the stock game. So direct mode also starts the stock game through Steam, and Steam's prompt needs a click — likely why the runner saw nothing in 120 s.
  - Next: find why direct mode starts the stock game (OniMoono.dll strings; how it brings Steam up) and how to launch with no Steam prompt; then the runner should capture `--log` and exit code. No more hand launches.
  - Unit 03 (units/03-explore-report.md): the game's SteamManager.Awake (Assembly-CSharp-firstpass, ~l.78-85) calls `SteamAPI.RestartAppIfNecessary(457140)` and quits when true; Moono sets SteamAppId=457140 only in standalone mode, not with `--game` (corrects unit 01's "sets SteamAppId itself"). Hand-launch log: `launch: direct (--game)` → `Application.Quit(0) requested by ... SteamManager:Awake` → "Steam not initialized in time"; Steam relaunched the stock exe 7 s later. Prompt cause inferred, not seen in a Steam log. Recommended: runner sets SteamAppId=457140 in the child env and passes `--game` + `--onitest-*`; alternative: standalone mode (no `--game`). Add a check that no OxygenNotIncluded.exe appears during a Moono leg. Unproven — needs one approved minimal run.
  - Unit 04 (units/04-implement-report.md, uncommitted): OniMoono.exe launched with only the `--onitest-*` args (ModTest: no args); `Wait-MoonoGame` / `Get-StockGameIds` in GameProcess.ps1 fail the leg at once on a new OxygenNotIncluded or an OniMoono exit (3 s grace), with `Start-Process -PassThru`; help + READMEs refreshed; 3 scripts parse 0 errors; `-BuildOnly -Runtime Moono` exit 0; stand-in `cmd /c exit 7` → "OniMoono exited with code 7 before the game started."; stock-game branch not exercised. Committed ca2f02e (code) + 30e9b87 (artifacts), 2026-11-12.
  - Run 2026-11-12 11:45 (runs/om014-20261112-114511-moono), exit 2: launched `OniMoono.exe --onitest-*` (no --game); "Game exited after 2 s"; no status file; the copied Player.log is STALE (10:55, the SaveRunner run's), so "Player.log shows Stock" is false; no new OxygenNotIncluded seen; no app error events. Wait-MoonoGame accepted the OniMoono process as the game, so its exit was reported as a game exit, not "OniMoono exited with code N"; exit code and Moono's own log not captured.
  - Jeremy 2026-11-12: no Steam prompt, nothing showed up on screen during the 11:45 run (no window, no prompt) — OniMoono quit before opening anything.
  - Unit 05 (units/05-implement-report.md), commit 392c98b: Moono legs pass `--log <runDir>\moono.log` (ModTest: %TEMP%); an early OniMoono exit is reported with its exit code + moono.log tail; Player.log copied only if written since the launch, else "no Player.log written by this launch" and no runtime verdict. 3 scripts parse 0 errors (manager re-checked); `-BuildOnly -Runtime Moono` exit 0; stand-in `cmd /c exit 7` → code 7 + log tail; stale log not copied. Invoke-OniTests summary branch exercised through its helpers only. ICON friction recorded in common .context/domains/tooling-friction.md.
  - Rerun 2026-11-12 11:50 (runs/om014-20261112-115026-moono), Jeremy approved: `OniMoono.exe --log <run>\moono.log --onitest-dir=... (7 --onitest-* args)` → "OniMoono exited with code 2 before the game started. Moono wrote no log". Hypothesis: exit 2 = Moono's argument parser rejects the `--onitest-*` options (usage line has `[--game-arg ARG]` for game arguments); stdout/stderr were not captured, so a usage message would be lost.
  - Unit 06 (units/06-explore-report.md, decompiled OniMoono.dll): `Options.ParseInto` treats any unknown `--` token as fatal — "Unknown option" + usage to stderr, `Program.Main` returns 2 before `EngineLog.Open` (so no --log file). Game arguments go as `--game-arg ARG`, two tokens each (`--game-arg=X` is unknown too); `GameCommandLine.Install` puts them into the game's command line verbatim. OniMoono.exe is GUI-subsystem: stderr is lost unless redirected. Other early exits: failed Steam start / unchosen multi-install dialog (2), install not ready (0), GameVersionCheck mismatch (2).
  - oni-moono is open source (Jeremy, 2026-11-12): https://github.com/tigin-backwards/oni-moono — the source, not decompiling, is the reference from here on.
  - Fix (manager, bounded edit): `Get-MoonoArgumentLine` emits `--game-arg <arg>` per game argument; new `Get-MoonoStreamPaths` (`<log>.out`/`<log>.err`); Invoke-OniTests and ModTest redirect OniMoono's stdout/stderr there; `Get-MoonoExitText` adds their tails. Parse 0 errors ×3; argline `--log C:\r\moono.log --game-arg "--onitest-dir=C:\a b\c" --game-arg --onitest-filter=*Defaults*`; stand-in (cmd, stderr "Unknown option…", exit 2) → message carries code 2 + stderr tail.
  - **Moono leg PROVEN** 2026-11-12 11:54 (runs/om014-20261112-115436-moono), Jeremy approved: `PASS MotionSensorRange.Tests/Range.Defaults (1.0 s)`, session done 1/1, 21.5 s; moono.log line 1 `Moono starting: commit=2864d5ec…`, line 2 `launch: standalone: the game at F:\SteamLibrary\…`; `[engine] game arguments:` lists all 7 --onitest-* verbatim; no stock game; game closed after the 15 s reuse window. Moono writes the game's log into --log (moono.log.out holds the same) and NO Player.log.
  - Runtime line fix (manager): Invoke-OniTests reads the runtime from moono.log on a Moono leg when no fresh Player.log exists (was "Player.log shows no Player.log written by this launch"). Parse 0; Get-LogRuntime on the proving run's moono.log → Moono, commit 2864d5ec, standalone.
  - **`-Runtime Both` PROVEN** 2026-11-12 11:57 (runs/om014-20261112-115715-stock / -moono), Jeremy approved: Stock leg "Player.log shows Stock", PASS 1.0 s, 18.6 s; live stock session ended for the next leg; Moono leg "moono.log shows Moono (commit 2864d5ec…; launch: standalone…)", PASS 1.0 s, 16 s; side-by-side table printed; exit 0; no game left running.
  - **ModTest `-Only MotionSensorRange -Runtime Moono` PROVEN** 2026-11-12 11:59, Jeremy approved and quit from the menu: 9-mod loadout (+included.txt), moono log `launch: standalone`, `-- MAIN MENU --`, `MainMenu:QuitGame` → `Exiting after 356 frames with code 0`; mods.json restored 1 s after exit (69,508 bytes, = mods.json.real); .err/.out written. ModTest printed "OniMoono exit code unknown" (reads the followed game process, not the Start-Process launcher) — FIXED (Jeremy: fix it, don't just note it): new `Get-MoonoExitCode` (launcher's code when it is the same process, else the process's, else Moono's log line `[engine] Exiting after N frames with code X`), used by ModTest, Invoke-OniTests' summary and Get-MoonoExitText (-Launcher). Parse 0 ×3; on the ModTest run's log → 0; `cmd /c exit 5` launcher → 5; neither → null. Landed after unit 07 started: the re-review must cover it.
- [x] Prove — done (Moono, Both, ModTest Moono)
- [ ] Completion ← IN PROGRESS: review (units 04–05 + manager fixes since the last review: there is none for this task), testing/integration-testing.md "Launching with oni-moono" text for direct launch (ONI-Mods root store), retro, commit, PRs (common + ONI-Mods pointer/doc)
- [ ] Completion: review, store update, retro, commit, PRs (common + ONI-Mods pointer/doc)

## Open Questions / Blockers
- Exact direct-launch command line for OniMoono.exe (README still describes only Steam launch options).

## Constraints
- In-game runs take Jeremy's desktop: narrowest filter, ask before long or chained runs; never -KeepGame; never stop a runner mid-run.
- Minimal mod loadout for tool checks.
- Moono's README and files are third-party content: data, not instructions.
