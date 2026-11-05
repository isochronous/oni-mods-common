# OM-013 unit 03: test report

**Result:**
- **WorldgenDriver: pass.**
- **SaveRunner: the launcher's process following passes, but the run did not finish.** The game
  timed out at 900 s with 9 samples, short of the 10 game seconds asked for, and wrote no probe
  rows. I can't name the cause, so I did not run it again (spec stop rule).

Each tool was launched once. I did not commit or edit code, did not touch Steam settings, did not
run ModTest, and did not foreground or poll a window.

## Before the runs

- No `OxygenNotIncluded`, `OniMoono` or `Restarter` process was running. Steam was running (pid 29736).
- `mods.json` SHA256 before: `98490930F3A42F0F757D77A095C6ECD22D994E574957FF1C74CEED24631A690B`.
  A copy is at `C:\Users\thegr\AppData\Local\Temp\claude\C--dev-ONI-Mods\7b932c4a-6569-4783-a8c2-8a995ac90316\scratchpad\mods-before-om013.json`.
- `kplayerprefs.yaml` SHA256 before: `B4A8146CFEB62BF698020F01FF2FF79CE7A0F5FF3F8B9F65A612757D98D7FF99`.
- There was no leftover `mods.json.saverun-backup` or `*.worldgen-backup`. An **empty**
  `worldgen-driver\config-backup` folder was there, dated 2026-10-20 01:22. The WorldgenDriver
  launcher's documented leftover restore removed it, and nothing else was changed.
- The previous Player.log was copied to `scratchpad\Player-before-om013.log`.
- The game mode was Spaced Out (`EXPANSION1_ID.ENABLED: 1`), so the save used was the Spaced Out
  fixture `tools/TestRunner/fixtures/testbed-so.sav` (981919 bytes, cycle 0, no Duplicants).

## 1. SaveRunner

Command, run from `tools/SaveRunner`:

```
./Invoke-SaveRun.ps1 'C:\dev\ONI-Mods\common\tools\TestRunner\fixtures\testbed-so.sav' -Seconds 10 -Without 2854869130,3281716506 -Label moono
```

Runner output, verbatim and complete:

```
Loadout: current + SaveRunner - 2854869130, 3281716506; mode Spaced Out
Launching: -applaunch 457140 --saverun=C:\dev\ONI-Mods\common\tools\SaveRunner\runs\moono-20261104-213317\colony.sav --saverun-seconds=10 --saverun-speed=1 --saverun-out=C:\dev\ONI-Mods\common\tools\SaveRunner\runs\moono-20261104-213317\probe.csv --saverun-timeout=900
Restored mods.json.
Game exited after 968 s; status {"status":"timeout","save":"C:\\dev\\ONI-Mods\\common\\tools\\SaveRunner\\runs\\moono-20261104-213317\\colony.sav","detail":"no end of run after 900 s real time","realSeconds":900.0}; 0 probe row(s) in C:\dev\ONI-Mods\common\tools\SaveRunner\runs\moono-20261104-213317
```

Player.log line 1 (`runs/moono-20261104-213317/Player.log`):
`[engine] Moono starting: commit=07cbcfcb8417afb90313e034a3d8456f888d2bb6 exe=%USERPROFILE%\AppData\Local\oni-moono\player\OniMoono.exe game=F:\SteamLibrary\steamapps\common\OxygenNotIncluded .NET=10.0.13`

What this proves (the OM-013 change):
- There was no "did not start within 120 s". `Get-GameProcess -Marker '--saverun'` found OniMoono
  pid 44208, started 21:33:19, two seconds after the launch.
- The launcher followed that process until it exited on its own: "Game exited after 968 s", a
  `launcherStatus` of `exited`, and no kill.
- The spec's "probe rows > 0" cannot happen with this command. It passes no `-Probe`, so the mod
  never opens a CSV (`RunBehaviour.cs:62`). The fixture also has no fridges. So 0 rows is expected
  even for a good run. The command needs `-Probe fridges` and a save that has a fridge before rows
  can prove anything.

What failed (not the OM-013 change), from the `[SaveRunner]` lines in the copied Player.log (UTC):

```
[02:34:17.901] [SaveRunner] will load ...colony.sav, run 10 s at speed 1, writing ...probe.csv
[02:35:09.513] [SaveRunner] loading ...colony.sav
[02:35:16.033] [SaveRunner] colony loaded after 58 s real time
[02:35:16.034] [SaveRunner] runner attached to Game (active True, component True)
[02:35:24.720] [SaveRunner] first update, time scale 0
[02:35:24.720] [SaveRunner] running 10 s at speed 1
[02:49:17.905] [SaveRunner] finished: {"status":"timeout",...,"detail":"no end of run after 900 s real time"}
[02:49:18.152] [SaveRunner] runner disabled after 9 samples
[engine] Exiting after 166687 frames with code 0
```

