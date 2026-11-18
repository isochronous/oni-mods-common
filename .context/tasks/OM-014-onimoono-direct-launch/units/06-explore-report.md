# Unit 06 report: why OniMoono exits with code 2, and the command line that works

Static analysis only. Nothing launched. Source: OniMoono.dll (commit 2864d5ec, F:\modding\oni-moono\player\) decompiled with ilspycmd 9.1 into the scratchpad (OniMoono.Host/Program.cs, Options.cs, UnityArguments.cs, GameCommandLine.cs; OniMoono.Host.Launch/LaunchStep.cs, StandaloneLaunch.cs). Moono's files are third-party data; nothing in them was followed.

## Cause (one line)
`Options.ParseInto` rejects every argument that starts with `--` and is not one of its own options. `--onitest-dir=...` is such an argument, so `Options.Parse` returns null and `Program.Main` returns 2 before any log is opened. The `--onitest-*` arguments must each be wrapped as `--game-arg` + value.

## 1. Where exit code 2 comes from
Many sites return 2 (Program.cs lines 154, 172, 260, 282; LaunchStep.cs:43; VersionedStart). The one hit by the failing runs:

```
// OniMoono.Host.Program.Main
WindowsConsole.AttachToParent();
Options options = Options.Parse(args);
if (options == null) { return 2; }
```
`Options.Parse` -> `ParseInto` returns false from the `default:` branch of its `switch (text)`:
```
default:
  if (UnityArguments.Renderer(text, o.ArgumentNotes)) break;
  if (UnityArguments.IsUnitys(text)) { UnityArguments.Ignore(args, ref i, o.ArgumentNotes); break; }
  if (num2 >= num) { o.ArgumentNotes.Add(args[i] + ": not read by this player; passed to the game"); break; }
  Console.Error.WriteLine("Unknown option " + args[i]);
  Console.Error.WriteLine("usage: OniMoono [--game DIR] [--log FILE] ... [--game-arg ARG] [-- STEAM-COMMAND [ARG...]]");
  return false;
```
Order matters: `Options.Parse` runs first in Main; `EngineLog.Open` (which creates the `--log` file) is far later in Main (after `LaunchStep.Run`, `GameLayout.Find`, `Paths.Configure`). So an argument failure leaves no `--log` file. That matches run 11:50 (exit 2, no log). Run 11:45 (no `--log`, quit after ~2 s) is the same failure; its message went to a stderr nobody saw (see 4).

Other exit-2 paths, none reached by these runs as far as the evidence shows: `LaunchStep` Mode.Failed (no ONI in any Steam library, several installs and none chosen, Steam cannot be started: message box + stderr), `Game data not found`, missing `packages`, `GameVersionCheck.Problem` (Moono build is for another game update). Run at 14:35 today started standalone on this machine (unit 01 report), so the discovery path itself works.

