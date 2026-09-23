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

Build with `dotnet build src/<ModName> -c Release`. A successful build deploys the DLL, `mod.yaml`, `mod_info.yaml`, and any `assets/` and `anim/` folders (custom kanims live at `anim/assets/<name>/`; see `tools/MakeKanim` for generating one from a PNG; its "place" anim must be the white outline the game tints as the construction ghost, which `kanim_writer.place_outline` derives from the sprite) to `Documents\Klei\OxygenNotIncluded\mods\local\<ModName>`.

## Knobs

| Property / env var | Effect |
|---|---|
| `GameFolder` property or `ONI_GAME_FOLDER` env var | Game install dir when not in a known Steam location |
| `GameLibsFolder` | Direct path to `OxygenNotIncluded_Data\Managed` (overrides `GameFolder`) |
| `ModDeployFolder` | Deploy destination; `none` disables deployment |
| `ReferenceUnityUI` | Adds UnityEngine.UI, TextMeshPro, TextRenderingModule references |
| `ReferenceImageConversion` | Adds UnityEngine.ImageConversionModule (PNG sprite loading) |

## After a game update

1. Fix whatever changed here (references, target framework, etc.) and push.
2. In each mod repo: `git submodule update --remote common`, rebuild, test, commit the submodule bump.

## Notes

- Targets **net48** — the game's own assemblies (including `0Harmony.dll`) target .NET Framework 4.8; net471 (old wiki guidance) silently drops references.
- The game runs Unity 6 Mono, which supports default interface methods; don't validate mod DLLs by loading them on the desktop .NET Framework CLR (it rejects DIMs) — use a .NET 8 host instead.

## Releases

The mods compile against the game's own assemblies, which cannot live on a CI runner, so releases are cut locally instead of by a GitHub Action. No zip is ever made by hand:

```
python tools/Release/release_mods.py --plan            # what would be released, and why
python tools/Release/release_mods.py                   # release every mod that changed since its last v* tag
python tools/Release/release_mods.py sweep-zones smart-weight-plate --bump-for sweep-zones=minor
```

`release_mods.py` looks at every mod repo next to this one (or just the ones named). A mod is due when commits since its last `v*` tag touched `src/` or `publish/preview.png`; its version is bumped in `mod_info.yaml` and the csproj (patch unless told otherwise, and not at all if you already bumped it or it has never been released), committed as "Release vX.Y.Z" and pushed. That commit also turns the `## Unreleased` section of the repo's `CHANGELOG.md` into `## X.Y.Z - <date>`; write the player-facing notes there before releasing (`--plan` flags a mod without an entry). Mods with uncommitted changes are skipped, never stashed.

`release.py`, run from a mod repo's root, does one mod: Release build, `publish/content` staged from the build (DLL, yamls, `anim/`, `assets/`, preview scaled to 512 px), `publish/<Mod>.zip`, tag, and a GitHub release with `<Mod>-<version>.zip` attached. If the mod is on the Steam Workshop, the same zip, the preview and `publish/workshop-description.txt` are then pushed to its item with `tools/WorkshopUpload` (Steam must be running), with the changelog section as the change note (the version plus the shipped commit subjects when there is none). The same section is the GitHub release body. The item id is read from `publish/workshop-id.txt`; when that file is missing, the `mod.yaml` title is looked up among your published items (`WorkshopUpload list`) and the id found is written to the file and committed, so nobody creates it by hand. The description is only uploaded when the live one is still the text from the previous release: a description edited on the Workshop page is never overwritten, it is saved as `publish/workshop-description.live.txt` for merging by hand (`WorkshopUpload description <id> [file]` reads the live text any time). A mod that is not listed is not on the Workshop yet and gets a GitHub release only; its first upload is a deliberate `WorkshopUpload publish` or Klei's uploader. `--dry-run` stops after the zip; `--no-workshop` makes it a GitHub-only release.

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

`tools/MakePreview/make_preview.py` (Pillow) composes the 256x256 preview used by every isochronous mod: a game sprite centred on a transparent canvas with a white caption on a translucent black band. Sprites come from the game's kanim textures, which UnityPy extracts from `OxygenNotIncluded_Data/sharedassets0.assets` (Texture2D named `<kanim>_0`, e.g. `critter_sensor_0`); keep the extracted atlases untracked since they are Klei art, and commit only the composed preview.

```
python -m pip install UnityPy Pillow
python tools/MakePreview/make_preview.py critter_sensor_0.png preview.png --auto --no-shadow --rotate 180 --text "4-bit"
```

Buildings drawn from several parts (the fridge, for one) only have the parts in their atlas. `tools/MakePreview/render_kanim.py` (Pillow, numpy) assembles one animation frame the way the game does, from the texture plus the `<kanim>_build` and `<kanim>_anim` TextAssets, and writes a transparent PNG to feed to `make_preview.py`:

```
python tools/MakePreview/render_kanim.py fridge_0.png fridge_build.bytes fridge_anim.bytes fridge.png --anim off --scale 1 --skip sweep
```

`--list` prints the animation and symbol names; `--skip` leaves out symbols such as the sweep marker.

