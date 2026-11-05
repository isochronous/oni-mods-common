# OM-012 unit 01: implement report

**Result: done, uncommitted, game not launched.** One helper `Get-GameProcess` in a new
`tools/TestRunner/GameProcess.ps1`, dot-sourced by both scripts the way `ModConfig.ps1` and
`GameMode.ps1` already are. It replaces the process-name lookup at all 9 listed call sites; no
other site in TestRunner finds the game by name or window class.

## The helper

`common/tools/TestRunner/GameProcess.ps1` (new, CRLF like its neighbours):

```powershell
function Get-GameProcess([int]$Id = 0) {
    $candidates = if ($Id -gt 0) {
        @(Get-Process -Id $Id -ErrorAction SilentlyContinue)
    } else {
        @(Get-Process -Name 'OxygenNotIncluded', 'OniMoono' -ErrorAction SilentlyContinue)
    }
    foreach ($p in $candidates) {
        if ($p.ProcessName -eq 'OxygenNotIncluded') {
            $p
        } elseif ($p.ProcessName -eq 'OniMoono') {
            $line = (Get-CimInstance -ClassName Win32_Process -Filter "ProcessId = $($p.Id)" -ErrorAction SilentlyContinue).CommandLine
            if ($line -like '*--onitest-*') { $p }
        }
    }
}
```

It returns `System.Diagnostics.Process` objects, the same type as before, so `.HasExited`,
`.Refresh()`, `.WaitForExit()`, `.StartTime`, `.Id` and `Stop-Process -Id` work unchanged. With
`-Id`, it returns that process only if it is the game; this replaces the old
`Get-Process -Id | Where-Object ProcessName -eq 'OxygenNotIncluded'`. A WMI query runs only when an
`OniMoono` process exists, so a plain launch makes the same `Get-Process` calls as before.

Moono's source (clone at 66b5f36, docs/ARCHITECTURE.md around line 2203) confirms the game runs
inside the `OniMoono` process. Its `Restarter.exe` is a separate process, started only by
`App.Restart` (a language change or "Restart to apply" in Moono Tweaks). It is named `Restarter`,
so the helper never matches it.

## Call sites changed

`common/tools/TestRunner/Invoke-OniTests.ps1` (line numbers after the change):
- :94: dot-sources `GameProcess.ps1`
- :155: `Get-LiveGame`, the check on the live-session pid (was :154)
- :216: the "warden still running with no game" check (was :215)
- :219: the wait for that warden (was :218)
- :221: the "no game running, put back leftovers" check (was :220)
- :233: `$running`, the check that refuses to start while a game is already running (was :232)
- :543: the 120 s start wait (was :542). This is the run-5 failure
- :577: the config/prefs restore on the launcher's own restore path (was :576)

`common/tools/TestRunner/Watch-OniGame.ps1`:
- :9-14: synopsis text, says which processes count
- :33: dot-sources `GameProcess.ps1`
- :37-38: comment above `Find-Game`
- :43: `Find-Game` by the pid file (was :41)
- :47: `Find-Game` as the newest game process since the warden started (was :45)

`common/tools/TestRunner/README.md` :41-44: one sentence in the warden paragraph. It names the two
process names, the `--onitest-` rule and `GameProcess.ps1`.

## Sites checked and left alone (they now work through the helper)

