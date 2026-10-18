# Unit 07 report: change-sets for the unit 06 review findings

Fourteen change-set files in `C:/dev/ONI-Mods/common/.context/tasks/OM-005-common-backfills/changesets/`, built by `scratchpad/fix07/build.mjs`, which copies each rewritten section body from the document on disk and edits it, failing if the expected text is not there exactly once. Nothing was applied and nothing was committed. Every file plans as accepted, except `fix07-TestRunner.json`. That one is refused only because `in-game-testing` is not minted yet.

Two engine rules forced a second file for four nodes (see Friction 1). A `describe` and a document-level tag write on the same document both replace its frontmatter. A section rewrite and a tag removal on the same section both replace that section. One change-set may replace each region only once. So `fix07-b-<name>.json` holds the descriptions and the file-map rewrites for ModCheck, StringsTemplate, WorkshopUpload and WorldgenDriver.

## Files

| File | Node | Findings |
|------|------|----------|
| `fix07-common.json` | `.` | 1.2, 1.3 |
| `fix07-tools.json` | `tools` | 1.1, 1.4, 2.1 |
| `fix07-ModCheck.json` | `tools/ModCheck` | 3.1, 4.3, 4.9 |
| `fix07-b-ModCheck.json` | `tools/ModCheck` | 2.4 |
| `fix07-SaveRunner.json` | `tools/SaveRunner` | 2.2, 4.3, 4.7, 4.9 |
| `fix07-mint-TestRunner.json` | `tools/TestRunner` | 3.1 (mint only) |
| `fix07-TestRunner.json` | `tools/TestRunner` | 3.1, 4.1, 4.6, 4.8, 4.9, 4.10, 1.7, 1.8 |
| `fix07-rmterm-TestRunner.json` | `tools/TestRunner` | 4.1 (term removal) |
| `fix07-StringsTemplate.json` | `tools/Translations/StringsTemplate` | 4.3, 4.4, 4.5, 1.6 |
| `fix07-b-StringsTemplate.json` | `tools/Translations/StringsTemplate` | 1.7, 2.3 |
| `fix07-WorkshopUpload.json` | `tools/WorkshopUpload` | 4.3, 4.4, 4.5, 4.9, 1.5 |
| `fix07-b-WorkshopUpload.json` | `tools/WorkshopUpload` | 1.7, 2.3 |
| `fix07-WorldgenDriver.json` | `tools/WorldgenDriver` | 3.1, 4.3 |
| `fix07-b-WorldgenDriver.json` | `tools/WorldgenDriver` | 1.7, 2.3 |

## Per finding

### Fixes

- **1.1** `tools/.context/overview.md`, "What this is" and "Where things are" rewritten so every path is store-relative: `tools/Kanim/kanim.py`, `tools/MakePreview/make_preview.py`, `tools/ModTest/Test-OniMods.ps1`, `tools/Release/release.py`, `tools/TestRunner/Invoke-OniTests.ps1`, `tools/requirements.txt`, and so on. Every named file was checked with `git ls-files`. The `.gitattributes` line now reads "line-ending and merge rules for this node's `.context/` files". The old text said "the `.context/` files below", but the patterns in `tools/.gitattributes` contain a slash, so they are anchored to `tools/` and do not reach the child nodes. (`fix07-tools.json`)
- **2.1** The `tools::overview.md` description now reads: "What the shared tools folder holds and which tools call others, one entry per tool folder from tools/Kanim to tools/WorldgenDriver, then tools/requirements.txt and tools/.gitattributes, and which six folders are child nodes". (`fix07-tools.json`)
- **3.1** `in-game-testing` is minted in `tools/TestRunner/.context/tags.md`. Its gloss is copied by the build script from `C:/dev/ONI-Mods/.context/tags.md`, word for word: "A record earns this term when it is about testing a mod inside the running game with TestRunner and its [OniTest] coroutines." The mint is the only op in `fix07-mint-TestRunner.json`. Each whole-tool term is now set at document level on overview, domains/README, domains/entities and domains/glossary:
  - TestRunner `in-game-testing`: added to all four (`fix07-TestRunner.json`).
  - ModCheck `game-update-compatibility`: added to domains/README and glossary. It is removed from the glossary sections "VTable setup failure" (blk-7d3bfdfef868) and "Missing member" (blk-cdaff890db76) in the same change-set. Overview and entities already carried it.
  - WorldgenDriver `generated-worlds`: added to domains/README and glossary. It is removed from glossary "Terms to Avoid" (blk-42c5bf5087eb). Overview and entities already carried it.
  - StringsTemplate `translations` and WorkshopUpload `steam-workshop`: all four documents already carried the term at document level, and no section of those four documents carries it. No change was needed.
