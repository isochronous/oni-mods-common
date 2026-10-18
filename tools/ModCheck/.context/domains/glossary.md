---
doc_id: doc-c4dec2d4a3f7
description: "Terms a newcomer to ModCheck needs: VTable setup failure, missing member, ECall, managed directory, JIT check"
---
# ModCheck glossary

Terms a newcomer to ModCheck needs. Entity detail is in [entities](entities.md).

## Abbreviations and Acronyms
**Id:** blk-3aa35e536ac3

| Abbreviation | Full Term | Context |
|--------------|-----------|---------|
| ONI | Oxygen Not Included | the game whose mods ModCheck checks |
| JIT | just-in-time compiler | `PrepareMethod` forces it to compile a method before the method runs |
| CLR | Common Language Runtime | the desktop (.NET Framework) runtime that ModCheck cannot use; CoreCLR is the .NET 8 one it does use |
| ECall | external call | a method implemented in the Unity engine's native code; the host cannot run it |
| DIM | default interface method | an interface method with a body; the game's interfaces have them |

## Cross-Domain Terms
**Id:** blk-497458e05560

### VTable setup failure
**Id:** blk-7d3bfdfef868

**Definition**: the runtime error `VTable setup of type X failed` raised when a type's virtual or interface methods cannot be laid out, which a game update causes by changing an interface or base class that the mod's type implements or derives from. `GetInterfaceMap` is how the check provokes it.  
**Used in**: ModCheck output (`FAIL type ...`).

### Missing member
**Id:** blk-cdaff890db76  
**Definition**: a `MissingMethodException` or `MissingFieldException`, raised when compiled code names a method or field that the game no longer has. Compiling every method body with `PrepareMethod` brings these out without calling the code.  
**Used in**: ModCheck output (`FAIL jit ...`).

### Managed directory
**Id:** blk-a6b4a8ed18b6  
**Definition**: the game's `OxygenNotIncluded_Data\Managed` folder, the first argument.  
**Used in**: ModCheck's `AssemblyResolve` handler; `Invoke-OniTests.ps1` finds it from the game folder.

### Warning
**Id:** blk-841800be19a4  
**Definition**: a `WARN` line for an exception caused by the check host and not by the mod (the `ECall` case); it does not fail the run.  
**Used in**: ModCheck output.

## Related
**Id:** blk-af856d8d8196

- See also: [entities](entities.md)
