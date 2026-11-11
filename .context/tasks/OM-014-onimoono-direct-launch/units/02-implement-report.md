# Unit 02 report: -Runtime Stock|Moono|Both for TestRunner and ModTest

Status: code done, committed, parse- and dry-checked. **The in-game proof was not run.** A Moono game
the user started by hand is open (pid 45596 since 14:42, `"F:\Modding\oni-moono\player\OniMoono.exe"`,
with its crash watcher pid 23252, `... --crash-watch 228 234 22c 230 238 0`). The launcher refuses
to run while it is open ("The game is already running; close it first."). I did not close the
user's game, wait on it, or launch anything.

## Files changed
- `tools/TestRunner/GameProcess.ps1`: header comment rewritten (no launch-options wording). Shared helpers added: `Get-GameFolder` (moved from Invoke-OniTests), `ConvertTo-CommandLineArgument` (moved), `Get-MoonoExe` (-Path, then ONI_MOONO_EXE, then F:\modding\oni-moono\player\OniMoono.exe; throws if missing), `Get-MoonoArgumentLine` (`--game <folder>` + game args, each quoted), `Get-LogRuntime` (Player.log line 1/2 -> Runtime, Commit, Launch). `Get-GameProcess` is unchanged.
- `tools/TestRunner/Invoke-OniTests.ps1`: `-Runtime Stock|Moono|Both` (default Stock), `-MoonoExe`. Legs are Stock, Moono, or Stock then Moono. Build/ModCheck runs once and the test DLL is copied into each leg's folder. Run folders are `<label>-<stamp>` (Stock alone), `-moono` (Moono alone), `-stock`/`-moono` (Both). `runtime=<leg>` is the first line of the loadout key. The per-leg body is today's code inside `foreach ($leg in $legs)`. The Moono launch is `Start-Process $moono -ArgumentList <one quoted line> -WorkingDirectory <player folder>`, and the game is still found by its `--onitest-` marker. The live session is stopped between legs. Each summary starts with `Runtime: <asked>; Player.log shows ...` and warns on a mismatch. Both writes a side-by-side table (`both.txt` in each leg folder, `<- differs`). The exit code is the worse leg. The Steam exe is required only for a Stock leg. Help and comments updated.
- `tools/TestRunner/README.md`: new "Stock or Moono" paragraph, `-Runtime`/`-MoonoExe` rows, stale launch-options wording replaced, and the `-Filter` row says to write a space as `?`.
- `tools/ModTest/Test-OniMods.ps1`: `-Runtime Stock|Moono|Both`, `-MoonoExe`. A Moono session is started with `OniMoono.exe --game <folder>`. Both runs the sessions one after the other, and each leg has its own backup and restore. The second leg waits up to 30 s for the first game to go, then checks that no game is open. A new `Get-AnyGame` drops Moono's `--crash-watch` process in the follow loops; the open-game pre-check still counts it. `-DryRun` prints each leg's launch command.
- `tools/ModTest/README.md`: usage line, the Moono bullet rewritten for direct launch, and the -DryRun bullet.

SaveRunner and WorldgenDriver are untouched. They call only `Get-GameProcess`, which is unchanged, and the new function names do not clash with anything in tools/. A grep for `ConvertTo-CommandLineArgument|Get-GameFolder|Get-MoonoExe|Get-LogRuntime` outside Invoke-OniTests found nothing before the move.

## Commits (common, branch OM-014-onimoono-direct-launch, not pushed)
```
a82d83c ModTest: -Runtime Stock|Moono|Both
6faceba TestRunner: -Runtime Stock|Moono|Both, Moono started directly with --game
```

## Command output

Parse check (`[System.Management.Automation.Language.Parser]::ParseFile` per file):
```
Invoke-OniTests.ps1: 0 parse error(s)
GameProcess.ps1: 0 parse error(s)
Watch-OniGame.ps1: 0 parse error(s)
Test-OniMods.ps1: 0 parse error(s)
Invoke-SaveRun.ps1: 0 parse error(s)
Invoke-Worldgen.ps1: 0 parse error(s)
```

