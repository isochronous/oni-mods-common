# Unit 08 report: are the unit 06 findings fixed?

Scope: `git diff f1cb73e..HEAD -- . ':!.context/tasks'` in `C:/dev/ONI-Mods/common` (commit 61e463f, 30 files, +75 -82), read against `units/06-review-report.md`, `units/07-implement-review-fixes-report.md`, the plan's Decisions and the code. Nothing was written to the store.

Result: **all 26 findings in scope are resolved.** No block or fix findings are new. Two nits are new (N1, N2). The checks pass: 98 of 98 relative links resolve, no section repeats an inherited tag, every term has a carrier, `ks check --all --strict` is 0 block and 1 advise (the known META.md aggregator advise), and `ks brief` reaches `tools:overview.md`.

## Findings in scope

### Fix findings

| # | State | Evidence |
|---|-------|----------|
| 1.1 | resolved | Every path in "What this is" and "Where things are" is now store-relative, and each one is tracked (`git ls-files`: kanim.py, make_preview.py, title_preview.py, fonts/, Test-OniMods.ps1, disabled.txt, included.txt, release.py, release_mods.py, Invoke-SaveRun.ps1, Invoke-OniTests.ps1, translations.py, Invoke-Worldgen.ps1, requirements.txt, tools/.gitattributes). `ks brief --json` over the eight nodes leaves `tools:overview.md` out of `unreached`. Its two blocks come back as `source-mention` pointers (blk-6c2bbf052519, blk-647210c6a78a). The `tools/.gitattributes` line ("this node's `.context/` files") is correct: the file's patterns contain a slash, so they are anchored to `tools/`. |
| 2.1 | resolved | The description now reads "from tools/Kanim to tools/WorldgenDriver, then tools/requirements.txt and tools/.gitattributes, and which six folders are child nodes", which matches the list order. |
| 3.1 | resolved | The whole-tool term sits at document level on overview, domains/README, domains/entities and domains/glossary in all five nodes: TestRunner in-game-testing, ModCheck game-update-compatibility, StringsTemplate translations, WorkshopUpload steam-workshop and WorldgenDriver generated-worlds. SaveRunner has none, as decided. The in-game-testing gloss in TestRunner matches `C:/dev/ONI-Mods/.context/tags.md` word for word. The section copies that the new document tags would repeat have been removed (ModCheck glossary blk-7d3bfdfef868 and blk-cdaff890db76, WorldgenDriver blk-42c5bf5087eb). |
| 4.1 | resolved | screenshots and player-log are gone from TestRunner's `tags.md` and from blk-982a2140c0af and blk-8aed4db67098. A grep finds no record that still carries either. mod-entry-point now has 3 carriers: TestRunnerMod, Code Naming (line 33: "The mod class is `TestRunnerMod : UserMod2`", matching `Mod.cs:13`) and Crash dialog. The Crash dialog section is about what `OnLoad` does (`Mod.cs:28-29`), so the term fits its gloss. |
| 4.3 | resolved | None of the five file maps carries tags now: blk-1d0e91d4e985, blk-3dee2f4998e2, blk-04261b4be94e, blk-6dcf139ddfa3, blk-ebff91fa90b7. |
| 4.4 | resolved | Both error-handling documents are tagged `exception-handling` only. StringsTemplate: Exit codes has exit-codes. Report keywords adds console-output. Anti-Patterns has both, which fits because its first and third bullets are about printing and about the exit code. WorkshopUpload: Exit codes and Principles have exit-codes; principles 1 and 2 are about returning 1 and 2. Both standards/README files are now `code-style, exception-handling`. |
| 4.5 | resolved | translation-template and legacy-workshop-item are off the overview document tags and on "What this is" (blk-e125ed2b68c6, blk-846080fa7238). |
| 4.6 | resolved | TestRunner domains/README is tagged in-game-testing and WorldgenDriver's is tagged generated-worlds. |
| 4.7 | resolved | SaveRunner blk-545208d8763a is tagged `mod-build` only. |

### Nits

