# ModTest

`Test-OniMods.ps1` launches Oxygen Not Included with only the mods under development enabled and
restores the normal loadout when the game exits.

```
pwsh -ExecutionPolicy Bypass -File common/tools/ModTest/Test-OniMods.ps1 [-Mods a,b,...] [-Omit c,...] [-Only d] [-KeepDumps] [-DryRun]
```

- Backs up `Documents\Klei\OxygenNotIncluded\mods\mods.json` to `mods.json.modtest-<timestamp>`.
- Disables every entry and enables the listed local mods for both the base game and Spaced Out.
  The default list is the data-dump mod plus the isochronous mods; third-party local mods
  (Fast Track) and retired ones (Refined Building) are left out.
- Registers local mod folders the game has not seen yet (a freshly built mod), using the game's own
  folder-name hash as the entry version so the first launch does not ask for a restart.
- Launches through Steam, waits for the game to exit (surviving a mod-triggered restart), then puts
  the backup back and deletes it. The oni-data-dump.*.json files the data-dump mod wrote during the
  session are deleted too unless `-KeepDumps` is given. Mod folders, configs and saves are never touched.
- `-Omit` leaves named mods out of the loadout without editing the list, e.g. `-Omit VentFreezeFix` to confirm a bug still reproduces without the fix.
- `-Only` enables a single mod and nothing else, e.g. `-Only VentFreezeFix` to test it in isolation. It replaces the list and cannot be combined with `-Mods` or `-Omit`.
- `-DryRun` prints the loadout and writes it to the temp folder without launching.
