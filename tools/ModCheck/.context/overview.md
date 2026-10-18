---
tags: game-update-compatibility
description: "What ModCheck is: the .NET 8 console tool's two checks, its exit codes, who runs it and the dotnet run command, then a map of Program.cs, ModCheck.csproj, build output and its store documents"
---

# ModCheck

## What this is
**Id:** blk-3ca89d57dfde  
**Tags:** type-load-check, jit-check, exit-codes, command-line

ModCheck is a .NET 8 console tool that loads one mod DLL against the real game assemblies and checks, without launching the game, for the two ways a mod breaks after a game update: a type whose vtable cannot be set up, and a call to a member the game no longer has. It lists every failing type and method and returns exit code 0 (passed), 1 (failures) or 2 (usage). `tools/TestRunner/Invoke-OniTests.ps1` runs it on each test DLL before a game session, and the mod author runs it by hand after a game update: `dotnet run --project common/tools/ModCheck -- <game Managed dir> <mod dll>`.

## Where things are
**Id:** blk-1d0e91d4e985  

Files:

- `Program.cs`: the whole tool. `Main` (arguments, assembly resolution, the type and method loops, the exit code), `Report`, `AllMethods` and `Unwrap`.
- `ModCheck.csproj`: console exe on net8.0 with C# 9 and no package references; it does not import `oni-mod.props`.
- `.gitattributes`: line-ending and merge rules for this node's `.context/` files.
- `bin/`, `obj/`: build output, ignored by `common/.gitignore`.

The `.context/` documents of this node: [domains](domains/README.md) (entities, glossary), [standards](standards/README.md) (code style, error handling, naming conventions), `decisions/` and `retrospectives/`.
