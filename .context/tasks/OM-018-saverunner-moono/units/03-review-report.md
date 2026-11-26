# OM-018 unit 03: review of the Save Runner -Runtime diff

Diff: `git diff main...OM-018-saverunner-moono -- . ':!.context'` (6 files, +136/-40). Line numbers are the branch's.

Result: 0 critical, 2 moderate, 5 minor.

## Moderate

### M1. A killed Moono game leaves its crash watcher running (Pass 1, Pass 3)
`tools/SaveRunner/Invoke-SaveRun.ps1:204` and `:206`. On both kill paths (status stale for 60 s, overall timeout) only `$proc.Id` is stopped. On Moono the crash watcher (`OniMoono.exe --crash-watch`) is a child of the game process (OM-014 plan, line 66), so it survives the kill and reacts to the game's death after mods.json has been restored. `Stop-MoonoLaunch` already kills the launcher and its descendants, but the script calls it only when no game was found (`:215`). The report lists this as untested ("expected to go with it").
Fix: on a Moono kill, after `Stop-Process`, call `Stop-MoonoLaunch $launcher $stockBefore $moonoLog "--saverun-out=$csv"`. Better still, kill the tree before the restore in `finally` whenever `$launcherStatus` starts with `killed`.

### M2. A failed Moono launch exits 0; the same failure on Stock exits 1 (Pass 1, Pass 3)
`tools/SaveRunner/Invoke-SaveRun.ps1:189-193`, `:245`. The script catches `Wait-MoonoGame`'s throw (OniMoono exited early, the stock game appeared, nothing within 120 s), prints "Launch failed: ..." and carries on to a normal end. Stock throws `'The game did not start within 120 s.'` (`:197`), so pwsh exits 1. A caller or agent that checks the exit code sees a successful Moono run with no probe rows.
Fix: after the summary lines, `if ($launchFailure) { exit 1 }`. Do it after the summary, not by rethrowing in the catch, so the restore and the report still run.

## Minor

### m1. The game is found by the generic `--saverun` marker, not by this run's own (Pass 1)
`tools/SaveRunner/Invoke-SaveRun.ps1:190` (and the Stock loop at `:196`, which predates this change). Any OniMoono whose command line contains `--saverun` counts. The `-Any` check at `:91` makes a foreign one unlikely. `Stop-MoonoLaunch` already uses the run-unique `--saverun-out=$csv`, though, and the crash watcher is excluded only by the sort order, not by a filter.
Fix: in the find block, add `-notlike '*--crash-watch*'` and match on `--saverun-out=$csv`. `Get-GameProcess` uses `-like "*$Marker*"`, so escape the marker with `[WildcardPattern]::Escape()`, or a `-Label`/`-OutDir` containing `[` or `]` will break the match.

### m2. The "press Ctrl+C" advice during the stray-stock wait may not work (Pass 1)
`tools/SaveRunner/Invoke-SaveRun.ps1:222`. The 900 s `Wait-Process` runs inside `finally`, and PowerShell defers a pipeline stop until a running `finally` block completes. So Ctrl+C may not interrupt the wait at all. Not verified here (no interactive console). The text is copied from ModTest (`tools/ModTest/Test-OniMods.ps1:352`), which has the same question open. Data stays safe either way: if the console is closed, the backup stays and the next run restores it first. Only the instruction is in doubt.
Fix: check it once by hand. If Ctrl+C is deferred, say "close this window and run: Copy-Item ..." in both scripts.

### m3. With `$stockBefore` still empty, a failure before the launch can wait 900 s on a player's game (Pass 1)
`tools/SaveRunner/Invoke-SaveRun.ps1:174`, `:184`, `:218`. If `Set-Mods` throws (`Local mod folder not found`), `$stockBefore` is still `@()`. `Stop-MoonoLaunch` then returns every running OxygenNotIncluded as a stray, and `finally` holds the restore for up to 900 s. This needs the player to start a stock game in the gap after the `-Any` check, so it is narrow.
Fix: take `$stockBefore = Get-StockGameIds` before `Set-Mods`, or skip `Stop-MoonoLaunch` while `$launcher` is `$null` and no launch was attempted (a `$launched` flag set right before `Start-Process`).

### m4. A Moono run reads Player.log before moono.log (Pass 3)
`tools/SaveRunner/Invoke-SaveRun.ps1:212`, `:239`. On a Moono run, a Player.log written since `$t0` by any other game (a stray stock game, say) is copied and wins over the launched game's own `moono.log`. The runtime line then reports the other game. TestRunner has the same order, so this is consistent, but for `-Runtime Moono` the file the launch asked for should answer first.
Fix: `$ranLog = if ($Runtime -eq 'Moono' -and (Test-Path -LiteralPath $moonoLog)) { $moonoLog } elseif ($logCopied) { ... }`. The mismatch warning still fires from moono.log if a stock game ran instead.

