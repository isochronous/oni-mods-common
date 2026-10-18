---
doc_id: doc-d57d9d21981a
description: "StringsTemplate's code style for Program.cs and StringsTemplate.csproj: indentation, line length, Allman braces, C# 9 on .NET 8 project settings, the build command, the using block, when to comment and control-flow patterns"
applies_to:
  - "*.cs"
  - "*.csproj"
---

# StringsTemplate code style

StringsTemplate is one C# file, `tools/Translations/StringsTemplate/Program.cs`, plus its project file. These are the conventions that file follows.

## Formatting
**Id:** blk-e0236b53a4c4

### Indentation
**Id:** blk-81781ba21fec

- Tabs, one per level, in `Program.cs`.
- `StringsTemplate.csproj` uses two spaces.
- No `.editorconfig` or formatter is configured. TODO(owner): record the tab and brace rules in an `.editorconfig` if editors should enforce them.

### Line Length
**Id:** blk-6ae96e09b047

- No limit. Long conditions and message strings stay on one line (the `Console.WriteLine` summary in `CheckPo` is the longest); a long concatenation breaks before a `+` and the continuation indents one tab.

### Braces
**Id:** blk-32bcfa6795e4

- Opening brace on its own line (Allman) for types, methods and multi-line blocks.
- A body of one short statement goes on the next line without braces:

```csharp
foreach (string po in Directory.GetFiles(translationsDir, "*.po").OrderBy(p => p))
	problems += CheckPo(po, entries);
```

## Language and Toolchain
**Id:** blk-0c006bb6c374

- C# 9 (`LangVersion` 9.0) on .NET 8 (`net8.0`), `OutputType` Exe, `Nullable` disabled. It runs on .NET 8, not on the game's .NET Framework 4.8, so it does not import `common/oni-mod.props`.
- No `ImplicitUsings`: every namespace is imported by hand.
- No NuGet packages. It loads the mod DLL and the game's assemblies by reflection at run time (`Assembly.LoadFrom`), so it carries no compile-time reference to either.
- One file, one `internal static class Program`, no namespace. A nested `private sealed class Entry` holds a key and its English text.
- Build: `dotnet build tools/Translations/StringsTemplate -c Release`, giving `bin/Release/net8.0/StringsTemplate.dll`, the path `tools/Translations/translations.py` runs.

## Imports/Includes
**Id:** blk-c3be4d343981

`using` directives sit at the top of the file in one block with no blank lines, alphabetical with `System` first:

```csharp
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
```

## Comments
**Id:** blk-07518595d112

### When to Comment
**Id:** blk-95ee88720bc2

- A `//` block at the top of `Program.cs` states what the tool does, its command line and its exit code. Keep it in step with `Main`.
- A `///` summary goes on a helper whose rule comes from the game and is not obvious from the code: `Escape` ("the escaping the game's reader undoes"), `ReadPo` (parsed the way `Localization.GetParameter` does it) and the `Parameters` regex ("the game's rule: the set of distinct {...} tokens must be the same").
- Say why a check exists, citing the game behaviour it mirrors, not what the line does.

### When NOT to Comment
**Id:** blk-1ed6f4eb6519

- Do not comment obvious code or leave commented-out code.
- TODO(owner): `ReadPo` carries two stacked `<summary>` blocks, the first a one-line leftover of the second; drop the first.

## Control Structures
**Id:** blk-5ff9e7024b0c

- `Main` returns early with an exit code for usage and fatal setup errors, then runs the checks in order and returns 0 or 1.
- Check methods return an `int` count of problems and print each one as they find it; `Main` adds the counts up (see `error-handling.md`).
- LINQ is used for sorting, filtering and set building (`entries.Sort`, `OrderBy`, `Select`, `HashSet`); explicit `foreach` loops where a loop prints or counts.
- Output is built with string concatenation, not interpolation or `string.Format`.

## Related
**Id:** blk-b753901f3854

- See also: [error handling](error-handling.md), [naming conventions](naming-conventions.md)
