## Task: OM-016
## Branch: OM-016-saverunner-memory-probe (common/, worktree C:\dev\ONI-Mods-wt\common-OM-016, cut from origin/main 5078e10)
## Objective: Jeremy's game (OniMoono.exe) reached 20.6 GB committed memory and the PC hard-reset on 2026-11-11 ~17:06 (Kernel-Power 41, no bugcheck, low-virtual-memory warnings 16:55-17:06; 63 GB RAM + fixed 4 GB page file). He reports his mod list roughly doubles ONI's RAM. Find out whether one mod is responsible or the list as a whole: add a `memory` probe to SaveRunner, run The Aftermath v009 with his full mod list and with no mods, compare.
## Folder: common/.context/tasks/OM-016-saverunner-memory-probe/

## Phase State
- **Phase plan**: implementation → testing → completion
- **Completed**: (none)
- **Current**: implementation   (status: pending)
- **Next**: testing
- **Loaded skill**: task-plan-phase-implementation
- **Branch**: OM-016-saverunner-memory-probe
- **Attempts (current phase)**: 0

## Decisions
- Task id OM-016: highest in use OM-015 (ONI-Mods branch OM-015-fast-wireless-automation-fix); common's store tops at OM-014.
- Worktree, not a branch switch in common/: common/ holds OM-014's uncommitted TestRunner/ModTest work (af9fd71 + working tree); this task must not touch it.
- Base save (Jeremy, 2026-11-12): The Aftermath v009 (save_files\The Aftermath\The Aftermath v009.sav, 11.9 MB, cycle 904, written 2026-11-11 17:07:50 — the minute of the crash). Checked: no zero blocks, normal tail. Not Frigidaire (older save, different mod list).
- Disk sizes already measured: 156 enabled mods total 624 MB on disk (largest Decor Pack I 123 MB, True Tiles 75, Rocketry Expanded 69); disk assets cannot explain ~10 GB, so the probe measures runtime memory.
- Probe content (proposed to Jeremy, accepted "Sure"): process private bytes / working set, Mono used + heap (UnityEngine.Profiling.Profiler.GetMonoUsedSizeLong / GetMonoHeapSizeLong), Unity total allocated / reserved, GC.GetTotalMemory, texture count + estimated bytes, sampled over game time; plus per-mod load deltas (private bytes and Mono heap before/after each mod's load).
- Runs: (1) full mod list, (2) no mods. Bisection (~8 runs) only if run 1 points at managed memory, and only after asking.

## Key Files
- tools/SaveRunner/Invoke-SaveRun.ps1: launcher (Steam launch, -Without/-Only loadout, runs/<label>-<stamp>/)
- tools/SaveRunner/RunBehaviour.cs, Patches.cs, Mod.cs: dev mod; probes write probe.csv

## Progress
- [x] Decide runtime — Jeremy 2026-11-12: stock first (full vs none), then one full-mod run on OniMoono
- [x] Implement memory probe + per-mod load deltas — unit 01, commit 54dcd5a; build 0/0, ModCheck pass, parse 0 errors; untested in game (units/01-implement-report.md: outputs probe.csv / probe-modload.csv / probe-snapshot.csv; SaveRunner moved first in mods.json for -Probe memory; defaults 300 game s, speed 3, timeout 1800 s). Rebuild from the worktree before each run (a build in C:\dev\ONI-Mods\common overwrites mods\local\SaveRunner). Store docs (code-style layout, entities, glossary) out of date → completion; KMod load-order note worth a domain doc.
- [x] Run 1: stock, full mod list — runs/mem-full-20261112-104004, status done, 270 s. private: boot 5.15 GB → after mod kanims (Animation pass) 10.4 → menu 12.5 → loaded 16.9 → run-start 19.4 → plateau 20.7 GB from t=150 (flat to t=300; no leak at this scale; late growth = textureMB 6.6→7.3 GB). Animation pass +4.4 GB, DLL pass +0.38, PostLoad +0.09. Top load deltas: Visual Fixes and Tweaks 1.0 GB, Rocketry Expanded 0.95, Ronivan's IR 0.81 (monoHeap +384), AI Controlled Rockets 0.5, Decor Pack I 0.28, Klei Stream Painting Skins 0.22, Signs Tags Ribbons 0.2, Winters Delight 0.16. Mod kanim textures stay readable (private ≈ 2× texture MB). Snapshot: textures est 7.76 GB (3.83 readable), managed heap 4.44 GB, 1.98 M objects; SuppressNotifications adds ~125k components (31k per type on every building).
- [x] Run 2: stock, no mods — runs/mem-none-20261112-104522, status done, 170 s; save loaded without its mods. private: loaded 10.8 → run-start-gc 12.26 → run-end-gc 12.34 GB (flat). Textures est 4.27 GB (1.13 readable), managed heap 3.65 GB, 1.28 M objects.
- [x] Compare: full 20.7 vs none 12.3 GB = +8.4 GB (1.7×). Textures +3.5 GB est (readable CPU copies +2.7), Unity reserved +3.3, managed +0.8, objects +0.7 M. Eight art-heavy mods ≈ 4.1 GB of load-time private (VFT 1.0, Rocketry Expanded 0.95, Ronivan IR 0.81, AI Controlled Rockets 0.5, Decor Pack I 0.28, Klei Stream Painting 0.22, Signs Tags Ribbons 0.2, Winters Delight 0.16); the rest is spread over ~150 mods. No single runaway mod, no leak. Side note: machine commit was 31.9 GB at run 1 boot with the game at 5.1 GB → ~27 GB held by other processes.
- [ ] Report to Jeremy; Run 3 (Moono) needs a launch path → ask
- [ ] Run 3: OniMoono, full mod list (SaveRunner needs a Moono launch path — OM-014's GameProcess helpers are on an unmerged branch; decide in unit 01 report)
- [ ] Compare, report to Jeremy; ask before any bisection
- [ ] Completion: review, store update, retro, commit, PR

## Open Questions / Blockers
- Steam's Play now starts the stock game (OM-014); SaveRunner launches through Steam, so a run today measures stock, while Jeremy plays on OniMoono. Direct Moono launch is unproven (OM-014 awaiting its first run).

## Constraints
- In-game runs take Jeremy's desktop: ask before long or chained runs; never -KeepGame; never stop a runner mid-run.
- SaveRunner blocks saving; never save over the Aftermath save.
- Disable Mod Profile Manager (2854869130) and its Light addon (3281716506) for runs.
