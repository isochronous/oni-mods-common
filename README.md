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

Build with `dotnet build src/<ModName> -c Release`. A successful build deploys the DLL, `mod.yaml`, `mod_info.yaml`, and any `assets/` folder to `Documents\Klei\OxygenNotIncluded\mods\local\<ModName>`.

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

## Publishing to the Steam Workshop

**Do not use steamcmd's `workshop_build_item` for ONI mods.** The game has no Workshop depot (Steam's `workshop_log.txt` says "Workshop depot not defined, legacy support only"), so it can only download *legacy* single-file items: one zip that Steam installs as `<handle>_legacy.bin` and the game opens with `ZipFile`. A folder of loose files uploaded by steamcmd becomes a manifest-based item that Steam skips as "non-legacy", and every subscriber gets "Steam failed to download the mod".

`tools/WorkshopUpload` publishes through the legacy `ISteamRemoteStorage` API instead, running as Klei's **Oxygen Not Included Uploader** tool (app 636750, free with the game; every working Workshop item has it as `creator_app_id`) with the game as the consumer app. Sharing cloud files under the game's own app id fails with FileNotFound, so `steam_appid.txt` must stay 636750. Build it with `dotnet build tools/WorkshopUpload -c Release`, put a `steam_api64.dll` from Steamworks SDK 1.60 or newer next to the exe (the game's own copy is too old for the Steamworks.NET wrapper), keep the Steam client running as the item owner, then:

```
WorkshopUpload info <itemId>                                  # is it a legacy item the game can download?
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

