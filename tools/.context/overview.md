---
description: "TODO(owner): one line saying what tools is."
---
# tools

## What this is
**Id:** blk-647210c6a78a

`tools/` holds the command-line tools and development mods shared by every isochronous Oxygen Not Included mod: making kanims and Workshop previews, checking a mod DLL against the game, testing a mod alone or through its in-game tests, reproducing a report in a save or a worldgen, writing translation templates, and cutting releases to GitHub and the Steam Workshop. The mod author runs them from the ONI-Mods root or a mod's folder. Some call others: `Release/release.py` uploads through `WorkshopUpload`, `TestRunner/Invoke-OniTests.ps1` runs `ModCheck`, and `Translations/translations.py` runs `StringsTemplate`.

## Where things are
**Id:** blk-6c2bbf052519

Folders and key files:

- `Kanim/`: `kanim.py` reads, writes, renders and extracts Klei kanims, from the command line or imported by a mod's art script.
- `MakePreview/`: `make_preview.py` composes the 256x256 sprite-and-caption Workshop preview; `title_preview.py` makes the 512x512 preview with the mod's title over a background; `fonts/` holds the fonts they use.
- `ModCheck/`: a .NET 8 console tool that loads a mod DLL against the game assemblies and reports type-load and missing-member failures without launching the game.
- `ModTest/`: `Test-OniMods.ps1` launches the game with only the mods under development enabled and restores `mods.json` afterwards; `disabled.txt` and `included.txt` adjust the default loadout.
- `Release/`: `release.py` builds, packages, tags and publishes one mod to GitHub and the Workshop; `release_mods.py` releases every mod that changed since its last release.
- `SaveRunner/`: a dev mod plus `Invoke-SaveRun.ps1` that load a save unattended with saving blocked, record a probe once per game second and quit.
- `TestRunner/`: a dev mod plus `Invoke-OniTests.ps1` that run a mod's `[OniTest]` in-game tests unattended in the real game.
- `Translations/`: `translations.py` rewrites each mod's `.pot` template from its built DLL and checks its `.po` files; `StringsTemplate/` is the .NET 8 program it runs for that.
- `WorkshopUpload/`: the legacy-API Steam Workshop publisher, run as Klei's Oxygen Not Included Uploader (app 636750).
- `WorldgenDriver/`: a dev mod plus `Invoke-Worldgen.ps1` that generate clusters in the real game unattended for a list of coordinates and write one result per coordinate.
- `requirements.txt`: the Python packages the Python tools need (Pillow, numpy, scipy, UnityPy).
- `.gitattributes`: line-ending and merge rules for the `.context/` files below.

Six folders are nodes with their own `.context/`: `ModCheck`, `SaveRunner`, `TestRunner`, `Translations/StringsTemplate`, `WorkshopUpload` and `WorldgenDriver`; `projects.md` lists them. `Kanim`, `MakePreview`, `ModTest`, `Release` and `Translations/translations.py` have none.

The `.context/` documents of this node: `decisions/` and `projects.md`.
