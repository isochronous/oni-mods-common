# OM-013 unit 01: implement report

**Result: done.** Nothing is committed and the game was not launched. TestRunner's
`Get-GameProcess` has a new `-Marker` parameter, and its default `'--onitest-'` keeps TestRunner's
behaviour the same. SaveRunner, WorldgenDriver and ModTest dot-source the file at
`$PSScriptRoot\..\TestRunner\GameProcess.ps1`. ModTest already reaches into a sibling tool the same
way (`..\WorkshopUpload\WorkshopUpload.csproj`). I did not move the file: moving it would add a
TestRunner diff and buy nothing. No tool dot-sourced a helper or started a warden before this
change.

## The helper (`tools/TestRunner/GameProcess.ps1`)

- :8-9: a new comment line says that the other launchers dot-source this file and pass their own
  argument prefix as `-Marker`.
- :13: `function Get-GameProcess([int]$Id = 0, [switch]$Any, [string]$Marker = '--onitest-')`.
- :26: `$line -like '*--onitest-*'` became `$line -like "*$Marker*"`.
- The unread-command-line warning still says "it is not taken for the test game". I kept the
  wording so that TestRunner's output does not change. SaveRunner and WorldgenDriver show the same
  wording if that warning ever fires.

## Argument marker per tool

| Tool | Launch | Marker |
|------|--------|--------|
| TestRunner | `steam.exe -applaunch 457140 --onitest-...` | `--onitest-` (default) |
| SaveRunner | `steam.exe -applaunch 457140 --saverun=<csv dir>\colony.sav --saverun-seconds=... ...` | `--saverun` (every run carries `--saverun=`) |
| WorldgenDriver | `steam.exe -applaunch 457140 [-batchmode -nographics] --worldgen-list=... --worldgen-out=... ...` | `--worldgen-` (every session carries `--worldgen-list=`) |
| ModTest | `Start-Process steam://rungameid/457140`, with no arguments | none, see below |

**ModTest has no marker.** It launches through a `steam://` URL that carries no arguments, so the
command line cannot tell its `OniMoono` apart from one the player started. The rule there uses time
instead:
- The pre-launch check refuses to start while any game is open: `OxygenNotIncluded`, any
  `OniMoono`, or `Restarter`.
- The start wait takes the first game process (either name) that started after `$started`.
- The restart wait already takes the first one that started after the exit.

The `-ge $started` filter is new for `OxygenNotIncluded` too. Before, the start wait took any
`OxygenNotIncluded`. That matters only if the player starts a game between the pre-launch check and
the launch, while the `-Steam` downloads run. Such a game is now no longer taken for the test game.
The alternative is to switch ModTest to `steam.exe -applaunch 457140 --modtest` so it gets a marker
like the other tools. I did not do that, because the spec did not ask for a launch change.

Moono's Restarter (scratchpad clone, `docs/ARCHITECTURE.md:2203-2213`) waits for the old process to
exit, then starts the player again with the same arguments. So ModTest's mod-change restart still
works: the old process exits, `Restarter` is present, and a new `OniMoono` starts after the exit. The
SaveRunner and WorldgenDriver flows never restart the game.

## Call sites changed (line numbers after the change)

`tools/SaveRunner/Invoke-SaveRun.ps1`
- :57-59: a comment and the dot-source of `..\TestRunner\GameProcess.ps1`.
- :62: the pre-launch refusal. It was `Get-Process OxygenNotIncluded` and is now `Get-GameProcess -Any` (was :59).
- :124: the 120 s start wait. It is now `Get-GameProcess -Marker '--saverun' | Select-Object -First 1` (was :121).
- Unchanged: :127/:129 `Stop-Process -Id $proc.Id` and the `$proc.HasExited` / `Refresh()` loop.
  They act on the object the wait found.

`tools/WorldgenDriver/Invoke-Worldgen.ps1`
- :52-54: a comment and the dot-source.
- :74: the pre-launch refusal, now `Get-GameProcess -Any` (was :71).
- :220: the start wait in each session, now `Get-GameProcess -Marker '--worldgen-' | Select-Object -First 1` (was :217).
- Unchanged: :231 `Stop-Process -Id $proc.Id` and the `HasExited` loop, both on the found object.

