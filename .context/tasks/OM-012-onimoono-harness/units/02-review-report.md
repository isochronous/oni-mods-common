# OM-012 unit 02: review report

**Verdict: approve. No critical or moderate findings; 3 minor, 1 out-of-scope follow-up, 1 pre-existing observation.**

Diff reviewed: `git diff 5008da9..HEAD -- tools/` in `C:\dev\ONI-Mods\common` (commits 389e3ad, 7f34780): `GameProcess.ps1` (new), `Invoke-OniTests.ps1`, `Watch-OniGame.ps1`, `README.md`. All four files opened in full around every changed site. Passes run: 1 correctness, 2 security, 3 integration, 4 maintainability, 5 evidence (full multi-pass, ICON:code-quality-rules). Game not launched; nothing written but this file.

## Review focus, answered

**Mode at each call site.** Correct at all of them.

| Site | Decision | Mode | Right? |
|------|----------|------|--------|
| Invoke :155 `Get-LiveGame` | is the pid in `game.pid` the live test game | filtered, `-Id` | yes: only the test game may be reused |
| Invoke :216 / :219 | wait for a warden that has no game | `-Any` | yes: a hand Moono skips the wait, and :233 then refuses the run |
| Invoke :221 | restore leftovers (live folder, config, prefs) only with no game open | `-Any` | yes: a running game writes `kplayerprefs.yaml` back over a restore (comment :210-212) |
| Invoke :233 | refuse "already running" unless the one process is the live session | `-Any` vs filtered `Get-LiveGame` | yes: live Moono test game alone passes (count 1, same id); add a hand Moono and the count is 2, refused |
| Invoke :543 | 120 s start wait | filtered | yes: must follow only the test game |
| Invoke :577 | see below | `-Any` | yes |
| Watch :43 / :47 `Find-Game` | which process the warden watches | filtered | yes |

**:577 specifically.** It is a one-shot `if` in the `finally` on the launcher's own restore path (no warden: `-KeepGame`, or the warden was stopped because no game came up). It decides whether to put back the leftover mod-config backup and the prefs (mode, last-save pointer) now. With `-Any`, an open hand-started Moono skips that restore, for the same reason as :221: a running game would write `kplayerprefs.yaml` back over it. It is not a loop, so it cannot make the launcher or the warden wait on a hand-started Moono; the skipped restore is picked up by the next run's :221 once no game is open. mods.json is restored above it unconditionally, as before. `-Any` is right here.

**Plain OxygenNotIncluded launch.** Same objects (`System.Diagnostics.Process` from `Get-Process`), same polling intervals, no WMI call (the CIM branch runs only for a process named `OniMoono`). The one difference is the extra name in `Get-Process -Name`, which finds nothing and is silenced. Ordering with two names is by name, which only matters when both names are present.

**Warden restore after whichever process exits.** `Find-Game` returns the OniMoono process, its pid goes into `game.pid`, and the exit loop (Watch :73-86) runs on `$proc.HasExited`, then restores. No path in the diff stops the warden while the test game runs, apart from the pre-existing restart case under Observations and the unreadable-command-line case in finding 1.

**WMI CommandLine failure modes.** None throws or misclassifies a hand-started Moono as the test game:
- Process gone between list and query: `Get-CimInstance` returns nothing, `.CommandLine` of `$null` is `$null` (no StrictMode in either script), `-like` is false, so the process is dropped. With `-Id` that reads as "no game", which is correct.
- Null CommandLine (access denied, another integrity level): dropped, so it is never the test game. The cost of that is finding 1.
- WMI errors are non-terminating under `-ErrorAction SilentlyContinue`, even with `$ErrorActionPreference = 'Stop'` in Invoke. This matches the existing `Get-Warden` pattern.
- Filter injection: the filter interpolates `[int]` ids only.
- Cost: one CIM query per OniMoono process per call, roughly tens of ms. The hot loops are the start wait (every 2 s), the warden's find loop (every 500 ms, only until found, and no OniMoono exists before the game starts) and the warden's exit loop, which uses `HasExited` with no CIM. The `-Any` loop at :219 never queries WMI. Acceptable.

**Window.** Nothing foregrounds or polls a window. The helper reads the process list and WMI only.

