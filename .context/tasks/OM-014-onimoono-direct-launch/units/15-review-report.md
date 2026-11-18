# Unit 15 review: b01ae19

Verdict: correct, with one low-severity gap.

## Passes run
Read `git show b01ae19`, then the surrounding code in Invoke-OniTests.ps1 (575-714), GameProcess.ps1 (87-190) and Test-OniMods.ps1. Nothing launched or changed.

- Catch no longer warns (Invoke-OniTests.ps1:586-588). The failure surfaces once, in the summary. OK.
- Summary (681-693):
  - No status: the session line carries line 1 and `Select-Object -Skip 1` prints the rest.
  - Status present: every line is printed as "FAILED:".
  - One-line failure: `-Skip 1` yields an empty array, and `$lines += @()` adds nothing. The text is still printed in the session line. OK.
  - Both footer (line 740): `$l.Session` is the session line, so it is never empty for a failed leg.
  - No failure path prints nothing. OK.
- Stray text: `$legFailure += "`n..."` (633) comes after the catch has set `$legFailure`. The empty-failure case, which would give a blank first line, is not reachable: a Start-Process error is caught at 586, and anything escaping the try aborts the script before the summary. OK.
- Wording: GameProcess.ps1:217, Invoke-OniTests.ps1:633 and Test-OniMods.ps1:352 and 359 state facts. "May be the player's" is gone. The Test-OniMods 900 s warning says mods.json holds the test loadout and your loadout is in the backup. Matches the spec. OK.

## Finding (low)
Invoke-OniTests.ps1:700-702 skips the "Moono's log: none written" line on any failure, on the assumption that the failure text already said "Moono wrote no log at ...". That text comes only from Get-MoonoExitText (GameProcess.ps1:127), which is used when the launcher exited early. Two other failures leave the log fact unstated when no log exists:
- "The stock game ... started during the Moono launch" (GameProcess.ps1:169)
- "The game did not start within 120 s." (GameProcess.ps1:180)

Expected fix: suppress the line only when the failure text matches `wrote no log at`, or when it contains "End of Moono's log".
Example: `if ($logThere -or -not ($legFailure -match 'Moono wrote no log at'))`.

## Pointer feedback
All the TestRunner and tools pointers applied. `tools/TestRunner:tags.md` was flagged as having no coverage and no tags; I did not open it.
