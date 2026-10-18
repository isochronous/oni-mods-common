# Unit 04 report: fix change-sets for the unit 03 review

Six change-sets, one per node, in `common/.context/tasks/OM-004-populate-common/changesets/`: `fix-StringsTemplate.json`, `fix-TestRunner.json`, `fix-SaveRunner.json`, `fix-WorkshopUpload.json`, `fix-WorldgenDriver.json`, `fix-ModCheck.json`. All are `section` ops. All six are accepted by `ks write plan` (results below). Nothing applied, nothing committed.

Generator (reads each live section body, asserts each old string occurs exactly once, replaces it, writes the JSON): `C:/Users/thegr/AppData/Local/Temp/claude/C--dev-ONI-Mods/16b1eb3e-feed-47e8-a9b9-623a69a610b9/scratchpad/om004-fix/gen.py`. Unchanged lines in a section are therefore byte-identical to the landed text.

Paths below are relative to `common/tools/<node>/.context/`.

## Per finding

| Finding | Change-set | Section | Code evidence |
|---|---|---|---|
| F1 | fix-StringsTemplate | `standards/error-handling.md` / Anti-Patterns to Avoid | `translations.py:66` `subprocess.run([...])` with no capture, `:67` `result.returncode != 0` counts one problem. Bullet now says output is passed through and any non-zero exit is one problem. |
| F2 | fix-TestRunner | `standards/naming-conventions.md` / Results | `Kit/TestContext.cs:191` `Id.Replace('/', '.') + "-" + name + ".png"`; `Session.cs:475` `Id => AssemblyName + "/" + Type.Name + "." + Method.Name`. Now `artifacts/<Assembly>.<Class>.<Method>-<name>.png`. |
| F3 | fix-TestRunner | `standards/code-style.md` / Private Game Members (plus Layout row, plus `domains/entities.md` / Key Entities/Reflect and Diagnostics) | `Worldgen.cs:134-135` `AccessTools.Field` for `proceedButton` and `button`; `Mod.cs:48-49` `AccessTools.Method` through `TracePatch` (skips a null target); `Reflect.SelfCheck` lists neither. No `AccessTools` or `Traverse` in `Kit/`. Chose the "scope the rule" option: kit and tests use `Reflect.cs` only, the two exceptions are named, and a `TODO(owner)` records moving the two fields. The Layout row ("every private game member the code touches") and the entities line made the same exclusive claim, so they are reworded in the same change-set. |
| F4 | fix-SaveRunner | `standards/naming-conventions.md` / Command Line and Launcher | `Invoke-SaveRun.ps1` gameArgs: `--saverun=$saveCopy`, `--saverun-seconds/-speed/-timeout`, `--saverun-out=$csv` where `$csv = <OutDir>/<Label>-<stamp>/probe.csv`, `--saverun-probe` only when `-Probe` is set. `-With`, `-Without`, `-SteamExe`, `-AppId` have no game argument. Bullet now lists the direct maps, `-Save`, the run-folder derivation, and the launcher-only parameters. |
| F5 | fix-WorkshopUpload | `standards/error-handling.md` / Principles (item 2); `standards/code-style.md` / Control Structures | `Program.cs:45-50` usage and return 2 before `SteamAPI.Init` (`:51`) only for no arguments; `:69-71` unknown command and `:146-150`, `:185-195` update/publish checks run inside the `try` (`:56`), `finally` at `:74-77`. |
| F6 | fix-WorkshopUpload | `standards/error-handling.md` / Exit codes (table rows 1 and 2 and the sentence under it) | `Info` `:96` returns 1 only if `m_eResult` is not OK, and prints "NOT a legacy..." at `:93-95` regardless. Returns 1 at `Download` `:259`, `:273`, `:288`, `:291`; `Description` `:110`; `List` `:129`. Failed commit and publish results are `Console.WriteLine` at `:177`, `:208`; `Download` not-installed lines `:272`, `:286`, `:290` are stdout; Init, usage, `Description`, `List`, `DownloadItem` messages are `Console.Error`. |
| F7 | fix-WorldgenDriver | `standards/error-handling.md` / Handling Patterns/Launcher | `Invoke-Worldgen.ps1:104` `if (-not $t.EndsWith('}')) { continue }` (silent); `:105` `Write-Warning "unreadable result line skipped..."` in the `catch` of `ConvertFrom-Json`. Bullet split into the two cases. |
| F8 | fix-WorldgenDriver | `standards/error-handling.md` / Principles (item 1) | `Driver.cs` `Finish`: `canContinue` only for `complete`, `loaded`, `failed`, `bad-args`, else `App.Quit()` with the later coordinates unwritten; `Invoke-Worldgen.ps1:150`, `:160` `no-process` and `killed` are launcher session statuses with no result line; `:173` gives up after two empty sessions. Item now says "attempts" and states the exceptions and the retry/give-up. |
| F9 | fix-WorldgenDriver | `domains/entities.md` / Files read and written | `common/.gitignore` ignores `tools/WorldgenDriver/runs/`; only `README.md` mentions `runs/coverage-*.txt`. Row dropped; a `TODO(owner)` sentence under the table asks whether to commit the lists. |
| F10 | fix-TestRunner | `standards/naming-conventions.md` / Files and Directories; `domains/glossary.md` / Abbreviations and Acronyms (SO row) and Cross-Domain Terms/Fixture | `git ls-files tools/TestRunner/fixtures` shows `README.md` and `testbed-so.sav` only; `Invoke-OniTests.ps1:156-163` builds `fixtures\testbed-base.sav` for base mode and throws `Fixture save not found`. Text now says only the Spaced Out fixture is supplied and a base-game run needs `testbed-base.sav` copied in (or `-Fixture`). `domains/entities.md` has no `testbed-base` reference (grep), so it needed no edit despite the review naming it. |
| F11 | fix-SaveRunner | `domains/entities.md` / Files it reads and writes (probe.csv row); `standards/naming-conventions.md` / CSV Columns | `RunBehaviour.cs:136-140` `writer?.WriteLine(head + ",,,,,,,,,,")` for a fridge with no items; `:62-64` writer opened only when `Runner.OutFile != null && Runner.Probe != null`; launcher always passes `--saverun-out` (`Invoke-SaveRun.ps1`). Both places corrected. |
| F12 | fix-SaveRunner | `domains/entities.md` / State Transitions (last row) | `Runner.cs:54` `StartedAt` set in `ReadCommandLine`; `Patches.cs:73-88` `App_LateUpdate_Patch` returns only when `Finished`, falls through to the `Time.realtimeSinceStartup - StartedAt > TimeoutSeconds` check when the menu was never seen. Trigger row now starts from "waiting for menu, loading or running". |
| F13 | fix-StringsTemplate | `domains/entities.md` / Key Code Paths (Caller bullet) | `translations.py:22` defines `TOOL`, `:52` build, `:53` builds the dll path, `:66` runs it with `managed, dll, <project>/translations`. Line numbers dropped. |
| F14 | fix-StringsTemplate | `standards/error-handling.md` / Principles (item 3) | `Program.cs:164`/`:266` parse FAIL names the game's reader; `:191`, `:196` REFUSED names the `{n}` and `<link>` rule; `:173-204` MISSING, EMPTY, STALE, OBSOLETE are po/DLL comparisons. Item now separates the two groups. |

