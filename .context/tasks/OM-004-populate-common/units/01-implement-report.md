# Unit 01 report: populate-context change-sets for ModCheck, SaveRunner, TestRunner

Nothing applied, nothing committed. Build scripts: `C:/Users/thegr/AppData/Local/Temp/claude/C--dev-ONI-Mods/16b1eb3e-feed-47e8-a9b9-623a69a610b9/scratchpad/om004-u01-build/` (`helper.py`, `modcheck.py`, `saverunner.py`, `testrunner.py`). Each change-set parses as JSON.

## Change-sets (all in `common/.context/tasks/OM-004-populate-common/changesets/`)

- `ModCheck.json`, node `tools/ModCheck`: a .NET 8 console program (one file, `Program.cs`) that loads a mod DLL against the game's Managed folder and forces type setup plus JIT of every method to catch VTable and missing-member breaks without launching the game. Went in: code style (tabs, Allman, net8.0, single static `Program`), error handling (exit codes 0/1/2, OK/FAIL/WARN lines, ECall-as-warning rule), naming (positional args, output vocabulary), entities (Program, the two checks, inputs/output, rules, caller in `Invoke-OniTests.ps1`), glossary (VTable setup, missing member, ECall, DIM, managed directory). applies_to `Program.cs`, `ModCheck.csproj` on the four code-bearing documents.
- `SaveRunner.json`, node `tools/SaveRunner`: a dev mod plus `Invoke-SaveRun.ps1` that loads a save unattended (`--saverun=FILE`), blocks all saves, samples the `fridges` probe each game second and quits with a status line. Went in: code style (layout table, inert-without-argument rule, patch conventions, how to add a probe), error handling (four run statuses, driver catch, crash dialog, launcher throws, mods.json restore, kill timers), naming (`--saverun-*`, `<Type>_<Method>_Patch`, CSV columns), entities (mod, Runner, patches, RunBehaviour, FridgeProbe, launcher parameters, files, rules, run states), glossary. applies_to `*.cs`, `*.ps1`, `*.csproj` (plus `*.yaml` on error handling, naming, entities).
- `TestRunner.json`, node `tools/TestRunner`: a harness mod, test kit (`Isochronous.TestKit`) and launcher/warden scripts that run `[OniTest]` coroutines in the real game. Went in: code style (two namespaces, layout table, Reflect rule, patches), error handling (test statuses, session statuses, launcher exit codes 0-3, hand-stepped coroutines, warden, mutex, logging), naming (files, patches, Reflect handles, waits, attributes, test ids, `--onitest-*`, run folders, result fields), entities (Session, TestBehaviour, patches, FixtureArea, Worldgen, Reflect, the kit table, launcher, warden, files, rules, states), glossary (harness, kit, fixture, area, layer, live session, warden, wait, box, loadout, generated world). applies_to `*.cs`, `Kit/**/*.cs`, `*.ps1`, `*.csproj` (plus `*.yaml` on error handling, naming, entities).

Glossaries carry no applies_to. No tags. No README, overview or decisions touched.

## TODO(owner) left
- `ModCheck` code-style: one `TODO(owner)` asking whether to add an `.editorconfig` for `common/` (the repo has none). That is the only one; the other facts were answerable from the code.

## Plan summaries
From `C:/dev/ONI-Mods`, `node c:/dev/ai/icon/bin/ks.mjs write plan --changeset <file> --at common`. All three accepted with the same result, node resolved as a leaf of store `C:/dev/ONI-Mods/common`, branch OM-004-populate-common:

```
create  .context/standards/code-style.md
create  .context/standards/error-handling.md
create  .context/standards/naming-conventions.md
create  .context/domains/entities.md
create  .context/domains/glossary.md
narrative  <each of the five>  created
plan only — nothing was written.
```

## Notes
- Claims are drawn from the source and each tool's README/launcher help. Two deliberate non-claims: the launcher scripts set no exit code (SaveRunner) so the document says read the status line; ModCheck `OK <type>` prints even when a method of the type failed JIT, and the document says so.
- `git -C common status` shows only the new `changesets/` folder and the unit 02 report as untracked, from this unit and its sibling.

## Friction
None met with the engine. `write plan` accepted every change-set on the first run. (The harness twice reported my scratch scripts as "changed on disk" after my own edits; no action needed.)
