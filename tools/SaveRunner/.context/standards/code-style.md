---
tags: code-style
doc_id: doc-b16e31474a05
description: "SaveRunner's C# and PowerShell style: indentation and braces, C# 9 on net48 through SaveRunner.csproj, the one-concern-per-file layout and how to add a probe, the Runner.Active guard, Harmony patch form and comments"
applies_to:
  - "*.cs"
  - "*.ps1"
  - "*.csproj"
---

# Code Style Guide

## Formatting
**Id:** blk-9a324aa6540e

### Indentation
**Id:** blk-2ac58ee6eb93

- Tabs, one per level, in every `.cs` file (`Mod.cs`, `Patches.cs`, `Runner.cs`, `RunBehaviour.cs`).
- Four spaces in `Invoke-SaveRun.ps1`; two spaces in `SaveRunner.csproj` and the yaml files.

### Braces
**Id:** blk-8de1cb25887c

- Opening brace on its own line (Allman) for namespaces, types, members and multi-line blocks.
- A body of one short statement goes on the next line without braces (`if (!Runner.Active)` followed by `return;` in `Mod.cs`).
- A `switch` case with one short statement stays on one line (`ReadCommandLine` in `Runner.cs`).

### Line Length
**Id:** blk-459605aea8ce

- No limit and no `.editorconfig`. Long log lines wrap before a `+` with the continuation indented one tab (the start-up `Debug.Log` in `Mod.cs`).

## Language and Toolchain
**Id:** blk-545208d8763a  
**Tags:** mod-build, mod-entry-point

- C# 9 on .NET Framework 4.8 (`common/oni-mod.props`: `LangVersion` 9.0, `TargetFramework` net48), built against the game's own assemblies through `common/oni-mod.targets`.
- `SaveRunner.csproj` imports the props, sets `AssemblyName`, `RootNamespace` and `Version`, then imports the targets. A successful build deploys the mod to `Documents\Klei\OxygenNotIncluded\mods\local\SaveRunner`.
- The tool is a mod (`mod.yaml`, `mod_info.yaml`, `SaveRunnerMod : UserMod2`) plus one PowerShell launcher. PowerShell is run with `pwsh`.
- Namespace blocks, not file-scoped namespaces.

## Layout
**Id:** blk-233f0aed6e04  
**Tags:** probe

One concern per file, all in the node root:

| File | Holds |
|------|-------|
| `Mod.cs` | `SaveRunnerMod`: reads the command line at load, sets the crash-reporter flags, traces who quits |
| `Runner.cs` | `Runner`: static command-line state, `ReadCommandLine`, `Finish` |
| `Patches.cs` | the Harmony patch classes |
| `RunBehaviour.cs` | `RunBehaviour` (the per-frame driver) and `FridgeProbe` (the only probe) |
| `Invoke-SaveRun.ps1` | the launcher |

A new probe is a static class beside `FridgeProbe` with a `Sample(int t, StreamWriter writer, bool toLog)` method, a branch in `RunBehaviour.Begin` for its CSV header and one in `RunBehaviour.Sample`, and an entry in the launcher's `-Probe` `ValidateSet`.

## Inert Without the Argument
**Id:** blk-969f0779ab20  
**Tags:** activation-guard

Every patch and hook starts with `if (!Runner.Active) return` (or the equivalent). The mod must stay installed and change nothing when the game starts without `--saverun`. `SaveRunnerMod.OnLoad` returns before patching the quit traces when `Runner.Active` is false.

## Harmony Patches
**Id:** blk-fa011f2987f3  
**Tags:** harmony-patching, exception-handling

- One static class per patched method, with `[HarmonyPatch(typeof(X), "Method")]` or `nameof` when the member is public, and a `Prefix` or `Postfix` inside (`Patches.cs`).
- The overload is pinned by parameter types when the method has several (`SaveLoader.Save(string, bool, bool)`).
- Private members are named by string; a patch that could be missing on another game build is applied by hand inside try/catch (`SaveRunnerMod.TracePatch`).

## Comments
**Id:** blk-9fd4a51454ee  
**Tags:** command-line

- Types and patch classes get a `///` summary that says what the code is for and, where a bug shaped it, what went wrong: the four-second menu settle in `Patches.cs`, the crash dialog in `Mod.cs`.
- Command-line options are documented in the `///` summary of `Runner`, in the launcher's comment-based help and in `README.md`; change all three together.
- No commented-out code.

## Related
**Id:** blk-41f957f9abf7

- See also: [error handling](error-handling.md)
- See also: [naming conventions](naming-conventions.md)