## The four soft phrases (N5)

| Phrase | Change-set | Section | Replacement and evidence |
|---|---|---|---|
| ModCheck code-style "usually the runtime difference" | fix-ModCheck | `standards/code-style.md` / Comments | "which is the runtime difference between the check host (CoreCLR) and the game's Mono". `Program.cs:63-66` and `:101-106` comments. |
| ModCheck error-handling `FAIL jit` "usually a missing method or field" | fix-ModCheck | `standards/error-handling.md` / Output Lines | "`PrepareMethod` threw, for example `MissingMethodException` or `MissingFieldException` ...". `Program.cs:69-73`, header comment `:10-11`. |
| ModCheck glossary "typically because" | fix-ModCheck | `domains/glossary.md` / Cross-Domain Terms/VTable setup failure | "which a game update causes by changing an interface or base class that the mod's type implements or derives from". `Program.cs:6-9` header. |
| WorkshopUpload error-handling "the likely cause" | fix-WorkshopUpload | `standards/error-handling.md` / Principles (item 4, merged into the F5 op) | "Messages name what to check: Steam running and logged in, `steam_appid.txt` next to the exe, cloud quota, the Workshop legal agreement." `Program.cs:53`, `:304`, `:177`, `:208`. |

The other nits (N1 to N4, N6 to N13) are left as instructed.