`tools/ModTest/Test-OniMods.ps1`
- :74-77: a comment and the dot-source, inside the "self-contained helpers" block.
- :80: `Test-OniRunning`, the check behind `Assert-OniNotRunning`. It is now `(Get-GameProcess -Any) -or (Get-Process -Name 'Restarter' ...)`.
- :261: the start wait, now `Get-GameProcess -Any | Where-Object { $_.StartTime -ge $started } | Select-Object -First 1`.
- :279: the restart wait, now `Get-GameProcess -Any | Where-Object { $_.StartTime -gt $exited } | Select-Object -First 1`.
- Unchanged: :271-272, the `Restarter` lookups, which use the same process name under Moono. Also
  :285, `$game.HasExited` in `finally`.

No tool looks up a window: there is no `MainWindowHandle`, `PrintWindow` or window class anywhere in
them. `'OxygenNotIncluded'` at SaveRunner :52, WorldgenDriver :43 and ModTest :73 is the Documents
folder name.

READMEs, one short addition each in the Setup or launch text:
- `tools/SaveRunner/README.md:13` and `tools/WorldgenDriver/README.md:13`: one sentence after
  "passes the arguments through".
- `tools/ModTest/README.md`: a new bullet after the launch bullet.

## Kanim and Translations

`tools/Kanim/kanim.py:79-80,223` and `tools/Translations/translations.py:28-29,31` use
`OxygenNotIncluded` only as the Steam install folder and its `OxygenNotIncluded_Data` subfolder.
Neither looks up a process. No change.

## Verification (no game)

