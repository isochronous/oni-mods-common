# Unit 04 review report: review fixes (54dcd5a..1fbb20a)

Verdict: F1-F4 fixed; one nit, no blocking findings.

## Passes run
Read the full `git diff 54dcd5a..1fbb20a` for MemoryProbe.cs and README.md, plus the touched call sites (Close, Row, Read/Value, Start) in the current file. Checked each finding's fix, the exception paths of the new helpers, and code-style (Allman, one-line bodies). No game launch, no code change.

## F1-F4
- F1 fixed. `Start` (MemoryProbe.cs:71-81) wraps `StartCore`, so `Row("", "boot", ...)` and `AccessTools.Method` are covered. `ModLoadPrefix`, `PostLoadPrefix`, `PassPrefix` and `PassPostfix` (about 185-245) each have try/catch that calls `Fail`, with `__state = null` in the two out-state prefixes. `Row` (about 446-460) catches IO errors, warns once and sets `series = null`, which stops later writes.
- F2 fixed. `Close()` (403-413) calls the new `Dispose` helpers (415-441). Flush and Dispose of each writer sit in separate try blocks, and `modLoad` is closed even if `series` fails. `Fail` only calls `Debug.LogWarning`, so `Close()` cannot throw and the status write and quit in `Finish` are reached.
- F3 fixed. Each of the three ProfilerRecorders is disposed on its own through `Dispose(ref ProfilerRecorder)`. `Value()` checks `Valid`, so a read after `Close` is safe.
- F4 fixed. README.md line 39 now lists `LayerableFiles` after `Translation`.

## New findings
- N1 nit: when `Row` hits an IO error it sets `series = null` without disposing the writer. The handle stays open until process exit, because `Close()` only sees null. This is harmless at that point (the run is ending or the disk is full). Optional fix: call the writer-dispose helper before nulling.
- Behaviour note, no action: `Fail` warns once per process, so a first warning from one hook hides later ones from other hooks. This matches the "warn once" suggestion in 02.
- The `catch (Exception e) { Fail(...); }` one-liners match the existing `Value()` style. The "probe skips from here" text is accurate only for the series. The mod-load rows keep writing until their own failure, which `Fail` then silences. This is cosmetic.

## ICON feedback
Packet pointers (entities, code-style, error-handling) were relevant. Nothing governing was missing. `ks read` was not needed; the pointers' content was reached through the task's own files and the code-style grep. No friction.
