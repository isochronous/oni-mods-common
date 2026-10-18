# Unit 03 review report: the 30 documents OM-004 landed in common/

Scope: `tools/{ModCheck,SaveRunner,TestRunner,Translations/StringsTemplate,WorkshopUpload,WorldgenDriver}/.context/{standards/{code-style,error-handling,naming-conventions},domains/{entities,glossary}}.md` (30 documents), checked against each tool's code, launcher script, csproj and yaml.

Nothing was modified except this report. Nothing committed. Helper script (read-only): `C:/Users/thegr/AppData/Local/Temp/claude/C--dev-ONI-Mods/16b1eb3e-feed-47e8-a9b9-623a69a610b9/scratchpad/om004-review/check.py` (modes: paths, idents, leftovers, hedges, todo).

Verdict: no block findings. The documents are accurate on nearly every concrete claim (classes, methods, options, exit codes, statuses, log and report formats, file names, defaults). 14 fix findings (a statement that is false or misleading against the code) and 13 nit findings.

## Passes run and coverage

| # | Pass | Method | Coverage | Result |
|---|------|--------|----------|--------|
| 1 | Cited file paths exist | script: every backticked token that looks like a path, plus every relative md link, resolved against the tool dir, common/, the repo root | 30 of 30 docs | All repo paths and links resolve. Unresolved tokens were all runtime or gitignored items (`Player.log`, `mods.json`, `colony.sav`, `runs/`, the Klei folder) or bare file names. One real miss: `runs\coverage-*.txt` (F9). `fixtures/testbed-base.sav` is cited as a fixture and does not exist (F10). |
| 2 | Concrete claims match code | read all code (every .cs, .ps1, .py, csproj, yaml, steam_appid.txt) of the six tools and compared line by line; script listed backticked identifiers absent from the tool's code and each was resolved by hand | 30 of 30 docs, all six tools. TestRunner: Session, TestBehaviour, Patches, Worldgen, Harness, Reflect, FixtureArea, Diagnostics, Mod, all 7 Kit files (public members), Invoke-OniTests.ps1 in full, Watch-OniGame.ps1. WorldgenDriver: Mod, Patches, Driver, Invoke-Worldgen.ps1 in full. SaveRunner, ModCheck, StringsTemplate (plus translations.py), WorkshopUpload (plus release.py, ModTest, common/README.md for cross-claims): in full. | Findings F1 to F14 and N1 to N13 below. |
| 3 | Invented claims that should be TODO(owner) | read each prescriptive sentence for a source in code, comments, README or release.py | 30 of 30 | Mostly sourced (for example "SDK 1.60", "free with the game" come from common/README.md; the csproj/mod_info version rule from release.py). Generalised conventions with no support: N9. The existing TODO(owner) items (editorconfig x5, ReadPo double summary, bad-path exit, ugc-describe, add arg checks) are all accurate against the code. |
| 4 | Template leftovers | script grep for placeholders, "Template", web vocabulary (REST, endpoint, database, JWT, React, SQL), plus heading review | 30 of 30 | No placeholders, no web examples, no "Template" in titles (hits were `.pot` = Portable Object Template and "Template and check run"). Generic template titles remain: N4. |
| 5 | Hedges, disclaimers | script grep for hedge words, then read each hit in context | 30 of 30 | No disclaimers, no "may not be accurate" text. Four soft "usually/typically/likely" phrases: N5. |
| 6 | `applies_to` globs match real files | script: each glob expanded from the tool dir, bin/obj excluded | 24 docs that carry `applies_to` (6 glossaries carry none) | Every glob matches at least one file. Gaps: N6, N10, N11. |

## Findings

Severity: block (document would send a reader wrong in a way that costs work) = none. fix = false or misleading against the code. nit = wording, consistency, or an omission.

### fix

