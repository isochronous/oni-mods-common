---
tags: code-style
doc_id: doc-dc570c7ff4d7
description: "WorkshopUpload's code style for Program.cs and WorkshopUpload.csproj: indentation, line length, Allman braces, .NET 8 and Steamworks.NET project settings, steam_appid.txt, the build command, imports, comment rules and control flow"
applies_to:
  - "*.cs"
  - "*.csproj"
---

# WorkshopUpload code style

WorkshopUpload is one C# file, `tools/WorkshopUpload/Program.cs`, plus its project file, `steam_appid.txt` and Valve's `steam_api64.dll`. These are the conventions that file follows.

## Formatting
**Id:** blk-8a4e964038bc

### Indentation
**Id:** blk-4da09c2f1ccd

- Tabs, one per level, in `Program.cs`.
- `WorkshopUpload.csproj` uses two spaces.
- No `.editorconfig` or formatter is configured. TODO(owner): record the tab and brace rules in an `.editorconfig` if editors should enforce them.

### Line Length
**Id:** blk-9f9a27861784

- No limit. Usage strings and `Console.WriteLine` calls stay on one line; a long call breaks after an argument and the continuation indents one tab (the `PublishWorkshopFile` call in `Publish`).

### Braces
**Id:** blk-29678b38623b

- Opening brace on its own line (Allman) for types, methods and multi-line blocks.
- A body of one short statement goes on the next line without braces (`if (cloudZip != null)` lines in `Update`).

## Language and Toolchain
**Id:** blk-c0c064d7a567  
**Tags:** tool-build, steamworks-api, steam-app-id

- .NET 8 (`net8.0`), `RuntimeIdentifier` win-x64, `SelfContained` false, `Nullable` disabled, `ImplicitUsings` enabled. `LangVersion` is not set, so the .NET 8 default (C# 12) applies; the file uses file-scoped `namespace WorkshopUpload;`, switch expressions and `using var`. It runs on .NET 8, not the game's .NET Framework 4.8, so it does not import `common/oni-mod.props`.
- Steamworks.NET 2024.8.0 from NuGet is the only package. The native `steam_api64.dll` next to it (Steamworks SDK 1.60 or newer, copied to the output) is Valve's; the game's own copy is too old for this wrapper.
- `steam_appid.txt` (content `636750`) is copied to the output and must stay next to the exe.
- Build: `dotnet build tools/WorkshopUpload -c Release`.

## Imports/Includes
**Id:** blk-13266dc4219e

- The only explicit directive is `using Steamworks;` at the top, followed by the file-scoped namespace. `System`, `System.IO`, `System.Collections.Generic` and `System.Threading` arrive through implicit usings; `System.IO.Compression.ZipFile` is written out in full where used.

## Comments
**Id:** blk-3bb9ad6f0f0a

### When to Comment
**Id:** blk-f24e90f24a9f

- The `///` summary on `Program` is the tool's manual: every command with its arguments, the legacy-API rule, and what the uploaded zip must hold. Keep it in step with the `switch` in `Main`.
- `///` summaries on constants and helpers say why: `UploaderAppId` explains the app id choice, `UploadToCloud` explains the cloud file names.
- Record field findings with a date where they drive a rule (the 2026-10-08 note on `publish`).

### When NOT to Comment
**Id:** blk-73fd567889db

- Do not comment obvious code or leave commented-out code.
- A `// ---- commands ----` rule separates the command methods from the helpers.

## Control Structures
**Id:** blk-9cef3a262ece  
**Tags:** exit-codes, command-line, console-output

- `Main` prints usage and returns 2 when there are no arguments, before touching Steam; it then initialises Steam and dispatches with a `switch` on `args[0]` inside `try`/`finally`. An unknown command and the argument checks of `update` and `publish` return 2 inside the `try`.
- Each command is a `private static int` method returning its exit code; early `return 1` or `return 2` on the first failed step.
- `switch` expressions map option strings to enums (`ParseVisibility`).
- Output uses string interpolation (`$"..."`).

## Related
**Id:** blk-2ba6b12a38af

- See also: [error handling](error-handling.md), [naming conventions](naming-conventions.md)
