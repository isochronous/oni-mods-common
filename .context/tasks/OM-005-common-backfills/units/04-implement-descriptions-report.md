# Unit 04 report: descriptions for 24 documents

Output: `.context/tasks/OM-005-common-backfills/work/desc-answers-04.json`

## Key count check

Ran against the unit spec's key list (`python -I`, the JSON parsed, keys compared with the `- \`<node>::<path>\`` lines in `units/04-implement-descriptions.md`):

```
keys: 24 expected: 24 match: True dupes in spec: 0
per-entry checks done
```

The per-entry checks (15–35 words, no line break, the tool's name in the text) flagged nothing.

## Nulls

None. Every document could be described.

## How the rules were applied

- I read all 24 documents in full from disk.
- Every code name in a description belongs to its own node. I checked them in the source files: `CheckPo`, `ReadPo`, `WritePot`, `Collect`, `CheckOptionKeys`, `Entry`, `FormatException` (StringsTemplate `Program.cs`); `UploadToCloud`, `Await`, `AppId`, `UploaderAppId`, `Info` (WorkshopUpload `Program.cs`); `Driver.Active`, `WriteResult` (WorldgenDriver `Driver.cs`); `WorldgenDriverMod` (WorldgenDriver `Mod.cs`). The files named also exist in their nodes: `steam_appid.txt`, `steam_api64.dll`, `mod_info.yaml`, `README.md`, `Invoke-Worldgen.ps1` and the csproj files.
- Several documents name things outside their own node. Their descriptions leave those names out:
  - `tools/Translations/translations.py`
  - `common/src/Translations.cs`
  - `tools/Release/release.py`
  - `tools/ModTest/Test-OniMods.ps1`
  - `common/README.md`
  - `oni-mod.props` and `oni-mod.targets`
- The WorldgenDriver overview names `runs/`, but that folder is gitignored and is not in this checkout, so its description leaves it out.
- The README index documents and the glossaries are not about any code, so their descriptions name no files.

## Rules that turned out wrong or in tension

- **First paragraph of `overview.md`.** Its first paragraph ("What this is") is already the one-paragraph summary of the tool. A short line about the tool can hardly avoid paraphrasing it. I described what the document holds instead: purpose, command line or commands, who runs it, and the file map.
- **Existing descriptions.** 15 of the 24 documents already had a real description: entities, glossary and the three standards documents in each node. Only 9 still had a `TODO(owner)` placeholder: overview, `domains/README.md` and `standards/README.md` in each node. The unit lists all 24, so I wrote all 24. Applying the whole file replaces those 15 working descriptions. Apply only the 9 placeholder keys if the old ones should stay.
- **Read in full versus retrieval-discipline.** The unit says to read every document in full from disk. The packet's section 2 and the retrieval-discipline skill say to resolve pointers with `ks read` rather than open files. For description writing, the unit is right: a description has to cover the whole document, so I read whole files with `cat`.

## Brief feedback (packet section 2)

- **Did not apply:** nothing. Every pointer shown was a document in scope.
- **Missed:** the three `overview.md` documents appear nowhere in the brief, neither as pointers nor in the uncovered-records warning, though they are 3 of the 24 targets. The description-backfill skill governs this work and does not appear either; the unit restates its rules, so nothing was lost.
- **Mismatched reasons:** none. The "names …" and "linked from …" reasons matched the documents.

## Friction met

- None with `ks`; I did not run it. The only tension is the `ks read` default against the read-in-full instruction, described above.
