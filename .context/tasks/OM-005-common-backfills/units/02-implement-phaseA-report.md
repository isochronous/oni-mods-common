# Unit 02 report: overviews and README indexes for TestRunner, StringsTemplate, WorkshopUpload, WorldgenDriver

Four change-sets drafted, each accepted by `write plan --at common` on the first run. Nothing applied, nothing committed.

Change-sets (in `C:/dev/ONI-Mods/common/.context/tasks/OM-005-common-backfills/changesets/`):

- `phaseA-TestRunner.json` (node `tools/TestRunner`)
- `phaseA-StringsTemplate.json` (node `tools/Translations/StringsTemplate`)
- `phaseA-WorkshopUpload.json` (node `tools/WorkshopUpload`)
- `phaseA-WorldgenDriver.json` (node `tools/WorldgenDriver`)

Each carries four `section` operations: `overview.md` "What this is" and "Where things are", `domains/README.md` "Domains", `standards/README.md` "Standards". No descriptions, frontmatter, tags or other documents touched. Builder script: `C:/Users/thegr/AppData/Local/Temp/claude/C--dev-ONI-Mods/16b1eb3e-feed-47e8-a9b9-623a69a610b9/scratchpad/om005-u02/build.py`.

## Per node

- **tools/TestRunner**: what the harness mod and the `Invoke-OniTests.ps1` launcher do and who runs them (every mod's `src/<ModName>.Tests/` project); a map of every top-level file, `Kit/`, `fixtures/` and `runs/`. The fixture line says `testbed-so.sav` is the only test save tracked in git (checked with `git ls-files tools/TestRunner/fixtures`) and that `testbed-base.sav` is not in the repo. Indexes link entities, glossary, code-style, error-handling, naming-conventions.
- **tools/Translations/StringsTemplate**: what the template writer and checker does, its command line and exit codes, and that it runs through `tools/Translations/translations.py` after string changes and before a release; a map of `Program.cs`, the csproj and its Release output path, the runner one folder up, and the Translations section of `common/README.md`. Indexes as above.
- **tools/WorkshopUpload**: the legacy `ISteamRemoteStorage` publisher, app ids 636750/457140, the seven commands, and its two callers (`tools/Release/release.py`, `tools/ModTest/Test-OniMods.ps1` for `download`); a map of `Program.cs` with its real helper names, the csproj, `steam_appid.txt`, `steam_api64.dll`, and the Workshop section of `common/README.md`. Indexes as above.
- **tools/WorldgenDriver**: the worldgen mod and `Invoke-Worldgen.ps1` launcher, sessions of up to 25 coordinates, restores of `kplayerprefs.yaml` and `mods.json`, and its use for worldgen bug reports and mod-build checks; a map of `Mod.cs`, the seven patches in `Patches.cs`, `Driver.cs`, the launcher, csproj/yamls, `README.md`, and the git-ignored `runs/` output (absent in this checkout, the first run creates it). Indexes as above.

## TODO(owner)s left

None. Every fact came from the code, csproj, yamls, READMEs, `.gitignore`, `git ls-files` or the nodes' own documents.

## Plan summaries

All four: exit 0, store `C:/dev/ONI-Mods/common`, `[leaf]` node, branch `OM-005-common-backfills`, three `update`s by the narrative writer of record, no protected operations, "plan only — nothing was written".

```
TestRunner
  narrative  overview.md — What this is updated; 1 line(s)/92 byte(s) removed, +1 line(s)/+1090 byte(s) added
  narrative  overview.md — Where things are updated; 1 line(s)/59 byte(s) removed, +18 line(s)/+2224 byte(s) added
  narrative  domains/README.md — Domains updated; 1 line(s)/87 byte(s) removed, +4 line(s)/+470 byte(s) added
  narrative  standards/README.md — Standards updated; 1 line(s)/97 byte(s) removed, +5 line(s)/+664 byte(s) added
StringsTemplate
  narrative  overview.md — What this is updated; 1 line(s)/92 byte(s) removed, +1 line(s)/+1117 byte(s) added
  narrative  overview.md — Where things are updated; 1 line(s)/59 byte(s) removed, +8 line(s)/+934 byte(s) added
  narrative  domains/README.md — Domains updated; 1 line(s)/87 byte(s) removed, +4 line(s)/+418 byte(s) added
  narrative  standards/README.md — Standards updated; 1 line(s)/97 byte(s) removed, +5 line(s)/+559 byte(s) added
WorkshopUpload
  narrative  overview.md — What this is updated; 1 line(s)/92 byte(s) removed, +1 line(s)/+930 byte(s) added
  narrative  overview.md — Where things are updated; 1 line(s)/59 byte(s) removed, +9 line(s)/+1081 byte(s) added
  narrative  domains/README.md — Domains updated; 1 line(s)/87 byte(s) removed, +4 line(s)/+389 byte(s) added
  narrative  standards/README.md — Standards updated; 1 line(s)/97 byte(s) removed, +5 line(s)/+570 byte(s) added
WorldgenDriver
  narrative  overview.md — What this is updated; 1 line(s)/92 byte(s) removed, +1 line(s)/+1022 byte(s) added
  narrative  overview.md — Where things are updated; 1 line(s)/59 byte(s) removed, +11 line(s)/+1158 byte(s) added
  narrative  domains/README.md — Domains updated; 1 line(s)/87 byte(s) removed, +4 line(s)/+445 byte(s) added
  narrative  standards/README.md — Standards updated; 1 line(s)/97 byte(s) removed, +5 line(s)/+564 byte(s) added
```

The plans were computed against these digests (no `base` given): TestRunner overview `sha256:8991aa63…`, StringsTemplate overview `sha256:97d9b2b2…`, WorkshopUpload overview `sha256:4a94f6f6…`, WorldgenDriver overview `sha256:ce76268d…`. If another unit touches these files before apply, plan again.

## Drift found in the repo (not fixed, out of scope)

- `tools/TestRunner/fixtures/README.md` says "The `.sav` files are not committed (see `.gitignore`)". That is wrong: `testbed-so.sav` is tracked, and `.gitignore` says the fixture saves are tracked. The README needs a one-line fix.
- `common/README.md` (Publishing to the Steam Workshop) lists a `WorkshopUpload ugc-describe` command that `tools/WorkshopUpload/Program.cs` does not have (its switch has seven commands, no `ugc`).

## Brief feedback (handoff section 2)

- Nothing listed turned out not to apply; every pointer's one-line reason matched its record.
- Missed: the two user-facing guides that govern StringsTemplate and WorkshopUpload, the Translations and Publishing sections of `common/README.md`, are not store records, so the brief could not reach them. The nodes' `entities.md` overviews name them, and that is how they were found.
- The 16 uncovered, untagged records the brief warned about include the very documents this unit rewrites (overview and README indexes). The "What this is" sections were left untagged; the bitshifter overview carries section tags there, so a tag pass may want to add them.

## Friction

- Invocation: `node c:/dev/ai/icon/bin/ks.mjs write schema --row narrative --mode section`. It lists narrative targets as `domains/**, standards/**, architecture/**, testing/**, styling/**, workflows/**, and <team-home>/**`, with no `overview.md`. Expected: the schema to name `overview.md` if the narrative row takes it. What happened: a `section` op on `overview.md` planned fine under "the narrative writer of record". What I did: relied on OM-002's change-sets (which wrote `overview.md` sections) and the plan output. The schema text and the engine disagree on which targets the narrative row accepts.
- Nothing else: all four plans were accepted on the first run.
