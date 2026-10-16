# oni-mods-common

Shared MSBuild configuration for [isochronous](https://github.com/isochronous) Oxygen Not Included mods. Centralizes everything that breaks when the game updates — target framework, game assembly references, game folder detection — plus the deploy-to-local-mods step, so a game-update fix lands here once and each mod just bumps its submodule.

## Usage in a mod repo

Add as a submodule at `common/`:

```
git submodule add https://github.com/isochronous/oni-mods-common.git common
```

Then the mod csproj (at `src/<ModName>/<ModName>.csproj`) reduces to:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <Import Project="$(MSBuildThisFileDirectory)..\..\common\oni-mod.props" />
  <PropertyGroup>
    <AssemblyName>ModName</AssemblyName>
    <RootNamespace>ModName</RootNamespace>
    <Version>1.0.0</Version>
  </PropertyGroup>
  <Import Project="$(MSBuildThisFileDirectory)..\..\common\oni-mod.targets" />
</Project>
```

Build with `dotnet build src/<ModName> -c Release`. A successful build deploys the DLL, `mod.yaml`, `mod_info.yaml`, and any `assets/` and `anim/` folders (custom kanims live at `anim/assets/<name>/`; see [Kanims](#kanims) for making one; its "place" anim must be the white outline the game tints as the construction ghost, which `kanim.place_outline` derives from the sprite) to `Documents\Klei\OxygenNotIncluded\mods\local\<ModName>`.

## Knobs

| Property / env var | Effect |
|---|---|
| `GameFolder` property or `ONI_GAME_FOLDER` env var | Game install dir when not in a known Steam location |
| `GameLibsFolder` | Direct path to `OxygenNotIncluded_Data\Managed` (overrides `GameFolder`) |
| `ModDeployFolder` | Deploy destination; `none` disables deployment |
| `ReferenceUnityUI` | Adds UnityEngine.UI, TextMeshPro, TextRenderingModule references |
| `ReferenceImageConversion` | Adds UnityEngine.ImageConversionModule (PNG sprite loading) |
| `UseTranslations` | Compiles `src/Translations.cs` into the mod and deploys its `translations/` folder (see Translations) |

## After a game update

1. Fix whatever changed here (references, target framework, etc.) and push.
2. In each mod repo: `git submodule update --remote common`, rebuild, test, commit the submodule bump.

## Notes

- Targets **net48** — the game's own assemblies (including `0Harmony.dll`) target .NET Framework 4.8; net471 (old wiki guidance) silently drops references.
- The game runs Unity 6 Mono, which supports default interface methods; don't validate mod DLLs by loading them on the desktop .NET Framework CLR (it rejects DIMs) — use a .NET 8 host instead.

## Translations

A mod's text lives in a `ModStrings` class whose nested classes mirror the game's `STRINGS` tree: `ModStrings.BUILDINGS.PREFABS.X.NAME` is registered as `STRINGS.BUILDINGS.PREFABS.X.NAME`, the key the game itself looks a building's name up by; mod-private text goes under its own branch, `ModStrings.MYMOD.SIDESCREEN.TITLE`. Every field is a `public static LocString`, and code reads the field when it needs the text (never into a const or a cached string). With `<UseTranslations>true</UseTranslations>` in the csproj, `src/Translations.cs` is compiled into the mod and `OnLoad` calls

```csharp
Translations.Register(harmony, typeof(ModStrings));   // optional third argument: a System.Action run after every language pass
```

which creates the keys, applies `translations/<locale>.po` from the mod folder (`zh.po`, `ko.po`, `ru.po`: the game's locale codes, the ones Klei ships; a Workshop language uses its own code) once from the saved language preference, so strings the game copies early are already translated, and again after `Localization.Initialize` with the locale the game settled on. Translated text goes into both the fields and the string table, so key lookups (`Strings.Get`, `LocString.ToString`, PLib `[Option("STRINGS.MYMOD.OPTIONS.X.NAME", ...)]` keys) agree with field reads. Entries whose `{n}` parameters or `<link>` tags differ from the English are refused, as the game does for its own files, and logged.

The template comes from the built DLL, not the source, so the English in it is exactly what the player sees:

```
python tools/Translations/translations.py [mod-folder ...] [--build]
```

rewrites `src/<Mod>/translations/<Mod>.pot` for every mod with `UseTranslations` (or the ones named) and checks each `<locale>.po` there: missing, empty, stale (translating English the mod no longer says), obsolete and refused entries, plus every `[Option]` key that is not in the tree. Run it after changing strings and before a release; a po file is edited by hand or in Poedit, and the `translations/` folder ships in the zip.

## Releases

The mods compile against the game's own assemblies, which cannot live on a CI runner, so releases are cut locally instead of by a GitHub Action. No zip is ever made by hand:

```
python tools/Release/release_mods.py --plan            # what would be released, and why
python tools/Release/release_mods.py                   # release every mod that changed since its last v* tag
python tools/Release/release_mods.py sweep-zones smart-weight-plate --bump-for sweep-zones=minor
```

`release_mods.py` looks at every mod repo next to this one (or just the ones named). A mod is due when commits since its last `v*` tag touched `src/` or `publish/preview.png`; its version is bumped in `mod_info.yaml` and the csproj (patch unless told otherwise, and not at all if you already bumped it or it has never been released), committed as "Release vX.Y.Z" and pushed. That commit also turns the `## Unreleased` section of the repo's `CHANGELOG.md` into `## X.Y.Z - <date>`; write the player-facing notes there before releasing (`--plan` flags a mod without an entry). Mods with uncommitted changes are skipped, never stashed.

`release.py`, run from a mod repo's root, does one mod: Release build, `publish/content` staged from the build (DLL, yamls, `anim/`, `assets/`, and any `worldgen/`, `templates/`, `elements/`, `strings/` or `codex/` folder, preview scaled to 512 px), `publish/<Mod>.zip`, tag, and a GitHub release with `<Mod>-<version>.zip` attached. If the mod is on the Steam Workshop, the same zip, the preview (`publish/preview.gif` when present, which Steam animates on the item page, otherwise the png) and `publish/workshop-description.txt` are then pushed to its item with `tools/WorkshopUpload` (Steam must be running), with the changelog section as the change note (the version plus the shipped commit subjects when there is none). The same section is the GitHub release body. The item id is read from `publish/workshop-id.txt`; when that file is missing, the `mod.yaml` title is looked up among your published items (`WorkshopUpload list`) and the id found is written to the file and committed, so nobody creates it by hand. The description is only uploaded when the live one is still the text from the previous release: a description edited on the Workshop page is never overwritten, it is saved as `publish/workshop-description.live.txt` for merging by hand (`WorkshopUpload description <id> [file]` reads the live text any time). A mod that is not listed is not on the Workshop yet and gets a GitHub release only; its first upload is a deliberate `WorkshopUpload publish` or Klei's uploader. `--dry-run` stops after the zip; `--no-workshop` makes it a GitHub-only release.

## Publishing to the Steam Workshop

**Do not use steamcmd's `workshop_build_item` for ONI mods.** The game has no Workshop depot (Steam's `workshop_log.txt` says "Workshop depot not defined, legacy support only"), so it can only download *legacy* single-file items: one zip that Steam installs as `<handle>_legacy.bin` and the game opens with `ZipFile`. A folder of loose files uploaded by steamcmd becomes a manifest-based item that Steam skips as "non-legacy", and every subscriber gets "Steam failed to download the mod".

