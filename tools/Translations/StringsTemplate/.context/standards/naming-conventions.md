---
doc_id: doc-718fdd9d28d5
description: "StringsTemplate (the translation template writer and checker): file, method, variable and translation key naming, output file names and report keywords"
applies_to:
  - "*.cs"
  - "*.csproj"
---
# StringsTemplate naming conventions

## Files and Directories
**Id:** blk-51503f510e6b

- Folder, project file and assembly share one name: `StringsTemplate` (`StringsTemplate.csproj`, `StringsTemplate.dll`). The tool lives under `tools/Translations/`, next to `translations.py`.
- C# source is a single `Program.cs`.
- Outputs are named after the checked mod's assembly: `<translations dir>/<Mod>.pot` (the assembly name of the DLL passed in). Inputs are `<locale>.po` files in the same folder.

## Code Naming
**Id:** blk-7f6a26ed0265

### Types
**Id:** blk-2114efedfc0a

- PascalCase. `Program` is `internal static`; the only other type is the nested `private sealed class Entry`. There is no namespace.

### Methods
**Id:** blk-5e970e3a44e3

- PascalCase verbs naming the job: `Collect`, `WritePot`, `Escape`, `Unescape`, `CheckOptionKeys`, `CheckPo`, `ReadPo`, `Parameter`, `Quoted`, `SameParameters`, `LinkCount`, `LinksMatch`.
- Methods that check something are `Check*` and return the number of problems; predicates read as questions (`SameParameters`, `LinksMatch`).
- All methods are `private static`.

### Variables
**Id:** blk-04b060e253dd

- camelCase for locals and parameters, named for what they hold: `managedDir`, `modDll`, `translationsDir`, `translated`, `entries`.
- Counters are short words in one declaration (`int missing = 0, stale = 0, empty = 0, refused = 0, obsolete = 0;`).
- The static regex field is PascalCase (`Parameters`) and `readonly`.
- Lambda parameters are one letter (`p`, `e`, `a`, `b`, `m`).

### Fields
**Id:** blk-0e13b825438b

- `Entry` has public fields `Key` and `English`; there are no properties.

## Project-Specific Conventions
**Id:** blk-96cf073a4c6d

### Translation keys
**Id:** blk-d15b30cc4eaf

- A key is the path through the mod's `ModStrings` tree with a `STRINGS.` prefix: `ModStrings.A.B.C` is `STRINGS.A.B.C`. The tool builds it from nested type names and the `LocString` field name, so the C# names are the keys.
- Keys are sorted with `string.CompareOrdinal` in the `.pot`.

### Report text
**Id:** blk-491fc8e9674c

- Each line starts with an upper-case keyword (`FAIL`, `MISSING`, `STALE`, `REFUSED`, `EMPTY`, `OBSOLETE`) followed by the po file name and key.

### Command line
**Id:** blk-4f71c0738539

- Positional arguments only, in this order: game Managed directory, mod DLL, translations directory.

## Related
**Id:** blk-ea3099460d90

- See also: [code style](code-style.md), [glossary](../domains/glossary.md)