### m5. README and doc comment (Pass 4)
- `README.md:140`: the synopsis lists `-Runtime` and `-DryRun` but not `-MoonoExe`. Add `[-MoonoExe <path>]`, or leave it to the tool README, which does document it.
- `tools/SaveRunner/Invoke-SaveRun.ps1:25`: the rewrapped `.DESCRIPTION` paragraph leaves one line far longer than the others. Rewrap it.

## Checked and fine
- **mods.json safety.** The backup is taken after the DryRun return (`:156-170`). `finally` restores on every path through the try: a Stock throw, a caught Moono failure, an early Moono exit (the wait loop ends, status missing, `Get-MoonoExitText`), a kill and Ctrl+C. When a stray stock game outlives the 900 s wait, the backup is kept, and the next run's `-Any` check (`:91`) refuses before the leftover-backup restore (`:165`), so a still-running game is never restored under.
- **Saves.** The save is only read, through `-LiteralPath` (`:90`, `:101`), and copied to `colony.sav`, so names with spaces, parentheses or brackets such as `The Aftermath v009 Cycle 993.sav (1).sav` are safe. The game gets only the copy. The Moono argument line is built with `ConvertTo-CommandLineArgument` and passed as one string, so a run path with spaces would be quoted. The Stock `-ArgumentList` array is unquoted, but that predates this change and the default `OutDir` has no spaces.
- **DryRun.** Reads only kplayerprefs.yaml, mods.json is not read, and `Get-MoonoExe`/Steam/dll existence are checked. No folder, no copy, no backup, no launch. `return` at script scope exits 0. It skips the open-game check, and the help says so.
- **Stock path.** Same Steam arguments (`-applaunch <id>` + the shared `$gameArgs`), the same wait and kill loop. Only two things changed: Player.log is copied only when fresh (`Copy-FreshLog`, which fixes the stale-log copy), and a runtime line is printed.
- **Launcher identity on a failed launch.** `Stop-MoonoLaunch` ends only the launcher, its descendants and processes whose command line holds this run's `moono.log` or `--saverun-out=$csv`. A stock game that was already up is excluded through `$stockBefore`. A Moono session the player started by hand can't be one of them (the `-Any` check refuses before launch).
- **No `$LASTEXITCODE` dependence** (Start-Process, not `&`), and no strict mode in the script. Variables first set inside try (`$logCopied`) are read only on paths where try completed.
- **Get-LogRuntime** (`tools/TestRunner/GameProcess.ps1:235-243`): same return shape (`Runtime`, `Commit`, `Launch`, `Line1`). The only other caller, `tools/TestRunner/Invoke-OniTests.ps1:673-674`, reads the same fields, and ModTest does not call it. A missing launch line gives `$null -match` → `''`. A stock log still returns Stock from line 1.
- **App.QuitCode trace** (`tools/TestRunner/Mod.cs:52`): decompiled `App.QuitCode(int)` is `Application.Quit(exitCode)` (Assembly-CSharp-firstpass). The prefix is `TraceQuit`, which returns void, so it cannot skip the original or change the code, and `TracePatch` catches patch failures. `Application.Quit(int)` is not patched. One thing to know when reading the traces: `App.Quit()` calls `QuitCode(0)`, so a quit through `App.Quit` now logs two stack traces.
- **ShowDialog log** (`tools/TestRunner/Patches.cs:48-56`): `KCrashReporter.ShowDialog(string error, string stack_trace, bool, string[], string[])` is the only overload, and the parameter name `error` matches, so the attribute patch binds. It is a void prefix gated on `Session.Active`. Its `Debug.Log` re-enters `HandleLog` as `LogType.Log`, which returns early, so there is no recursion. It is attribute-patched rather than guarded like `TracePatch`, so a game build without that method would fail the whole patch pass. The method exists in the current build.

## Passes run
1. Correctness and logic: every exit path through try/finally (Stock throw, Moono caught failure, early exit, kill, Ctrl+C, stray stock), DryRun early return, variable initialisation.
2. Security and trust boundaries: no secrets, and no writes outside the run folder and mods.json. The player's save is only read.
3. Integration and side effects: `Get-LogRuntime` callers (grep across tools/), shared helpers unchanged, ModTest/TestRunner text parity, game method signatures decompiled with ilspycmd from `F:\SteamLibrary\...\Managed` (App, KCrashReporter).
4. Maintainability and clarity: comments, README accuracy against the code.
5. Evidence: implement report's dry runs and parse checks, the real Moono run (runs/om017-moono-20261126-150108) in plan.md. Not covered by any run: kill paths and failed-launch paths on Moono.