**F1. StringsTemplate / standards/error-handling.md / "Anti-Patterns to Avoid", third bullet.** Says `translations.py` "reads the standard output and the 0/1/2 codes". `translations.py` does not capture or read stdout (`subprocess.run([...])`, output passes straight to the console) and treats any non-zero return code as one problem (`result.returncode != 0`); it does not distinguish 1 from 2. Expected fix: "translations.py runs it with its output passed through to the console and counts any non-zero exit as a problem; keep stdout human readable and the exit code non-zero for any problem."

**F2. TestRunner / standards/naming-conventions.md / "Results".** Screenshot names are given as `artifacts/<Class>.<Method>-<name>.png`. `TestContext.Screenshot` uses `Id.Replace('/', '.') + "-" + name + ".png"`, and `Id` is `<Assembly>/<Class>.<Method>`, so the file is `artifacts/<Assembly>.<Class>.<Method>-<name>.png`. Expected fix: add `<Assembly>.`.

**F3. TestRunner / standards/code-style.md / "Private Game Members".** States "Reach game internals only through `Reflect.cs` ... and a line in `Reflect.SelfCheck()`". `Worldgen.cs:134-135` reads two private members with `AccessTools.Field` (`CharacterSelectionController.proceedButton`, `WattsonMessage.button`) that are not in `Reflect.cs` or `SelfCheck`; `Mod.cs:48-49` uses `AccessTools.Method` for the quit trace (guarded by `TracePatch`). The error-handling anti-pattern scopes the rule to "a test or kit file", the code-style section does not. Expected fix: scope the rule to the kit and tests, name `Worldgen.cs` as the harness exception, or record `TODO(owner): move proceedButton and WattsonMessage.button into Reflect.cs and SelfCheck`.

**F4. SaveRunner / standards/naming-conventions.md / "Command Line and Launcher", second bullet.** "Launcher parameters ... map to the game arguments one to one." Only `-Seconds`, `-Speed`, `-Probe`, `-TimeoutSec` map directly; `-Save` becomes `--saverun=<copy of the save>`; `-With`, `-Without`, `-Label`, `-OutDir`, `-SteamExe`, `-AppId` have no game argument (`-OutDir` and `-Label` become `--saverun-out=<run folder>/probe.csv`). Expected fix: list which map and which configure only the launcher.

**F5. WorkshopUpload / standards/error-handling.md / "Principles" item 2 (and code-style.md / "Control Structures", first bullet).** "Usage errors return 2 without initialising Steam." True only for no arguments. The unknown-command case (`Main` default branch) and the `Update` and `Publish` argument checks run after `SteamAPI.Init()` and inside the `try`, so Steam is initialised and then shut down in `finally`. Expected fix: "No arguments prints usage and returns 2 before Steam is initialised; other usage errors return 2 inside the try, so Steam is shut down."

**F6. WorkshopUpload / standards/error-handling.md / "Exit codes" row 1 and the sentence under the table.** Row 1 lists `Info` as returning 1 for "the item is not downloadable". `Info` prints `NOT a legacy single-file item` and still returns 0 when the Steam result is OK; it returns 1 only when `m_eResult` is not OK. Only `Download` (directory install path, not installed) returns 1 for that reason. Also "failures go to standard error": the failed commit and publish results are written with `Console.WriteLine` (standard output) before `return 1`. Expected fix: correct the row, and say the result line of `update` and `publish` is on standard output.

**F7. WorldgenDriver / standards/error-handling.md / "Launcher", second bullet.** "A result line the game was still writing is skipped with `Write-Warning`". `Read-Results` skips a line that does not end in `}` silently (`continue`); `Write-Warning` fires only for a complete-looking line that fails `ConvertFrom-Json`. Expected fix: split the two cases.

**F8. WorldgenDriver / standards/error-handling.md / "Principles" item 1.** "Every coordinate gets exactly one result line." Coordinates after a session-ending status (`wrong-mode`, `driver-error`, `crash`, `hang`, `stuck-before-start`), and the coordinate in flight when the launcher kills a game (`killed`) or never sees it start (`no-process`), get no line from that session; the launcher retries them in the next session or gives up after two empty ones. Expected fix: "Every coordinate the driver attempts gets exactly one result line, written once through `Finish`."

