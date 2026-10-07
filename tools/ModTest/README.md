# ModTest

`Test-OniMods.ps1` launches Oxygen Not Included with only the mods under development enabled and
restores the normal loadout when the game exits.

```
pwsh -ExecutionPolicy Bypass -File common/tools/ModTest/Test-OniMods.ps1 [-Mods a,b,...] [-DryRun]
```

- Backs up `Documents\Klei\OxygenNotIncluded\mods\mods.json` to `mods.json.modtest-<timestamp>`.
- Disables every entry and enables the listed local mods for both the base game and Spaced Out.
  The default list is the data-dump mod plus the isochronous mods; third-party local mods
  (Fast Track) and retired ones (Refined Building) are left out.
- Registers local mod folders the game has not seen yet (a freshly built mod), using the game's own
  folder-name hash as the entry version so the first launch does not ask for a restart.
- Launches through Steam, waits for the game to exit (surviving a mod-triggered restart), then puts
  the backup back. Mod folders, configs and saves are never touched.
- `-DryRun` prints the loadout and writes it to the temp folder without launching.