- **4.1** TestRunner:
  - `screenshots` is taken off Results (blk-982a2140c0af).
  - `player-log` is taken off Launcher Exit Codes (blk-8aed4db67098).
  - Both terms are removed from `tags.md` in `fix07-rmterm-TestRunner.json`.
  - **mod-entry-point is kept**, with three carriers across three documents:
    - TestRunnerMod (blk-12154230c41b), the existing carrier.
    - Code Naming (blk-81d3dab6af5e), whose bullet "The mod class is `TestRunnerMod : UserMod2`" matches SaveRunner's Code Naming and WorldgenDriver's Classes.
    - Handling Patterns/Crash dialog (blk-ed7fda19418a), which describes what `OnLoad` sets when the game loads the mod. `Mod.cs:28-29` confirms it.
- **4.3** Every tag is removed from the five file-map blocks: ModCheck blk-1d0e91d4e985, SaveRunner blk-3dee2f4998e2, StringsTemplate blk-04261b4be94e, WorkshopUpload blk-6dcf139ddfa3 and WorldgenDriver blk-ebff91fa90b7. Each plan reports "no tags left; the field was removed".
- **4.4**
  - StringsTemplate `standards/error-handling.md`: the document tag is now `exception-handling` only. "Exit codes" (blk-4943aa3d2f64) gets `exit-codes`. "Report keywords" (blk-4e29c6e83e7b) gets `console-output`. "Anti-Patterns to Avoid" (blk-8314ecaf5051) gets both, because two of its three bullets are about printing and counting problems and about the exit code.
  - WorkshopUpload `standards/error-handling.md`: the document tag is now `exception-handling` only. `exit-codes` goes on "Exit codes" (blk-f5665fc1a217) and "Principles" (blk-57f9d76b2323).
  - Both nodes' `standards/README.md` are now tagged `code-style, exception-handling`.
- **4.5** `translation-template` (StringsTemplate) and `legacy-workshop-item` (WorkshopUpload) are taken off the overview's document level and put on "What this is" (blk-e125ed2b68c6, blk-846080fa7238). This is done through the `tags` field of the section rewrites that also carry 1.6 and 1.5. The document-level removal comes first in each file.
- **4.6** Covered by 3.1. WorldgenDriver and TestRunner `domains/README.md` now carry their node's whole-tool term.
- **4.7** `mod-entry-point` is removed from SaveRunner "Language and Toolchain" (blk-545208d8763a). `mod-build` stays.

### Nits

- **1.2** The `.gitattributes` bullet in the common overview now reads: "LF line endings for the `.context/` files and for ICON's agent instructions, commands, skills, hooks and plugin files, union merges for the retrospective, ledger and friction logs, and binary image, document, archive and font types". This was checked against `common/.gitattributes`. (`fix07-common.json`)
- **1.3** The `.gitignore` bullet now reads: "build and editor output (`bin/`, `obj/`, `.vs/`, `*.user`, `__pycache__/`), the `runs/` folders ... and the Graystroke font". (`fix07-common.json`)
- **1.4** Two links added to "Some call others" in the tools overview, both checked against the code:
  - `tools/Release/release_mods.py` runs `tools/Release/release.py` for each mod it releases (`release_mods.py:43,188`).
  - `tools/ModTest/Test-OniMods.ps1` runs the `download` command of `tools/WorkshopUpload/` for a `-Steam` item the game has not registered yet (`Test-OniMods.ps1:160-166`).
- **1.5** WorkshopUpload "What this is" gains: "`download` runs as the game (457140) through the `SteamAppId` and `SteamGameId` environment variables, which `tools/ModTest/Test-OniMods.ps1` sets". This was checked against the header of `Program.cs` and `Test-OniMods.ps1:165`.
- **1.6** StringsTemplate "What this is" now says the tool "exits 1 on a `FAIL` (no `ModStrings` class, no `LocString` fields, an unknown option key, a po that does not parse) or when a po has missing, stale or refused entries, and 2 for a usage error". This was checked against `Program.cs:35,52-62,144,164-165`.
- **1.7** The line "`.gitattributes`: line-ending and merge rules for this node's `.context/` files." is added to "Where things are" in TestRunner (`fix07-TestRunner.json`) and in StringsTemplate, WorkshopUpload and WorldgenDriver (`fix07-b-*.json`). All four files exist in git.
- **1.8** The glossary line in TestRunner `domains/README.md` now starts: "the abbreviations ONI, SO, HUD and DLL, and the terms a newcomer needs: ...".
- **2.2** The SaveRunner overview description now says "load and run a player's save" in place of "replay".
- **2.3** The six index descriptions are rewritten in the "X standards index / X domains index: links to ..., from A to B" form. Each one names topics from its own README lines:
  - StringsTemplate standards: "from formatting and the single Program.cs to the report keywords, translation keys and command line".
  - StringsTemplate domains: "from Program.Main, Collect, WritePot and the po checks to ModStrings, keys, pot and po and the terms to avoid".
  - WorkshopUpload standards: "from formatting and Steamworks.NET to exit codes, Steam call results, the legacy-API rule and Steam Cloud file names".
  - WorkshopUpload domains: "from Program.Main and the seven commands to legacy items, the uploader and consumer apps, Steam Cloud staging and visibility".
  - WorldgenDriver standards: "from formatting and patch guards to result statuses, launcher recovery, command-line options and coordinates".
  - WorldgenDriver domains: "from WorldgenDriverMod, Driver, the seven patches and the launcher to coordinates, game modes, sessions and the watchdog".
