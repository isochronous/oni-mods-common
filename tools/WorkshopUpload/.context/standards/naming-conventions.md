---
doc_id: doc-af4f639c22ff
description: "WorkshopUpload (the legacy Steam Workshop publisher): command, option, method, constant and Steam Cloud file naming"
applies_to:
  - "*.cs"
  - "*.csproj"
---
# WorkshopUpload naming conventions

## Files and Directories
**Id:** blk-d3e770073713

- Folder, project file and root namespace share one name: `WorkshopUpload` (`WorkshopUpload.csproj`, `namespace WorkshopUpload;`).
- One source file, `Program.cs`. Steam files keep Valve's names: `steam_appid.txt`, `steam_api64.dll`.

## Code Naming
**Id:** blk-aee6995bc1bd

### Methods
**Id:** blk-38dfe1c2fab0

- PascalCase. A command is a method named for it with an initial capital: `info` is `Info`, `description` is `Description`, `list` is `List`, `update` is `Update`, `publish` is `Publish`, `cloud` is `Cloud`, `download` is `Download`. Each takes the parsed arguments (`ulong id`, or `string[] args`) and returns the exit code.
- Helpers are verbs: `UploadToCloud`, `ParseTags`, `ParseVisibility`, `ParseOptions`, `Check`, `Await<T>`.
- All methods are `private static`; the class is `internal static class Program`.

### Constants
**Id:** blk-4c50c395882e

- PascalCase `private const`: `AppId` (457140, the game) and `UploaderAppId` (636750, Klei's uploader tool).

### Variables
**Id:** blk-85ee488abfcb

- camelCase: `cloudZip`, `cloudPreview`, `zipPath`, `previewPath`, `descFile`, `tagList`.
- Option values are looked up in a `Dictionary<string, string>` called `opts`.
- Steam handle and result types keep Steamworks naming, including its Hungarian-style fields (`details.m_rgchTitle`, `page.m_nResultsReturned`, `result.m_eResult`).
- Item ids are `ulong id` and wrapped as `new PublishedFileId_t(id)`.

## Project-Specific Conventions
**Id:** blk-885f77fd96da

### Command line
**Id:** blk-d4922475357d

- A command is the first argument, lower-case: `info`, `description`, `list`, `update`, `publish`, `cloud`, `download`.
- Options are `--kebab-case` pairs of name and value: `--title`, `--description-file`, `--changenote`, `--visibility`, `--tags`. `ParseOptions` stores them under the name without the dashes, so an option the code reads is spelled exactly as it is typed.
- Visibility words: `public`, `friends`, `unlisted`, `private` (anything else is private).
- Tags are one comma-separated string using the Workshop's own tag names (`tweaks`, `Base Game`, `Spaced Out!`).

### Steam Cloud names
**Id:** blk-dc54fea8c769

- The zip is always `mod_publish_data_file.zip` and a preview is `mod_publish_preview` plus the preview's lower-cased extension. These are the names Klei's own uploader uses; the extension lets Steam treat a GIF as animated.

### Output
**Id:** blk-7d64c1088dd3

- `label:  value` lines padded to align (`result:   `, `title:    `, `file:     `) in `Info`; `Description`, `Cloud` and `Download` use their own short labels.

## Related
**Id:** blk-94be299675d2

- See also: [code style](code-style.md), [glossary](../domains/glossary.md)
