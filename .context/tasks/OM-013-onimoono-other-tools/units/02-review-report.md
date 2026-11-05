# OM-013 unit 02: review report

**Verdict: approve after one moderate fix (ModTest restart wait). 0 critical, 1 moderate, 3 minor, 1 out-of-scope note.**

Diff reviewed: `git diff 1d45792..326871b -- tools/` in `C:\dev\ONI-Mods\common` (commits 9b3b138, 5c48ea1, 326871b; HEAD = 326871b, no uncommitted changes under `tools/`). Every changed file was opened around each changed site: `GameProcess.ps1` in full, plus the start, wait, kill and `finally` blocks of `Invoke-SaveRun.ps1`, `Invoke-Worldgen.ps1` and `Test-OniMods.ps1`, and every `Get-GameProcess` caller in TestRunner. I ran the full multi-pass review from ICON:code-quality-rules: 1 correctness, 2 security, 3 integration, 4 maintainability, 5 evidence. I did not launch the game or start any game-named process. I parse-checked the six scripts and ran one throwaway missing-dot-source script in the scratchpad (`scratchpad\om013rev\t.ps1`). This report is the only file I wrote.

## Review focus, answered

**TestRunner unchanged.** `Get-GameProcess([int]$Id = 0, [switch]$Any, [string]$Marker = '--onitest-')`. Every TestRunner caller (`Invoke-OniTests.ps1:155,216,219,221,233,543,577`, `Watch-OniGame.ps1:43,47`) passes `-Id`, `-Any` or nothing. None passes a second positional argument that could now bind to `-Marker`. With the default, `"*$Marker*"` is the same pattern as the old literal `'*--onitest-*'`. Behaviour is the same.

**Marker against the real arguments.**

| Tool | Launch built at | Always carries | Marker | Matches? |
|------|-----------------|----------------|--------|----------|
| SaveRunner | `Invoke-SaveRun.ps1:117` | `--saverun=<save>` | `--saverun` (:124) | yes |
| WorldgenDriver | `Invoke-Worldgen.ps1:210-215` | `--worldgen-list=` | `--worldgen-` (:220) | yes |
| ModTest | `Test-OniMods.ps1:254`, `steam://rungameid/457140` | nothing | none, `-Any` with start time | n/a |

The markers do not overlap: TestRunner's `--onitest-...` lines do not contain `--saverun` or `--worldgen-`. The markers are constants with no `-like` wildcard characters.

**Every lookup goes through the helper.**
- Pre-launch refusals use `-Any`: SaveRunner :62, WorldgenDriver :74, ModTest :80 (plus `Restarter`).
- Start waits are marker-filtered in SaveRunner (:124) and WorldgenDriver (:220).
- Kill and exit-follow act on the found `$proc` object: SaveRunner :127-133, WorldgenDriver :225-231.
- None of the three tools starts a warden or another helper.
- `mods.json` and config restore stay in `finally`, after the loop on the found process. That covers success, timeout (`throw` at SaveRunner :125, `no-process` in WorldgenDriver), kill (`Stop-Process` on that pid, then break), and an exception, on both tools.
- For a plain `OxygenNotIncluded` launch, both tools behave as before: the helper returns every `OxygenNotIncluded` unfiltered and makes no CIM call.

**ModTest start-time detection.**
- *A game started just before the launch:* `Assert-OniNotRunning` (:129) refuses any open game, and `$started` (:249) is taken after the `-Steam` downloads, just before `Start-Process`. A game the player starts between the check and `$started` has `StartTime < $started` and is not followed. The new `-ge` filter is an improvement for `OxygenNotIncluded` as well.
- *A game started after `$started` but before Steam's launch:* Steam treats app 457140 as running and does not start a second copy, so the followed game is the one that boots on the test `mods.json`. Following it is correct.
- *Clock resolution:* `Process.StartTime` (kernel FILETIME) and `Get-Date` read the same clock. The launch is seconds after `$started`, so `-ge` cannot misfire.
- *Moono parent/child pair:* OM-012's in-game run (`OM-012.../03-test-report.md`, warden pid 56816 watched from launch to exit) shows a single OniMoono process for the whole session, so `Select-Object -First 1` takes the game.
- *Restart path:* see finding 1.
- Refusing while any game is open makes the start wait safe. It does not make the restart wait safe.

**Dot-source path.** `Join-Path $PSScriptRoot '..\TestRunner\GameProcess.ps1'` does not depend on the working directory. `Test-Path` is True from all three tools. All three set `$ErrorActionPreference = 'Stop'` before the dot-source: SaveRunner :47/:59, WorldgenDriver :42/:54, ModTest :47/:77. A missing file stops the script before it touches anything, with "The term '...\TestRunner\GameProcess.ps1' is not recognized", exit 1. I checked this with a throwaway script; the line after the dot-source did not run. In ModTest only `-Any` is called, so the CIM branch (`.CommandLine` of `$null`) never runs under its `Set-StrictMode -Version Latest`.

**Window.** None of the four files reads `MainWindowHandle`, foregrounds a window or polls one. The lookups use only the process list and WMI.

## Findings

### Critical
None.

### Moderate

