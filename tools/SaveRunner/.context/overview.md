---
description: "TODO(owner): one line saying what SaveRunner is."
---
# SaveRunner

## What this is
**Id:** blk-da4886ca7cad

SaveRunner is a development mod plus a launcher script that load a save in the real game with nobody at the keyboard, run the colony for a set number of game seconds with every save to disk blocked, record a probe's readings once per game second and quit. The mod author uses it to reproduce a player's report from their save, once with the mod under test and once without it (`-Without <Mod>`), under the real sim and the player's own loadout, and compares the two `probe.csv` files. The mod does nothing unless the game starts with `--saverun=FILE`, so it can stay installed. Version 0.1.0 in `mod_info.yaml`; not for the Workshop. Mod Profile Manager restarts the game on a changed loadout and is left out with `-Without 2854869130,3281716506`.

## Where things are
**Id:** blk-3dee2f4998e2

Files:

- `Mod.cs`: `SaveRunnerMod`, the `UserMod2` entry point. Reads the command line, sets the crash-reporter flags and traces quits.
- `Runner.cs`: `Runner`, the run's options and state, and `Finish`, which writes the status line and quits.
- `Patches.cs`: the Harmony patches on `MainMenu.OnSpawn`, `App.LateUpdate` (loads the save from the menu, timeout watchdog), `Game.OnSpawn`, `SaveLoader.Save` (blocked) and `App.Quit`.
- `RunBehaviour.cs`: `RunBehaviour`, which runs the colony and samples once per game second, and `FridgeProbe`, the `fridges` probe.
- `Invoke-SaveRun.ps1`: the launcher. Copies the save into `runs/<Label>-<stamp>/`, rewrites `mods.json` for the run (`-With`, `-Without`), starts the game through Steam, restores `mods.json` and prints the status.
- `SaveRunner.csproj`: builds the mod through `common/oni-mod.props` and `common/oni-mod.targets`, which deploy it to the local mods folder.
- `mod.yaml`, `mod_info.yaml`: title "Save Runner (dev tool)", static id `Isochronous.SaveRunner`, version 0.1.0, minimum build 744825.
- `README.md`: setup, running, the probes and how it works.
- `runs/`: created by the launcher; one folder per run with the save copy, `probe.csv`, `probe.csv.status` and `Player.log`. Ignored by `common/.gitignore`.
- `.gitattributes`: line-ending and merge rules for this node's `.context/` files.

The `.context/` documents of this node: [domains](domains/README.md) (entities, glossary), [standards](standards/README.md) (code style, error handling, naming conventions), `decisions/` and `retrospectives/`.