- **Player.log copy** (Invoke :556, and :490 on the reused path): this is a path, not a process
  lookup. It was skipped in run 5 only because the :543 wait threw first. Moono writes the same
  `LocalLow\Klei\Oxygen Not Included\Player.log` (run 5's evidence).
- **Timeout and stop kills**: Invoke :431 (`Stop-LiveSession`, on the object from `Get-LiveGame`),
  Invoke :562 (kills the warden, not the game), and Watch :80/:83 (the idle and busy kills by
  `$GamePid`). All of these act on a pid or an object the helper found.
- **Watch :52** `Get-Process -Id $GamePid` (the `-GamePid` given path): a lookup by pid with no name
  check, before and after. Left as is.
- **Screenshots**: there is no PrintWindow or window lookup in TestRunner. `Kit/TestContext.cs:192`
  uses Unity's `ScreenCapture.CaptureScreenshot` inside the game. No window is foregrounded or polled
  anywhere; the helper reads only the process list and WMI.
- `'OxygenNotIncluded'` at Invoke :86/:99/:237 is a folder name (Documents, the game folder,
  `_Data\Managed`), not a process lookup.

## Behaviour

- **Plain `OxygenNotIncluded` launch**: the same process objects at every site. The only change is
  that a second process name (`OniMoono`) is also queried, and that query finds nothing.
- **The warden restores after whichever process exits**: `Find-Game` now returns the `OniMoono`
  process, so the warden writes its pid file and runs its `HasExited` loop on it, and restores after
  that process exits.
- **A Moono session the player started by hand**: it has no `--onitest-` arguments, so it is
  ignored, as it was before. Note that this also means the launcher will not refuse to start while
  such a session is running, and will put leftovers back under it (:221). That is the same as before
  this change, and the same as with any non-game process. If Jeremy wants a hand-started Moono to
  block runs the way a hand-started `OxygenNotIncluded` does, :233 and :216/:221 would need an
  "any OniMoono" check. I did not add one: the spec gives the `--onitest-` rule for every site.

## Verification (no game)

Script: `C:\Users\thegr\AppData\Local\Temp\claude\C--dev-ONI-Mods\7b932c4a-6569-4783-a8c2-8a995ac90316\scratchpad\om012-verify.ps1`.
It runs the parse check with `[System.Management.Automation.Language.Parser]::ParseFile`, then the
helper against the live process list, then against stand-ins. The stand-ins are copies of
`cmd.exe` in `%TEMP%\om012-standins`, named `OxygenNotIncluded.exe` and `OniMoono.exe`, each running
a hidden `ping -n 30 127.0.0.1`. The script removes them afterwards. Output (the parse lines were
re-run after the final edits: 0 errors in all 3):

```
parse GameProcess.ps1 : 0 error(s)
parse Invoke-OniTests.ps1 : 0 error(s)
parse Watch-OniGame.ps1 : 0 error(s)
running OxygenNotIncluded/OniMoono: 0
Get-GameProcess (no game): 0
Get-GameProcess -Id $PID (pwsh, not a game): 0
Get-GameProcess -Id 999999 (no such pid): 0
Get-GameProcess -Id 0 behaves as the list: 0
stand-ins: OxygenNotIncluded 63048, OniMoono by hand 61556
  Get-GameProcess ids: 63048
  -Id OxygenNotIncluded: 1; -Id hand OniMoono: 0
  plus OniMoono with --onitest- 77748: Get-GameProcess ids: 77748,63048
  -Id test OniMoono: 1; its command line: "C:\Users\thegr\AppData\Local\Temp\om012-standins\OniMoono.exe" /c ping -n 30 127.0.0.1 >nul & rem --onitest-dir=x
  warden-style (StartTime >= since, first): 77748
  after stopping OxygenNotIncluded: 77748
  test OniMoono HasExited after kill: True; Get-GameProcess count: 0
after cleanup: 0
```

`git diff --stat` (common): Invoke-OniTests.ps1 15 lines, README.md 5, Watch-OniGame.ps1 16, plus
the untracked `tools/TestRunner/GameProcess.ps1`.

## What the in-game run must prove

One run through Moono, for example `-Mods FridgeThresholdsExtended` with the run-5 `-Steam` ids. It
should show:
1. The launcher finds the game: no "did not start within 120 s", and the line "The game stays up
   15 s ..." appears. Exit code 0 with the results printed.
2. Steam passes `--onitest-*` on the `OniMoono` command line, which is what the helper matches. Run 5
   showed that Moono received the arguments (Player.log lines 14-20). The helper reads them from WMI
   `CommandLine`. This assumes Moono's own host process is the one Steam starts with those arguments,
   and not some intermediate process.
3. The warden log shows "watching" with the OniMoono pid, then "game exited", then "restored
   mods.json" after the game quits. mods.json matches `mods.json.real`.
4. `Player.log` is copied into the run folder.
5. Optionally, a second run straight after, to check that live-session reuse finds the Moono game by
   its pid file (`Get-LiveGame`).
6. A plain `OxygenNotIncluded` run is unchanged. The stand-in check above covers the selection
   logic. The real game is covered by any later normal test run.

## Brief feedback

- Everything in the brief applied or was harmless. The TestRunner `code-style.md` and
  `error-handling.md` pointers were relevant. The ModCheck and tools overview blocks only name the
  script, so they were not needed.
- Nothing that governs this work was missing.
- The one-line reasons matched what the records say.

## Tooling friction

None with ICON. One Bash `sed -i` with `/c` and `&` in the replacement failed with "unknown option to
`s'". I used Edit instead. That is my own toolchain, not ICON.