| # | State | Evidence |
|---|-------|----------|
| 1.2 | resolved | The new `.gitattributes` bullet matches `common/.gitattributes`: `.context/**` eol=lf, the ICON agent, command, skill, hook, bin and plugin paths, union merges for retrospectives, ledger and friction log, and `-text` for png/jpg/gif/ico/pdf/zip/gz/woff/woff2. |
| 1.3 | resolved | `bin/`, `obj/`, `.vs/`, `*.user`, `__pycache__/` and the three `runs/` folders match `common/.gitignore`. |
| 1.4 | resolved | `release_mods.py:43` defines `RELEASE = .../release.py` and `:188` runs it. `Test-OniMods.ps1:166` runs `WorkshopUpload ... download $id` for an id it has not found registered. |
| 1.5 | resolved | `Test-OniMods.ps1:165` sets `SteamAppId`/`SteamGameId` to 457140, and the `Program.cs` header says to run `download` that way. |
| 1.6 | resolved | Each cause listed is a `FAIL` that returns 1: `Program.cs:52-53` (no ModStrings), `:61-62` (no LocString fields), `:144` (option key not in ModStrings) and `:164-165` (FormatException from ReadPo). `:35` returns 2 for a usage error. |
| 1.7 | resolved | The `.gitattributes` line is in all six leaf overviews with the same wording, and each node's `.gitattributes` exists and holds `.context/**` rules only. |
| 1.8 | resolved | The TestRunner domains/README glossary line now starts with "the abbreviations ONI, SO, HUD and DLL". |
| 2.2 | resolved | The SaveRunner overview description now reads "load and run a player's save". |
| 2.3 | resolved | The six index descriptions use the "X standards/domains index: ... from A to B" form. Each topic named in them appears in that README's own lines: report keywords, translation keys, command line; Steamworks.NET, Steam call results, legacy-API rule, Steam Cloud file names; patch guards, launcher recovery, coordinates; Collect, WritePot, terms to avoid; seven commands, uploader/consumer apps, visibility; WorldgenDriverMod, Driver, seven patches, watchdog. |
| 2.4 | resolved | The abbreviation table has a Context column, and all four cross-domain terms (VTable setup failure, Missing member, Managed directory, Warning) carry a "Used in" line. |
| 4.8 | resolved | TestBehaviour blk-49587b759cd2 is tagged `watchdog, test-cleanup, test-assertions`. |
| 4.9 | resolved | All five removals landed: blk-9bbfb0a9ecda console-output, blk-615b71d8cae1 command-line, blk-9fd4a51454ee command-line, blk-07c96f923cc0 command-line, blk-f88dca2a1b27 game-mode. |
| 4.10 | resolved | TestRunner Principles blk-23a64c67c430 carries save-files. |

### Manager change after unit 07

Resolved. `mod-build` is off WorldgenDriver code-style "Language and Toolchain" (blk-e794fe66eb73), and its block is removed from WorldgenDriver's `tags.md`. A grep of `tools/WorldgenDriver/.context` finds no record that still carries it.

### Left on purpose (plan Decisions)

3.2 (mod-strings gloss), 3.3 (token families), 4.2 (two-carrier terms), 4.11 (code-style on naming documents) and the whole-tool term rule match the plan. The diff touches none of the first four.

## Checks that the fixes broke nothing

- **Links:** a script walked all 81 tracked `.md` files under the eight `.context/` folders (task folder excluded). 98 relative links, 0 missing.
- **Inherited-tag repeats:** 0. For each `**Tags:**` line, the script compared against the document's frontmatter `tags:` and every enclosing heading's tags.
- **Carriers:** every term in all six `tags.md` has at least one carrier. Counted from frontmatter `tags:` and `**Tags:**` lines, not from `ks tags`. Terms under 3:
  - ModCheck tool-build 2.
  - SaveRunner mod-build 2.
  - TestRunner crash-dialog 2, plus exception-handling 2 (document level on standards/README and error-handling).
  - StringsTemplate exception-handling 2 (document level).
  - WorkshopUpload tool-build 2, exception-handling 2 (document level).
  - WorldgenDriver mod-entry-point 2, player-log 2, worldgen-progress 2, exception-handling 2 (document level).

  No record carries a term its node's dictionary does not declare.
