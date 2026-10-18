---
doc_id: doc-32e91bfd48ae
description: "StringsTemplate (the translation template writer and checker): terms a newcomer needs: ModStrings, key, pot, po, stale, refused, obsolete, Managed directory"
---
# StringsTemplate glossary

Terms a newcomer to StringsTemplate needs. The tool's own things are in [entities](entities.md).

## Abbreviations and Acronyms
**Id:** blk-7e2793659fb3

| Abbreviation | Full Term | Context |
|--------------|-----------|---------|
| pot | Portable Object Template | the `<Mod>.pot` the tool writes |
| po | Portable Object | a locale's translation file, `<locale>.po` |
| LocString | the game's localized string type | the field type the tool collects from `ModStrings` |
| PLib | the PeterHan mod library | its `[Option]` attribute is checked for unknown `STRINGS.` keys |
| DLC | downloadable content | not used by this tool |

## Terms to Avoid
**Id:** blk-388ad4844aad

| Avoid | Use Instead | Reason |
|-------|-------------|--------|
| "translation file" for the template | pot | the template has no translations; only `.po` files do |
| "string id" | key | the code and the po files call `STRINGS.A.B.C` the key (`msgctxt`) |
| "missing" for a blank translation | empty | `MISSING` means the key is absent from the po; `EMPTY` means it is there with no text |

## Cross-Domain Terms
**Id:** blk-c52c80493ed0

### ModStrings
**Id:** blk-3c9e14f660b8  
**Definition**: The top-level static class, named exactly `ModStrings` and not nested, in each mod's DLL. Its nested classes hold public static `LocString` fields; the nesting is the key.  
**Used in**: `Program.Main`, `Collect`; the `Isochronous.Translations` loader that registers the tree at game start.

### Key (msgctxt)
**Id:** blk-b20d63f89dd0  
**Definition**: `STRINGS.` plus the nested type names and field name, such as `STRINGS.UI.TOOLS.SWEEPZONES.<FIELD>` for a field in the Sweep Zones mod's `ModStrings.UI.TOOLS.SWEEPZONES` class. It is the `msgctxt` of an entry in both the pot and the po files.  
**Used in**: every report line.

### Stale
**Id:** blk-b9b671421f4a  
**Definition**: A po entry whose `msgid` differs from the English the DLL now holds, meaning the English changed after the translation was made.  
**Used in**: `CheckPo`; counts toward exit code 1.

### Refused
**Id:** blk-4dde81fafde8  
**Definition**: A translation the game rejects when loading it: a different set of `{0}`-style parameters than the English, more `<link=` tags than the English, or a `<link=` without `</link>`.  
**Used in**: `CheckPo`, `SameParameters`, `LinkCount`, `LinksMatch`.

### Obsolete
**Id:** blk-dd97c8315b8b  
**Definition**: A po key that no longer exists in the DLL. Reported, not counted.  
**Used in**: `CheckPo`.

### Managed directory
**Id:** blk-8e28af007c68  
**Definition**: The game's `OxygenNotIncluded_Data\Managed` folder, searched for assemblies the mod DLL references.  
**Used in**: first command-line argument; the `AssemblyResolve` handler.

## Related
**Id:** blk-c4070a6745c1

- See also: [entities](entities.md)
