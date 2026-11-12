# Unit 02 review report: SaveRunner memory probe (54dcd5a)

Verdict: no critical findings; fix the two moderate fail-safe gaps (F1, F2) before the PR, minors at discretion.

## Passes run
1. Full diff read (`git diff origin/main...HEAD -- tools`) plus whole MemoryProbe.cs, MemorySnapshot.cs, Invoke-SaveRun.ps1, Mod.cs, Runner.cs, RunBehaviour.cs, Patches.cs.
2. Focus 1, mods.json: the reorder (ps1:121-124) runs inside Set-Mods, on the run's rewritten file only; the backup is taken before (ps1:131-136) and copied back in `finally` (ps1:162-166), leftover-backup recovery unchanged. Sound. `$first -notcontains $_` compares by reference, which is correct for objects from the same parsed list.
3. Focus 2, other runs: every new hook is gated on `MemoryProbe.On` (false unless `Start` ran, which only `Runner.Probe == "memory"` does). Harmony patches are applied by hand only in Start. The fridges branch in RunBehaviour is unchanged (`else if`). `Runner.Finish` calls `Close()`, which returns at once when `On` is false. No behaviour change found.
4. Focus 3: SaveLoader.Save prefix untouched; the probe adds no save path.
5. Focus 4 and 5: exception and resource-path trace (F1-F3 below).
6. Focus 6: README column lists compared with `Header` and `ModLoadHeader` (match); README flags compared with ps1 defaults (match).
7. Evidence: both runs ended `done` (301 samples); probe.csv 321 rows x 22 columns, probe-modload 354 x 19, probe-snapshot 2510 x 7, no ragged rows; snapshot took 0.8 s / 0.6 s; Player.log has no probe warnings; mods.json restored (from the unit-01 evidence).

## Findings

### F1 moderate: hook prefixes and `Start` can throw into the game's mod loading
MemoryProbe.cs:156-196 and 71-101. `ModLoadPrefix`/`PostLoadPrefix` call `Begin()` (Resources.FindObjectsOfTypeAll, Assets.ModLoadedKAnims, Read()); `PassPrefix`/`PassPostfix` call `Row()` (series.WriteLine/Flush: IOException on a full disk or locked file). None is wrapped, while `End()` is. A Harmony prefix exception propagates into `Mod.Load`/`Manager.Load`, so the original mod would not load and the run would change what it measures or break the boot, which Focus 4 forbids. The same holds for `Start()` in `SaveRunnerMod.OnLoad` (Mod.cs:41-42): `Row("", "boot", Read())` and `AccessTools.Method(...)` (called outside Hook's try, ambiguous match throws) are unguarded, and an exception marks SaveRunner crashed.
Fix: wrap the bodies of the five hook methods and `Start` in try/catch that logs a warning (as `End` does); set `__state = null` in the catch. Also make `Row` itself catch IO errors (warn once, then stop writing).

### F2 moderate: `Close()` can throw before the status file is written
MemoryProbe.cs:355-373, called at Runner.cs:64 before the status write and `App.Quit()`. `series?.Flush()`/`Dispose()` and `modLoad?.Flush()` are outside any try. An IOException (the same disk-full case) leaves `Finished = true` with no status and no quit, so the run ends only at the launcher kill (TimeoutSec+180 s) with "no status file"; this breaks the "a run always ends with a status" rule in standards/error-handling.md. Also, if `series.Flush()` throws, `modLoad` is never closed.
Fix: put the whole body of `Close()` in try/catch (or one try per writer) logging a warning, or move the `Close()` call in `Finish` to after the status write and inside a try.

### F3 minor: ProfilerRecorder dispose chain stops at the first failure
MemoryProbe.cs:366-372. One try covers three `Dispose()` calls; if the first throws the other two leak. Dispose each in its own try (or helper).

### F4 minor: README omits the `LayerableFiles` pass
README.md (modload bullet): the `pass` values listed are DLL, PostLoad, Animation, Strings, Translation. The real run's probe-modload.csv also has `LayerableFiles` (6 rows) and probe.csv has `before-/after-LayerableFiles`. Fix: add it to the list.

### F5 minor: the .context docs still call FridgeProbe the only probe
tools/SaveRunner/.context/standards/code-style.md:54,57 (layout table, "how to add a probe": `Sample(int, StreamWriter, bool)`), domains/entities.md (:67-69), domains/README.md:14, overview.md:21, glossary.md:35, naming-conventions.md:31. They are outside the diff scope here, but MemoryProbe is a second probe with its own writers and a different shape (own files, `Begin/Sample/End/Close`, boot-time start) and adds MemoryProbe.cs/MemorySnapshot.cs. Fix: update through the write path in the completion phase (add the two files to the layout table, state the deliberate deviation from the probe form, add the `memory` ValidateSet value and the new run files to entities).

## No finding
- ps1 default handling uses `$PSBoundParameters.ContainsKey`, so explicit `-Seconds/-Speed/-TimeoutSec` still win.
- Row/Sample/End after `Close()`: `On` is false and `series` null, so they are no-ops.
- Snapshot failure is caught in `End(int)` and the run still finishes `done`; texture/mesh size helpers catch internally.
- External kill mid-run: each row and modload line is flushed, so data survives.

## ICON tooling friction
- `ks read` on `tools/SaveRunner:standards/...` ids worked. The brief flagged `tools/SaveRunner:tags.md` as having no coverage or tags; I did not open it.
- Pointer check: all five SaveRunner pointers applied; code-style.md and error-handling.md governed. entities.md/glossary.md were not opened beyond grep (see F5). Nothing governing was missing from the brief.
