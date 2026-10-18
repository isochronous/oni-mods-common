---
description: "TODO(owner): one line saying what common is."
---
# common

## What this is
**Id:** blk-c218b0b8e576

common is oni-mods-common (`https://github.com/isochronous/oni-mods-common`), the shared build configuration and tooling of the isochronous Oxygen Not Included mods, included in the ONI-Mods monorepo as the `common/` submodule. Every mod csproj imports `oni-mod.props` and `oni-mod.targets` through `$(OniCommon)`, which ONI-Mods sets in its `Directory.Build.props`, so the target framework, the game assembly references, game folder detection and the deploy-to-local-mods step change in one place after a game update. It also holds the translation loader compiled into mods that set `UseTranslations`, and the tools that make art and previews, check, test, release and publish the mods. The mod author uses it every time a mod in `mods/` is built, tested or released.

## Where things are
**Id:** blk-0aca56964b6a

Top-level folders and key files:

- `oni-mod.props`: imported at the top of a mod csproj. net48, C# 9, the game folder (`GameFolder`, `GameLibsFolder`, the `ONI_GAME_FOLDER` environment variable, then the known Steam folders) and `ModDeployFolder`.
- `oni-mod.targets`: imported at the bottom of a mod csproj. The game assembly references, the opt-in `ReferenceUnityUI`, `ReferenceImageConversion`, `UsePLib` and `UseTranslations` extras, the `EnsureGameLibs` check and the `DeployToLocalMods` step.
- `ilrepack-plib.targets`: the `MergePLib` target that merges PLib into the mod DLL when `UsePLib` is set, wired in by `oni-mod.targets` as `ILRepackTargetsFile`.
- `src/Translations.cs`: `Isochronous.Translations`, which registers a mod's `ModStrings` tree and applies its `translations/<locale>.po` files; compiled into mods that set `UseTranslations`.
- `tools/`: the shared tools (Kanim, MakePreview, ModCheck, ModTest, Release, SaveRunner, TestRunner, Translations, WorkshopUpload, WorldgenDriver) and `requirements.txt` for the Python ones. It is this node's one child node.
- `README.md`: using common in a mod, the build knobs, what to do after a game update, translations, releases, Workshop publishing, previews, kanims, testing a mod in isolation and reproducing a report in a save.
- `.gitignore`: build output (`bin/`, `obj/`), the `runs/` folders of WorldgenDriver, SaveRunner and TestRunner, and the Graystroke font.
- `.gitattributes`: line-ending and merge rules for the `.context/` files.

The `.context/` documents of this node: `decisions/`, `retrospectives/`, and `projects.md`, which lists the child node `tools`.
