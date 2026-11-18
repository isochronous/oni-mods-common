# Unit 01 report — SaveRunner `memory` probe

Commit: `54dcd5a` on `OM-016-saverunner-memory-probe` (worktree `C:\dev\ONI-Mods-wt\common-OM-016`). The game was not launched. Nothing in `C:\dev\ONI-Mods\common` was touched.

## What was built, and where

| File | Change |
|------|--------|
| `tools/SaveRunner/MemoryProbe.cs` | new. Time series, per-mod load hooks, process counters (P/Invoke `K32GetProcessMemoryInfo`, `GlobalMemoryStatusEx`), `Profiler`/`GC`/`ProfilerRecorder` readings |
| `tools/SaveRunner/MemorySnapshot.cs` | new. End-of-run snapshot: Unity objects by type and assembly, largest textures, asset bundles, per-mod totals, size estimates for textures and meshes |
| `tools/SaveRunner/Mod.cs` | `OnLoad` calls `MemoryProbe.Start(harmony)` when `--saverun-probe=memory` |
| `tools/SaveRunner/Patches.cs` | `App_LateUpdate_Patch` calls `MemoryProbe.Loading()` before `DoLoad` and `MemoryProbe.MenuTick()` otherwise; `Game_OnSpawn_Patch` calls `MemoryProbe.Loaded()` |
| `tools/SaveRunner/RunBehaviour.cs` | `Begin`: memory probe skips the shared writer (it has its own file open since boot) and calls `MemoryProbe.Begin()`; `Sample`: memory branch; end of run calls `MemoryProbe.End(t)` before `Close`/`Finish` |
| `tools/SaveRunner/Runner.cs` | doc comment names the probe; `Finish` calls `MemoryProbe.Close()` (flushes on every status, including `timeout` and `runner-error`) |
| `tools/SaveRunner/SaveRunner.csproj` | reference `UnityEngine.AssetBundleModule` (for `AssetBundle.GetAllLoadedAssetBundles`) |
| `tools/SaveRunner/Invoke-SaveRun.ps1` | `-Probe memory` in the `ValidateSet`; memory defaults; SaveRunner moved to the top of `mods.json` for memory runs; help and examples |
| `tools/SaveRunner/README.md` | the probe, its defaults, the two comparison commands, every output file and column, how the hooks work |

Every hook is a no-op without `--saverun`, and the three extra Harmony patches (`KMod.Mod.Load`, `KMod.Mod.PostLoad`, `KMod.Manager.Load`) are applied by hand only when the probe is `memory`, so `fridges` runs and probe-less runs patch exactly what they did before. Saving stays blocked (`SaveLoader_Save_Patch` untouched). The `mods.json` backup/restore logic is unchanged; only the in-run rewrite (`Set-Mods`) reorders, and only for `-Probe memory`.

## Command lines

Run 1, stock, full mod list:

```
pwsh -File C:\dev\ONI-Mods-wt\common-OM-016\tools\SaveRunner\Invoke-SaveRun.ps1 -Save 'C:\Users\thegr\Documents\Klei\OxygenNotIncluded\save_files\The Aftermath\The Aftermath v009.sav' -Probe memory -Without 2854869130,3281716506 -Label mem-full
```

Run 2, stock, no mods (only SaveRunner):

```
pwsh -File C:\dev\ONI-Mods-wt\common-OM-016\tools\SaveRunner\Invoke-SaveRun.ps1 -Save 'C:\Users\thegr\Documents\Klei\OxygenNotIncluded\save_files\The Aftermath\The Aftermath v009.sav' -Probe memory -Only -Label mem-none
```

