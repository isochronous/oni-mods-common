---
doc_id: doc-6fac7cccfdc2
description: "Naming in SaveRunner: files, patch classes, Runner state, probes, the --saverun-* options, launcher parameters, run folders and CSV columns"
applies_to:
  - "*.cs"
  - "*.ps1"
  - "*.csproj"
  - "*.yaml"
---
# Naming Conventions

## Files and Directories
**Id:** blk-9f3b33d81a6e

- C# files are PascalCase and named for the main type they hold: `Mod.cs`, `Runner.cs`, `Patches.cs`, `RunBehaviour.cs`. `Patches.cs` holds every patch class.
- The launcher is `Invoke-SaveRun.ps1`, a PowerShell Verb-Noun name.
- The mod's folder, assembly (`SaveRunner`) and deployed folder (`mods\local\SaveRunner`) share one name. `mod.yaml` carries `staticID: "Isochronous.SaveRunner"`; the title is "Save Runner (dev tool)".
- Run output lives in `runs/<Label>-<yyyyMMdd-HHmmss>/` (ignored by `common/.gitignore`): `colony.sav`, `probe.csv`, `probe.csv.status`, `Player.log`.

## Code Naming
**Id:** blk-4cf0ed5fa17e

- Namespace `SaveRunner`; types PascalCase; locals and parameters camelCase; private fields camelCase with no prefix (`elapsed`, `samples`, `writer`).
- The mod class is `<Tool>Mod : UserMod2` (`SaveRunnerMod`).
- Patch classes are `<Type>_<Method>_Patch` (`MainMenu_OnSpawn_Patch`, `SaveLoader_Save_Patch`, `App_LateUpdate_Patch`), with methods named `Prefix` or `Postfix`.
- Run state is public static fields on `Runner` named for what they hold (`SavePath`, `Seconds`, `Speed`, `Probe`, `OutFile`, `TimeoutSeconds`); flags are `Active`, `Started`, `Finished`; times with a real-time meaning end in `At` (`StartedAt`, `MenuSeenAt`).
- A probe is a static class named `<Subject>Probe` (`FridgeProbe`) with a `Sample` method; the probe's name on the command line is the subject in lower case (`fridges`).
- Log tag: `Runner.Tag`, the string `[SaveRunner] `.

## Command Line and Launcher
**Id:** blk-5068e4f0855f

- Game arguments are `--saverun` followed by `-<option>`, lower case, hyphen-separated, as `--saverun-seconds=N`. `--saverun=FILE` itself names the save. `Runner` reads `--key=value` and a bare `--key` as an empty value.
- Launcher parameters are PowerShell PascalCase (`-Seconds`, `-Speed`, `-Probe`, `-With`, `-Without`, `-TimeoutSec`, `-Label`, `-OutDir`, `-SteamExe`, `-AppId`). `-Seconds`, `-Speed`, `-Probe` and `-TimeoutSec` map straight to `--saverun-seconds`, `--saverun-speed`, `--saverun-probe` and `--saverun-timeout`. `-Save` becomes `--saverun=<copy of the save in the run folder>`, and the run folder, named from `-OutDir` and `-Label`, gives `--saverun-out=<run folder>/probe.csv`. `-With`, `-Without`, `-SteamExe` and `-AppId` configure only the launcher: the loadout, the Steam path and the app to launch.
- `-With` and `-Without` take local mod folder names or Workshop item ids, comma-separated.
- Probe names are lower case plurals of the thing sampled (`fridges`).

## CSV Columns
**Id:** blk-885a11539bd3

Header names are camelCase with units spelled in the name (`setpointC`, `cellTempC`, `massKg`, `deltaKJ`), sample time first as `t`. Each game second writes one row per item in a fridge, or one row for a fridge with no items.

## Related
**Id:** blk-8e408dfe956d

- See also: [code style](code-style.md)
