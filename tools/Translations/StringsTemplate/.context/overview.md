---
tags: translations, translation-template
description: StringsTemplate's purpose, command line, checks and exit codes, who runs it and when, and a map of its files (Program.cs, StringsTemplate.csproj and the Release build output) and its .context documents
---

# StringsTemplate

## What this is
**Id:** blk-e125ed2b68c6  
**Tags:** mod-strings, command-line, exit-codes, mod-options

StringsTemplate writes an Oxygen Not Included mod's translation template and checks its translations from the built mod DLL, without launching the game. It is a .NET 8 console app (C# 9) that loads the DLL against the game's own assemblies in the `Managed` folder, runs the `ModStrings` initializers and walks that class tree the way `Isochronous.Translations` (`common/src/Translations.cs`) registers it: `ModStrings.A.B.C` is the key `STRINGS.A.B.C`, with the field's English text. Run as `StringsTemplate <game Managed dir> <mod dll> <translations dir>`, it rewrites `<Mod>.pot` in the translations folder, reports missing, stale, empty, obsolete and refused entries in every `<locale>.po` there, checks that every PLib `[Option]` key written as a `STRINGS.` key exists, and exits 1 when a po has missing, stale or refused entries or an option key is unknown (2 for a usage error). The mod developer runs it through `tools/Translations/translations.py` after changing strings and before a release; that script builds this tool and runs it for every mod whose csproj sets `<UseTranslations>true</UseTranslations>`.

## Where things are
**Id:** blk-04261b4be94e  
**Tags:** tool-build

Top-level folders and key files:

- `Program.cs`: the whole tool: `Main`, the `ModStrings` walk (`Collect`), `WritePot`, `CheckOptionKeys`, and the po checks (`CheckPo`, `ReadPo` and the parameter and link comparisons).
- `StringsTemplate.csproj`: the .NET 8 console project, C# 9, nullable off, no package references. A Release build lands at `bin/Release/net8.0/StringsTemplate.dll`, where `translations.py` runs it; `bin/` and `obj/` are ignored by git.
- `tools/Translations/translations.py`, one folder up and outside this node: finds the game's `Managed` folder (`ONI_GAME_FOLDER` or the Steam library), builds this tool and runs it per mod.
- The user-facing guide is the Translations section of `common/README.md`.

The `.context/` documents of this node: [domains](domains/README.md) (entities, glossary), [standards](standards/README.md) (code style, error handling, naming conventions), `decisions/` and `retrospectives/`.
