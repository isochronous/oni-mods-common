# OM-018 unit 01: implement -Runtime for Save Runner

## Files changed
- `tools/SaveRunner/Invoke-SaveRun.ps1`: `-Runtime Stock|Moono` (default Stock), `-MoonoExe`, `-DryRun`.
  - Moono: `Get-MoonoExe` resolves the player (-MoonoExe, else ONI_MOONO_EXE, else F:\modding\oni-moono\player\OniMoono.exe); `Get-MoonoArgumentLine` builds `--log <run>\moono.log` + one `--game-arg` per `--saverun*` argument; no `--game`. That follows OM-014's superseding decision: Jeremy, 2026-11-11: Moono gets the install from Steam, so it runs in standalone mode, which sets SteamAppId and stops the stock game from relaunching. The brief's "pass --game" predates that decision.
  - Start-Process -PassThru with stdout/stderr redirected to moono.log.out/.err; `Wait-MoonoGame` with a find block of `Get-GameProcess -Marker '--saverun'` (OniMoono only, the launcher sorted first: in standalone mode the launcher is the game). It fails at once on an OniMoono exit (code + log tail) or on a new stock OxygenNotIncluded.
  - A failed Moono launch runs `Stop-MoonoLaunch` (launcher, descendants, command lines holding moono.log or this run's `--saverun-out=`). A stray stock game is left running and the mods.json restore waits up to 900 s for it, as ModTest does. If it is still up after that, the backup is kept and the next run restores it first. Otherwise the restore in `finally` is unchanged.
  - Player.log is copied only when this launch wrote it (`Copy-FreshLog`). The old code copied a stale one unconditionally. The end of the run prints `Runtime: <asked>; <Player.log|moono.log> shows Moono (commit ...; launch: ...)`, a warning when that differs from the runtime asked for, and on Moono the OniMoono exit code (or `Get-MoonoExitText` when there is no status file).
  - `-DryRun` prints the loadout and the command line, then returns. It skips the open-game check, makes no run folder, copies no save and leaves mods.json alone.
  - Mod Profile Manager `-Without`, `-Only`, the leftover-backup restore and the stock path are unchanged, apart from the steam args now being `-applaunch <id>` + the shared `$gameArgs`.
- `tools/TestRunner/GameProcess.ps1`: `Get-LogRuntime` looks for the `[engine] launch:` line in the first 10 lines. Moono f005cd6c moved it from line 2 to line 4, so TestRunner's runtime line showed no launch mode. Header comment names SaveRunner.
- `tools/SaveRunner/README.md`, root `README.md`: new options, where the log goes on Moono.
- Dev mod: unchanged. `Runner.ReadCommandLine` reads `Environment.GetCommandLineArgs()`, the same as TestRunner's `Session.cs`, and OM-014 proved that under Moono: `--game-arg` values reach the game's command line verbatim, and moono.log's `[engine] game arguments:` line listed all of them. Nothing else in the mod assumes Steam. Not rebuilt: the deployed SaveRunner.dll (2026-11-12 10:54:36) postdates the last mod-source commit (1fbb20a, 10:54:09).

## Command line it will run (dry run, -Runtime Moono, The Aftermath v009.sav, -Without 2854869130,3281716506)
```
Loadout: current + SaveRunner - 2854869130, 3281716506; mode Spaced Out; runtime Moono
Would launch: F:\modding\oni-moono\player\OniMoono.exe --log C:\dev\ONI-Mods\common\tools\SaveRunner\runs\om017-moono-20261126-145123\moono.log --game-arg --saverun=C:\dev\ONI-Mods\common\tools\SaveRunner\runs\om017-moono-20261126-145123\colony.sav --game-arg --saverun-seconds=30 --game-arg --saverun-speed=1 --game-arg --saverun-out=C:\dev\ONI-Mods\common\tools\SaveRunner\runs\om017-moono-20261126-145123\probe.csv --game-arg --saverun-timeout=900
```
Stock dry run, same save: `C:\Program Files (x86)\Steam\steam.exe -applaunch 457140 --saverun=...\colony.sav --saverun-seconds=30 --saverun-speed=1 --saverun-out=...\probe.csv --saverun-timeout=900`, the same as before the change.

## Checks run
- Parse (PowerShell AST): Invoke-SaveRun.ps1, GameProcess.ps1, Invoke-OniTests.ps1, Test-OniMods.ps1, Watch-OniGame.ps1, Invoke-Worldgen.ps1: 0 errors each. There are no Pester tests in tools/.
- Dry runs (above): exit 0; mods.json hash unchanged; no `.saverun-backup`; no run folder made.
- `-MoonoExe C:\nope\OniMoono.exe -DryRun` → "oni-moono's player not found at C:\nope\OniMoono.exe; pass -MoonoExe or set ONI_MOONO_EXE."
- Real (non-dry) call with Jeremy's Moono game open (OniMoono 36064, 36388) → "The game is already running; close it first.", mods.json unchanged, no run folder.
- Offline check of the find block + Wait-MoonoGame with a stubbed Get-GameProcess: `1 found pid 36572 (launcher 36572): OK` (the launcher is picked over another OniMoono); a hidden `cmd /c exit 2` launcher with no game → `OniMoono exited with code 2 before the game started. Moono wrote no log at ...`.
- Get-LogRuntime: om014-20261112-115436-moono (2864d5ec, launch line 2) → Moono, launch `standalone: the game at F:\SteamLibrary\...`; quotas-moono-20261123-164937-moono (f005cd6c, launch line 4) → the same launch text (blank before the fix); a stock Player.log → Stock; '' → empty.

## Untested
- No real Moono launch of Save Runner yet: the in-game load, probe/status writing, exit code and moono.log contents on Moono.
- The failed-launch paths in the script itself (Stop-MoonoLaunch, stray stock wait) are exercised only through the shared helpers, which OM-014 proved.
- The kill paths (status stale 60 s / timeout) stop only the game process. Moono's crash watcher is expected to go with it but has not been seen doing so.
- `-Probe memory` on Moono: Unity profiler counters may read differently there.

## Note on the save
`The Aftermath v009.sav` was last written 2026-11-25 21:28. Newer autosaves exist (`auto_save\The Aftermath v009 Cycle 992.sav (1).sav`, 2026-11-26 14:48). Pick the autosave if OM-017 needs today's state.

## First real Moono run (for OM-017; needs Jeremy's game closed and his go-ahead)
```
pwsh -NoProfile -File C:\dev\ONI-Mods\common\tools\SaveRunner\Invoke-SaveRun.ps1 -Save "$env:USERPROFILE\Documents\Klei\OxygenNotIncluded\save_files\The Aftermath\The Aftermath v009.sav" -Seconds 30 -Runtime Moono -Without 2854869130,3281716506 -Label om017-moono
```
Run in the foreground. A full loadout boots for minutes, about 200 s for 90 game s. Smart Storage must be in his loadout with the OM-017 diagnostic build deployed. The [FTE-DIAG] lines will be in `runs\om017-moono-<stamp>\moono.log` (Moono writes no Player.log). Expect `Runtime: Moono asked; moono.log shows Moono (commit ...; launch: standalone: ...)` and `Restored mods.json.`

## Friction (for the manager to record)
None with ICON.