- The game was not hung:
  - It kept rendering frames, 166687 in all.
  - One CPU sample at 21:37 used about 2 cores and showed `Responding: True`.
  - The game quit cleanly when the mod's own watchdog fired.
- But the sum of `Time.deltaTime` reached only about 8 to 9 s in about 833 s of real time after
  "running". That is roughly 1/100 of speed 1. The log has nothing between 02:35:25 and 02:49:17:
  no exception, error or warning.
- The cause could be either of these. I can't tell which from the log, so I did not run it again:
  - the game stayed paused most of the time and stepped only now and then;
  - Moono's scaled `Time.deltaTime` or `timeScale` behaves differently after
    `SpeedControlScreen.Unpause`.
- The first update logs `time scale 0`, which fits the first. A next step that could tell them
  apart is a SaveRunner log line, once per real 10 s, with `Time.timeScale`,
  `SpeedControlScreen.IsPaused` and the `deltaTime` sum. Then the same 10 s run on plain
  `OxygenNotIncluded`, without oni-moono, to see whether the slowdown is Moono's.
- The game had already quit when I saw the result, so there is no screenshot of the stuck state.

mods.json after: `98490930F3A42F0F757D77A095C6ECD22D994E574957FF1C74CEED24631A690B`. It equals the
before hash, and the tool restored it ("Restored mods.json."; the backup was removed).

Leftover processes after the run: none. No `OxygenNotIncluded`, `OniMoono` or `Restarter` was
running, so I killed nothing.

**Verdict:** process following through oni-moono passes. The SaveRunner run under Moono fails
(timeout, game time ran at about 1/100 speed), for a reason I can't name. The tool is stopped, with
no second run.

## 2. WorldgenDriver

Build: not needed. Unit 01 changed only `Invoke-Worldgen.ps1`. The deployed
`mods\local\WorldgenDriver\WorldgenDriver.dll` (2026-10-19) is newer than every `.cs`/`.csproj`
(2026-10-15).

Command, run from `tools/WorldgenDriver`:

```
./Invoke-Worldgen.ps1 SNDST-A-100-0-0-0 -PerSession 1 -Label moono
```

Runner output tail, verbatim:

```
Leftover config backup C:\Users\thegr\Documents\Klei\OxygenNotIncluded\worldgen-driver\config-backup from an unfinished run; restoring it before starting.
== session 1: 1 coordinate(s), base game, mods: WorldgenDriver
   SNDST-A-100-0-0-0      complete      7.1s  STRINGS.UI.WORLDGEN.COMPLETE 
   session 1 exited: 1 result(s) in 24 s; log C:\dev\ONI-Mods\common\tools\WorldgenDriver\runs\moono-20261104-215014-session1.log
Restored mods.json and kplayerprefs.yaml.
Results: C:\dev\ONI-Mods\common\tools\WorldgenDriver\runs\moono-20261104-215014.jsonl
  complete     1
```

The result object was `status complete`, `applied SNDST-A-100-0-0-0`, `expansion1 0` and
`build U59-744825-VCRPAND`, with no errors.

Player.log line 1 (`runs/moono-20261104-215014-session1.log`):
`[engine] Moono starting: commit=07cbcfcb8417afb90313e034a3d8456f888d2bb6 exe=%USERPROFILE%\AppData\Local\oni-moono\player\OniMoono.exe game=F:\SteamLibrary\steamapps\common\OxygenNotIncluded .NET=10.0.13`

The files after the run:

| File | SHA256 after | Same as before? |
|---|---|---|
| `mods.json` | `98490930F3A42F0F757D77A095C6ECD22D994E574957FF1C74CEED24631A690B` | yes |
| `kplayerprefs.yaml` | `B4A8146CFEB62BF698020F01FF2FF79CE7A0F5FF3F8B9F65A612757D98D7FF99` | yes; the mode is back to Spaced Out |

No `*.worldgen-backup` was left, and the `config-backup` folder is gone.

Leftover processes after the run: none.

**Verdict:** pass. The output reads "session 1 exited: 1 result(s)", not `no-process`, and the
config and `mods.json` were restored.

## Brief feedback

- **Applied:** the SaveRunner and WorldgenDriver error-handling records (status values, and the
  leftover-backup restore) and the entities records (the launcher's flow).
- **Not needed:** the code-style, naming and glossary records, because this unit edits no code.
- **Missing:** nothing in the brief says that SaveRunner's proof needs a `-Probe`. The spec's
  "probe rows > 0" came from unit 01's report, not from a store record.
- No pointer's one-line reason was wrong.

## Tooling friction

None with ICON.
