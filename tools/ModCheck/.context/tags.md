---
description: "The ModCheck node tag dictionary: each term that records here may carry, with the one sentence saying what a record must be about to earn it"
---
# Tags

## code-style
**Id:** blk-1b6f2b304ad9

A record earns this term when it is about the house style in which the tool's C# source code is written.

## tool-build
**Id:** blk-f265bb80ed87

A record earns this term when it is about building the tool with dotnet from its project file and the output that build produces.

## command-line
**Id:** blk-517c0d1313a3

A record earns this term when it is about the arguments and options the tool is started with and how it reads them.

## exit-codes
**Id:** blk-8e6d2d1e8f66

A record earns this term when it is about the exit code the tool's process ends with and what each value means.

## console-output
**Id:** blk-6871f38eb495

A record earns this term when it is about the lines the tool prints to the console and how they are worded.

## exception-handling
**Id:** blk-94cdc34bdbb3

A record earns this term when it is about where the tool catches exceptions and what it does once it has caught one.

## assembly-resolution
**Id:** blk-5ff0dcd76445

A record earns this term when it is about how the tool finds the game assemblies that a loaded mod DLL references.

## game-update-compatibility
**Id:** blk-f2441d0f8491

A record earns this term when it is about keeping mods and their tests working after a game update changes the game's own code.

## type-load-check
**Id:** blk-d67003b8cc5f

A record earns this term when it is about the check that sets up each type's interface maps and static constructor to catch vtable setup failures.

## jit-check
**Id:** blk-8a2329da0ff8

A record earns this term when it is about the check that compiles every method body with PrepareMethod to catch members the game no longer has.

## runtime-mismatch
**Id:** blk-bab0d39a58b7

A record earns this term when it is about the differences between the check's .NET 8 host and the game's Mono runtime that the check has to allow for.
