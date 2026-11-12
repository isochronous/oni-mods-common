# Save Runner

A development-only mod plus a launcher script that load a save in the real game with nobody at the keyboard, let it run for a while, record readings and quit: start the game with `--saverun=FILE` and the mod loads that save from the main menu, blocks every save to disk for the session, runs the colony for `--saverun-seconds` of game time, writes what `--saverun-probe` observes once per game second to `--saverun-out`, and quits. Without the argument the mod does nothing, so it can stay installed.

Use it to reproduce an in-game observation from a player's save ("food in the Freezer never gets colder than -12.5 °C") with and without a mod, with real sim behaviour and the player's own mod loadout.

## Setup

```
dotnet build tools/SaveRunner -c Release
```

deploys the mod to `Documents\Klei\OxygenNotIncluded\mods\local\SaveRunner`. Steam must be running and logged in; the launcher starts the game through `steam.exe -applaunch 457140`, which passes the arguments through. With oni-moono's launch options the game runs as `OniMoono` instead of `OxygenNotIncluded`; the launcher follows an `OniMoono` whose command line holds the `--saverun` arguments, and refuses to start while any `OxygenNotIncluded` or `OniMoono` is open (`tools/TestRunner/GameProcess.ps1`).

## Running

```
./Invoke-SaveRun.ps1 'C:\...\save_files\My Colony\My Colony.sav' -Seconds 90 -Probe fridges -Label with-mod
./Invoke-SaveRun.ps1 'C:\...\save_files\My Colony\My Colony.sav' -Seconds 90 -Probe fridges -Without NoMagicFridges -Label without-mod
```

Mod Profile Manager (Workshop 2854869130) and its Light addon (3281716506) restart the game as soon as a save loads with a mod list different from the one it was saved with, which a run always has (SaveRunner itself is the difference); the restart shows up as a quit from native code a second after the colony finishes loading, with no probe rows. Disable them for the run with `-Without 2854869130,3281716506`.

