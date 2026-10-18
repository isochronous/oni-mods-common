---
tags: steam-workshop
doc_id: doc-435cfd6110bc
description: WorkshopUpload's Program.Main, a table of its seven commands and their methods in Program.cs, the helpers, the two app id constants, files read and written, business rules, code paths and item visibility states
applies_to:
  - "*.cs"
  - "*.csproj"
---

# WorkshopUpload domain

## Overview
**Id:** blk-deeb9ba55606  
**Tags:** legacy-workshop-item, steamworks-api, steam-app-id, release-process

WorkshopUpload publishes and updates Oxygen Not Included mods on the Steam Workshop and inspects items. It is a .NET 8 console app over Steamworks.NET. It uses the legacy `ISteamRemoteStorage` API, which is the only kind of item the game can download. It runs as Klei's "Oxygen Not Included Uploader" tool (app 636750) with the game (app 457140) as the consumer app, and needs the Steam client running and logged in as the item's owner.

Source: `tools/WorkshopUpload/Program.cs`. `tools/Release/release.py` calls it to push a release zip, preview, description and change note to the mod's Workshop item. The commands are documented in `common/README.md`.

## Key Entities
**Id:** blk-cea83f7f6342

### Program.Main
**Id:** blk-c95b82024244  
**Tags:** command-line, exit-codes, steamworks-api, console-output  
**Description**: Entry point. Prints usage (return 2) when there are no arguments, calls `SteamAPI.Init`, prints the running app id and Steam user, dispatches on `args[0]` and shuts Steam down in `finally`.  
**Location**: `tools/WorkshopUpload/Program.cs`

### Commands
**Id:** blk-e1e1ae357f1d  
**Tags:** command-line, steam-cloud, item-download, item-description

| Command | Method | What it does |
|---------|--------|--------------|
| `info <id>` | `Info` | prints result, title, a 90-character description excerpt, file name and size, preview handle, visibility, type, update time, and whether the item is a legacy single-file item |
| `description <id> [out.txt]` | `Description` | writes the item's live description to a file or prints it |
| `list` | `List` | pages through `EnumerateUserPublishedFiles` and prints id, title, visibility and `legacy` or `NON-LEGACY (broken)` per item |
| `update <id> [zip [preview]] [options]` | `Update` | uploads the files to Steam Cloud, builds an update request (file, preview, title, description, change note, visibility, tags, each only when given), commits it, then runs `Info` |
| `publish <zip> <preview> --title T [options]` | `Publish` | uploads zip and preview, calls `PublishWorkshopFile` as a community item (default visibility private), prints the new id and URL, then runs `Info` |
| `cloud` | `Cloud` | Steam Cloud diagnostics: enabled for account and app, quota, every cloud file with its size and persisted flag |
| `download <id>` | `Download` | has this client download the item, then reports the install path, whether it is a file (a zip, listing its entries) or a directory (a failure), and the update time |

**Location**: `Program.cs`. **Relationships**: `Update` and `Publish` call `UploadToCloud` and `ParseOptions`, and end by calling `Info`; `Info`, `Description`, `List` and `Publish` use `Await<T>`.

### Helpers
**Id:** blk-0561d852e27b  
**Tags:** steam-cloud, steamworks-api, workshop-tags, item-visibility

- `UploadToCloud(localPath)`: `SteamRemoteStorage.FileWrite` into the app's cloud under a fixed name, then `FileShare` to give it a UGC handle; returns the cloud name.
- `Await<T>(call, timeoutSeconds = 120)`: pumps callbacks until a call result arrives.
- `ParseOptions(args)`, `ParseTags`, `ParseVisibility`, `Check`: option, tag and visibility parsing and the Steam `bool` check.

### Constants
**Id:** blk-87f0abb1660a  
**Tags:** steam-app-id

| Name | Value | Meaning |
|------|-------|---------|
| `AppId` | 457140 | the game; consumer app of every item |
| `UploaderAppId` | 636750 | Klei's uploader tool; the creator app of every working item; selected by `steam_appid.txt` |

## Files read and written
**Id:** blk-ef9f834e5cca  
**Tags:** steam-app-id, steam-cloud, item-description

| File | Direction | Description |
|------|-----------|-------------|
| `steam_appid.txt` | read by Steam | `636750`, next to the exe |
| `steam_api64.dll` | loaded | Valve's native library |
| mod zip, preview, `--description-file` | read | uploaded to Steam Cloud and the Workshop |
| `description` output file | written | the live description text |
| Steam Cloud: `mod_publish_data_file.zip`, `mod_publish_preview.<ext>` | written | staging copies shared before publish or update |
| Workshop item | written | created or changed by `publish` and `update` |

## Business Rules
**Id:** blk-5e79385150d5  
**Tags:** legacy-workshop-item, steam-app-id, item-download, workshop-tags

- Publish and update only through `ISteamRemoteStorage`. An item updated through `ISteamUGC` stops being downloadable by the game, permanently.
- The process must run as app 636750; sharing cloud files under the game's own app id fails with FileNotFound. `steam_appid.txt` stays `636750`.
- `download` answers about the game's view of the item only when run with the environment variables `SteamAppId=457140` and `SteamGameId=457140` (ModTest sets them); otherwise it answers for the uploader app.
- The zip holds `mod.yaml`, `mod_info.yaml`, the DLL and `preview.png` at its root, as Klei's uploader produces.
- Prefer Klei's uploader for new items: the one item `publish` created (2026-10-08) never appeared in the Workshop's Most Recent listing. Updating existing items with this tool is proven.
- Give `publish` a category tag plus the DLCs supported, since the Workshop's browse views key off tags.
- Read the live description (`description`) before overwriting it, since it can be edited on the Workshop page.

## Key Code Paths
**Id:** blk-cc023dbf02c0  
**Tags:** steam-cloud, steamworks-api, release-process

- **Update an item**: `Main` -> `Update` -> `UploadToCloud` (zip, preview) -> `CreatePublishedFileUpdateRequest` -> `UpdatePublishedFile*` through `Check` -> `CommitPublishedFileUpdate` -> `Info` -- `Program.cs`.
- **Publish an item**: `Main` -> `Publish` -> `UploadToCloud` x2 -> `PublishWorkshopFile` -> `Info`.
- **Release use**: `tools/Release/release.py` runs `update` with the release zip, the preview, the tags, the description file and the change note, `description` to read the live text first, and `list` to find an item id by title.

TODO(owner): `common/README.md` lists a `ugc-describe` command that `Main` does not dispatch, and the usage line printed by `Main` omits `description` and `cloud`; decide whether to add `ugc-describe` or drop it from the README, and bring the usage text in line.

## State Transitions
**Id:** blk-b0755166f77a  
**Tags:** item-visibility

None kept by the tool. The Workshop item carries visibility (`private`, `friends`, `unlisted`, `public`) that `publish` sets at creation and `update --visibility` changes.

## Related
**Id:** blk-65430db8c417

- See also: [glossary](glossary.md)
- Governed by: [naming conventions](../standards/naming-conventions.md)