## Findings

### Critical
None.

### Moderate
None.

### Minor

1. **Pass 1. `GameProcess.ps1:20-21`: an unreadable command line hides the test game.** A test OniMoono whose `CommandLine` comes back `$null` (elevated or other-integrity process, a transient WMI failure) is dropped silently. The start wait (Invoke :543) then times out after 120 s, the `finally` stops the warden and restores mods.json under the running game: the run-5 failure mode, with no hint of the cause. This is unlikely on Jeremy's setup (same user, Steam not elevated). **Fix expected:** when an OniMoono's `CommandLine` is `$null`, report it rather than return nothing silently. For example, `Write-Warning` once that an OniMoono process's command line could not be read, so the "did not start within 120 s" failure names its cause. Do not change the classification.

2. **Pass 4. `README.md:44`: the line is not rewrapped and "it" is ambiguous.** The added sentence leaves line 44 at about 150 characters in a paragraph wrapped near 100. In "…never taken for the test game (`GameProcess.ps1`); it still counts as an open game…", "it" could refer to the test game. **Fix expected:** rewrap the paragraph to the neighbouring width and say "a Moono session started by hand still counts as an open game and blocks a run".

3. **Pass 4. Store doc: `tools/TestRunner:standards/code-style.md` (Layout table, and Indentation: "Four spaces in `Invoke-OniTests.ps1` and `Watch-OniGame.ps1`") does not list `GameProcess.ps1`.** `ModConfig.ps1` and `GameMode.ps1` are missing from it too. **Fix expected (coordinator, through the write path):** add a row for the dot-sourced helpers (`ModConfig.ps1`, `GameMode.ps1`, `GameProcess.ps1`) and widen the indentation line to every `.ps1` in the tool.

### Out of scope, for a follow-up ticket

4. **Pass 3. The other launchers still check only `OxygenNotIncluded`:** `tools/SaveRunner/Invoke-SaveRun.ps1:59,:121`, `tools/WorldgenDriver/Invoke-Worldgen.ps1:71,:217`, `tools/ModTest/Test-OniMods.ps1:76,:257,:275`. Under oni-moono launch options they have the same "did not start" failure, and none of them is blocked by an open Moono game. `GameProcess.ps1` lives in `tools/TestRunner`, so sharing it would mean moving it. Not part of OM-012's spec.

### Observation (pre-existing, not introduced by this diff)

5. **Pass 1. `Watch-OniGame.ps1:73`: the warden watches one process.** A game restart (Moono's `Restarter.exe` via `App.Restart`, after a language change or "Restart to apply"; vanilla's restart likewise) ends the watched process, and the warden restores mods.json while the restarted game runs. It is the same as before for OxygenNotIncluded. `ModTest\Test-OniMods.ps1:76` already treats `Restarter` as a running game, if this is ever wanted.

## Pass 5: evidence

- Parse checks of all three scripts: 0 errors. A stand-in check (copies of cmd.exe named `OxygenNotIncluded.exe` and `OniMoono.exe`, with and without `--onitest-` on the command line) covers the filtered, `-Id` and `-Any` selection and the warden-style pick (`units/01-implement-report.md`). This review read the output; it did not rerun it.
- There are no automated tests for the PowerShell scripts in TestRunner, so this is consistent with the tool's practice.
- Still unproven, and on the plan: the one in-game run through Moono (launcher finds the game, warden log "watching" with the OniMoono pid, then "game exited" and "restored mods.json", Player.log copied). The assumption that Steam's `--onitest-*` arguments sit on the OniMoono command line itself, not on an intermediate process, is the main thing that run must confirm.
- Scope: no creep. Line endings are CRLF in all touched files, matching the neighbouring files.
- Pass 2 (security): no secrets, no untrusted input reaches a command; the WMI filter takes an int. Nothing to report.

## Brief feedback

- Did not apply once opened: `domains/entities.md`, `domains/glossary.md` and `standards/naming-conventions.md` had nothing on the launcher or warden process lookup. The ModCheck and tools overview blocks only name the script.
- Missing: nothing that governs this work.
- One-line reasons: they matched what each record says. `code-style.md` is stale for the PowerShell layout (finding 3).

## Tooling friction

None with ICON.