`tools/WorkshopUpload` publishes through the legacy `ISteamRemoteStorage` API instead, running as Klei's **Oxygen Not Included Uploader** tool (app 636750, free with the game; every working Workshop item has it as `creator_app_id`) with the game as the consumer app. Sharing cloud files under the game's own app id fails with FileNotFound, so `steam_appid.txt` must stay 636750. Build it with `dotnet build tools/WorkshopUpload -c Release`, put a `steam_api64.dll` from Steamworks SDK 1.60 or newer next to the exe (the game's own copy is too old for the Steamworks.NET wrapper), keep the Steam client running as the item owner, then:

```
WorkshopUpload info <itemId>                                  # is it a legacy item the game can download?
WorkshopUpload description <itemId> [out.txt]                 # the full LIVE description (it can be edited on the site)
WorkshopUpload list                                           # all your items for app 457140
WorkshopUpload update <itemId> <Mod.zip> [preview.png] [--changenote "..."]
WorkshopUpload publish <Mod.zip> <preview.png> --title "..." [--description-file f] [--visibility public]
WorkshopUpload download <itemId>                              # fetch with this client and show what the game would see
WorkshopUpload cloud                                          # Steam Cloud diagnostics
WorkshopUpload ugc-describe <itemId> --description-file f [--visibility unlisted]   # metadata-only update, works on steamcmd-made items; run with SteamAppId=457140
```

The zip must contain `mod.yaml`, `mod_info.yaml`, the DLL, and `preview.png` at its root, the same layout Klei's own uploader produces.

## Workshop preview images

`tools/MakePreview/make_preview.py` (Pillow) composes the 256x256 preview used by every isochronous mod: a game sprite centred on a transparent canvas with a white caption on a translucent black band. Sprites come from the game's kanim textures (`python tools/Kanim/kanim.py extract critter_sensor --out <folder>` copies `critter_sensor_0.png` and its build and anim out of the game); keep the extracted atlases untracked since they are Klei art, and commit only the composed preview.

```
python tools/MakePreview/make_preview.py critter_sensor_0.png preview.png --auto --no-shadow --rotate 180 --text "4-bit"
```

Buildings drawn from several parts (the fridge, for one) only have the parts in their atlas; `kanim.py render` assembles a frame to feed to `make_preview.py` (see below).

## Kanims

`tools/Kanim/kanim.py` reads, writes, renders and extracts Klei kanims, from the command line or imported by a mod's art script (`mods/<name>/tools/`). A `<kanim>` argument is a kanim folder (a mod's `src/<Mod>/anim/assets/<name>`) or a vanilla kanim's name; vanilla kanims are read from the installed game's `sharedassets0.assets` and cached in the temp folder (the game folder is `--game`, else `ONI_GAME_FOLDER`, else the Steam folders the builds look in).

