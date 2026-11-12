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
- [ ] Run 1: stock, full mod list (-Without 2854869130,3281716506 per save-runner notes) ← IN PROGRESS
- [ ] Run 2: stock, no mods
- [ ] Run 3: OniMoono, full mod list (SaveRunner needs a Moono launch path — OM-014's GameProcess helpers are on an unmerged branch; decide in unit 01 report)
- [ ] Compare, report to Jeremy; ask before any bisection
- [ ] Completion: review, store update, retro, commit, PR

## Open Questions / Blockers
- Steam's Play now starts the stock game (OM-014); SaveRunner launches through Steam, so a run today measures stock, while Jeremy plays on OniMoono. Direct Moono launch is unproven (OM-014 awaiting its first run).

## Constraints
- In-game runs take Jeremy's desktop: ask before long or chained runs; never -KeepGame; never stop a runner mid-run.
- SaveRunner blocks saving; never save over the Aftermath save.
- Disable Mod Profile Manager (2854869130) and its Light addon (3281716506) for runs.
