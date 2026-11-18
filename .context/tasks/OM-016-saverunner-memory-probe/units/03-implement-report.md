# Unit 03 report: review fixes F1-F4

Commit: see git log (message "Memory probe fails safe: guarded hooks, Start, Row and Close"). Files: tools/SaveRunner/MemoryProbe.cs, README.md. Mod.cs and MemorySnapshot.cs unchanged (Start is now guarded inside MemoryProbe; the snapshot call in End(int) was already in try/catch).

- F1: Start() now wraps StartCore() in try/catch (MemoryProbe.cs Start/StartCore); ModLoadPrefix, PostLoadPrefix, PassPrefix, PassPostfix each wrapped, prefixes set __state = null on failure; Row() catches IO errors, warns once and sets series = null so writing stops. New Fail(where, e) logs one `[SaveRunner] memory probe, ...` warning, later failures silent.
- F2: Close() no longer throws: each writer flushed and disposed through Dispose(ref ...) with its own try/catch, so Runner.Finish always reaches the status write and App.Quit.
- F3: the three ProfilerRecorders are each disposed in their own try (Dispose(ref ProfilerRecorder)).
- F4: README modload `pass` list now includes `LayerableFiles`.

Build: `dotnet build tools/SaveRunner -c Release --no-incremental` -> Build succeeded, 0 Warning(s), 0 Error(s). (First attempt failed CS0103 from a bad edit of Row; fixed.)
ModCheck: `dotnet run --project tools/ModCheck -- F:/SteamLibrary/.../Managed tools/SaveRunner/bin/Release/SaveRunner.dll` -> "11 warning(s) - runtime-mismatch artifacts, fine in-game. ALL CHECKS PASSED".

Worktree has uncommitted .context/ edits (domains/entities.md, glossary.md, standards/code-style.md) that were not mine; left alone.
ICON tooling friction: none; ks read worked.