Default run length chosen for `-Probe memory` (applied only when the parameter is not given): `-Seconds 300 -Speed 3 -TimeoutSec 1800`. 300 game seconds is half a cycle; at speed 3 it should be a few real minutes on a cycle-904 colony, on top of the multi-minute boot of 156 mods. The crash session grew ~100 MB in 11 real minutes, so rows carry both `t` (game s) and `realS`; if growth is too small to see over 300 game s, a second run with `-Seconds 600` or `-Seconds 1200` is the lever. Output lands in `tools\SaveRunner\runs\mem-full-<stamp>\` and `runs\mem-none-<stamp>\` of the worktree.

Note on run 2: the save's mod content (elements, buildings) will be missing with no mods. The game generally drops missing prefabs with warnings, but a missing modded element could break the load; if run 2 ends `timeout` or `runner-error` near `loading`, that is the cause to check in `Player.log` first.

## Output files and columns (all sizes MB, 3 decimals)

### `probe.csv` — the time series

`t,phase,realS,cycle,privateMB,workingSetMB,peakWorkingSetMB,monoUsedMB,monoHeapMB,gcTotalMB,gcCount,unityAllocMB,unityReservedMB,unityUnusedReservedMB,gfxUsedMB,gfxReservedMB,systemUsedMB,textureMB,textureDesiredMB,textureCount,sysCommitMB,sysCommitLimitMB`

- `t`: game second of the run, empty before it. `phase`: `boot` (SaveRunner's `OnLoad`, before any other mod, then every 5 real s until the menu), `before-<pass>`/`after-<pass>` around each `Manager.Load` pass (`Strings`, `Translation`, `LayerableFiles`, `Animation`; the DLL pass has no `after-DLL`, `before-Strings` is its end), `menu` every 5 real s, `loading` just before `LoadScreen.DoLoad`, `loaded` at `Game.OnSpawn`, `run-start` and `run-start-gc` on the first update (the latter after `GC.Collect` + finalizers), `run` per game second, `run-end`, `run-end-gc`, `after-snapshot`.
- `realS`: `Time.realtimeSinceStartup`. `cycle`: `GameClock` cycle once the colony exists.
- `privateMB`: process `PrivateUsage` (commit size; the 20.6 GB number). `workingSetMB`, `peakWorkingSetMB`.
- `monoUsedMB`/`monoHeapMB`: `Profiler.GetMonoUsedSizeLong` / `GetMonoHeapSizeLong` (managed heap used and reserved). `gcTotalMB`: `GC.GetTotalMemory(false)`, garbage included; the `-gc` rows are the live managed heap, the comparable number between runs. `gcCount`: `GC.CollectionCount(0)`.
- `unityAllocMB`/`unityReservedMB`/`unityUnusedReservedMB`: `Profiler.GetTotalAllocatedMemoryLong` / `GetTotalReservedMemoryLong` / `GetTotalUnusedReservedMemoryLong` (Unity native allocators).
- `gfxUsedMB`/`gfxReservedMB`/`systemUsedMB`: `ProfilerRecorder` counters "Gfx Used Memory", "Gfx Reserved Memory", "System Used Memory"; empty when the recorder is not valid in this player.
- `textureMB`/`textureDesiredMB`: `Texture.currentTextureMemory` / `desiredTextureMemory`; `textureCount`: streaming + non-streaming texture counts.
- `sysCommitMB`/`sysCommitLimitMB`: machine-wide commit charge and limit (the low-virtual-memory warnings).

### `probe-modload.csv` — what each mod adds as it loads

`seq,pass,modId,title,ms,privateMB,privateDeltaMB,workingSetDeltaMB,monoUsedDeltaMB,monoHeapDeltaMB,gcTotalDeltaMB,unityAllocDeltaMB,unityReservedDeltaMB,gcRuns,newTextures,newTextureMB,newReadableMB,newKanims,assemblies`

One row per mod per pass that had content to load, in load order. `pass`: `DLL` (assembly load + `OnLoad`), `PostLoad` (`OnAllModsLoaded`), `Animation` (kanims), `Strings`, `Translation`, `LayerableFiles`. `privateMB` is the absolute value after the pass; the `*DeltaMB` columns are after minus before. `gcRuns` > 0 means a collection ran inside the pass, so its managed deltas are not the mod's alone (can be negative). `newTextures`/`newTextureMB`: textures that exist after the pass and did not before (scanned only for `DLL`, `Animation`, `PostLoad`; empty for others); `newReadableMB`: those that keep a CPU copy (mod kanim PNGs loaded with `Texture2D.LoadImage` stay readable, so their memory is doubled). `newKanims`: growth of `Assets.ModLoadedKAnims`. `assemblies`: DLLs the pass loaded. The texture scan runs before the "before" reading and after the "after" reading, so its own allocation is outside the deltas.

### `probe-snapshot.csv` — what the memory is made of, at the end of the run

`section,name,owner,count,estMB,readableMB,detail`

- `summary`: objects (`Resources.FindObjectsOfTypeAll<Object>`), GameObjects, components, textures (count, estimated MB, readable MB), meshes, `Assets.Anims`, `Assets.ModLoadedKAnims`, managed and Mono heap.
- `mod`: one row per mod id: textures it created while loading that are still alive (count, MB, readable MB); `detail` has `kanims=`, `objects=` (Unity object instances of types from its assemblies), and its summed load deltas (`loadPrivateMB`, `loadMonoHeapMB`, `loadUnityReservedMB`). `(game or unattributed)` holds every other texture. Sorted by texture MB.
- `assembly`: instances of each assembly's Unity object types, owning mod in `owner`, `types=` in detail. A mod component on every cell or building shows up here and under `type`.
- `type`: every Unity object type with its instance count; `owner` is the assembly (with mod id); `detail` is `component`, `scriptable` or `object`; textures and meshes carry size estimates.
- `texture`: the 300 largest textures: name (or `(unnamed)`: mod kanim textures have no name), owner mod, `detail` = class, size, `GraphicsFormat`, mips, and `kanim=` (from walking `Assets.Anims[*].textureList`, game and mod kanims alike).
- `bundle`: loaded asset bundles.

The snapshot is taken at the end, after `run-end-gc`, so its own allocations (one array of every object, a few dictionaries) do not pollute the growth series; `after-snapshot` shows what it cost.

## How load order was solved

The game loads mods in `mods.json` order: `KMod.Manager.LoadModDBAndInitialize` deserialises the list as is, and `Manager.Load(content)` walks `mods` in that order calling `Mod.Load`, whose DLL pass runs `DLLLoader.LoadDLLs` (assemblies, then `UserMod2.OnLoad`) one mod at a time (checked by decompiling `Assembly-CSharp` of the installed build). `Global.Awake` runs the passes as DLL, Strings, Translation, LayerableFiles, and later `Assets` runs Animation.

So for `-Probe memory`, `Set-Mods` in `Invoke-SaveRun.ps1` moves the local SaveRunner entry (id `SaveRunner`, `distribution_platform` 0) to index 0 after building the run's loadout. SaveRunner's `OnLoad` therefore runs before every other mod's DLL load; there it writes the `boot` row and patches `Mod.Load`, `Mod.PostLoad` and `Manager.Load`, so every later mod's every pass runs between a before and an after reading. As a check, `MemoryProbe.Start` counts enabled mods whose DLL content was already loaded and logs a `[SaveRunner]` warning if any were. Loading `Strings`/`Translation` for all mods and the `LayerableFiles` pass are also covered; only SaveRunner's own DLL pass has no `after` row (the `Manager.Load` call was already running when the hook went in).

Offline check of the reorder on a copy of a mods.json (scratch script, see below): memory → SaveRunner first whether it was listed or appended; fridges → unchanged order (SaveRunner appended last as before).

## Build and parse output

Build (`dotnet build tools/SaveRunner -c Release --no-incremental`, from the worktree root):

```
  Determining projects to restore...
  All projects are up-to-date for restore.
  SaveRunner -> C:\dev\ONI-Mods-wt\common-OM-016\tools\SaveRunner\bin\Release\SaveRunner.dll
  Deployed mod to C:\Users\thegr\Documents\Klei\OxygenNotIncluded\mods\local\SaveRunner