1. **Pass 1. `tools/ModTest/Test-OniMods.ps1:265,279`: the restart wait can miss the restarted game, and then `mods.json` is restored under it.**
   - **The race.** `$exited = Get-Date` (:265) is taken after the 250 ms `HasExited` poll (:264) has noticed the exit, so it lags the real exit by up to about 250 ms. Moono's Restarter waits for the player to exit and then starts it again immediately (oni-moono `docs/ARCHITECTURE.md`, Restarter paragraph). The new OniMoono can therefore get `StartTime < $exited`. The `-gt $exited` filter (:279) then excludes it. The 60 s loop finds nothing and breaks, and `finally` sees `$game = $null`. It copies the player's loadout back while the restarted game runs on the test loadout, and prints the normal "Restored the normal loadout". A game that later writes `mods.json` (on quit or a mod change) can then overwrite the player's loadout, which is the loadout-loss case in the mod-loadout-recovery notes.
   - **Vanilla is worse.** Klei's `Restarter.exe` starts the game at once without waiting (same doc), so its new process can even start before the old one exits.
   - **Origin.** The filter predates this diff (1f71ce8). This diff is what first routes Moono games through it, and the review focus names this path.
   - **Fix expected:** before nulling `$game`, record `$oldId = $game.Id; $oldStart = $game.StartTime`. Then follow `Get-GameProcess -Any | Where-Object { $_.Id -ne $oldId -and $_.StartTime -ge $oldStart } | Select-Object -First 1`. The pre-launch refusal and Steam's single instance make any other game process started after the old one's start the restart. This works for both restarters and has no clock lag. The planned in-game ModTest check should then include one mod-change restart under Moono, so that "following it..." is seen and "Restored" prints only after the restarted game exits.

### Minor

2. **Pass 4. `tools/TestRunner/.context/standards/code-style.md:60` (Layout row added in 9b3b138): the row says `Get-GameProcess` finds an OniMoono "started with `--onitest-` arguments". It does not mention `-Marker` or that SaveRunner, WorldgenDriver and ModTest dot-source the file.** **Fix expected (coordinator, through the write path):** extend the row as `01-implement-report.md` proposes: "`-Marker` (default `--onitest-`) sets the argument prefix; SaveRunner, WorldgenDriver and ModTest dot-source it too". Add matching rows to the SaveRunner and WorldgenDriver code-style Layout tables, and the shared-helper sentence to `tools/.context/overview.md` ("Some call others").

3. **Pass 4. `tools/TestRunner/GameProcess.ps1:1` and `:29`: the header still introduces the file as being for "the launcher (Invoke-OniTests.ps1) and the warden", and the warning says "not taken for the test game".** The note at :8-9 extends the header, and keeping the warning text unchanged was a deliberate choice to keep TestRunner's output the same. Under SaveRunner or WorldgenDriver, though, "test game" names the wrong thing. **Fix expected (optional):** make line 1 read "The game process, for the launchers in tools/ ...". In the warning, say "launched game" in place of "test game". The OM-012 rule is about behaviour, not wording, so the change is safe.

4. **Pass 3. `tools/WorldgenDriver/Invoke-Worldgen.ps1:203-220`: there is no open-game check before sessions 2..n.** Only the run start refuses an open game (:74). A Moono game the player opens between sessions is ignored by the marker-filtered wait, which is correct. Steam then refuses the session's launch as "already running", so the session ends `no-process` and, after two such sessions, the run gives up. That is a pre-existing gap with a clear outcome, not a regression. **Fix expected (optional):** before each session's `Start-Process`, check `Get-GameProcess -Any` and stop with "a game was opened during the run". Otherwise, leave it as is and note it.

### Out of scope

5. **ModTest launches with no arguments, so its OniMoono can never carry a marker.** Switching it to `steam.exe -applaunch 457140 --modtest` would give it the same filter as the other tools and make it independent of start times. The implementer noted this and the spec did not ask for it. Worth a follow-up ticket only if finding 1's fix proves fragile in game.

## Pass 2: security
No secrets, and no user input reaches a command. The WMI filter interpolates only `[int]` ids, and the markers are constants. Nothing to report.

## Pass 5: evidence
- Parse check (this review, `[Parser]::ParseFile`): 0 errors in all six scripts (the three tools, plus TestRunner's `GameProcess.ps1`, `Invoke-OniTests.ps1` and `Watch-OniGame.ps1`). Each tool's dot-source path resolves.
- The implementer's stand-in runs (renamed `cmd.exe` processes, `01-implement-report.md`) cover the marker, default, `-Id` and `-Any` selection and the ModTest-style start-time pick, and the unchanged OM-012 stand-in script still passes. I read that output and did not rerun it, because it starts game-named processes. The stand-ins do not exercise the restart wait (finding 1).
- No automated tests exist for these PowerShell launchers, which matches the tools' practice. The plan has an in-game check per tool, and ModTest's should include a restart (finding 1).
- Scope: no creep. The README additions are one sentence or bullet each and accurate. The implementer reports CRLF preserved.

## Brief feedback
- Not needed once opened: the entities, glossary, naming-conventions and error-handling records for all three nodes, and the WorkshopUpload and `tools/` overview blocks. They name the scripts but say nothing about process lookup.
- Missing: ModTest has no `.context/` node, so nothing governs `Test-OniMods.ps1` beyond the `tools/` overview. The oni-moono Restarter behaviour that decides finding 1 lives only in the scratchpad clone's `docs/ARCHITECTURE.md`. A store note for it (for example, the "Launching with oni-moono" testing note) would let the next review find it.
- One-line reasons: each matched its record.

## Tooling friction
None with ICON.
