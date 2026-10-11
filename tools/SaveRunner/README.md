# Save Runner

A development-only mod plus a launcher script that load a save in the real game with nobody at the keyboard, let it run for a while, record readings and quit: start the game with `--saverun=FILE` and the mod loads that save from the main menu, blocks every save to disk for the session, runs the colony for `--saverun-seconds` of game time, writes what `--saverun-probe` observes once per game second to `--saverun-out`, and quits. Without the argument the mod does nothing, so it can stay installed.

Use it to reproduce an in-game observation from a player's save ("food in the Freezer never gets colder than -12.5 °C") with and without a mod, with real sim behaviour and the player's own mod loadout.

## Setup

```
dotnet build tools/SaveRunner -c Release
```

deploys the mod to `Documents\Klei\OxygenNotIncluded\mods\local\SaveRunner`. Steam must be running and logged in; the launcher starts the game through `steam.exe -applaunch 457140`, which passes the arguments through.

## Running

```
./Invoke-SaveRun.ps1 'C:\...\save_files\My Colony\My Colony.sav' -Seconds 90 -Probe fridges -Label with-mod
./Invoke-SaveRun.ps1 'C:\...\save_files\My Colony\My Colony.sav' -Seconds 90 -Probe fridges -Without NoMagicFridges -Label without-mod
```

Mod Profile Manager (Workshop 2854869130) and its Light addon (3281716506) restart the game as soon as a save loads with a mod list different from the one it was saved with, which a run always has (SaveRunner itself is the difference); the restart shows up as a quit from native code a second after the colony finishes loading, with no probe rows. Disable them for the run with `-Without 2854869130,3281716506`.

The script keeps the current loadout (the save's own mods), adds SaveRunner and anything in `-With`, removes anything in `-Without`, and restores `mods.json` when the game has quit. The game's mode (base game or Spaced Out) is left as it is, so pick a save made in the current mode. The save is copied to `runs/<Label>-<stamp>/colony.sav` first and the game is pointed at the copy; the mod also blocks `SaveLoader.Save`, so neither the copy nor the original is ever written. Output in the same folder: `probe.csv`, `probe.csv.status` (one JSON line: `done`, `timeout`, `no-save`, `runner-error`) and a copy of `Player.log`.

Probes:

- `fridges`: every building with a `RefrigeratorController.Def` (Refrigerator, Mini Fridge, modded fridges) and every item in it: controller state, powered state, whether No Magic Fridges' thermostat is on the building, the def's setpoint, the cell's temperature, element and mass, and per item its element, mass, temperature as `PrimaryElement.Temperature` reads it, `SimTemperatureTransfer` surface area and ground-transfer scale, whether that component is enabled, its sim handle and `deltaKJ`.

## How it works

`Patches.cs`: a postfix on `MainMenu.OnSpawn` notes the time, and the `App.LateUpdate` postfix calls `LoadScreen.DoLoad(path)`, the Load screen's own path minus its dialogs, four seconds later (loading straight from OnSpawn tore the menu down under a popup still laying out, which threw and opened the crash dialog, and that dialog quits the game when the scene change destroys it; the mod also sets `KCrashReporter.ignoreAll` for the session so no crash dialog can end a run); a postfix on `Game.OnSpawn` adds `RunBehaviour` to the Game object, which unpauses, sets the speed, accumulates scaled frame time as game seconds, samples the probe each second and calls `Runner.Finish`, which writes the status file and `App.Quit()`s. A prefix on `SaveLoader.Save(string, bool, bool)` returns the filename without saving while a run is active. A postfix on `App.LateUpdate` is the watchdog for a load that never finishes.