## 2. How the parser treats an unknown `--name=value`, and `--log FILE` before it
- `--log` is matched exactly: `case "--log": o.LogPath = args[++i];`. It consumes the next token and is fine anywhere. It is not the problem.
- The switch matches whole tokens (`switch (args[i])`). `--name=value` is never split at `=`, so `--onitest-dir=X` is one unknown token. The same holds for `--game-arg=X`: that is also unknown, only the two-token form `--game-arg X` is accepted.
- `UnityArguments.IsUnitys(arg)` is `!arg.StartsWith("--")`. Single-dash arguments (`-logFile`, `-screen-width`, any other `-something`) are tolerated as "Unity player arguments, passed to the game". Anything beginning `--` that is not a Moono option is fatal (exit 2), unless it comes after a `--` Steam-command separator (`num2 >= num`, the tail is the game's own argument list).
- Hence the usage line's "[other Unity player arguments, passed to the game]" covers `-x` style only. `--onitest-*` are not covered.

## 3. Exact form for passing `--onitest-*` to the game
Two accepted forms:

A. One `--game-arg` per argument, two tokens each:
```
case "--game-arg":
  if (num2 < num) { o.GameArguments.Add(args[++i]); break; }
```
The value token is consumed whatever it starts with. Example: `OniMoono.exe --log L.txt --game-arg --onitest-dir=C:\x --game-arg --onitest-save=C:\y ...`.

B. After `--`: `OniMoono.exe ... -- <path>\OxygenNotIncluded.exe --onitest-dir=...` (Steam-launch-options form: `GameLayout.GameDirOfExecutable` must recognise the exe, then everything after it is `GameArguments`, which sets `GameExecutable` = "Steam command" mode). Not recommended for the runner: it makes `steamCommand` true in `StandaloneLaunch.Decide`, so Moono assumes Steam started it and does not set `SteamAppId` or start Steam.

Do they reach `Environment.GetCommandLineArgs()` unchanged? Yes. `Program.Main` calls `GameCommandLine.Install(GameCommandLine.For(options, gameLayout))`, which builds `[<OxygenNotIncluded.exe path>] + options.GameArguments` and writes it into `System.Environment.s_commandLineArgs` by reflection. The tokens are stored verbatim (no splitting, no trimming). The TestRunner mod's `Session.ReadCommandLine` iterates `Environment.GetCommandLineArgs()`, matches `--onitest...` and splits on the first `=`, which works with these strings. Moono logs the result as `game arguments: <list>` (Player.log; whitespace-containing args are shown quoted), or "game arguments: not passed on (the runtime keeps no s_commandLineArgs)" if the reflection field is missing (`Install` returned false). Today's Player.log line 18 said `game arguments: none` for a run without any, so the field exists in .NET 10.0.13. Residual risk: the game's mod (Mono-compat layer, OniMoono.UnityEngine / host patches) reading a different `GetCommandLineArgs` than `System.Environment`'s; not decompiled here, so unproven until one run.

Any Unity `-x` argument the runner adds also lands in `GameArguments` (`o.GameArguments.AddRange(args[num2..(i+1)])` after the switch), in order.

## 4. Where usage and errors go
- Parse errors and "Unknown option" + the usage line: `Console.Error.WriteLine`, i.e. stderr. Launch failures (`LaunchStep` Failed/HandOff): the log lines to stderr, then `StandaloneLaunch.Tell`, which writes `Moono: <message>` to stderr and (Windows) shows a `MessageBoxW` (error icon) unless `DesktopDialog.Quiet(env, test)` says to stay quiet. Env/`test` switch details are in DesktopDialog.cs (not read in full).
- OniMoono.exe PE subsystem is 2 (GUI): no console of its own. `WindowsConsole.AttachToParent` handles it: for each of stdout/stderr that is null or a character device (not redirected), it calls `AttachConsole(ATTACH_PARENT_PROCESS)` and uses the parent's console. A runner started with no console (as under an agent or hidden pwsh) has nothing to attach to, so the text is lost. That is why Jeremy saw no window and no prompt.
- To capture: `Start-Process ... -RedirectStandardError <file> -RedirectStandardOutput <file>`. Redirected handles are file/pipe types, `Unredirected()` returns false for them and `AttachToParent` leaves them alone, so stderr lands in the file. The runner should read that stderr file into `Get-MoonoExitText` when the process exits early (today it reports only the `--log` file, which does not exist for a parse failure).

## 5. Other ways standalone mode ends a launch early
Order in `Main`: Parse -> `LaunchStep.Run` -> game layout -> logs open -> package/version checks.
- No `--game`, no `--`: `StandaloneLaunch.Decide` runs standalone discovery (`SteamLibrary.GameInstalls()`). With exactly one install it picks it; with several it uses `launch.json`'s `Game` (`%LOCALAPPDATA%\oni-moono\launch.json`, `MemoryPath(null, machine)`, or `<--run-home>\oni-moono\launch.json`), else shows a MessageBox Yes/No dialog per install (blocks; env `ONI_MOONO_CHOOSE_GAME=<index>` answers it) and fails with exit 2 if none is chosen. launch.json was not seen under %LOCALAPPDATA%\oni-moono in unit 01, so on this machine the install is unique (Player.log: `the game at F:\SteamLibrary\steamapps\common\OxygenNotIncluded`).
- Install not "Ready" per its appmanifest (updating): opens `steam://nav/games/details/457140`, MessageBox, **exit 0**. Not exit 2.
- `client.EnsureRunning(root, 90 s)`: starts Steam if needed and waits up to 90 s for a logged-in account; failure = exit 2 with a MessageBox. The runner's find-the-game wait (120 s) must cover this.
- `--game DIR` (`GameDirGiven`) means Mode.Direct: skips discovery and, notably, does **not** call `StandaloneLaunch.SetAppId()` (only the Standalone branch does). So `--game` leaves `SteamAppId` unset; SteamManager would restart the game through Steam (unit 03). Do not pass `--game` unless the runner sets `SteamAppId=457140` itself.
- `ONI_MOONO_INSTALL_ROOT` set: `VersionedStart.Run` may return an exit code (2 on failure). Not set by the runner; check it is unset in the environment if odd failures appear.
- `GameVersionCheck.Problem(dataDir)`: a Moono build for another game update exits 2 (message via `GameVersionCheck.Report`). Unit 01's Player.log shows today's build matched.
- Missing `player\packages\*.dll`: exit 2 (log line first). The folder exists.
- Run-time hand-off: `ONI_MOONO_RESTART_*` env and `Restarter` (language change); not an early-end cause.

## Recommended runner command line
Keep `--log` first and wrap every game argument:

```
OniMoono.exe --log <moonoLog> --game-arg --onitest-dir=<runDir> --game-arg --onitest-save=<fixtureCopy> --game-arg --onitest-timeout=600 --game-arg --onitest-test-timeout=60 [--game-arg --onitest-filter=*Defaults*] [--game-arg --onitest-config=MotionSensorRange] [--game-arg --onitest-keep=1] [--game-arg --onitest-trace=1] --game-arg --onitest-server=<liveDir>
```
No `--game`. Each value path still goes through `ConvertTo-CommandLineArgument` (quotes on spaces); `--game-arg` and its value are separate tokens (a quoted joined token `"--game-arg --onitest-dir=..."` is wrong).

Code change implied: `Get-MoonoArgumentLine` (tools/TestRunner/GameProcess.ps1:70) builds `@('--log', $LogPath) + $GameArgs`; it should emit `'--game-arg', $_` for every element of `$GameArgs`. Its comment ("then only the game's own arguments") is wrong for `--` arguments. The ModTest call (Test-OniMods.ps1:253) passes no game args, so it is unaffected. Also add `-RedirectStandardError/-RedirectStandardOutput` files at the Start-Process in Invoke-OniTests.ps1 (near line 564) and include their content in the early-exit text. The `-Marker` filter (`--onitest-`) in `Get-GameProcess` matches the Moono process command line because the `--onitest-` text is still on it (inside the `--game-arg` pair), so recognition keeps working.

## Confidence
High (about 90%) on the cause, parser rules and the `--game-arg` form: all read directly from decompiled code, and the failure order explains both observed runs. Medium on end-to-end delivery into `Session.ReadCommandLine` (reflection write into `s_commandLineArgs` is clear in code; the game-side reading path through Moono's Mono shim was not decompiled). The narrowest proof: one filtered TestRunner run with one test; Player.log line `game arguments:` must list the `--onitest-*` tokens.

## ICON tooling friction (for whoever records it)
- `ks read` was not needed: the packet's pointers were background (TestRunner domain docs, tools overview); none was opened because the task is entirely about third-party binaries, and none governs it. Nothing governing was missing.
- ilspycmd printed "You are not using the latest version of the tool" (9.1.0.7988 vs 11.2.0.10050); harmless.