```
python tools/Kanim/kanim.py list logic_ribbon_reader                 # anims, symbols and their frames
python tools/Kanim/kanim.py dump logic_ribbon_reader --anim 5_1      # every element of a frame: symbol, layer, alpha, matrix, position
python tools/Kanim/kanim.py render fridge fridge.png --anim off --scale 1 --hide sweep
python tools/Kanim/kanim.py extract fridge critter_sensor --out <folder>
python tools/Kanim/kanim.py make sprite.png <mod>/src/<Mod>/anim/assets/my_building --name my_building
```

`make` builds a one-sprite building kanim from a PNG. Art scripts import `write_kanim` for several symbols and anims, `read_kanim` and `vanilla` to start from a vanilla kanim, `render` to measure or preview the result, and `drift` for a `--check` option that compares a fresh build with the committed files; the module's docstring has the details and the file format. The tools need `python -m pip install -r tools/requirements.txt`.

## Testing a mod in isolation

`tools/ModTest/Test-OniMods.ps1` launches the game with only the mods under development enabled and restores the normal loadout afterwards: it backs up `mods.json` (`mods.json.modtest-<timestamp>`), disables every entry, enables the local mods for both the base game and Spaced Out (default: every `mods/local` folder with a `mod.yaml`, minus the names in `tools/ModTest/disabled.txt`), registers freshly built local mods the game has not seen yet, launches through Steam, waits for the game to exit, copies the backup back and deletes it, and removes the data-dump json files written during the session (`-KeepDumps` keeps them). `-Omit` leaves named mods out without editing the list (for checking a bug without its fix); `-Only` enables a single mod and nothing else; `-NoLocal` enables no local mods; `-Steam` adds subscribed Workshop items by id; `-DryRun` only prints the loadout.

```
pwsh -ExecutionPolicy Bypass -File common/tools/ModTest/Test-OniMods.ps1 [-Mods a,b,...] [-Omit c,...] [-Only d] [-NoLocal] [-Steam id,...] [-KeepDumps] [-DryRun]
```

## Reproducing a report in a save

`tools/SaveRunner` is a dev mod plus `Invoke-SaveRun.ps1` that load a save with nobody at the keyboard, run it for a while with every save to disk blocked, record a probe once per game second and quit: run it once with the mod under test and once with `-Without <Mod>` and compare the two `probe.csv` files. The player's own loadout stays enabled, so a report reproduces with their mods. Mod Profile Manager restarts the game on a changed loadout and has to be left out with `-Without 2854869130,3281716506`. Details and the probes in `tools/SaveRunner/README.md`.

```
pwsh -File common/tools/SaveRunner/Invoke-SaveRun.ps1 -Save <file.sav> -Seconds 90 -Probe fridges [-With ...] [-Without ...] -Label <name>
```
