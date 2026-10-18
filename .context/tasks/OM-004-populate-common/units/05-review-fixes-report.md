# Unit 05 review report: fixes for the unit 03 findings

Scope: uncommitted diff `git -C C:/dev/ONI-Mods/common diff -- tools` (15 files, 35 insertions, 29 deletions), checked against `units/03-review-report.md` (F1 to F14) and `units/04-implement-fixes-report.md`. Read only. Nothing modified, nothing committed, no store writes.

Verdict: all 14 findings are resolved, all four soft phrases are gone, and no heading is broken. One new defect: a merged line in SaveRunner (N-new-1). It needs one fix.

## Per-finding status

| # | Status | Evidence |
|---|--------|----------|
| F1 | resolved | StringsTemplate `standards/error-handling.md` Anti-Patterns now says output is passed through and any non-zero exit counts as one problem. `translations.py:66` is `subprocess.run([...])` with no capture; `:67-68` `if result.returncode != 0: problems += 1`. |
| F2 | resolved | TestRunner `standards/naming-conventions.md` Results now reads `artifacts/<Assembly>.<Class>.<Method>-<name>.png`, matching the `Id.Replace('/', '.')` path in `Kit/TestContext.cs` (per unit 04 evidence). |
| F3 | resolved | TestRunner `standards/code-style.md` scopes the Reflect rule to kit and tests and names the two exceptions. `Worldgen.cs:134-135` has the two `AccessTools.Field` calls. `Mod.cs:48-49` has the `AccessTools.Method` calls through `TracePatch`. Neither is in `Reflect.cs`. A TODO(owner) records the move. Layout row and entities Reflect line reworded to match. |
| F4 | resolved in substance, but see N-new-1 | SaveRunner `naming-conventions.md` now separates direct maps from launcher-only parameters. `Invoke-SaveRun.ps1:114-115` confirms `--saverun-seconds/-speed/-timeout`, `--saverun-out=$csv`, `--saverun=$saveCopy` (copy of the save in the run folder) and `--saverun-probe` only when `-Probe` is set. The bullet text itself is malformed (N-new-1). |
| F5 | resolved | WorkshopUpload `error-handling.md` Principles 2 and `code-style.md` Control Structures now say only "no arguments" returns 2 before Steam init. `Program.cs:47-49` returns 2 before `SteamAPI.Init` at `:51-53`. Unknown command (`:70-71`) and the update/publish checks (`:148-149`, `:187-194`) are inside the `try`. |
| F6 | resolved | Exit-code row 1 no longer lists `Info` for "not downloadable". Row 2 drops "no command" wording as needed. Stream placement verified: `Program.cs:109-110`, `:128-129`, `:258-259` are `Console.Error`. `:177`, `:208` (update/publish result), `:272`, `:286`, `:290` (download not-installed and bad-path) are `Console.WriteLine`. The new text matches. |
| F7 | resolved | WorldgenDriver `error-handling.md` Launcher bullet splits the two cases. `Invoke-Worldgen.ps1:104` `continue` is silent for a line not ending in `}`. `:105` `Write-Warning` is in the `ConvertFrom-Json` catch. |
| F8 | resolved | WorldgenDriver Principles 1 now says "attempts" and names the exceptions. `Driver.cs:138` `canContinue` covers only complete, loaded, failed, bad-args. Other statuses reach `App.Quit()` at `:144` with later coordinates unwritten. The launcher's `no-process` and `killed` have no result line. |
| F9 | resolved | Row dropped from entities "Files read and written". A TODO(owner) sentence replaces it. `README.md` line 29 mentions `runs/coverage-*.txt`, and `runs/` is gitignored. |
| F10 | resolved | TestRunner naming, glossary SO, and glossary Fixture now say only `testbed-so.sav` is supplied. `git ls-files tools/TestRunner/fixtures` shows `README.md` and `testbed-so.sav` only. Base-game run needs a copy or `-Fixture`. |
| F11 | resolved | SaveRunner entities probe row and CSV Columns now cover the empty-fridge row. `RunBehaviour.cs:139` writes `,,,,,,,,,,`. `:62` opens the writer only when `OutFile` and `Probe` are both set. |
| F12 | resolved | SaveRunner State Transitions timeout row now starts from "waiting for menu, loading or running". The state "waiting for menu" exists at `entities.md:111-112`. |
| F13 | resolved | StringsTemplate entities Caller bullet drops the line numbers. `translations.py:52` builds, `:53` builds the dll path, `:66` runs it with managed, dll and the translations folder, inside the `mod_projects` loop. |
| F14 | resolved | StringsTemplate error-handling Principles 3 separates the game-mirroring checks (parse FAIL, REFUSED) from bookkeeping (MISSING, STALE, EMPTY, OBSOLETE). `Program.cs:191` and `:196` REFUSED lines match the `{n}` and `<link>` rule. |

## Soft phrases (N5)

Grep over `tools/**/*.md` for `usually|typically|likely|probably|may not` returns no hits. All four are gone:
- ModCheck code-style Comments: "which is the runtime difference between the check host (CoreCLR) and the game's Mono". Matches `Program.cs:14-15`, `:102-104`.
- ModCheck error-handling FAIL jit: "`PrepareMethod` threw, for example `MissingMethodException` or `MissingFieldException`". Matches `Program.cs:10` and `:69`.
- ModCheck glossary VTable: "which a game update causes by changing an interface or base class". No soft word left.
- WorkshopUpload error-handling Principles 4: "Messages name what to check". No soft word left.

## New findings

**N-new-1. Moderate. Merged line.** `tools/SaveRunner/.context/standards/naming-conventions.md`, Command Line and Launcher, second bullet (line 34). The replacement kept the head of the old sentence and appended the new text after it, so the bullet now reads:

> Launcher parameters are PowerShell PascalCase (`-Seconds`, ..., `-AppId`) `-Seconds`, `-Speed`, `-Probe` and `-TimeoutSec` map straight to ...

There is no connector or verb between the parenthetical list and the new clause. The old "and map to the game arguments one to one" tail was dropped, which is correct, but the head is still there. Fix: end the parenthetical with a period, then start a new sentence, for example "...`-AppId`). `-Seconds`, `-Speed`, `-Probe` and `-TimeoutSec` map straight to ...".

## Checks that found nothing

- `git diff --check -- tools` flags trailing whitespace on the `**Definition**`, `**Description**` and `**Id:**`-adjacent lines in ModCheck glossary, TestRunner entities, and TestRunner glossary. That whitespace is the markdown hard line break the originals already used on the same lines, so it is not a defect.
- Id-line merges: the diff shows a blank line now separates `**Id:**` from `**Definition**`/`**Description**` in the three edited glossary and entities sections. No line merged.
- Headings: the diff touches no heading line, and none are broken.
- Shared-artifact scope: the diff touches only `tools/**/.context/` (15 files). Untracked `changesets/`, `handoff-*.md`, `units/*` and `retrospectives/` are outside `tools` and were not reviewed as part of this diff.

## Not verified

I checked each changed claim against the cited code for the lines listed above. I did not re-derive every sentence in the 15 files, nor the WorkshopUpload `Info` exit-code claim (`Program.cs:93-96`), which I took from unit 04's evidence. Those are the two places to look if a second pass is wanted.

## Summary

14 of 14 resolved. 4 of 4 soft phrases gone. 1 new Moderate finding (N-new-1), a merged line in one SaveRunner bullet. No new false claim found in the spot-checks above.