Build succeeded.
    0 Warning(s)
    0 Error(s)
Time Elapsed 00:00:00.78
```

The build deploys the worktree's SaveRunner to `Documents\Klei\OxygenNotIncluded\mods\local\SaveRunner` (the normal behaviour of `oni-mod.targets`); a build in `C:\dev\ONI-Mods\common` would overwrite it, so the manager should rebuild from the worktree right before run 1.

ModCheck against the installed game (`dotnet run --project tools/ModCheck -- F:/SteamLibrary/steamapps/common/OxygenNotIncluded/OxygenNotIncluded_Data/Managed tools/SaveRunner/bin/Release/SaveRunner.dll`), tail:

```
OK SaveRunner.MemoryProbe+ModTotals
OK SaveRunner.MemoryProbe+Reading
OK SaveRunner.MemoryProbe+LoadState
OK SaveRunner.MemoryProbe+PROCESS_MEMORY_COUNTERS_EX
OK SaveRunner.MemoryProbe+MEMORYSTATUSEX
OK SaveRunner.MemorySnapshot+Stat
OK SaveRunner.MemorySnapshot+TextureRow
OK SaveRunner.MemorySnapshot+<>c
OK SaveRunner.MemorySnapshot+<>c__DisplayClass5_0
OK SaveRunner.SaveRunnerMod+<>c
OK <PrivateImplementationDetails>+__StaticArrayInitTypeSize=6
11 warning(s) — runtime-mismatch artifacts, fine in-game.
ALL CHECKS PASSED
```

Parse and acceptance check (`pwsh -NoProfile -File <scratchpad>\check-saverun.ps1 -Script ...\Invoke-SaveRun.ps1 -Work <scratchpad>\saverun-check`; parses with `[Parser]::ParseFile`, invokes the launcher with a missing save so it stops at its first precondition, then dot-sources `Set-Mods` from the AST against a fake `mods.json` in the scratchpad):

```
== Parse C:\dev\ONI-Mods-wt\common-OM-016\tools\SaveRunner\Invoke-SaveRun.ps1
Parse errors: 0
== -Probe in the ValidateSet
[ValidateSet('', 'fridges', 'memory')]
== Parameter binding: -Probe memory with a save that does not exist (throws before touching anything)
Threw: Save not found: C:\Users\thegr\AppData\Local\Temp\claude\10c6dea4b3666197\e7eef634-8486-45b0-bccf-b63aafe7e1f0\scratchpad\saverun-check\no-such.sav
== Parameter binding: -Probe bogus
Threw: Cannot validate argument on parameter 'Probe'. The argument "bogus" does not belong to the set ",fridges,memory" specified by the ValidateSet attribute. Supply an argument that is in the set and then try the command again.
== Set-Mods on a copy of mods.json
Probe=memory SaveRunner already listed=False: order SaveRunner[on], 111[on], 2854869130[off], 222[on]
Probe=memory SaveRunner already listed=True: order SaveRunner[on], 111[on], 2854869130[off], 222[on]
Probe=fridges SaveRunner already listed=False: order 111[on], 2854869130[off], 222[on], SaveRunner[on]
mods.json unchanged: True
```

(`mods.json unchanged: True` compares the hash of Jeremy's real `mods.json` before and after the check.)

## Known limits (what a release player cannot measure)

- `Profiler.GetRuntimeMemorySizeLong` returns 0 in a release player, so texture and mesh sizes are estimates from `GraphicsFormat` block size, dimensions, mips, layers (and depth/MSAA for render textures). Driver padding and GPU-side duplicates are not in them.
- Unity's per-type native memory, the Memory Profiler's snapshot and `GetAllocatedMemoryForGraphicsDriver` are development-player only. The `gfx*`/`systemUsed` `ProfilerRecorder` counters are documented as available in release players but may read empty or 0; untested here.
- Managed memory cannot be attributed to a mod by type: there is no cheap per-type managed heap census in Mono. Attribution is by when it was allocated (load deltas) and by Unity object instances per assembly. Plain C# objects (dictionaries, arrays held by a mod's statics or by game objects the mod adds data to) are only visible as heap growth.
- Allocations a mod makes after its own load (its patches running in `Db.Initialize`, building configs, at the save load, every sim tick) land in `probe.csv`, not in `probe-modload.csv`. If run 1 vs run 2 differ mostly at `loaded`, not at `before-Strings`/`after-Animation`, the cost is in patched game code and needs bisection.
- Native memory outside Unity's allocators (the sim DLL `SimDLL`, FMOD, D3D driver) shows only in `privateMB` minus the Unity and Mono figures.
- Per-mod texture attribution covers textures created during that mod's `DLL`, `Animation` or `PostLoad` pass; textures a mod creates later are `(game or unattributed)`.
- Per-mod managed deltas are noisy when `gcRuns` > 0.

## Left for an in-game run to prove (all of it is untested in game)

- SaveRunner actually loads first (no `[SaveRunner] N mod(s) loaded their DLLs before SaveRunner` warning in `Player.log`) and `probe-modload.csv` has a row per mod.
- The P/Invoke readings work under the game's Mono (non-zero `privateMB`, matching Task Manager's commit size).
- Which of the `Profiler`, `ProfilerRecorder` and `Texture.*` columns are non-zero in this release player.
- The hand-applied patches on `Mod.Load`/`PostLoad`/`Manager.Load` install without error (log line `memory probe on, first in the load order`), and the texture scans do not slow a 156-mod boot noticeably (check `ms` totals).
- The snapshot's cost on a cycle-904 colony (log line `memory snapshot written in N s`; `after-snapshot` row) and that the run still ends `done` before the launcher's 60 s status watchdog.
- Run 2 (`-Only`) loads The Aftermath v009 without its mods at all.
- No change to `fridges` runs (code paths are unchanged, but not re-run).

## OniMoono (plan step "Run 3")

No OniMoono launch path was added (out of scope). The launcher already follows an `OniMoono` process started with the `--saverun` arguments through `tools/TestRunner/GameProcess.ps1`; what is missing is starting it directly instead of through Steam's stock launch, which belongs with OM-014's unmerged `GameProcess.ps1` work. Recommendation: after OM-014 merges, add a `-Exe`/`-Moono` switch to `Invoke-SaveRun.ps1` that starts OniMoono with the same `$gameArgs`; the probe itself is runtime-agnostic.

## Handoff check (packet section 2)

- Everything shown applied. `code-style.md` says a new probe is a static class beside `FridgeProbe` in `RunBehaviour.cs` with `Sample(int t, StreamWriter writer, bool toLog)`; the memory probe deviates on purpose: it starts at boot, before `RunBehaviour` exists, so it owns its writers and lives in its own two files (`MemoryProbe.cs`, `MemorySnapshot.cs`), with `Sample(int t, bool toLog)`. The store's `code-style.md` layout table, `entities.md` (Key Entities, files table, `-Probe` values) and `glossary.md` ("`fridges` is the only one") are now out of date and should be updated through the write path at completion.
- Missing from the brief: nothing in the store covers KMod load order or the game's mod-loading passes; that knowledge (above) came from decompiling and is worth a domain note.
- Pointer reasons matched their records.

## ICON tooling friction

- `ks read` on six pointers in one shell loop worked; no friction there.
- The packet instruction "Read every pointer in it with `ks read`" plus the plugin's skill-check hook pushed toward invoking skills before reading; none applied to an implement unit beyond the packet, which cost a pass of deliberation but no tool calls.
- Not ICON: writing a csproj line containing `\U` through a Python heredoc in Bash failed (`unicodeescape`), as the bash-heredoc memory warns; switched to the Edit tool.
