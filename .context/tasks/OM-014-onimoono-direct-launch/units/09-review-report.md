# Unit 09 review report: OM-014 fixes after unit 07 (5c65587..4c7fad1, tools)

Verdict: all seven unit 07 findings fixed. One new minor finding (Stop-MoonoLaunch scope). No critical or moderate findings.
Read-only: `git diff 5c65587..4c7fad1 -- tools` read in full; no game or OniMoono launched or touched; no code changed.

## Unit 07 findings

- M1 fixed. `Get-GameProcess -Any` skips OniMoono processes whose command line holds `--crash-watch` (GameProcess.ps1:27-30), so game + watcher is one process. All the checks use it (Invoke-OniTests.ps1:212, 215, 217, 229, 631; ModTest Test-OniMods.ps1:92, 307, 312, 336). ModTest's `Get-AnyGame` is deleted. README.md:44-48 matches.
- M2 fixed. `Stop-MoonoLaunch` (GameProcess.ps1:183-198) runs in the finally before the restore, on `-not $proc` (Invoke-OniTests.ps1:608-612) and `-not $game` (Test-OniMods.ps1:344). The new `$launcher`/`$stockBefore` initialisations (Invoke 475-476, ModTest 291-292) keep it safe when the launch never got that far.
- m1 fixed. `Get-MoonoExitCode` (GameProcess.ps1:105-117): launcher code if same process or no process, then the process, then the last `[engine] Exiting after N frames with code X` in the log. Used at Invoke 673, ModTest 325, and in `Get-MoonoExitText` (launcher passed at GameProcess.ps1:173 and Invoke 675). The log line exists in the evidence logs (runs/om014-20261112-115436-moono/moono.log:1511). The log path is per run (run folder; ModTest uses a timestamped temp file), so it is never stale. The `$Launcher.Handle` touch unit 07 suggested was not added; harmless, the log fallback covers a null launcher code.
- m2 fixed. The Moono launch and `Wait-MoonoGame` sit in a try/catch that sets `$legFailure` (Invoke 571-583); the post-launch wait and log copy are skipped; the summary gets `FAILED:` plus a "Leg failed before the game was found" session line (Invoke 663-668); no status gives `$legExit = 2` (692); the leg lands in `$legResults`, so the Both table and the worse-of exit code work. No null dereference found on that path (`$proc`, `$status`, `$results` all handled).
- m3 fixed. Invoke 514-517: the Moono reuse path uses `Copy-FreshLog ... $t0`; stock reuse keeps main's unconditional copy.
- m4 fixed. Summary line is `Runtime: <leg> asked; no game log written by this launch` (Invoke 656); README.md:114 uses the same words.
- m5 fixed. GameProcess.ps1:5 now names the real functions (the line is long, cosmetic); README options table says "above" (README:124); README:30 adds `moono.log`.

## Focus checks

- `Get-GameProcess -Any` still refuses on Jeremy's game: stock `OxygenNotIncluded` always counts; an OniMoono counts unless its command line is readable and holds `--crash-watch`. An unreadable command line counts as a game (errs toward refusing). Invoke 229 still throws when anything but the live session's own pid is running. The non-`-Any` (marker) branch is unchanged.
- `Stop-MoonoLaunch` scope: see N1. It is never called on success paths, and the pre-launch check (Invoke 229, Test-OniRunning) guarantees no game of Jeremy's is open at launch, so a hand-started Moono that was open before the run is safe. A Moono he starts by hand during the failure window is not.

## New finding

N1 (minor). `Stop-MoonoLaunch` selects by start time, not by ownership. GameProcess.ps1:188-189: every `OniMoono` with `StartTime >= Since - 2 s` and every `OxygenNotIncluded` not in `$StockBefore` is stopped. The launch's `Since` is `$t0`, set at the top of the leg (Invoke 468), well before the launch (after the mods.json backup, config and prefs work); ModTest's `$started` is right before the launch (286), so tighter. So a Moono or stock game Jeremy starts by hand between `$t0` and a failed launch is ended, and so is one started in the 2 s before. Also when the failure is before `Start-Process` (nothing of ours exists), a game he started in that window is still a victim. Narrow, only on a failed Moono leg, but it is the one path that stops processes. Expected fix: stop only the launcher plus processes tied to it: children by `ParentProcessId = $Launcher.Id`, and OniMoono processes whose command line holds the run's `--onitest-` arguments or its `--log` path (the crash watcher likely carries the same `--log`; if not, take its parent). Keep the stock-appeared case limited to ids not in `$StockBefore` AND started after the launcher started. If the stock case cannot be told apart from Jeremy's own launch, leave it running and say so in the failure message instead of stopping it.

## Passes run

1. Each unit 07 finding against the diff, file:line.
2. `Stop-MoonoLaunch` victims and callers (success paths, early failure, `-KeepGame`, Both).
3. `Get-GameProcess -Any` refuse logic versus the old behaviour, unreadable command lines, all callers.
4. Moono-failure flow in Invoke (try/catch, finally order: stop, warden stop, restore, then report; exit code and Both table).
5. Exit code sources against the evidence log format.
6. README/header comments versus code.

## ICON tooling friction

Packet pointers were not opened with `ks read`; the review needed only the diff, the unit 07 report and the evidence logs, so no friction to record. Nothing that governs this work failed to appear that I could tell. The packet notes `tools/TestRunner:tags.md` declares no coverage or tags (the brief's own warning); not acted on.