## Plan results

All run from `C:/dev/ONI-Mods` with `node c:/dev/ai/icon/bin/ks.mjs write plan --changeset <file> --at common`. All accepted first time ("plan only - nothing was written"). Each prints "no base digest given ... planned against it as it is now", expected for section ops without a digest.

- fix-StringsTemplate: error-handling.md Principles (-198/+366 bytes), Anti-Patterns (-122/+293); entities.md Key Code Paths (-154/+230).
- fix-TestRunner: naming-conventions.md Results (-320/+331), Files and Directories (-134/+363); code-style.md Private Game Members (-341/+821), Layout (-62/+68); entities.md Key Entities/Reflect and Diagnostics (-174/+264); glossary.md Abbreviations (-84/+96), Cross-Domain Terms/Fixture (-215/+317).
- fix-SaveRunner: naming-conventions.md Command Line and Launcher (-203/+602), CSV Columns (-167/+226); entities.md Files it reads and writes (-73/+196), State Transitions (-80/+156).
- fix-WorkshopUpload: error-handling.md Principles (-229/+478), Exit codes (-387/+951); code-style.md Control Structures (-147/+278).
- fix-WorldgenDriver: error-handling.md Principles (-153/+568), Handling Patterns/Launcher (-103/+208); entities.md Files read and written (-101/+166).
- fix-ModCheck: code-style.md Comments (-230/+274), error-handling.md Output Lines (-109/+198), glossary.md Cross-Domain Terms/VTable setup failure (-256/+299).

## Unresolved and notes

- The plan reports line counts only, so I did not see the rendered result. For the glossary Fixture and VTable sections (and the entities Reflect section), whose `**Id:**` line ends in two trailing spaces before the next line, the plan shows "+2 lines" for a one-line replacement. Whoever applies should read the section after applying to confirm the Id line and the `**Definition**` or `**Description**` line did not merge.
- Beyond the review's list, I also changed the TestRunner Layout row and the entities Reflect description (same exclusive claim as F3). Drop those two ops if only the code-style section should change.
- The review notes `fixtures/README.md` and the comment in `Invoke-OniTests.ps1:153` say the `.sav` files are not committed, while `testbed-so.sav` is tracked (and `common/.gitignore` says so). Those are code and README files, outside this unit; not touched.
- The F9 and F3 `TODO(owner)` lines are the only unresolved decisions I added.

## ICON friction

None to record. The section address `parent/child` form worked for nested headings on the first try.

## Report-back questions from the packet (section 2)

- Pointers that did not apply: none surfaced; I read the documents from their paths and the review report rather than via `ks read`.
- Anything governing that did not appear: `units/03-review-report.md` and `units/common-rules.md` were reached through the task spec, not the brief; the non-store sources I checked against (`Program.cs`, `translations.py`, `Invoke-*.ps1`, `.gitignore`) are not records, so a brief cannot list them.
- Pointer reasons that did not match their record: none seen.