- **2.4** The ModCheck glossary description now reads: "ModCheck glossary: the abbreviations ONI, JIT, CLR, ECall and DIM with the context of each, and definitions of VTable setup failure, missing member, managed directory and warning, each with where it is used". (`fix07-b-ModCheck.json`)
- **4.8** TestRunner TestBehaviour (blk-49587b759cd2): `logging` is replaced by `test-assertions`.
- **4.9** Dropped:
  - ModCheck Comments blk-9bbfb0a9ecda: `console-output`.
  - ModCheck Exit Codes blk-615b71d8cae1: `command-line`.
  - SaveRunner Comments blk-9fd4a51454ee: `command-line`.
  - WorkshopUpload Exceptions blk-07c96f923cc0: `command-line`.
  - TestRunner Abbreviations blk-f88dca2a1b27: `game-mode`.
- **4.10** TestRunner error-handling Principles (blk-23a64c67c430) gets `save-files`.

### Side effects for the manager to weigh (4.2 is out of scope, so not changed)

Removing tags under 4.3 and 4.7 takes these terms under the 3-record floor. The counts come from parsing the `**Tags:**` and frontmatter lines.

- WorldgenDriver `mod-build`: from 2 carriers to 1. Only code-style "Language and Toolchain" is left.
- WorldgenDriver `mod-entry-point`: from 3 to 2 (WorldgenDriverMod, naming "Classes").
- SaveRunner `mod-build`: from 3 to 2.
- ModCheck `tool-build`: from 3 to 2.
- WorkshopUpload `tool-build`: from 3 to 2.

These terms stay at or above the floor: SaveRunner `mod-entry-point` (3), StringsTemplate `tool-build` (3), TestRunner `mod-entry-point` (3), `logging` (4) and `game-mode` (4).

## Apply order

1. Apply these in any order. None carries a base digest, so each is planned against the file as it is when applied: `fix07-common`, `fix07-tools`, `fix07-ModCheck`, `fix07-b-ModCheck`, `fix07-SaveRunner`, `fix07-StringsTemplate`, `fix07-b-StringsTemplate`, `fix07-WorkshopUpload`, `fix07-b-WorkshopUpload`, `fix07-WorldgenDriver`, `fix07-b-WorldgenDriver`.
2. TestRunner, in this order:
   1. `fix07-mint-TestRunner`.
   2. Plan `fix07-TestRunner` again, then apply it.
   3. Plan `fix07-rmterm-TestRunner` again and apply it with the `--confirm` digest that new plan prints. The removal is protected, and the digest printed today (sha256:eaff0047...) goes stale once the mint changes `tags.md`.

## Plan summaries

All runs were `node c:/dev/ai/icon/bin/ks.mjs write plan --changeset <file> --at common`, from `C:/dev/ONI-Mods`.

- `fix07-common`: accepted. `overview.md`, "Where things are" updated (2 lines replaced).
- `fix07-tools`: accepted. `overview.md` described; "What this is" updated; "Where things are" updated (13 lines replaced).
- `fix07-ModCheck`: accepted.
  - glossary "VTable setup failure": `type-load-check`; "Missing member": `jit-check`.
  - glossary document: `game-update-compatibility`.
  - domains/README: `type-load-check, jit-check, game-update-compatibility`.
  - overview "Where things are": no tags left.
  - code-style "Comments": `runtime-mismatch`.
  - error-handling "Exit Codes": `exit-codes`.
- `fix07-b-ModCheck`: accepted. `domains/glossary.md` described.
- `fix07-SaveRunner`: accepted.
  - `overview.md` described; "Where things are": no tags left.
  - code-style "Language and Toolchain": `mod-build`.
  - code-style "Comments": no tags left.