**F9. WorldgenDriver / domains/entities.md / "Files read and written", row `runs\coverage-*.txt`.** Listed as an input. `runs/` is gitignored (`common/.gitignore`) and absent from a checkout; the only source is a sentence in `README.md`. Expected fix: drop the row, or mark it `TODO(owner): the coverage lists are local only, decide whether to commit them`.

**F10. TestRunner / standards/naming-conventions.md / "Files and Directories" (Fixtures bullet), domains/entities.md (Business Rules reference to the fixture) and domains/glossary.md / "Fixture", "SO" row.** Present `fixtures/testbed-base.sav` as an existing fixture. Only `testbed-so.sav` exists in `fixtures/` and in git; `Invoke-OniTests.ps1` throws "Fixture save not found" in base-game mode. Related stale statement the docs do not repeat: `fixtures/README.md` and the script comment say the `.sav` files are not committed, but `testbed-so.sav` is tracked. Expected fix: state that only the Spaced Out fixture is supplied, or `TODO(owner): supply testbed-base.sav or require -Fixture in base-game mode`.

**F11. SaveRunner / domains/entities.md / "Files it reads and writes" (`probe.csv` row) and standards/naming-conventions.md / "CSV Columns".** "One row per item per game second." `FridgeProbe.Sample` also writes a row for a fridge with no items (`head + ",,,,,,,,,,"`), and no CSV is opened at all unless both `--saverun-out` and `--saverun-probe` are given. Expected fix: "one row per item, or one row for an empty fridge, per game second".

**F12. SaveRunner / domains/entities.md / "State Transitions", last row.** `timeout` is listed from "loading or running". `StartedAt` is set when the command line is read and `App_LateUpdate_Patch` checks the deadline whenever the run is not finished, so a game that never reaches the main menu, or stalls waiting for it, also ends as `timeout`. Expected fix: trigger "real time past `TimeoutSeconds` in any state before finished".

**F13. StringsTemplate / domains/entities.md / "Key Code Paths", "Caller" bullet.** "`translations.py` builds the project (`TOOL` path, line 22) and runs `bin/Release/net8.0/StringsTemplate.dll` (line 53)." Line 22 only defines `TOOL`; the build is line 52, line 53 only builds the path string, the run is line 66. Line numbers in prose go stale. Expected fix: drop the numbers ("`main()` builds the project at `TOOL`, then runs `bin/Release/net8.0/StringsTemplate.dll` on each mod's DLL").

**F14. StringsTemplate / standards/error-handling.md / "Principles" item 3.** "A check exists only where the game would drop or refuse the text." MISSING, STALE, EMPTY and OBSOLETE are bookkeeping checks between the po and the DLL, not game refusals; only REFUSED and the parse FAIL mirror the game. Expected fix: "Where a check mirrors the game, the line says what the game would do."

### nit

