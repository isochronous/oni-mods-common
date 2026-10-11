# Worldgen Driver

A development-only mod plus a launcher script that generate a cluster in the real game without anyone at the keyboard: start the game with `--worldgen=COORD`, and the mod applies that coordinate at the main menu, opens the worldgen screen (the same path as the game's own `devAutoWorldGen` setting), watches progress, writes a result file and quits. Without the argument the mod does nothing, so it can stay installed.

Use it to reproduce worldgen bug reports ("generation never finishes on Blasted Ceres") and to check a mod build against a list of worlds and seeds, with the game's real worldgen and sim rather than an emulation.

## Setup

```
dotnet build tools/WorldgenDriver -c Release
```

deploys the mod to `Documents\Klei\OxygenNotIncluded\mods\local\WorldgenDriver`. Steam must be running and logged in; the launcher starts the game through `steam.exe -applaunch 457140`, which passes the arguments through.

## Running

```
./Invoke-Worldgen.ps1 CERS-A-100-0-0-0 -Mods NaturalBackwalls
./Invoke-Worldgen.ps1 -ListFile seeds.txt -Mods NaturalBackwalls013 -Label nb013
./Invoke-Worldgen.ps1 CERS-A-100-0-0-0, PRES-A-100-0-0-0, 'M-FLIP-C-100-0-0-0' -Mods NaturalBackwalls, 3816744732
```

Coordinates come from the command line and/or `-ListFile` (one per line, `#` comments). The script groups them by mode (base game or Spaced Out; `-Mode auto` reads it from the coordinate, a prefix ending in `-C` meaning Spaced Out) and runs each group in sessions of up to `-PerSession` (default 25) coordinates: one game start per session, the driver going back to the main menu between coordinates. A hang or crash ends the session and the launcher starts another for what is left; two sessions in a row without a single result abandon the rest of that group.

Before each session the script sets the mode in `kplayerprefs.yaml` and rewrites `mods.json` so that only the driver and the mods named in `-Mods` are enabled (local folder names or Workshop item ids), with local mods reset to "installed" (a rebuilt local mod otherwise comes up "reinstall pending", and the boot-time reinstall blocks the main menu). Both files are backed up before the first session and restored at the end, even on failure; the backups are then deleted. If a backup is still there when the script starts, an earlier run was killed before it could restore, so the script restores that backup first rather than overwriting it.

Output: `runs/<Label>-<stamp>.jsonl` with one JSON object per coordinate, a `Player.log` copy per session next to it, and a summary on the console (every result as it arrives, then counts by status and the non-complete ones). Timing: about 25 seconds of game startup per session, then about 10 seconds per base-game world and 15 to 45 seconds per Spaced Out cluster.

`-Stories all` forces every story trait on and `-Mixing all` (or `guarantee`) turns on every DLC mixing toggle and sets every world and subworld mixing to "try" (or "guarantee"), after the coordinate is applied; the result's `applied` field carries the coordinate the game actually used (for example `SNDST-A-9-0-4A-PE2MEMF2`). The `runs/coverage-*.txt` lists cover every selectable cluster in both modes.

Options: `-TimeoutSec` (default 300) is how long the driver waits without any progress report before calling it a hang; `-Load` also loads each generated colony before moving on; `-NoGraphics` adds Unity's `-batchmode -nographics` (untested with this game).

To test an older build of a mod against the current one, build it from a scratch copy of the repo into a second local folder (`-p:ModDeployFolder=<mods>/local/NaturalBackwalls013`) and name that folder in `-Mods`.

## How it works

`Patches.cs`: a postfix on `MainMenu.OnSpawn` applies the coordinate through `CustomGameSettings` (cluster by coordinate prefix, seed, settings/story/mixing codes, as the destination screen's coordinate box does) and instantiates `ScreenPrefabs.Instance.WorldGenScreen`. A postfix on `OfflineWorldGen.UpdateProgress` (worldgen thread) records progress; the watchdog is a postfix on `App.LateUpdate`, which runs in every scene, so a game stuck at the main menu is caught too. `OnError` and `DoExitFlow` catch the failure path, a prefix on `App.LoadScene("backend")` catches completion, and `Game.OnSpawn` catches a loaded colony. `Driver.cs` holds the coordinate list and per-coordinate state, appends one JSON line per result, and either returns to the main menu for the next coordinate (`App.LoadScene("frontend")`, the path the game itself takes after a failed generation) or quits.
