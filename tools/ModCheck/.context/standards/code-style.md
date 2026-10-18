---
doc_id: doc-11cf3f1ecd90
description: "C# code style for the ModCheck console tool: formatting, toolchain, structure and comments, with references to Program.cs"
applies_to:
  - "Program.cs"
  - "ModCheck.csproj"
---
# Code Style Guide

## Formatting
**Id:** blk-59184ca785a3

ModCheck is one file, `Program.cs`, and one project file, `ModCheck.csproj`.

- Tabs, one per level, in `Program.cs`, the same as the mod sources. The csproj uses two spaces.
- Opening brace on its own line (Allman) for the class, methods and multi-line blocks. A body of one short statement goes on the next line without braces (`if (args.Length < 2)` and the `continue` guards in `Main`).
- No line length limit. Long conditions and calls wrap after an operator or argument with the continuation indented one tab (`const BindingFlags all = ...` in `Program.cs`).
- The repository has no `.editorconfig`. TODO(owner): decide whether to add one recording tabs and brace placement for all of `common/`.

## Language and Toolchain
**Id:** blk-3d0e75841249

- C# 9 (`LangVersion` 9.0) on .NET 8 (`TargetFramework` net8.0), `OutputType` Exe. The project does not import `common/oni-mod.props`: it is a console program, not a mod, and has no game references at build time.
- ModCheck must run on .NET 8 or later, never .NET Framework. The game's interfaces use default interface methods, which the desktop CLR rejects and CoreCLR accepts, as does the game's Mono runtime. The header comment of `Program.cs` says so; keep it when the file changes.
- No NuGet packages. The code uses only `System`, `System.IO`, `System.Reflection` and `System.Runtime.CompilerServices`.

## Imports
**Id:** blk-d831aad7e6ca

`using` directives sit at the top, alphabetical, with no blank lines between them (`System`, `System.IO`, `System.Reflection`, `System.Runtime.CompilerServices`). No aliases and no global usings.

## Structure
**Id:** blk-224789a75291

- A single `internal static class Program` holds `Main` and its private static helpers (`Report`, `AllMethods`, `Unwrap`). Helpers are `private static` and sit below `Main`.
- There are no instances and no shared state: the failure and warning counters are locals of `Main` passed to `Report` by `ref`.
- A new check goes in as another loop body in `Main` or another helper beside the existing ones, and reports through `Report` so the failure and warning counts stay accurate.

## Comments
**Id:** blk-9bbfb0a9ecda

- A block comment at the top of `Program.cs` says what the tool verifies, why it needs .NET 8, and the usage line. Update it with any behaviour change.
- Comments inside the code give the reason for a rule, which is the runtime difference between the check host (CoreCLR) and the game's Mono: why `BeginInvoke` and `EndInvoke` are skipped on delegates, why the `ECall` message is a warning. No comments that restate the code.
- Console strings are plain text; the summary line uses an em dash (`runtime-mismatch artifacts, fine in-game`).

## Related
**Id:** blk-a530d607ccfd

- See also: [error handling](error-handling.md)
- See also: [naming conventions](naming-conventions.md)
