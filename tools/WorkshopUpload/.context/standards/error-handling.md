---
doc_id: doc-42a5e5cee89e
description: "WorkshopUpload (the legacy Steam Workshop publisher): exit codes 0/1/2, Steam result checks, Await timeouts, Check and UploadToCloud throws, and the legacy-API-only rule"
applies_to:
  - "*.cs"
  - "*.csproj"
---
# WorkshopUpload error handling

WorkshopUpload is a console tool run by a person or by `tools/Release/release.py`. Its error handling is its exit code and what it prints. A failed Steam call must stop the run: a half-applied Workshop update is worse than none.

## Principles
**Id:** blk-57f9d76b2323

1. **Check every Steam result.** Each call-result is awaited and its `EResult` compared with `k_EResultOK`; anything else prints the result and returns 1.
2. **Return 2 for a usage error.** No arguments prints the usage and returns 2 before `SteamAPI.Init`. Any other usage error (an unknown command, `update` or `publish` with too few arguments, a missing `--title`) returns 2 inside the `try`, after Steam is initialised, so the `finally` shuts Steam down.
3. **Always shut Steam down.** `SteamAPI.Shutdown()` is in a `finally` around the dispatch.
4. **Say what the user needs to do.** Messages name what to check: Steam running and logged in, `steam_appid.txt` next to the exe, cloud quota, the Workshop legal agreement.

## Exit codes
**Id:** blk-f5665fc1a217

| Code | Meaning | Where |
|------|---------|-------|
| 0 | the command succeeded | each command method |
| 1 | `SteamAPI.Init` failed; a Steam result was not OK (`Info`, `Description`, `List`, `Update`, `Publish`); or `Download` could not leave the item installed as a file (`DownloadItem` refused, not installed, install path is a directory or missing) | `Main`, `Info`, `Description`, `List`, `Update`, `Publish`, `Download` |
| 2 | usage error: no command, unknown command, `update` or `publish` with too few arguments, `--title` missing on publish | `Main`, `Update`, `Publish` |

`Info` prints `NOT a legacy single-file item` for an item with no file handle and still returns 0 when the Steam result is OK. Usage text, the `SteamAPI.Init` failure and the failure messages of `Description`, `List` and `Download`'s `DownloadItem` call go to standard error (`Console.Error`). The result line of `update` and `publish`, which carries a failed Steam result, and `Download`'s not-installed and bad-path lines go to standard output with the progress output.

## Handling Patterns
**Id:** blk-df99a0cd7953

### Steam initialisation
**Id:** blk-d559ab194fa5

```csharp
if (!SteamAPI.Init())
{
	Console.Error.WriteLine("SteamAPI.Init failed. Is the Steam client running and logged in? Is steam_appid.txt next to the exe?");
	return 1;
}
```

### Steam call results
**Id:** blk-f4c749372262

`Await<T>` pumps `SteamAPI.RunCallbacks()` every 50 ms until the result arrives. It throws `TimeoutException` after 120 s and `Exception("Steam call I/O failure: ...")` when Steam reports an I/O failure.

### Update calls
**Id:** blk-f9f9642e21b3

`Check(bool ok, string what)` throws `Exception(what + " returned false")` when an `UpdatePublishedFile*` call refuses its input. `UploadToCloud` throws when `FileWrite` fails (cloud quota, or Steam Cloud disabled for the app). A failed `FileShare` only prints a warning, and the result of the later publish decides.

### Warnings that do not stop the run
**Id:** blk-7b957b93956e

`publish` without `--tags` prints a warning, because untagged items do not show up in the Workshop's browse views.

### Exceptions
**Id:** blk-07c96f923cc0

There is no `catch` in the tool. A thrown exception ends the process with the .NET unhandled-exception output and a non-zero code, after `finally` has shut Steam down. `info`, `description` and `download` read `args[1]` without a length check, so a missing id ends that way rather than with a usage line. TODO(owner): add argument checks to those three commands so they return 2 with a usage line.

## Rules the code enforces
**Id:** blk-3df32ebac5d5

- Publishing and updating use only the legacy `ISteamRemoteStorage` API. An item touched through `ISteamUGC` item update stops being downloadable by the game, permanently. `SteamUGC` appears only in `Download`, which reads and downloads and never writes.
- `Info` reports `NOT a legacy single-file item` for an item whose `m_hFile` is invalid: the game cannot download it. `Download` reports a directory install path as a failure ("Steam failed to download the mod").

## Anti-Patterns to Avoid
**Id:** blk-1fa28db3e3f8

- Ignoring an `EResult` and printing success.
- Adding a modern `ISteamUGC` update path for publish or update.
- Printing secrets: the tool prints the Steam persona name and id and item ids only; nothing else from the account.

## Related
**Id:** blk-3361ff0c531c

- See also: [code style](code-style.md), [entities](../domains/entities.md)