- `fix07-mint-TestRunner`: accepted. `tags.md`, `in-game-testing` created. The plan warns that the term shares a token with `game-launch`, `game-mode` and `game-update-compatibility` and so "answers for 15 record(s) today".
- `fix07-TestRunner`: **refused, for this reason only**: ops[0], [2], [4] and [6] add `in-game-testing`, which no `tags.md` at the node declares yet. Every other op was reported as it would land:
  - overview "Where things are" +1 line; domains/README "Domains" line replaced.
  - TestBehaviour: `watchdog, test-cleanup, test-assertions`.
  - glossary Abbreviations: no tags left.
  - naming "Results": `run-result`.
  - naming "Code Naming": `harmony-patching, reflection, test-discovery, test-assertions, mod-entry-point`.
  - error-handling "Launcher Exit Codes": `run-result, game-launch, mod-build`.
  - error-handling "Principles": `run-result, logging, mod-loadout, game-update-compatibility, save-files`.
  - error-handling "Crash dialog": `crash-dialog, mod-entry-point`.
- `fix07-rmterm-TestRunner`: accepted, protected. `tags.md`: `screenshots` and `player-log` removed. It needs `--confirm`.
- `fix07-StringsTemplate`: accepted.
  - overview document: `translations`.
  - "What this is" updated; its tags are `mod-strings, command-line, exit-codes, mod-options, translation-template`.
  - "Where things are": no tags left.
  - error-handling document: `exception-handling`.
  - "Exit codes": `exit-codes`.
  - "Report keywords": `translations, refused-translation, console-output`.
  - "Anti-Patterns to Avoid": `refused-translation, translations, exit-codes, console-output`.
  - standards/README: `code-style, exception-handling`.
- `fix07-b-StringsTemplate`: accepted. overview "Where things are" +1 line; standards/README and domains/README described.
- `fix07-WorkshopUpload`: accepted.
  - overview document: `steam-workshop`.
  - "What this is" updated; its tags are `steam-app-id, steamworks-api, release-process, item-download, legacy-workshop-item`.
  - "Where things are": no tags left.
  - error-handling document: `exception-handling`.
  - "Exit codes": `console-output, item-download, legacy-workshop-item, exit-codes`.
  - "Principles": `steamworks-api, console-output, command-line, exit-codes`.
  - "Exceptions": no tags left.
  - standards/README: `code-style, exception-handling`.
- `fix07-b-WorkshopUpload`: accepted. overview "Where things are" +1 line; standards/README and domains/README described.
- `fix07-WorldgenDriver`: accepted.
  - glossary "Terms to Avoid": `world-coordinate, game-launch`.
  - glossary document: `generated-worlds`.
  - domains/README: `generated-worlds`.
  - overview "Where things are": no tags left.
- `fix07-b-WorldgenDriver`: accepted. overview "Where things are" +1 line; standards/README and domains/README described.

## Brief feedback (handoff section 2)

- **Did not apply:** nothing. Every pointer shown was a StringsTemplate, WorkshopUpload, ModCheck, WorldgenDriver or SaveRunner record in scope.
- **Missing:** the gloss of `in-game-testing` lives in `C:/dev/ONI-Mods/.context/tags.md`, outside common's store boundary, so the brief could not show it. I read it from disk. `tools:overview.md`, the document 1.1 is about, did not appear in the listed part either. That is the unreached state that 1.1 fixes.
- **Pointer reasons:** each one I opened matched its record. For example, blk-04261b4be94e names `tools/Translations/translations.py`.

## Friction

1. `ks write plan` refused a `describe` and a document-level `tag` op on the same document: "ops[4] and ops[2] both replace the frontmatter of domains/glossary.md; one change-set replaces each region once". It also refused a `section` rewrite and a `tag` removal on the same section: "ops[3] and ops[2] both replace the "Where things are" section". I expected both pairs to compose, because a describe touches only the `description:` line, a tag write touches only `tags`, and the section op carries tags across. The section op's `tags` field can add terms but not remove them, so no single op can rewrite a body and drop a tag. What I did: moved the describes and file-map rewrites into `fix07-b-<name>.json`, which breaks the one-change-set-per-node rule for four nodes.
2. A plan computed for a protected removal goes stale whenever an earlier change-set in the sequence touches the same file. Here the mint and the term removal both edit TestRunner `tags.md`, so the manager has to plan `fix07-rmterm-TestRunner` again after the mint. Nothing in the plan output says the digest depends on an unapplied sibling change-set.
3. The mint plan says `in-game-testing` "answers for 15 record(s) today" through the shared tokens `game-*`. This is the same token-family effect as review 3.3. With the gloss fixed word for word, there was no respelling to choose.
