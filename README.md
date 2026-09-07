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
