---
doc_id: doc-f1aa1ec42c48
description: "C# and PowerShell style for the TestRunner harness mod and test kit: formatting, toolchain, namespaces, file layout, patches, Reflect and comments"
applies_to:
  - "*.cs"
  - "Kit/**/*.cs"
  - "*.ps1"
  - "*.csproj"
---
# Code Style Guide

## Formatting
**Id:** blk-a5a05cca59dd

### Indentation
**Id:** blk-48bf49a3a8d9

- Tabs, one per level, in every `.cs` file, including `Kit/`.
- Four spaces in `Invoke-OniTests.ps1` and `Watch-OniGame.ps1`; two spaces in `TestRunner.csproj` and the yaml files.

### Braces
**Id:** blk-d5d494815e7f

- Opening brace on its own line (Allman) for namespaces, types, members and multi-line blocks.
- A body of one short statement goes on the next line without braces (`if (!Session.Active)` followed by `return;` in `Mod.cs`).
- A `switch` case with one short statement stays on one line (`Session.ReadCommandLine`).

### Line Length
**Id:** blk-c5dee5000810

- No limit and no `.editorconfig`. Long log lines wrap before a `+` with the continuation indented one tab (the start-up `Debug.Log` in `Mod.cs`).

## Language and Toolchain
**Id:** blk-827b53b95cbe

- C# 9 on .NET Framework 4.8, built against the game's assemblies (`common/oni-mod.props` and `oni-mod.targets`). `TestRunner.csproj` sets `ReferenceUnityUI` and references `UnityEngine.ScreenCaptureModule` for `t.Screenshot`.
- The tool is a mod (`mod.yaml`, `mod_info.yaml`, `TestRunnerMod : UserMod2`) and also a library: a mod's test project references `TestRunner.csproj` for the kit. A successful build deploys it to `Documents\Klei\OxygenNotIncluded\mods\local\TestRunner`.
- Two namespaces: `Isochronous.TestRunner` for the harness (the code that runs a session) and `Isochronous.TestKit` for what a test author uses (`Kit/`: the test context, assertions, attributes, dupes, tools, world helpers). Harness types a test may touch are public; the rest is `internal`.
- Namespace blocks, not file-scoped namespaces. PowerShell is run with `pwsh`.

## Layout
**Id:** blk-3eafd0a33a08

| Path | Holds |
|------|-------|
| `Mod.cs` | `TestRunnerMod`: command line at load, crash-reporter flags, assembly resolve, quit tracing |
| `Session.cs` | `Session` (command line, test discovery, results, live-session queue), `TestCase`, `TestResult`, `Json` |
| `TestBehaviour.cs` | the driver that steps each test, `LogCapture` |
| `Patches.cs` | the Harmony patches |
| `Harness.cs` | the global switches a run throws |
| `FixtureArea.cs` | choosing and clearing the test area |
| `Worldgen.cs` | tests in a generated world |
| `Reflect.cs` | every private game member the code touches |
| `Diagnostics.cs` | opt-in grid-mark tracing |
| `Kit/` | the test kit |
| `Invoke-OniTests.ps1`, `Watch-OniGame.ps1` | launcher and warden |
| `fixtures/` | fixture saves and their README |

## Inert Without the Argument
**Id:** blk-2fc671d7925c

Every patch starts with `if (!Session.Active) ...`. `Session.Active` is true only with both `--onitest-dir` and `--onitest-save`. The mod stays installed and changes nothing in a normal game session.

## Harmony Patches
**Id:** blk-bb5bf3709593

- One static class per patched method, named `<Type>_<Method>_Patch`, with `Prefix` or `Postfix` (`Patches.cs`); trace patches in `Diagnostics.cs` are named `_Trace`.
- Overloads are pinned by parameter types (`BuildingDef.TryPlace` in `Diagnostics.cs`).
- A patch that might not exist on another game build is applied by hand in try/catch (`TestRunnerMod.TracePatch`).

## Private Game Members
**Id:** blk-7ca12f96cdb9

Reach game internals only through `Reflect.cs`: a `public static readonly FieldInfo` per member from `AccessTools`, with a `///` summary saying what it is for, and a line in `Reflect.SelfCheck()`. `FieldInfo` is used and `FieldRefAccess` is not, because a `FieldRef` dynamic method cannot be built on the CoreCLR host that ModCheck runs on.

## Comments
**Id:** blk-2536019dd87f

- Types, patches and switches get a `///` summary giving the purpose and, where a failure shaped the code, what failed (the settle before a reload in `TestBehaviour`, the focus patches in `Patches.cs`).
- Options are documented in three places that change together: the `///` summary of `Session`, the launcher's comment-based help and `README.md`.
- Kit members that a test author calls carry a summary written for that author.
- No commented-out code.

## Related
**Id:** blk-657b3eb1a17f

- See also: [error handling](error-handling.md)
- See also: [naming conventions](naming-conventions.md)