Script: `C:\Users\thegr\AppData\Local\Temp\claude\C--dev-ONI-Mods\7b932c4a-6569-4783-a8c2-8a995ac90316\scratchpad\om013-verify.ps1`.
It runs a parse check of every changed `.ps1` (TestRunner's three included), checks that each
dot-source path resolves, and dot-sources the helper under `Set-StrictMode -Version Latest`, as
ModTest does. It then runs the stand-in cases: renamed `cmd.exe` copies running a hidden `ping`, with
each tool's arguments after `& rem`. The script removes them afterwards.

```
parse TestRunner\GameProcess.ps1 : 0 error(s)
parse TestRunner\Invoke-OniTests.ps1 : 0 error(s)
parse TestRunner\Watch-OniGame.ps1 : 0 error(s)
parse SaveRunner\Invoke-SaveRun.ps1 : 0 error(s)
parse WorldgenDriver\Invoke-Worldgen.ps1 : 0 error(s)
parse ModTest\Test-OniMods.ps1 : 0 error(s)
dot-source path from SaveRunner: True
dot-source path from WorldgenDriver: True
dot-source path from ModTest: True
running OxygenNotIncluded/OniMoono: 0
no game: default 0, -Any 0, saverun 0, worldgen 0
-Id $PID (pwsh): 0; -Id 999999: 0
stand-ins (OniMoono): hand 17096, onitest 71484, saverun 40316, worldgen 35740
  default (TestRunner): 71484   expect 71484
  -Marker --saverun:    40316   expect 40316
  -Marker --worldgen-:  35740   expect 35740
  -Any:                 17096,35740,40316,71484   expect all four
  -Id saverun, default marker: 0 (expect 0); with --saverun: 1 (expect 1)
  -Id onitest, default: 1 (expect 1); -Id hand: 0 (expect 0); -Any -Id hand: 1 (expect 1)
  saverun stand-in after Stop-Process: HasExited True; -Marker --saverun count 0
  ModTest-style (-Any, StartTime -ge launch): 50344   expect 50344
  OxygenNotIncluded stand-in 21812: default 1, saverun 1, worldgen 1 (expect 1 each)
after cleanup: -Any 0
```

I re-ran OM-012's stand-in script (`scratchpad\om012-verify.ps1`) unchanged against the edited
helper. Every TestRunner case still passes:

```
parse GameProcess.ps1 : 0 error(s)
parse Invoke-OniTests.ps1 : 0 error(s)
parse Watch-OniGame.ps1 : 0 error(s)
Get-GameProcess (no game): 0
Get-GameProcess -Id $PID (pwsh, not a game): 0
Get-GameProcess -Id 999999 (no such pid): 0
Get-GameProcess -Id 0 behaves as the list: 0
stand-ins: OxygenNotIncluded 82304, OniMoono by hand 67668
  Get-GameProcess ids: 82304
  -Id OxygenNotIncluded: 1; -Id hand OniMoono: 0
  -Any ids (open-game checks): 67668,82304
  -Any -Id hand OniMoono: 1
  hand OniMoono alone: Get-GameProcess 0, -Any 1 (launcher :233 would refuse)
  plus OniMoono with --onitest- 77256: Get-GameProcess ids: 77256,33716
  warden-style (StartTime >= since, first): 77256
  after stopping OxygenNotIncluded: 77256
  test OniMoono HasExited after kill: True; Get-GameProcess count: 0
after cleanup: 0
```

Line ends stay CRLF in all four `.ps1` files and the READMEs. `git diff --stat` and
`git diff --ignore-cr-at-eol --stat` agree: 7 files, 27 insertions, 11 deletions.

## The in-game check that proves each tool (through Moono, one run each)

- **SaveRunner**: `./Invoke-SaveRun.ps1 <a small save> -Seconds 10 -Without 2854869130,3281716506 -Label moono`.
  It proves itself if it prints no "did not start within 120 s" and the closing line reads
  `Game exited ...; status done` with rows above 0. `mods.json` must also be restored. A run that
  hits `killed-after-status` is still proof that the process was followed.
- **WorldgenDriver**: `./Invoke-Worldgen.ps1 SNDST-A-100-0-0-0 -PerSession 1 -Label moono`, a single
  base-game coordinate. It proves itself if it prints `session 1 exited: 1 result(s)` and not
  `no-process`.
- **ModTest**: `./Test-OniMods.ps1 -NoLocal`, then quit the game from the main menu. It proves itself
  if it prints "Game running; waiting for it to exit...", then "Game exited", then "Restored the
  normal loadout". This one needs Jeremy to quit the game. A mod-change restart in that session
  would also prove the restart path ("following it...").
- Each tool: with a Moono game open by hand, the launcher refuses at once. The stand-in check
  already covers that.

## code-style.md text (for the coordinator to write)

- **SaveRunner** `standards/code-style.md`, Layout table, `Invoke-SaveRun.ps1` row: "the launcher;
  dot-sources `../TestRunner/GameProcess.ps1` for `Get-GameProcess -Marker '--saverun'` (the game is
  `OxygenNotIncluded`, or an oni-moono `OniMoono` started with the `--saverun` arguments; `-Any`
  for the open-game refusal)". Indentation needs no change.
- **WorldgenDriver** `standards/code-style.md`: the same note wherever it lists
  `Invoke-Worldgen.ps1`, with marker `'--worldgen-'`. Indentation needs no change.
- **TestRunner** `standards/code-style.md` Layout row for `GameProcess.ps1`: add "`-Marker` (default
  `--onitest-`) sets the argument prefix; SaveRunner, WorldgenDriver and ModTest dot-source it too."
- **ModTest** has no `.context/` node. Its README bullet is its only record. If one is wanted:
  "`Test-OniMods.ps1` dot-sources `../TestRunner/GameProcess.ps1`; its launch has no arguments, so it
  follows `Get-GameProcess -Any` filtered by start time."
- The parent `tools/.context/overview.md` could say that `GameProcess.ps1` is shared. It is the
  first cross-tool dot-source.

## Brief feedback

- **Applied**: the SaveRunner, WorldgenDriver and TestRunner code-style records (indentation, and
  the Layout tables that the notes above extend). The TestRunner code-style row for
  `GameProcess.ps1` matched the file.
- **Not needed**:
  - the entities, glossary, naming-conventions and error-handling records
  - the WorkshopUpload "What this is" block (it names ModTest only as a caller)
  - the `tools/` overview blocks, which only name the scripts
- **Missing**: ModTest has no node. The `tools/.context/overview.md` "Some call others" sentence
  will be out of date once it should mention the shared helper. No pointer's one-line reason was
  wrong.

## Tooling friction

None with ICON.
