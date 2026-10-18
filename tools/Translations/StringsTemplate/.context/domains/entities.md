---
doc_id: doc-1533a2e8de9a
description: "StringsTemplate (the translation template writer and checker): Program.Main and its helpers, the files it reads and writes, the rules it enforces and its code paths"
applies_to:
  - "*.cs"
  - "*.csproj"
---
# StringsTemplate domain

## Overview
**Id:** blk-77fa8ff1839f

StringsTemplate writes a mod's translation template and checks its translations without starting the game. It is a .NET 8 console app. It loads a built mod DLL against the game's real assemblies, reads the mod's `ModStrings` tree (the classes the `Isochronous.Translations` loader registers as `STRINGS.*`), writes `<Mod>.pot`, and compares every `<locale>.po` in the translations folder against it. `tools/Translations/translations.py` runs it for each mod; it can also be run by hand:

```
StringsTemplate <game Managed dir> <mod dll> <translations dir>
```

Source: `tools/Translations/StringsTemplate/Program.cs`. It has no game hooks and patches nothing; it only reads.

## Key Entities
**Id:** blk-37a6176b8829

### Program.Main
**Id:** blk-b1319f3547fd  
**Description**: Entry point. Validates the three arguments, installs the `AssemblyResolve` handler, loads the DLL, finds the top-level `ModStrings` type, collects the entries, writes the `.pot`, runs the option-key check and the po checks, prints the verdict and returns the exit code (0, 1 or 2).  
**Location**: `tools/Translations/StringsTemplate/Program.cs`

**Relationships**: calls `Collect` for every nested type of `ModStrings`, then `WritePot`, `CheckOptionKeys`, and `CheckPo` once per po file in name order.

### Entry
**Id:** blk-87683f136a98  
**Description**: One translatable string: its key (`STRINGS.A.B.C`) and the English text of the `LocString`.  
**Location**: `Program.cs` (nested `private sealed class Entry`)

| Property | Type | Description |
|----------|------|-------------|
| Key | string | `STRINGS.` plus the nested type names and the field name |
| English | string | the `LocString.text` read by reflection after the `ModStrings` initializers ran; empty when the value is null |

**Relationships**: `Collect` produces a sorted `List<Entry>`; `WritePot` and `CheckPo` consume it.

### Collect
**Id:** blk-eea74830c54d  
**Description**: Walks a type's public static `LocString` fields and its nested types, building keys from the names.  
**Location**: `Program.cs`

### WritePot
**Id:** blk-d9891737717a  
**Description**: Writes the template: a gettext header, then per entry a `#. <key>` comment, `msgctxt` (the key), `msgid` (the English, with quotes and newlines escaped) and an empty `msgstr`. Overwrites the file every run, UTF-8 without a BOM, LF line ends.  
**Location**: `Program.cs`

### CheckOptionKeys
**Id:** blk-765f8c52e2f8  
**Description**: Finds `OptionAttribute` and `ConfigFileAttribute` (PLib) on every type, property and field of the mod by attribute type name, and fails any `Title`, `Tooltip` or `Category` that starts with `STRINGS.` and is not in the entry list.  
**Location**: `Program.cs`

### CheckPo, ReadPo, Parameter, Quoted
**Id:** blk-f451620721c1  
**Description**: `CheckPo` reads a po with `ReadPo` (msgctxt to (msgid, msgstr), parsed the way the game's reader does) and compares it to the entries, counting missing, empty, stale, refused and obsolete ones. `Parameter` joins a keyword line with the quoted continuation lines under it; `Quoted` strips the quotes and throws `FormatException` for a line the game's reader would throw on.  
**Location**: `Program.cs`

### SameParameters, LinkCount, LinksMatch
**Id:** blk-0c8a3abcea91  
**Description**: The game's acceptance rules for a translated string: the set of distinct `{...}` tokens equals the English set; the `<link=` count does not exceed the English count; every `<link=` has a `</link>`.  
**Location**: `Program.cs`

## Files read and written
**Id:** blk-1cfa0c1f2c1d

| File | Direction | Description |
|------|-----------|-------------|
| `<mod dll>` | read | the built mod (for example `src/<Mod>/bin/Release/<Mod>.dll`), loaded with `Assembly.LoadFrom` |
| `<game Managed dir>\*.dll` | read | resolves the mod's references (`Assembly-CSharp`, Unity, PLib) |
| `<translations dir>/<Mod>.pot` | written | template, created or overwritten |
| `<translations dir>/*.po` | read | one per locale, checked, never modified |

## Business Rules
**Id:** blk-bfa5d623d61d

- The key of a string is its path in `ModStrings`: `ModStrings.A.B.C` is `STRINGS.A.B.C`. Only public static fields of type `LocString` count.
- The `.pot` is regenerated from the DLL every run, so it always matches the code.
- A po entry is stale when its `msgid` is not the English in the DLL now; stale, missing and refused entries and unknown option keys fail the run (exit 1). Empty and obsolete entries are reported only.
- A po line the game's reader cannot parse fails the whole file, as it would in the game.
- The tool never edits a po file.

## Domain Terminology
**Id:** blk-55b5f3a241f8

| Term | Definition |
|------|-----------|
| ModStrings | the mod's static class tree of `LocString`s; the source of every key |
| pot | the template this tool writes: every key with its English and an empty translation |
| po | a locale's translation file, `<locale>.po` |
| stale | the po's English no longer matches the DLL's |
| refused | a translation the game would reject at load |

## Key Code Paths
**Id:** blk-aa442e0b3e88

- **Template and check run**: `Main` -> `Collect` -> `WritePot` -> `CheckOptionKeys` -> `CheckPo` per po -- see `Program.cs`.
- **Po parse**: `CheckPo` -> `ReadPo` -> `Parameter` -> `Quoted` -- see `Program.cs`.
- **Caller**: `main()` in `tools/Translations/translations.py` builds the project at `TOOL`, then runs `bin/Release/net8.0/StringsTemplate.dll` on each mod's DLL with the game's Managed folder and the mod's `translations` folder.

## State Transitions
**Id:** blk-5650565fc4ce

None. The tool keeps no state between runs.

## Related
**Id:** blk-dda6f3110a4e

- See also: [glossary](glossary.md)
- Governed by: [naming conventions](../standards/naming-conventions.md)
