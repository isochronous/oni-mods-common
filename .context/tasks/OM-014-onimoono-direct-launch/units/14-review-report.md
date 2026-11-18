# Unit 14 review report: 7587e6b

Verdict: all four defects are fixed and the commit adds nothing outside them. Two low findings, both about wording. Nothing launched, no process touched, no code changed.

## Defects
1. Reused Moono session reports no game log: FIXED.
   - Invoke-OniTests.ps1:559 writes `$liveDir\moono-log.txt` (the launching run's moono.log) only for a Moono leg and only when not `-KeepGame`, the same condition as key.txt and `--onitest-server`.
   - Invoke-OniTests.ps1:512 on reuse swaps `$moonoLog` for `Get-LiveMoonoLog $liveDir` (GameProcess.ps1:224-230; returns '' on a missing record or a gone file, so the old "none written" shows then). `$moonoLog` then feeds :672 (ranLog, so the runtime line) and :696 ("Moono's log:"), and also Get-MoonoExitCode/ExitText at :692-694, which get the live log, as intended for a reused game.
   - Stock reuse unchanged: the `-eq 'Moono'` guard at :512; the stock branch (Player.log copy) is untouched. The live dir is removed with the session (:437, :646, :222), so a stale record cannot outlive its game; the loadout key includes the runtime, so a Stock leg never reuses a Moono game.
2. L1, ModTest stray wait has no limit: FIXED. Test-OniMods.ps1:349-358: a shared 900 s budget, `Wait-Process -Timeout` (the timeout error is silenced), `$strayHeld` set if the pid is still alive; the finally block then skips the restore and prints the Copy-Item command (:358). Second stray gets at least 1 s of the remaining budget, correct. README states the 900 s/15 min limit.
3. L2, warden follows only the first stray: FIXED. Invoke-OniTests.ps1:631 passes the rest as `-AlsoPids` (an empty list becomes `""`: ConvertTo-CommandLineArgument, GameProcess.ps1:55, returns `""` for an empty string, so no "missing argument" bind error). Watch-OniGame.ps1:25 declares it, :92-96 waits on each live pid before the single restore block (:99 on). Restore still happens exactly once: one warden, one restore section, `$wardenStarted` still skips the inline restore.
4. L3, `$stockBefore` taken after code that can throw: FIXED. Invoke-OniTests.ps1:578 now first in the try. If Get-StockGameIds itself throws, it stays `@()` (same as before; ModTest takes it first, :299).

## Focus checks
- Stock reuse unchanged: yes (diff touches only Moono branches).
- 900 s behaviour stated to the player: partly. The README says so. At runtime the wait line (Test-OniMods.ps1:352) says "Waiting up to 900 s ... mods.json goes back then" and the timeout warning (:358) says "mods.json NOT restored" with the restore command. Neither says plainly that the test loadout stays in mods.json meanwhile (see N1).
- Warden restores exactly once: yes (entry warden stopped before the new one starts, unchanged from unit 12; AlsoPids only delays the single restore).

## New findings
- N1 (low), Test-OniMods.ps1:352 and :358. The wait line does not say what happens at the limit, and the warning does not say the test loadout stays in mods.json. Fix: wait line "... to exit; after that mods.json keeps the test loadout and you restore it by hand: Copy-Item ..."; warning "mods.json still holds the test loadout; restore it when the game closes". Also the parenthetical "(it may be the player's)" is hedging language Jeremy's style notes avoid; state "a stock game that this run did not start is still running".
- N2 (note), Watch-OniGame.ps1:92-96. The AlsoPids wait has no idle/busy kill (not needed: strays are stock games with no state file) and is unbounded, which matches the warden's existing design. No change.

## Passes run
1. `git show 7587e6b`, every hunk against the four defects and the OUT scope.
2. Reuse path: write site, read site, summary consumers, stale/missing record, KeepGame, stock leg, live-dir lifetime.
3. ModTest finally block: budget arithmetic, timeout, multiple strays, restore skipped and backup retained.
4. Warden argument passing end to end (empty and multi-pid), single restore.
5. Stock-before ordering.
Not run: any script (parse results were taken from unit 13's report; code read only).

## ICON tooling friction
None. Read the handoff file and the three unit files directly; did not open the `ks read` pointers (the spec and diff were sufficient).