The helpers, dot-sourced under `Set-StrictMode -Version Latest`. The first run threw on `Get-LogRuntime` with no log (`@()` from an `if` is $null under StrictMode). I fixed it with `$head = @(if ...)`, and this is the rerun:
```
Get-GameFolder: F:\SteamLibrary\steamapps\common\OxygenNotIncluded
Get-MoonoExe (default): F:\modding\oni-moono\player\OniMoono.exe
Get-MoonoExe (ONI_MOONO_EXE): C:\Users\thegr\AppData\Local\oni-moono\player\OniMoono.exe
Get-MoonoExe (-Path wins): F:\modding\oni-moono\player\OniMoono.exe
missing exe: oni-moono's player not found at C:\nowhere\OniMoono.exe; pass -MoonoExe or set ONI_MOONO_EXE.
Argument line, folder with spaces: --game "C:\Program Files (x86)\Steam\steamapps\common\OxygenNotIncluded" --onitest-dir=C:\x --onitest-filter=*A?B*
Live Player.log (hand-started Moono): {"Runtime":"Moono","Commit":"2864d5ec5ed7b70275b5b22e91cf95dfbb0506f9","Launch":"standalone: the game at F:\\SteamLibrary\\steamapps\\common\\OxygenNotIncluded (Steam library F:\\SteamLibrary)",...}
Player-prev.log: {"Runtime":"Moono","Commit":"2864d5ec...","Launch":"from Steam's launch options (-- %command%)",...}
Stock-like log: {"Runtime":"Stock","Commit":"","Launch":"",...}
No log: {"Runtime":"","Commit":"","Launch":"","Line1":""}
Get-GameProcess -Any: OniMoono 23252, OniMoono 45596
Get-GameProcess (marker):
```
(The marker lookup correctly does not take the user's hand-started Moono for a test game.)

TestRunner folder logic without a game: `Invoke-OniTests.ps1 -Mods MotionSensorRange -Runtime Both -NoBuild -NoCheck -BuildOnly -Label om014-buildonly`:
```
Built and checked against the game assemblies (-BuildOnly: no game session).
exit 0
om014-buildonly-20261111-150234-moono
  om014-buildonly-20261111-150234-moono\tests\MotionSensorRange.Tests.dll
om014-buildonly-20261111-150234-stock
  om014-buildonly-20261111-150234-stock\tests\MotionSensorRange.Tests.dll
```
The same command with `-Runtime Moono -MoonoExe C:\nowhere\OniMoono.exe` stopped before any work (`oni-moono's player not found at C:\nowhere\OniMoono.exe; pass -MoonoExe or set ONI_MOONO_EXE.`, exit 1). I deleted the two scratch run folders afterwards; `runs/` is gitignored.

The launch lines TestRunner builds (the same helpers and argument list, sample run folder):
```
Moono leg: F:\modding\oni-moono\player\OniMoono.exe --game F:\SteamLibrary\steamapps\common\OxygenNotIncluded --onitest-dir=C:\dev\ONI-Mods\common\tools\TestRunner\runs\run-20261111-000000-moono --onitest-save=...\run-20261111-000000-moono\fixture.sav --onitest-timeout=600 --onitest-test-timeout=60 --onitest-filter=*SomeTest* --onitest-config=MotionSensorRange --onitest-server=C:\Users\thegr\AppData\Local\IsochronousOniTests\live
Stock leg: C:\Program Files (x86)\Steam\steam.exe -applaunch 457140 --onitest-dir=... (same arguments)
```

ModTest dry trace (`Test-OniMods.ps1 -Only TestRunner -Runtime <r> -DryRun`):
```
=== -Runtime Stock
Would launch (Stock): steam://rungameid/457140
Dry run: loadout written to C:\Users\thegr\AppData\Local\Temp\oni-modtest.mods.json, game not launched.
exit 0
=== -Runtime Moono
Would launch (Moono): F:\modding\oni-moono\player\OniMoono.exe --game F:\SteamLibrary\steamapps\common\OxygenNotIncluded
exit 0
=== -Runtime Both
Would launch (Stock): steam://rungameid/457140
Would launch (Moono): F:\modding\oni-moono\player\OniMoono.exe --game F:\SteamLibrary\steamapps\common\OxygenNotIncluded
exit 0
=== -MoonoExe C:\nowhere\OniMoono.exe
oni-moono's player not found at C:\nowhere\OniMoono.exe; pass -MoonoExe or set ONI_MOONO_EXE.
```
I did not run ModTest in game, as the spec says.

## Run folders
No in-game run, so there are no game run folders. The only folders made were the BuildOnly scratch pair above, deleted afterwards.

## The exact Moono command line
`F:\modding\oni-moono\player\OniMoono.exe --game F:\SteamLibrary\steamapps\common\OxygenNotIncluded --onitest-dir=<runDir> --onitest-save=<runDir>\fixture.sav --onitest-timeout=<n> --onitest-test-timeout=<n> [--onitest-filter=...] --onitest-config=<names> --onitest-server=%LOCALAPPDATA%\IsochronousOniTests\live`. The working directory is the player folder. The game is found as an OniMoono whose command line holds `--onitest-`. Not yet run.

## The proof still owed (once the user's game is closed)
From C:\dev\ONI-Mods:
`pwsh common/tools/TestRunner/Invoke-OniTests.ps1 -Mods MotionSensorRange -Filter "*Defaults*" -Runtime Both -Label om014`
(Range.cs `Defaults`, Layer 0, Timeout 30: the shortest test I found.) What to check:
- `runs/om014-<stamp>-stock/summary.txt` and `-moono/summary.txt` both have a PASS line.
- Each leg's `Player.log` line 1: the Moono leg's reads `[engine] Moono starting: commit=...` and line 2 reads `launch: direct (--game)`. The stock leg's line 1 does not mention Moono.
- `both.txt` is written.
- About 15 s after the end, `%LOCALAPPDATA%\IsochronousOniTests\live` is gone and `mods.json` matches its hash from before the run (the warden's restore; `warden.log` next to the live dir).
- If Player.log line ~18 shows `game arguments: none` on the Moono leg, the arguments are not passed through, and the run ends as "no status file".

## Deviations and why
- No in-game proof: the user's own Moono game is open, and closing it is not mine to do.
- The Moono exe is not searched for under `%LOCALAPPDATA%\oni-moono\player` (the explore report proposed that). plan.md Decisions set the default as F:\modding\oni-moono\player\OniMoono.exe with ONI_MOONO_EXE / -MoonoExe as overrides, and I followed the plan.
- The live game's process name is not compared on reuse. The `runtime=` key line is enough: a stock live game never matches a Moono run's key, and the reverse.
- A Stock run alone keeps today's unsuffixed run folder name (`<label>-<stamp>`). Moono alone gets `-moono`.
- A launcher throw in leg 1 (for example "did not start within 120 s") still ends the whole run. Leg 2 is skipped and the restore runs in `finally`, as today.
- ModTest: `Get-AnyGame` leaves out Moono's `--crash-watch` process when following the launched game. `Get-GameProcess -Any` would otherwise have counted the watcher, which starts at the same moment, as the game.
- Not touched, outside my scope: the TestRunner node's `code-style.md:60` and ONI-Mods `integration-testing.md:203-207` still describe the launch-options route. They need a store write by the coordinator.

## Store notes
- The brief's pointers applied (TestRunner code-style was the one used: 4-space ps1, options documented in help + README). The ModTest/WorkshopUpload and ModCheck "What this is" pointers were neutral. Nothing governing was missing.
- ICON friction: none.