- **`ks check --all --strict --store C:/dev/ONI-Mods/common`:** exit 0. 27 checks, 17 ran, 0 block, 1 advise (`.context/META.md`, the known aggregator advise), 0 inform. All 48 tagged paths name declared terms.
- **`ks brief --at C:/dev/ONI-Mods/common --for <the eight nodes> --budget 40000 --json`:** 48 candidates, 48 shown. `unreached` holds the six leaf `tags.md` files only. `tools:overview.md` is reached.

## New findings

| # | Sev | Location | Finding | Expected fix |
|---|-----|----------|---------|--------------|
| N1 | nit | `plan.md` Decisions, the "Left as they are" line for review 4.2 | Unit 07's 4.3 and 4.7 removals left four terms at 2 carriers, and no decision covers them: ModCheck tool-build (code-style Language and Toolchain, naming Files and Directories), SaveRunner mod-build (the same two sections), WorkshopUpload tool-build (the same two sections) and WorldgenDriver mod-entry-point (entities WorldgenDriverMod, naming Classes). The same line still lists "mod-build in WorldgenDriver", but that term is now retired. | Rewrite the 4.2 decision: drop WorldgenDriver mod-build and add the four terms (each fits both of its records), or extend or retire them. This is task scratch, not the store. |
| N2 | nit | `ks brief` over the eight nodes, `unreached` | The six leaf `tags.md` dictionaries are reported as records with no coverage, no tags and no source path. This is the same shape as the OM-002 precedent: `C:/dev/ONI-Mods/.context/tags.md` and the mod dictionaries carry only a description. The diff did not cause it. | No store change. Dictionaries are read through `ks tags --glosses`. Record the decision to leave them so the next review does not re-raise it. |

Cosmetic only, not a finding: blocks whose tags were removed by a tag op keep two trailing spaces on the `**Id:**` line (ModCheck blk-1d0e91d4e985, SaveRunner blk-3dee2f4998e2 and blk-9fd4a51454ee, TestRunner blk-f88dca2a1b27). Blocks rewritten by a section op lost them (StringsTemplate, WorkshopUpload, WorldgenDriver file maps). Both render the same.

## Brief feedback (handoff section 2)

- **Did not apply:** nothing. Every pointer named a StringsTemplate, ModCheck or WorkshopUpload record in scope.
- **Missing:**
  - The ONI-Mods root `tags.md`. It is needed to confirm the in-game-testing gloss is identical, and it sits outside common's store boundary. I read it from disk.
  - Units 06 and 07 and `plan.md`. They are task scratch, so the brief does not reach them by design.
- **Pointer reasons:** the ones I opened matched their records. blk-04261b4be94e names `tools/Translations/translations.py` in the StringsTemplate file map. blk-846080fa7238 names `tools/ModTest/Test-OniMods.ps1` and `tools/Release/release.py`.

## Friction

1. Invocation: `ks brief --at C:/dev/ONI-Mods/common --for <eight nodes>`, in the handoff and again in this run.
   - What it did: warned that the six `tags.md` records "declare no coverage and no tags and name no source path", with the remedy "Give them `applies_to` or tags through the write path".
   - Expected: no warning. A tag dictionary is reached through `ks tags`. Tagging a dictionary, or giving it `applies_to`, would be the wrong remedy, and the OM-002 dictionaries carry neither.
   - What I did instead: treated the warning as expected (N2) and confirmed the precedent from disk.
2. `ks tags` counts still include records matched through a shared word, as unit 06 reported, so they cannot test the 3-record floor. I counted carriers with a script that parses frontmatter `tags:` and `**Tags:**` lines: `C:\Users\thegr\AppData\Local\Temp\claude\C--dev-ONI-Mods\16b1eb3e-feed-47e8-a9b9-623a69a610b9\scratchpad\u08\check.mjs`.