The script keeps the current loadout (the save's own mods), adds SaveRunner and anything in `-With`, removes anything in `-Without`, and restores `mods.json` when the game has quit. With `-Only` every other mod is disabled, so the run has just SaveRunner and the `-With` mods; a full loadout of a hundred-plus mods takes minutes to boot, so use `-Only` for a save that does not need them, such as a TestRunner fixture (`-Only -Seconds 10 -TimeoutSec 180` for a quick check). The game's mode (base game or Spaced Out) is left as it is, so pick a save made in the current mode. The save is copied to `runs/<Label>-<stamp>/colony.sav` first and the game is pointed at the copy; the mod also blocks `SaveLoader.Save`, so neither the copy nor the original is ever written. Output in the same folder: `probe.csv`, `probe.csv.status` (one JSON line: `done`, `timeout`, `no-save`, `runner-error`) and a copy of `Player.log`.

Probes:

- `fridges`: every building with a `RefrigeratorController.Def` (Refrigerator, Mini Fridge, modded fridges) and every item in it: controller state, powered state, whether No Magic Fridges' thermostat is on the building, the def's setpoint, the cell's temperature, element and mass, and per item its element, mass, temperature as `PrimaryElement.Temperature` reads it, `SimTemperatureTransfer` surface area and ground-transfer scale, whether that component is enabled, its sim handle and `deltaKJ`.
- `memory`: where the game's memory goes, from boot to the end of the run, with what each mod adds as it loads and what the memory is made of at the end. Defaults for this probe: `-Seconds 300 -Speed 3 -TimeoutSec 1800` (half a cycle, long enough to see growth after the load; a full loadout boots for minutes first). The launcher moves SaveRunner to the top of `mods.json` for the run, since the game loads mods in that order and the probe starts in SaveRunner's `OnLoad`; the log warns when any mod's DLLs loaded before it. Compare a run with the loadout against one without it:

  ```
  ./Invoke-SaveRun.ps1 'C:\...\My Colony.sav' -Probe memory -Without 2854869130,3281716506 -Label mem-full
  ./Invoke-SaveRun.ps1 'C:\...\My Colony.sav' -Probe memory -Only -Label mem-none
  ```

  Three files, all sizes in MB:

  - `probe.csv`, one row per reading: `t` (game second, empty before the run), `phase` (`boot` at SaveRunner's `OnLoad` and every five real seconds until the menu, `before-<pass>`/`after-<pass>` around each mod-loading pass, `menu` every five real seconds, `loading` as the load starts, `loaded` at `Game.OnSpawn`, `run-start` and `run-start-gc` (after a full collection) on the first update, `run` every game second, `run-end`, `run-end-gc`, `after-snapshot`), `realS`, `cycle`, then `privateMB` (committed private bytes, Task Manager's "commit size"), `workingSetMB`, `peakWorkingSetMB`, `monoUsedMB` and `monoHeapMB` (Mono heap used and reserved), `gcTotalMB` (`GC.GetTotalMemory`, garbage included until a collection; the `-gc` rows are the live heap), `gcCount`, `unityAllocMB`, `unityReservedMB`, `unityUnusedReservedMB` (Unity's native allocators), `gfxUsedMB`, `gfxReservedMB`, `systemUsedMB` (profiler counters; empty where the player has none), `textureMB` and `textureDesiredMB` (`Texture.currentTextureMemory`, `desiredTextureMemory`), `textureCount`, `sysCommitMB` and `sysCommitLimitMB` (the whole machine's commit charge and limit).
  - `probe-modload.csv`, one row per mod and loading pass, in load order: `seq`, `pass` (`DLL` runs the mod's `OnLoad`, `PostLoad` its `OnAllModsLoaded`, `Animation` loads its kanims; `Strings`, `Translation`), `modId` (folder name or Workshop id), `title`, `ms`, `privateMB` after, then what the pass added: `privateDeltaMB`, `workingSetDeltaMB`, `monoUsedDeltaMB`, `monoHeapDeltaMB`, `gcTotalDeltaMB`, `unityAllocDeltaMB`, `unityReservedDeltaMB`, `gcRuns` (collections during the pass; a delta with one is not the mod's alone), `newTextures`, `newTextureMB` and `newReadableMB` (textures created during the pass, and the CPU copy the readable ones keep), `newKanims`, `assemblies`. Memory a mod's patches allocate later, at `Db.Initialize` or during the load, shows in `probe.csv`, not here.
  - `probe-snapshot.csv`, written once after the run: `section,name,owner,count,estMB,readableMB,detail`. Sections: `summary` (object, GameObject, component, texture, mesh and kanim totals, managed and Mono heap); `mod` (per mod: its textures still alive and their size, and in `detail` its kanims, instances of its types and its summed load deltas; `(game or unattributed)` holds every texture no mod created while loading); `assembly` (instances of each assembly's Unity object types, with the owning mod); `type` (every Unity object type: components, ScriptableObjects, Unity's own, with counts and, for textures and meshes, estimated size); `texture` (the 300 largest textures with their kanim, owner, class, size, format and mips); `bundle` (loaded asset bundles). Texture and mesh sizes are estimated from format, dimensions and mips: a release player reads 0 from `Profiler.GetRuntimeMemorySizeLong`.

## How it works

`Patches.cs`: a postfix on `MainMenu.OnSpawn` notes the time, and the `App.LateUpdate` postfix calls `LoadScreen.DoLoad(path)`, the Load screen's own path minus its dialogs, four seconds later (loading straight from OnSpawn tore the menu down under a popup still laying out, which threw and opened the crash dialog, and that dialog quits the game when the scene change destroys it; the mod also sets `KCrashReporter.ignoreAll` for the session so no crash dialog can end a run); a postfix on `Game.OnSpawn` adds `RunBehaviour` to the Game object, which unpauses (and again whenever anything pauses the game during the run), sets the speed, accumulates scaled frame time as game seconds, samples the probe each second and calls `Runner.Finish`, which writes the status file and `App.Quit()`s. A prefix on `SaveLoader.Save(string, bool, bool)` returns the filename without saving while a run is active, and prefixes on `GameFlowManager.StatesInstance.CheckForGameOver`, `KMod.Manager.MatchFootprint` and `KMod.Manager.Report` keep the "colony lost" screen (a save with no Duplicants) and the "mod differences" and "mod events" dialogs from opening, since each pauses the game; the mod events go to the log. A postfix on `App.LateUpdate` is the watchdog for a load that never finishes.

`MemoryProbe.cs` and `MemorySnapshot.cs` are the `memory` probe. With that probe only, `SaveRunnerMod.OnLoad` opens its files and patches `KMod.Mod.Load`, `KMod.Mod.PostLoad` and `KMod.Manager.Load` by hand, so other runs patch nothing more. Each mod's pass is read before and after (process counters through `K32GetProcessMemoryInfo`, `Profiler` and `GC` for the heaps); textures that appear during a pass are recorded as that mod's, and the assemblies of its DLL pass as its own, which is how the snapshot names an owner. The DLL pass SaveRunner itself runs in gets no `after-DLL` row, since it was already under way when the hook went in; `before-Strings` follows it directly.
