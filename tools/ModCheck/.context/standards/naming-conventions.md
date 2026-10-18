---
doc_id: doc-b5a7b9c0a575
description: "Naming in ModCheck: project and file names, C# member casing, the two positional arguments and the output prefixes"
applies_to:
  - "Program.cs"
  - "ModCheck.csproj"
---
# Naming Conventions

## Files and Directories
**Id:** blk-8ff7fead9871

- The tool lives in `common/tools/ModCheck`: `ModCheck.csproj` and `Program.cs`. The project, the assembly and the folder share the name `ModCheck`.
- The ICON store for the tool is `common/tools/ModCheck/.context`.
- Build output goes to `bin/` and `obj/` (ignored by `common/.gitignore`).

## Code Naming
**Id:** blk-aa161d844e40

- Types, methods and parameters follow C# convention: PascalCase for types and methods (`Program`, `Report`, `AllMethods`, `Unwrap`), camelCase for parameters and locals (`managedDir`, `modDll`, `isDelegate`, `failures`).
- Local constants are camelCase too (`const BindingFlags all`).
- Methods start with a verb or name what they return: `Report`, `AllMethods`, `Unwrap`.
- Counters are plural nouns (`failures`, `warnings`) passed by `ref`.
- No `I` interfaces and no type prefixes; there is one class.

## Command Line
**Id:** blk-6bdab3ca51dd

- Two positional arguments in a fixed order: the game's Managed directory, then the mod DLL. No options or flags.
- The mod DLL path is made absolute with `Path.GetFullPath` before use.

## Output Vocabulary
**Id:** blk-0902df8ac85c

- Stages in a result line are the lowercase words `type` and `jit`; the loader failure is `loader`.
- Line prefixes are upper case: `OK`, `FAIL`, `WARN`. The final lines are `ALL CHECKS PASSED` or `<n> FAILURE(S)`.
- Types are printed by `Type.FullName`, methods by `Name` after the type, as `Namespace.Type.Method`.

## Related
**Id:** blk-d692513e0e50

- See also: [code style](code-style.md)
