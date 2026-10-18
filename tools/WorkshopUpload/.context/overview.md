---
tags: steam-workshop, legacy-workshop-item
description: WorkshopUpload's purpose (legacy ISteamRemoteStorage publishing for the game), the app ids it runs under, its seven commands and its callers, and a map of Program.cs, WorkshopUpload.csproj, steam_appid.txt and steam_api64.dll
---

# WorkshopUpload

## What this is
**Id:** blk-846080fa7238  
**Tags:** steam-app-id, steamworks-api, release-process, item-download

WorkshopUpload publishes and updates Oxygen Not Included mods on the Steam Workshop as legacy single-file items, and inspects items. The game has no Workshop depot and downloads only items published through the old `ISteamRemoteStorage` API, so this .NET 8 console app over Steamworks.NET publishes and updates through that API only; an item updated through `ISteamUGC` stops being downloadable by the game. It runs as Klei's Oxygen Not Included Uploader (app 636750, chosen by `steam_appid.txt`) with the game (app 457140) as the consumer app, and needs the Steam client running and logged in as the item's owner. Its commands are `info`, `description`, `list`, `update`, `publish`, `cloud` and `download`. `tools/Release/release.py` calls it to push a release zip, preview, description and change note to a mod's Workshop item, and `tools/ModTest/Test-OniMods.ps1` uses `download` to fetch a newly subscribed item for `-Steam`.

## Where things are
**Id:** blk-6dcf139ddfa3  
**Tags:** tool-build, steam-app-id, steamworks-api

Top-level folders and key files:

- `Program.cs`: the whole tool: `Main` and the seven commands, the helpers (`UploadToCloud`, `ParseOptions`, `ParseTags`, `ParseVisibility`, `Check`, `Await`) and the app id constants.
- `WorkshopUpload.csproj`: the .NET 8 win-x64 console project referencing Steamworks.NET 2024.8.0; copies `steam_appid.txt` and `steam_api64.dll` next to the exe. `release.py` runs the Release build from `bin/Release/net8.0/win-x64/`; `bin/` and `obj/` are ignored by git.
- `steam_appid.txt`: `636750`, the uploader app id. It stays that value: sharing cloud files under the game's own app id fails.
- `steam_api64.dll`: Valve's native Steam library for that Steamworks.NET version; the NuGet package does not carry it and the game's own copy is too old.
- The user-facing guide is the Publishing to the Steam Workshop section of `common/README.md`.

The `.context/` documents of this node: [domains](domains/README.md) (entities, glossary), [standards](standards/README.md) (code style, error handling, naming conventions), `decisions/` and `retrospectives/`.
