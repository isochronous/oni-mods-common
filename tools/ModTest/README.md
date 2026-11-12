# ModTest

`Test-OniMods.ps1` launches Oxygen Not Included with only the mods under development enabled and
restores the normal loadout when the game exits.

```
pwsh -ExecutionPolicy Bypass -File common/tools/ModTest/Test-OniMods.ps1 [-Mods a,b,...] [-Omit c,...] [-Only d] [-NoLocal] [-Steam id,...] [-KeepDumps] [-Runtime Stock|Moono|Both] [-MoonoExe path] [-DryRun]
```

- Backs up `Documents\Klei\OxygenNotIncluded\mods\mods.json` to `mods.json.modtest-<timestamp>`.
- Disables every entry and enables the local mods for both the base game and Spaced Out. By default
  that is every folder under `mods/local` with a `mod.yaml`, minus the names in `disabled.txt` next
  to the script (one per line, `#` for comments): third-party local mods such as Fast Track, retired
  ones such as Refined Building, and personal ones that will not be published. `-Mods` replaces the
  default list. Workshop items listed in `included.txt` next to the script (one id per line) are
  always enabled too, for quality-of-life mods that make testing easier (a wider zoom out, sandbox
  tools); `-Steam` adds more for one run.
- Registers local mod folders the game has not seen yet (a freshly built mod), using the game's own
  folder-name hash as the entry version so the first launch does not ask for a restart.
- Launches through Steam, follows the game it started (through a mod-triggered restart, which goes
  through the game's Restarter) and puts the backup back the moment that game exits, so a game
  launched by hand straight after gets the normal loadout. The restored file is dated when it comes
  back. Neither the test loadout nor the restored one carries the game's "mod load in progress" mark,
  which a game closed before the main menu leaves and which starts the next boot in mod safe mode.
  The backup is deleted once restored. The oni-data-dump.*.json files the data-dump mod wrote during the
  session are deleted too unless `-KeepDumps` is given. Mod folders, configs and saves are never touched.
- `-Runtime Moono` starts oni-moono's player directly instead of Steam:
  `OniMoono.exe` with no arguments, no `--game`: Moono gets the game folder from Steam (the player is `-MoonoExe`, else `ONI_MOONO_EXE`, else
  `F:\modding\oni-moono\player\OniMoono.exe`; Moono starts Steam itself when it is not running).
  `-Runtime Both` runs a stock session and, once that game is closed and the loadout put back, a
  Moono session with the same loadout. On Moono the game runs as `OniMoono` instead of
  `OxygenNotIncluded`. The launch carries no arguments to tell a launched game from one opened by
  hand, so the script refuses to start while either is open (or the Restarter) and follows the one
  that starts after the launch, leaving out Moono's crash watcher, a second `OniMoono.exe` with
  `--crash-watch` (`tools/TestRunner/GameProcess.ps1`). The Moono session fails at once if a stock
  `OxygenNotIncluded` starts that was not running before the launch, or if `OniMoono` exits before the game is found.
- `-Omit` leaves named mods out of the loadout without editing the list, e.g. `-Omit VentFreezeFix` to confirm a bug still reproduces without the fix.
- `-Only` enables a single mod and nothing else, e.g. `-Only VentFreezeFix` to test it in isolation. It replaces the list and cannot be combined with `-Mods` or `-Omit`.
- `-NoLocal` enables no local mods at all, for running only `-Steam` items. It cannot be combined with `-Mods`, `-Omit` or `-Only`.
- `-Steam` enables subscribed Workshop items as well, by id, e.g. `-Steam 3816086407` to test alongside another author's mod. An item subscribed since the game last ran is downloaded through the Steam client (via `tools/WorkshopUpload download`), unpacked into `mods/Steam/<id>` and registered, which is what the game would do at its next launch; an unsubscribed id is skipped.
- `-DryRun` prints the loadout and each session's launch command, and writes the loadout to the temp folder without launching.
