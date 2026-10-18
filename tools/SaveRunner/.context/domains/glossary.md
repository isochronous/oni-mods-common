---
doc_id: doc-7f969f33e853
description: "SaveRunner glossary: the abbreviations ONI, SMI, STT, SO and NMF used in probe columns and code, and definitions of run, probe, game versus real seconds, status file, loadout and menu settle"
---

# SaveRunner glossary

Terms a newcomer to SaveRunner needs. Entity detail is in [entities](entities.md).

## Abbreviations and Acronyms
**Id:** blk-bd9fd4d15776

| Abbreviation | Full Term | Context |
|--------------|-----------|---------|
| ONI | Oxygen Not Included | the game |
| SMI | state machine instance | `RefrigeratorController.StatesInstance`, read by the fridge probe |
| STT | `SimTemperatureTransfer` | the component that exchanges heat between an item and its cell; columns `surfaceArea`, `groundTransferScale`, `sttEnabled`, `simHandle`, `deltaKJ` |
| SO | Spaced Out | the expansion; the launcher reads it from `kplayerprefs.yaml` |
| NMF | No Magic Fridges | a mod whose `FridgeThermostat` the probe looks for |

## Cross-Domain Terms
**Id:** blk-cfb3406478e6

### Run
**Id:** blk-fca79a60d4bc  
**Definition**: one game launch by `Invoke-SaveRun.ps1`: load a save, play for `--saverun-seconds` game seconds, write the probe, quit. Its folder is `runs/<Label>-<stamp>`.  
**Used in**: the launcher, `Runner`, every file name under `runs/`.

### Probe
**Id:** blk-4e73227a1e2f  
**Definition**: a named recorder sampled once per game second during a run. `fridges` is the only one.  
**Used in**: `--saverun-probe`, `RunBehaviour`, `FridgeProbe`.

### Game seconds and real seconds
**Id:** blk-dd09f79f5775  
**Definition**: game seconds are scaled frame time (speed 3 passes three of them per real second); `--saverun-seconds` is in game seconds, `--saverun-timeout` in real seconds.  
**Used in**: `RunBehaviour.Update`, `App_LateUpdate_Patch`.

### Status file
**Id:** blk-67145d475ebb  
**Definition**: `<out>.status`, one JSON line (`done`, `timeout`, `no-save` or `runner-error`) that tells the launcher how the run ended.  
**Used in**: `Runner.Finish`, the launcher's final report.

### Loadout
**Id:** blk-e9f19f346fa7  
**Definition**: the list of enabled mods in `mods.json`. A run keeps the player's loadout, adds SaveRunner and `-With`, removes `-Without`, and puts the original back afterwards.  
**Used in**: `Invoke-SaveRun.ps1` (`Set-Mods`).

### Menu settle
**Id:** blk-480a83d50352  
**Definition**: the four-second wait between the main menu appearing and the load starting. Loading straight from `MainMenu.OnSpawn` threw inside a popup that was still laying out.  
**Used in**: `App_LateUpdate_Patch`.

## Related
**Id:** blk-a61962c63d5a

- See also: [entities](entities.md)
