---
tags: steam-workshop
doc_id: doc-7dcc2a05f7ea
description: "WorkshopUpload glossary: abbreviations (UGC, ONI, DLC, EResult), terms to avoid with their replacements, and definitions of legacy item, uploader app, consumer app, Steam Cloud staging, tags, visibility and change note"
---

# WorkshopUpload glossary

Terms a newcomer to WorkshopUpload needs. The tool's own things are in [entities](entities.md).

## Abbreviations and Acronyms
**Id:** blk-581cdeae0e45  
**Tags:** steamworks-api, steam-cloud, workshop-tags

| Abbreviation | Full Term | Context |
|--------------|-----------|---------|
| UGC | user-generated content | Steam's name for Workshop files; a UGC handle is what a shared cloud file gets |
| ONI | Oxygen Not Included | the game, app 457140 |
| DLC | downloadable content | Workshop tags name the DLCs an item supports |
| EResult | Steam's result code | `k_EResultOK` is success; checked after every call |

## Terms to Avoid
**Id:** blk-311bd945ab82  
**Tags:** legacy-workshop-item, steam-cloud

| Avoid | Use Instead | Reason |
|-------|-------------|--------|
| "upload" for a Workshop item change | update, or publish for a new item | the commands are `update` and `publish`; "upload" is the Steam Cloud write (`UploadToCloud`) |
| steamcmd or `workshop_build_item` for ONI items | this tool or Klei's uploader | steamcmd items are manifest-based and fail to download for every subscriber |
| ISteamUGC for publishing | ISteamRemoteStorage | an item touched through `ISteamUGC` item update stops being downloadable by the game, permanently |

## Cross-Domain Terms
**Id:** blk-bda516d5626b

### Legacy item
**Id:** blk-a8b23fa71212  
**Tags:** legacy-workshop-item, item-download  
**Definition**: A Workshop item made through `ISteamRemoteStorage`: one zip file that Steam installs as `<handle>_legacy.bin` and the game opens with `ZipFile`. ONI has no Workshop depot, so only legacy items download.  
**Used in**: `Info` (`Legacy single-file item` or `NOT a legacy single-file item`), `List`, `Download`.

### Uploader app (636750)
**Id:** blk-ec23c3e1a40f  
**Tags:** steam-app-id, steam-cloud  
**Definition**: Klei's "Oxygen Not Included Uploader" tool, free with the game. The tool runs as this app, set by `steam_appid.txt`, because cloud files shared under the game's own id fail with FileNotFound. It is the `creator_app_id` of every working item.  
**Used in**: `UploaderAppId`, `Main`.

### Consumer app (457140)
**Id:** blk-9a0de1cbdd76  
**Tags:** steam-app-id  
**Definition**: The game; the app whose Workshop the item belongs to and whose players download it.  
**Used in**: `AppId`, `PublishWorkshopFile`.

### Steam Cloud staging
**Id:** blk-3f69734ff5c6  
**Tags:** steam-cloud  
**Definition**: The step where the zip and preview are written into the user's Steam Cloud for the uploader app under fixed names and shared, so the publish or update call can refer to them.  
**Used in**: `UploadToCloud`.

### Tags
**Id:** blk-2a0c81ae5d5a  
**Tags:** workshop-tags  
**Definition**: Workshop labels that its browse views filter on: a category (`tweaks`) plus supported DLCs (`Base Game`, `Spaced Out!`, `The Frosty Planet Pack`, `The Bionic Booster Pack`, `The Prehistoric Planet Pack`, `The Aquatic Planet Pack`).  
**Used in**: `--tags`, `ParseTags`.

### Visibility
**Id:** blk-b897b622b7a9  
**Tags:** item-visibility  
**Definition**: `public`, `friends`, `unlisted` or `private`. `publish` defaults to private.  
**Used in**: `ParseVisibility`, `--visibility`.

### Change note
**Id:** blk-95241105067f  
**Tags:** item-description  
**Definition**: The text shown in an item's change history, set with `update --changenote`.  
**Used in**: `Update`, `release.py`.

## Related
**Id:** blk-3691ae7e3898

- See also: [entities](entities.md)
