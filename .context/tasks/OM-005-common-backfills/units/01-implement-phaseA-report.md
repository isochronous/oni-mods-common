# Unit 01 report: overviews and README indexes for `.`, `tools`, `tools/ModCheck`, `tools/SaveRunner`

All four change-sets are drafted, and `write plan --at common` accepts each one. Nothing was applied or committed.

Change-sets (in `C:/dev/ONI-Mods/common/.context/tasks/OM-005-common-backfills/changesets/`):
`phaseA-common.json`, `phaseA-tools.json`, `phaseA-ModCheck.json`, `phaseA-SaveRunner.json`.
They were built by `C:\Users\thegr\AppData\Local\Temp\claude\C--dev-ONI-Mods\16b1eb3e-feed-47e8-a9b9-623a69a610b9\scratchpad\om005-u01\build.py`. Every operation is a `section` write. None touches descriptions, frontmatter, META.md, projects.md or iconrc.json.

## Per node

- `.` (common): What this is says common is oni-mods-common, the shared build configuration and tooling, included in ONI-Mods as the `common/` submodule and imported through `$(OniCommon)` from `Directory.Build.props`. It names what centralising buys after a game update. Where things are covers `oni-mod.props`, `oni-mod.targets`, `ilrepack-plib.targets`, `src/Translations.cs`, `tools/`, `README.md`, `.gitignore` and `.gitattributes`, plus the node's `decisions/`, `retrospectives/` and `projects.md`.
- `tools`: What this is describes the shared tools and dev mods and who runs them. It names the calls between tools: release.py runs WorkshopUpload, Invoke-OniTests.ps1 runs ModCheck, translations.py runs StringsTemplate. Where things are gives one line each for Kanim, MakePreview, ModCheck, ModTest, Release, SaveRunner, TestRunner, Translations (translations.py and StringsTemplate), WorkshopUpload, WorldgenDriver, `requirements.txt` and `.gitattributes`. It names the six folders that are nodes and the five that are not.
- `tools/ModCheck`: the overview covers what the tool checks, its exit codes, its caller in TestRunner and the command line. It names the files `Program.cs`, `ModCheck.csproj`, `.gitattributes` and `bin/`/`obj/`. The domains index lists entities and glossary. The standards index lists code-style, error-handling and naming-conventions.
- `tools/SaveRunner`: the overview covers what the dev mod and launcher do, the with/without comparison, that the mod stays inert without `--saverun`, version 0.1.0 and the Mod Profile Manager exclusion. It names `Mod.cs`, `Runner.cs`, `Patches.cs`, `RunBehaviour.cs`, `Invoke-SaveRun.ps1`, `SaveRunner.csproj`, the yamls, `README.md`, `runs/` (made by the launcher, ignored) and `.gitattributes`. The domains and standards indexes follow the same pattern as ModCheck.

Every file and folder named was checked with `ls`.

## TODO(owner)s left

None. The repo answered everything these sections ask.

## Plan summaries (`node c:/dev/ai/icon/bin/ks.mjs write plan --changeset <file> --at common`, from C:/dev/ONI-Mods)

```
phaseA-common.json      node C:/dev/ONI-Mods/common [aggregator]; update .context/overview.md
  narrative  overview.md — What this is updated; 1 line(s)/92 byte(s) removed, +1 line(s)/+769 byte(s) added
  narrative  overview.md — Where things are updated; 1 line(s)/59 byte(s) removed, +12 line(s)/+1636 byte(s) added
phaseA-tools.json       node C:/dev/ONI-Mods/common/tools [aggregator]; update .context/overview.md
  narrative  overview.md — What this is updated; 1 line(s)/92 byte(s) removed, +1 line(s)/+617 byte(s) added
  narrative  overview.md — Where things are updated; 1 line(s)/59 byte(s) removed, +18 line(s)/+2174 byte(s) added
phaseA-ModCheck.json    node C:/dev/ONI-Mods/common/tools/ModCheck [leaf]; update overview.md, domains/README.md, standards/README.md
  narrative  overview.md — What this is updated; 1 line(s)/92 byte(s) removed, +1 line(s)/+587 byte(s) added
  narrative  overview.md — Where things are updated; 1 line(s)/59 byte(s) removed, +8 line(s)/+634 byte(s) added
  narrative  domains/README.md — Domains updated; 1 line(s)/87 byte(s) removed, +4 line(s)/+344 byte(s) added
  narrative  standards/README.md — Standards updated; 1 line(s)/97 byte(s) removed, +5 line(s)/+483 byte(s) added
phaseA-SaveRunner.json  node C:/dev/ONI-Mods/common/tools/SaveRunner [leaf]; update overview.md, domains/README.md, standards/README.md
  narrative  overview.md — What this is updated; 1 line(s)/92 byte(s) removed, +1 line(s)/+748 byte(s) added
  narrative  overview.md — Where things are updated; 1 line(s)/59 byte(s) removed, +14 line(s)/+1567 byte(s) added
  narrative  domains/README.md — Domains updated; 1 line(s)/87 byte(s) removed, +4 line(s)/+403 byte(s) added
  narrative  standards/README.md — Standards updated; 1 line(s)/97 byte(s) removed, +5 line(s)/+551 byte(s) added
```

Each plan ended with "plan only — nothing was written" and exit 0. None was refused.

## Friction met

- `ks write schema --row narrative --mode section`: the `target` description lists `domains/**`, `standards/**`, `architecture/**`, `testing/**`, `styling/**`, `workflows/**` and `<team-home>/**` as narrative targets. It does not list `overview.md`, which also goes through the section write. I expected the schema to name `overview.md`. I tried it anyway, and the plan accepted it.

## Brief feedback (handoff section 2)

- Pointers that did not apply: the StringsTemplate, WorkshopUpload and WorldgenDriver records matched `.` and `tools` only because they name tool paths. They helped confirm one-line purposes for the tools overview but do not govern this work.
- Missing: the house-style examples, the OM-002 bitshifter overview and READMEs and `mods/.context/overview.md`, are in the ONI-Mods store, not common, so the brief could not reach them. `common-rules.md` carried them instead.
- Pointer reasons that did not match their records: none.
