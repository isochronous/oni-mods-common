# Unit 01 — implement: SaveRunner `memory` probe

## Objective
Give SaveRunner a probe that shows where a modded ONI's memory goes, for plan step "Implement memory probe + per-mod load deltas". Jeremy's game reached 20.6 GB committed (OniMoono.exe, 156 mods, The Aftermath v009, cycle 904) and he wants to know whether one mod is responsible or the list as a whole. The 156 mods are only 624 MB on disk, so the answer is in runtime memory. Two runs will be compared — full mod list vs no mods — so every number must be comparable between them.

## Scope
- IN: tools/SaveRunner/** in the worktree C:\dev\ONI-Mods-wt\common-OM-016 (branch OM-016-saverunner-memory-probe). Its README gets the new probe and outputs.
- OUT: tools/TestRunner, tools/ModTest, GameProcess.ps1 (OM-014 owns uncommitted changes to those in the other checkout C:\dev\ONI-Mods\common — never touch that checkout). No OniMoono launch path in this unit. Do not launch the game: the in-game runs are the manager's, after this report.
- What the run must answer (you choose how to measure; these are the questions):
  1. Over game time: process private bytes / working set, managed heap (Mono used and reserved, GC.GetTotalMemory), Unity native allocated/reserved, graphics/texture memory. Sampled from main menu, after the save loads, and on through the run, so growth (a leak) is visible apart from the load-time jump. The crash session grew ~100 MB in 11 minutes on top of ~20 GB.
  2. At mod load: how much each mod adds as it loads (private bytes and managed heap before/after each mod), so a single heavy mod stands out. The probe mod has to be measuring before the others load — solve the load-order problem (e.g. Invoke-SaveRun.ps1 already rewrites mods.json) and say how.
  3. A snapshot after the save is loaded: what the memory is made of, attributable to mods where possible — e.g. largest textures and their names, loaded asset bundles, kanim counts, instance counts of component types per assembly (a mod component on every cell or building shows up here). Whatever is cheap in a release Unity player and points at a mod.
- Must not change the save or the existing probes' behaviour; saving stays blocked.
- Acceptance: the dev mod builds; Invoke-SaveRun.ps1 parses with 0 errors and accepts the new probe; the README documents it. Your report must include the actual build and parse command output — do not claim success without showing evidence. Untested-in-game parts are listed as such.

## Context the packet does not carry
- Save: C:\Users\thegr\Documents\Klei\OxygenNotIncluded\save_files\The Aftermath\The Aftermath v009.sav (11.9 MB, verified intact). Runs pass `-Without 2854869130,3281716506` (Mod Profile Manager + Light addon restart the game when a save's mod list differs).
- A full run of a ~150-mod list took ~200 s real time for 90 game seconds (Oct 2026); boot alone takes minutes. Pick a default run length that shows growth without holding Jeremy's desktop for long, and say what you chose.
- Jeremy's mods.json: C:\Users\thegr\Documents\Klei\OxygenNotIncluded\mods\mods.json — the launcher's backup/restore of it must stay exactly as it is.
- Steam now starts the stock game; runs 1–2 are on stock.

## Report
- File: .context/tasks/OM-016-saverunner-memory-probe/units/01-implement-report.md
- Must contain: what was built and where (file list), the exact command lines for run 1 (full mods) and run 2 (no mods), the output files and what each column means, how load order was solved, build + parse output, known limits (what a release player cannot measure), anything left for an in-game run to prove.
- Model tier: complex
