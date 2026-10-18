---
doc_id: doc-cba1a5728ae1
description: "ModCheck's entities: the Program class in Program.cs and its members, the type-load and JIT checks, inputs, output and exit codes, warning and skip rules, terminology, key code paths and the script that calls it"
applies_to:
  - "Program.cs"
  - "ModCheck.csproj"
---

# ModCheck domain

## Overview
**Id:** blk-303b29f17bb1

ModCheck loads one mod DLL against the real game assemblies on a .NET 8 host and checks, without launching the game, the two ways a mod dies after a game update: a type that cannot be set up, and a call to a member that no longer exists. It is a console program, `common/tools/ModCheck`, built and run with `dotnet run`. `common/tools/TestRunner/Invoke-OniTests.ps1` runs it on every test DLL before a game session; a mod author runs it by hand after a game update.

## Key Entities
**Id:** blk-dda267d443bd

### Program
**Id:** blk-051b98125fc9  
**Description**: the whole tool: argument handling, assembly resolution, the type and method loops and the exit code.  
**Location**: `Program.cs`

| Member | Description |
|--------|-------------|
| `Main(string[] args)` | entry point; returns the exit code |
| `Report(stage, typeName, memberName, ex, ref failures, ref warnings)` | classifies one exception as `FAIL` or `WARN` and prints it |
| `AllMethods(type, flags)` | the type's methods and constructors in one array |
| `Unwrap(ex)` | `ExceptionType: message`, seen through `TypeInitializationException` |

**Relationships**: reads the game's Managed folder (first argument) and the mod DLL (second); writes only to standard output and the exit code.

### The two checks
**Id:** blk-6fe7024707c2

1. **Type load**: for each type in the assembly, `GetInterfaceMap` for every interface of a non-interface type, then `RuntimeHelpers.RunClassConstructor`. Catches `VTable setup of type X failed` and failing static constructors.
2. **JIT**: for each non-abstract method and constructor with no open generic parameters, declared on the type (public and non-public, instance and static), `RuntimeHelpers.PrepareMethod`. Compiling the body resolves every member it references, so a missing method or field fails here instead of at call time in the game.

### Inputs
**Id:** blk-ff7f3244936c

| Input | Description |
|-------|-------------|
| game Managed directory | `OxygenNotIncluded_Data\Managed` of the installed game; `Assembly-CSharp`, `0Harmony`, Unity modules and the rest resolve from here |
| mod DLL | the assembly under test; its own dependencies are resolved from the same Managed folder, so a DLL with others (TestRunner, the mod under test) is checked from a folder that holds them (see `Invoke-OniTests.ps1`) |

### Output
**Id:** blk-c3c2239a19af

Lines on standard output (see [error handling](../standards/error-handling.md)) and an exit code of 0, 1 or 2.

## Business Rules
**Id:** blk-1031f9e9f9e0

- Every type is checked even after a failure; every method of a type that loaded is checked.
- A message containing `ECall methods must be packaged into a system module` is a warning: the host cannot run Unity native code that the game runs. Everything else is a failure.
- `BeginInvoke` and `EndInvoke` of delegate types are not compiled; abstract methods and open generic methods are not compiled.
- ModCheck runs on .NET 8 or later. The desktop CLR rejects the game's default interface methods.
- A pass says nothing about behaviour at run time; it covers type setup and member resolution only.

## Domain Terminology
**Id:** blk-732559c369a5

| Term | Definition |
|------|-----------|
| Managed directory | the folder of the game's .NET assemblies |
| JIT check | forcing compilation of a method body with `PrepareMethod` |
| Type check | touching a type's interface maps and static constructor |

## Key Code Paths
**Id:** blk-9a1ec07e6286

- **Whole run**: `Main` builds the resolver, loads the assembly, loops over `asm.GetTypes()` and prints the summary and exit code. See `Program.cs`.
- **One type**: the type stage in a `try`, then `AllMethods` and `PrepareMethod` per method, then `OK <type>`. See `Program.cs`.
- **Classification**: `Report` decides `FAIL` or `WARN` from the unwrapped message. See `Program.cs`.
- **Caller in the repository**: the `dotnet run -c Release --project tools\ModCheck -- <managed> <dll>` line in `common/tools/TestRunner/Invoke-OniTests.ps1`, which exits 3 when ModCheck fails.

## Related
**Id:** blk-f3cc2c826e21

- See also: [glossary](glossary.md)
- Governed by: [naming conventions](../standards/naming-conventions.md)