- **N1.** StringsTemplate / standards/code-style.md / "Line Length": names the `CheckPo` summary `Console.WriteLine` as the longest. The longest lines are the `ReadPo` throw (`Program.cs:246`, 184 characters) and `Program.cs:266`. Drop the claim.
- **N2.** StringsTemplate / domains/entities.md / "Entry": the member table is headed "Property"; `Entry` has public fields (naming-conventions says "no properties").
- **N3.** StringsTemplate / domains/glossary.md / "Abbreviations": the DLC row says "not used by this tool". A row that says it is unused is padding; remove.
- **N4.** Generic template titles `# Code Style Guide`, `# Error Handling Conventions`, `# Naming Conventions` remain in ModCheck, SaveRunner and TestRunner (9 documents); the other three tools put the tool name in the title. Make them `# ModCheck code style` and so on.
- **N5.** Soft wording: ModCheck / code-style.md / "Comments" ("usually the runtime difference"), ModCheck / error-handling.md / "Output Lines" (`FAIL jit` row, "usually a missing method or field"), ModCheck / glossary.md / "VTable setup failure" ("typically because"), WorkshopUpload / error-handling.md / "Principles" 4 ("the likely cause"). Each can state the cause the code names.
- **N6.** `.editorconfig`: ModCheck records `TODO(owner)` scoped "for all of common/", StringsTemplate, WorkshopUpload and WorldgenDriver record a per-tool TODO, SaveRunner and TestRunner state the absence with no TODO. Also SaveRunner and TestRunner code-style.md say the yaml files use two spaces; `mod.yaml` and `mod_info.yaml` are unindented. Their `applies_to` omit `*.yaml` while the text covers yaml (WorldgenDriver's code-style includes it).
- **N7.** WorldgenDriver / standards/code-style.md / "Braces": the inline form `if (!Driver.Active) return;` is shown; the code (and the "Control Structures" section of the same document) uses the two-line form.
- **N8.** WorldgenDriver / domains/glossary.md / "Coordinate" lists "prefix, seed, settings code, story code and mixing code", omitting the mode letter (`A` or `C`) the tool itself keys on; "a prefix ending in -C" (also in README and entities) is loose, the letter is its own part. `Driver.Started` is set in `BeginCoordinate` before the mode check, so the `hang` versus `stuck-before-start` boundary ("after/before the screen opened") is slightly earlier than stated.
- **N9.** Conventions generalised from one case: SaveRunner / naming-conventions.md ("Probe names are lower case plurals", "units spelled in the name": `surfaceArea` and `groundTransferScale` carry none, ModCheck's "Local constants are camelCase too" from one constant, TestRunner's "label is short and names the work". Phrase as observed examples or mark `TODO(owner)` if they are meant as rules.
- **N10.** WorkshopUpload / domains/entities.md / "Commands" relationships: `Update` also uses `Await<T>` (and `UploadToCloud` does). WorkshopUpload `applies_to` omits `steam_appid.txt`, which code-style and entities document.
- **N11.** All six `glossary.md` files carry no `applies_to`; they are reachable only through links from entities and naming. (The coordinator's brief also notes `tools:overview.md` has no coverage; outside this unit's 30 documents.)
- **N12.** TestRunner / naming-conventions.md: the live-session folder is listed as `queue`, `state`, `game.pid`; the launcher also writes `key.txt` there. `mods.json.real` exists only for a fresh game launch (not for a run handed to a live game).
- **N13.** SaveRunner / naming-conventions.md / "Code Naming": "private fields camelCase with no prefix"; `FridgeProbe.Inv` is a private static readonly in PascalCase. Say "instance fields".

## Documents with no finding

ModCheck: all five documents (accurate against `Program.cs`, the csproj and the `Invoke-OniTests.ps1` caller, including exit 3) except the title (N4) and soft wording (N5). TestRunner `domains/entities.md` and `standards/error-handling.md` are the most detailed in the set and match the code (statuses, exit codes 0 to 3, warden thresholds of 60 s and 180 s, mutex name, hand-stepped coroutines, kit member lists) apart from F10 and F3.

## Answers to the handoff's report-back questions (section 2 of the packet)

- Did a pointer turn out not to apply? I read the documents directly from the paths in the brief and did not run `ks read`; every listed pointer pointed at a document in this unit and each one-line reason matched what the document says.
- Did anything governing this work fail to appear? The brief was truncated (12 candidates not listed). Nothing relevant seemed missing: the cross-checks I needed were `common/README.md`, `tools/Release/release.py`, `tools/ModTest/*`, `common/oni-mod.props` and `common/.gitignore`, none of which the brief listed; they are non-store sources, so only a record naming them would surface them.
- Pointer reasons that did not match their record: none seen.

## Friction with ICON

None to record.

## Suggested order for the fixer

Highest value first: F5, F3, F10, F1, F4, F6, then the rest. F2, F7, F11 and F12 are one-line corrections. N4 touches nine files and is the only change that is more than a sentence per file.
