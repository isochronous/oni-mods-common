## Task: OM-016
## Branch: OM-016-saverunner-memory-probe (common/, worktree C:\dev\ONI-Mods-wt\common-OM-016, cut from origin/main 5078e10)
## Objective: Jeremy's game (OniMoono.exe) reached 20.6 GB committed memory and the PC hard-reset on 2026-11-11 ~17:06 (Kernel-Power 41, no bugcheck, low-virtual-memory warnings 16:55-17:06; 63 GB RAM + fixed 4 GB page file). He reports his mod list roughly doubles ONI's RAM. Find out whether one mod is responsible or the list as a whole: add a `memory` probe to SaveRunner, run The Aftermath v009 with his full mod list and with no mods, compare.
## Folder: common/.context/tasks/OM-016-saverunner-memory-probe/

## Phase State
- **Phase plan**: implementation → testing → completion
- **Completed**: implementation, testing (in-game runs), completion
- **Current**: none   (status: done)
- **Next**: none — task complete once the PR merges
- **Loaded skill**: task-plan-phase-completion
- **Branch**: OM-016-saverunner-memory-probe
- **Attempts (current phase)**: 1

## Decisions
- Task id OM-016: highest in use OM-015 (ONI-Mods branch OM-015-fast-wireless-automation-fix); common's store tops at OM-014.
- Worktree, not a branch switch in common/: common/ holds OM-014's uncommitted TestRunner/ModTest work (af9fd71 + working tree); this task must not touch it.
- Base save (Jeremy, 2026-11-12): The Aftermath v009 (save_files\The Aftermath\The Aftermath v009.sav, 11.9 MB, cycle 904, written 2026-11-11 17:07:50 — the minute of the crash). Checked: no zero blocks, normal tail. Not Frigidaire (older save, different mod list).
- Disk sizes already measured: 156 enabled mods total 624 MB on disk (largest Decor Pack I 123 MB, True Tiles 75, Rocketry Expanded 69); disk assets cannot explain ~10 GB, so the probe measures runtime memory.
- Probe content (proposed to Jeremy, accepted "Sure"): process private bytes / working set, Mono used + heap (UnityEngine.Profiling.Profiler.GetMonoUsedSizeLong / GetMonoHeapSizeLong), Unity total allocated / reserved, GC.GetTotalMemory, texture count + estimated bytes, sampled over game time; plus per-mod load deltas (private bytes and Mono heap before/after each mod's load).
- Runs: (1) full mod list, (2) no mods. Bisection (~8 runs) only if run 1 points at managed memory, and only after asking.

## Key Files
- tools/SaveRunner/MemoryProbe.cs (new): time series from boot, KMod load hooks, P/Invoke process counters; fail-safe after 1fbb20a
- tools/SaveRunner/MemorySnapshot.cs (new): end-of-run census
- tools/SaveRunner/Invoke-SaveRun.ps1: `-Probe memory`, its defaults, SaveRunner first in the run's mods.json
- tools/SaveRunner/Mod.cs, Patches.cs, RunBehaviour.cs, Runner.cs, SaveRunner.csproj, README.md: hook-up, AssetBundleModule reference, docs
- tools/SaveRunner/.context/domains/entities.md, standards/code-style.md, domains/glossary.md; .context/retrospectives/icon.md: store writes
- Run data (gitignored): tools/SaveRunner/runs/mem-full-20261112-104004, mem-none-20261112-104522, mem-verify-20261112-105442

## Progress
- [x] Decide runtime — Jeremy 2026-11-12: stock first (full vs none), then one full-mod run on OniMoono
- [x] Implement memory probe + per-mod load deltas — unit 01, commit 54dcd5a; build 0/0, ModCheck pass, parse 0 errors; untested in game (units/01-implement-report.md: outputs probe.csv / probe-modload.csv / probe-snapshot.csv; SaveRunner moved first in mods.json for -Probe memory; defaults 300 game s, speed 3, timeout 1800 s). Rebuild from the worktree before each run (a build in C:\dev\ONI-Mods\common overwrites mods\local\SaveRunner). Store docs (code-style layout, entities, glossary) out of date → completion; KMod load-order note worth a domain doc.
- [x] Run 1: stock, full mod list — runs/mem-full-20261112-104004, status done, 270 s. private: boot 5.15 GB → after mod kanims (Animation pass) 10.4 → menu 12.5 → loaded 16.9 → run-start 19.4 → plateau 20.7 GB from t=150 (flat to t=300; no leak at this scale; late growth = textureMB 6.6→7.3 GB). Animation pass +4.4 GB, DLL pass +0.38, PostLoad +0.09. Top load deltas: Visual Fixes and Tweaks 1.0 GB, Rocketry Expanded 0.95, Ronivan's IR 0.81 (monoHeap +384), AI Controlled Rockets 0.5, Decor Pack I 0.28, Klei Stream Painting Skins 0.22, Signs Tags Ribbons 0.2, Winters Delight 0.16. Mod kanim textures stay readable (private ≈ 2× texture MB). Snapshot: textures est 7.76 GB (3.83 readable), managed heap 4.44 GB, 1.98 M objects; SuppressNotifications adds ~125k components (31k per type on every building).
- [x] Run 2: stock, no mods — runs/mem-none-20261112-104522, status done, 170 s; save loaded without its mods. private: loaded 10.8 → run-start-gc 12.26 → run-end-gc 12.34 GB (flat). Textures est 4.27 GB (1.13 readable), managed heap 3.65 GB, 1.28 M objects.
- [x] Compare: full 20.7 vs none 12.3 GB = +8.4 GB (1.7×). Textures +3.5 GB est (readable CPU copies +2.7), Unity reserved +3.3, managed +0.8, objects +0.7 M. Eight art-heavy mods ≈ 4.1 GB of load-time private (VFT 1.0, Rocketry Expanded 0.95, Ronivan IR 0.81, AI Controlled Rockets 0.5, Decor Pack I 0.28, Klei Stream Painting 0.22, Signs Tags Ribbons 0.2, Winters Delight 0.16); the rest is spread over ~150 mods. No single runaway mod, no leak. Side note: machine commit was 31.9 GB at run 1 boot with the game at 5.1 GB → ~27 GB held by other processes.
- [x] Report to Jeremy — done 2026-11-12; no bisection needed (no single runaway mod)
- [x] Run 3: OniMoono — dropped: Jeremy 2026-11-12 "treat it as a ticket and finish up"; stock already reproduces 20.7 GB. Follow-up below.
- [x] Review — unit 02 (units/02-review-report.md): 0 critical, 2 moderate (F1 unguarded hooks/Start, F2 Close could throw before the status write), 3 minor (F3 recorder disposes, F4 README LayerableFiles, F5 store docs)
- [x] Fix F1–F4 — unit 03, commit 1fbb20a; build 0/0, ModCheck pass (units/03-implement-report.md). F5 by the store update below.
- [x] Re-review 54dcd5a..1fbb20a — unit 04 (units/04-review-report.md): F1–F4 fixed, no blocking findings; nit N1 below
- [x] Verify in game after the fixes — runs/mem-verify-20261112-105442 (-Only, 30 game s): status done, "memory probe on, first in the load order", snapshot 0.6 s, run-end-gc 12.25 GB (matches run 2)
- [x] Store update — tools/SaveRunner domains/entities.md (Memory probe, Which build a run uses, RunBehaviour, FridgeProbe, Invoke-SaveRun.ps1, Files), standards/code-style.md (Layout), domains/glossary.md (Probe); `ks check --store .` 0 block
- [x] Retro — common .context/retrospectives/icon.md (OM-016 entry)
- [ ] Commit, PR (isochronous/oni-mods-common) ← IN PROGRESS

## Review Checkpoint
- Covers `git diff origin/main...1fbb20a -- tools` (9 files under tools/SaveRunner): unit 02 over 54dcd5a, unit 04 over 54dcd5a..1fbb20a. No code change after 1fbb20a.

## Open Questions / Blockers
- Follow-up: SaveRunner cannot start OniMoono until OM-014's direct launch lands; then a `-Runtime`/`-Exe` switch on Invoke-SaveRun.ps1 (unit 01 report, "OniMoono") and one full-mod memory run on Moono.
- Minor N1 (unit 04): `MemoryProbe.Row` drops the writer reference on an IO error without disposing it; harmless (process exits), tidy when next touched.
- Idea offered to Jeremy, not taken: a mod that makes mod kanim textures non-readable after load could save ~2.7 GB, at the risk of breaking mods that read their own textures.

## Phase Handoff Log
### Handoff: completion
- Review: unit 02 findings F1–F4 fixed in 1fbb20a and confirmed by unit 04; F5 by the store write; N1 left (minor).
- Verification: build 0 warnings 0 errors (worktree, deployed DLL hash matches); ModCheck ALL CHECKS PASSED; in-game runs mem-full / mem-none / mem-verify all status done with mods.json restored; `ks check` 0 block.
- Retro draft: Avoid — check which checkout's build the game will load (deploy goes to mods\local\SaveRunner from the last build); Repeat — two runs of one save (full vs -Only) with a first-loaded probe answered the question without bisection; Updated — SaveRunner entities/code-style/glossary.

## Constraints
- In-game runs take Jeremy's desktop: ask before long or chained runs; never -KeepGame; never stop a runner mid-run.
- SaveRunner blocks saving; never save over the Aftermath save.
- Disable Mod Profile Manager (2854869130) and its Light addon (3281716506) for runs.
