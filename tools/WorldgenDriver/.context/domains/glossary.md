---
doc_id: doc-22b3d4e48b4f
description: "WorldgenDriver glossary: abbreviations (ONI, DLC, JSONL, cluster prefixes), terms to avoid with their replacements, and definitions of coordinate, applied coordinate, game modes, session, driver, launcher, watchdog, stage and percent, stories and mixing, Klei folder"
---

# WorldgenDriver glossary

Terms a newcomer to WorldgenDriver needs. The tool's own things are in [entities](entities.md).

## Abbreviations and Acronyms
**Id:** blk-362787e8d4b0

| Abbreviation | Full Term | Context |
|--------------|-----------|---------|
| ONI | Oxygen Not Included | the game, Steam app 457140 |
| DLC | downloadable content | Spaced Out is the DLC `EXPANSION1_ID` |
| JSONL | JSON Lines | the result file: one JSON object per line |
| CERS, PRES, FLIP, ... | cluster coordinate prefixes | the first part of a coordinate; each selects a cluster layout |

## Terms to Avoid
**Id:** blk-42c5bf5087eb

| Avoid | Use Instead | Reason |
|-------|-------------|--------|
| seed (for the whole string) | coordinate | the seed is one number inside the coordinate |
| run (for one game start) | session | a run is the whole launcher invocation; a session is one game start |
| world (for a cluster) | cluster | Spaced Out clusters hold several worlds; the driver generates a cluster |

## Cross-Domain Terms
**Id:** blk-f68ffe8e1af9

### Coordinate
**Id:** blk-67bd0802627c  
**Definition**: The game's settings coordinate for a cluster, such as `CERS-A-100-0-0-0` or `M-FLIP-C-100-0-0-0`: prefix, seed, settings code, story code and mixing code, joined by `-`.  
**Used in**: `--worldgen`, `Driver.Coordinates`, every result line, `Invoke-Worldgen.ps1`.

### Applied coordinate
**Id:** blk-78528b04c13b  
**Definition**: The coordinate the game reports after the driver has applied everything, recorded as `applied` in the result. It differs from the requested one when `-Stories` or `-Mixing` force settings on.  
**Used in**: `Patches.cs`, the result JSON.

### Spaced Out mode and base-game mode
**Id:** blk-e73beb812681  
**Definition**: The game runs either with the Spaced Out DLC enabled or without it, set by `EXPANSION1_ID.ENABLED` in `kplayerprefs.yaml`. A coordinate whose prefix ends in `-C` before the seed belongs to Spaced Out. Each mode needs its own game start.  
**Used in**: `Driver.WantExpansion1`, `IsSpacedOutCoordinate`, `Set-Mode`, the `wrong-mode` status.

### Session
**Id:** blk-2ab95eb3d015  
**Definition**: One game start handling up to `-PerSession` coordinates of one mode. A hang, crash or driver error ends it and the launcher starts another.  
**Used in**: `Invoke-Worldgen.ps1`, the `session` field and `-session<N>.log` files.

### Driver
**Id:** blk-c56103e5279a  
**Definition**: The mod half: it applies coordinates and writes results inside the game. The launcher is the other half.  
**Used in**: `Driver.cs`, `Patches.cs`.

### Launcher
**Id:** blk-2c6f7226991b  
**Definition**: `Invoke-Worldgen.ps1`, which sets up the game's files, starts the game and collects results.  
**Used in**: `README.md`, `Invoke-Worldgen.ps1`.

### Watchdog
**Id:** blk-76efb890c2be  
**Definition**: The `App.LateUpdate` postfix that ends a session on a crash screen or when no progress arrives for the timeout.  
**Used in**: `App_LateUpdate_Patch`, statuses `crash`, `hang`, `stuck-before-start`.

### Stage and percent
**Id:** blk-85f6c7b162b9  
**Definition**: The worldgen progress key and fraction the game reports through `OfflineWorldGen.UpdateProgress`; the result records the last ones seen.  
**Used in**: `Driver.Progress`, the `stage` and `percent` result fields.

### Stories and mixing
**Id:** blk-2c576fe6c640  
**Definition**: Story traits and world/subworld/DLC mixing are cluster settings with codes inside the coordinate. `-Stories all` forces every story on; `-Mixing all` or `guarantee` forces every mixing to try or guarantee.  
**Used in**: `Driver.Stories`, `Driver.Mixing`, `Patches.cs`.

### Klei folder
**Id:** blk-5acff59ffab2  
**Definition**: `Documents\Klei\OxygenNotIncluded`, holding `kplayerprefs.yaml`, `mods\mods.json`, local mods and the `worldgen-driver` working files.  
**Used in**: `Invoke-Worldgen.ps1`, `Driver.OutFile` default.

## Related
**Id:** blk-776586f7cf5e

- See also: [entities](entities.md)
