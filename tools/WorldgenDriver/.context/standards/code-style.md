---
doc_id: doc-01fc74e4c079
description: "WorldgenDriver (the unattended worldgen mod and its PowerShell launcher): C# 9 on net48 with tabs and Allman braces, PowerShell with four spaces, shared build files, patch guards and comments"
applies_to:
  - "*.cs"
  - "*.ps1"
  - "*.csproj"
  - "*.yaml"
---
# WorldgenDriver code style

WorldgenDriver is two programs in one folder: a development-only game mod in C# (`Mod.cs`, `Patches.cs`, `Driver.cs`) and the PowerShell launcher that drives it (`Invoke-Worldgen.ps1`). Each follows its own conventions below.

## Formatting
**Id:** blk-ceea1fc9e82a

### Indentation
**Id:** blk-6e1063014054

- C#: tabs, one per level.
- PowerShell: four spaces.
- `WorldgenDriver.csproj`: two spaces.
- No `.editorconfig` or formatter is configured. TODO(owner): record the tab and brace rules in an `.editorconfig` if editors should enforce them.

### Line Length
**Id:** blk-2abe1f2873a7

- No limit in either language. Long log messages and `Finish(...)` calls stay on one line; the settings code in `Patches.cs` is the pattern.

### Braces
**Id:** blk-d5016a3ddc0f

- C#: opening brace on its own line (Allman) for namespaces, types, methods and multi-line blocks; a body of one short statement goes on the next line without braces (`if (!Driver.Active) return;` guards).
- PowerShell: opening brace on the same line (`function Set-Mode([bool]$spacedOut) {`, `foreach (...) {`).

## Language and Toolchain
**Id:** blk-e794fe66eb73

- The mod is C# 9 on .NET Framework 4.8 with the shared build files: `WorldgenDriver.csproj` imports `..\..\oni-mod.props` and `..\..\oni-mod.targets` (that is `common/`), sets `AssemblyName`, `RootNamespace` WorldgenDriver and `Version` 0.1.0, and a successful build deploys to `Documents\Klei\OxygenNotIncluded\mods\local\WorldgenDriver`.
- Mod metadata is `mod.yaml` (title, description, `staticID: "Isochronous.WorldgenDriver"`) and `mod_info.yaml` (`supportedContent: ALL`, `minimumSupportedBuild: 744825`, `version: 0.1.0`, `APIVersion: 2`). Keep `version` in the csproj and `mod_info.yaml` equal.
- One namespace, `WorldgenDriver`, in block form (no file-scoped namespace); no nullable reference types.
- The launcher is PowerShell 7 style script with `[CmdletBinding()]`, a typed `param()` block and `$ErrorActionPreference = 'Stop'`.
- Build: `dotnet build tools/WorldgenDriver -c Release`.

## Imports/Includes
**Id:** blk-70aed427b5a2

C# `using` directives sit above the namespace in one block, `System.*` first, then the rest alphabetically (`HarmonyLib`, `Klei.CustomSettings`, `KMod`, `ProcGen`):

```csharp
using System;
using HarmonyLib;
using Klei.CustomSettings;
using ProcGen;
```

## Comments
**Id:** blk-7d17f388ee51

### When to Comment
**Id:** blk-f87fd4025ee7

- `///` summaries on each patch class say what game moment it catches and why that hook (the `App.LateUpdate` watchdog: "the one per-frame hook that exists in every scene").
- The `///` summary on `Driver` lists every `--worldgen-*` command-line option; keep it equal to `ReadCommandLine`.
- The launcher has comment-based help (`.SYNOPSIS`, `.DESCRIPTION`, `.EXAMPLE`) at the top and a trailing `#` comment on each `param` that needs one.
- Say why (a rebuilt local mod is left "reinstall pending" and blocks the main menu), not what the line does.

### When NOT to Comment
**Id:** blk-6243625bf041

- Do not comment obvious code or leave commented-out code.

## Control Structures
**Id:** blk-9b7318d348ff

- Every patch body starts with a guard that returns when the driver is idle:

```csharp
if (!Driver.Active)
	return;
```

- `Driver` is a static class holding all session state; patches call into it and hold none of their own.
- Mod startup is also guarded: `OnLoad` returns before `base.OnLoad(harmony)` when there is no `--worldgen` argument, so the patches are never applied and the mod can stay installed.
- The launcher uses `try`/`finally` around the session loop so backups are always restored.

## Related
**Id:** blk-2c1b228a8481

- See also: [error handling](error-handling.md), [naming conventions](naming-conventions.md)
