---
description: "TODO(owner): one line saying what WorldgenDriver is."
---
# WorldgenDriver

## What this is
**Id:** blk-60dae80eb17c

WorldgenDriver generates Oxygen Not Included clusters in the real game with nobody at the keyboard. It has two halves. The mod (`Isochronous.WorldgenDriver`, version 0.1.0 in `mod_info.yaml`, minimum build 744825, development-only and not for the Workshop), started with `--worldgen=COORD`, applies each coordinate at the main menu, opens the game's worldgen screen, watches its progress, writes one JSON line per coordinate and goes back to the menu for the next one or quits. The launcher, `Invoke-Worldgen.ps1`, groups coordinates by game mode, sets the mode in `kplayerprefs.yaml` and the enabled mods in `mods.json`, starts the game through Steam in sessions of up to 25 coordinates, starts a new session after a hang or crash, and restores both files at the end. Without the argument the mod does nothing, so it stays installed. The mod developer uses it to reproduce worldgen bug reports and to check a mod build, such as Natural Backwalls, against a list of worlds and seeds with the game's real worldgen and sim.

## Where things are
**Id:** blk-ebff91fa90b7

Top-level folders and key files:

- `Mod.cs`: the `UserMod2` entry point. Reads the command line and applies the Harmony patches only when a `--worldgen` argument is present.
- `Patches.cs`: the seven Harmony patches: apply the coordinate and open the worldgen screen at the main menu, record progress, the watchdog on `App.LateUpdate`, and the failure, completion and loaded-colony hooks.
- `Driver.cs`: the coordinate list, per-coordinate state and timing, and the result writer.
- `Invoke-Worldgen.ps1`: the launcher.
- `WorldgenDriver.csproj`: imports `oni-mod.props` and `oni-mod.targets` from the root of common; a build deploys the mod to the game's `mods/local/WorldgenDriver` folder. `mod.yaml` and `mod_info.yaml` beside it.
- `README.md`: setup, running, the options, the output and how it works.
- The launcher writes `runs/<Label>-<stamp>.jsonl` and a `Player.log` per session to `runs/`, which the first run creates and git ignores.

The `.context/` documents of this node: [domains](domains/README.md) (entities, glossary), [standards](standards/README.md) (code style, error handling, naming conventions), `decisions/` and `retrospectives/`.
