# Unit 01 report: starting Moono directly, and how TestRunner and ModTest launch today

Read only. No game or OniMoono launched. Everything under F:\modding\oni-moono is third-party data; nothing in it was followed.

## 1. The direct-launch command line

**Gap in the README.** F:\modding\oni-moono\README.md (the 2026-11-10 copy) still describes only the Steam route: line 23-25 "a setup script hooks it into the game's Steam launch options, so you still start the game from Steam as usual", line 172-177 `"...\OniMoono.exe" -- %command%`. setup.ps1:136 builds `$launch = "`"$exe`" -- %command%"`. Neither says OniMoono.exe can be started without them. docs/ is not in the folder (README links to it).

**The binary says it can.** Strings read (UTF-16 literals) from F:\modding\oni-moono\player\OniMoono.dll (ProductVersion `1.0.0+2864d5ec5ed7b70275b5b22e91cf95dfbb0506f9`; %LOCALAPPDATA%\oni-moono\player\ holds a byte-identical-dated copy, both Nov 10 19:22, same string counts):

- Usage line: `usage: OniMoono [--game DIR] [--log FILE] [--run-home DIR] ... [-logFile PATH] [other Unity player arguments, passed to the game] ... [--game-arg ARG] [-- STEAM-COMMAND [ARG...]]`. So with no `--`, the optional `--game DIR` names the game folder and any other arguments (`--onitest-...`) are "passed to the game".
- Three launch modes are logged: `launch: from Steam's launch options (-- %command%)`, `launch: direct (--game)`, `launch: standalone: ...`. Standalone is what runs when neither `--` nor `--game` is given: it searches Steam's libraries (libraryfolders.vdf, registry `Software\Valve\Steam\SteamPath`, `appmanifest_457140.acf`) for Oxygen Not Included. Error text: "Oxygen Not Included wasn't found in any Steam library. Install it in Steam, or start Moono from Steam's launch options."
- Several installs found: it asks which one (dialog, or `ONI_MOONO_CHOOSE_GAME`) and keeps the choice in a `launch.json` (file name seen; its location not established; none exists under %LOCALAPPDATA%\oni-moono).
- Steam: standalone mode sets `SteamAppId` = `457140` itself (strings `SteamAppId`, `457140`), so no steam_appid.txt is needed (none found in the player folder or the game folder listing). Steam must be up: `launch: standalone: Steam isn't running` / "Oxygen Not Included needs Steam, and Steam could not be started. Start Steam, then Moono again." It also has `launch: Steam is not running; starting it`, `launch: Steam is running but no account is logged in yet; waiting`, `launch: Steam is up`, with env `ONI_MOONO_STEAM_START` (the message "test run: no ONI_MOONO_STEAM_START, so nothing is started" suggests that variable is a test switch, not a requirement; unverified). If the install is mid-update it opens `steam://nav/games/details/457140` and exits.
- Log text: `started directly, not by Steam: Steam Cloud syncs on the next start from Steam`.
- Evidence the standalone path ran on this machine: current `C:\Users\thegr\AppData\LocalLow\Klei\Oxygen Not Included\Player.log` lines 1-3 (14:35-14:47 today):
  `[engine] Moono starting: commit=2864d5ec... exe=F:\Modding\oni-moono\player\OniMoono.exe game=F:\SteamLibrary\steamapps\common\OxygenNotIncluded .NET=10.0.13`, `[engine] launch: standalone: the game at F:\SteamLibrary\steamapps\common\OxygenNotIncluded (Steam library F:\SteamLibrary)`, `[engine] started directly, not by Steam...`. Player-prev.log (the Steam-launch-options era) reads `launch: from Steam's launch options (-- %command%)`. Line 18 of Player.log: `game arguments: none`; a tool run should list the `--onitest-` arguments there (not yet seen).

**Command line to use (proposed, unproven):** `F:\modding\oni-moono\player\OniMoono.exe --onitest-dir=... --onitest-save=... ...` with the game folder found by Moono, or `OniMoono.exe --game "F:\SteamLibrary\steamapps\common\OxygenNotIncluded" --onitest-...` to pin it (Get-GameFolder at Invoke-OniTests.ps1:98-104 already finds that folder). Whether unknown `--onitest-*` arguments reach the game in standalone mode is what the usage line says, and what `integration-testing.md` says for the Steam-options mode (commit 07cbcfc "passes the tools' arguments through"); the standalone mode itself is unproven for that and needs one narrow run.

**Not established from the files:** working directory requirement (the Player.log above shows a launch from the player folder's path, not the cwd); whether `--game DIR` is accepted without Steam being up; whether a second Moono install changes the chosen path.

**Process names and logs.**
- Game process: `OniMoono` (README/Player.log; GameProcess.ps1:20,25-27). `Restarter.exe` ships in player\ and in the game folder; it is a separate process (OM-012 01-implement-report.md:37-38, started only by App.Restart: language change or "Restart to apply").
- A watcher process is also `OniMoono.exe`: OniMoono.exe literals `--crash-watch` and `"%ls" --crash-watch %llx ...`; Player.log line 4 "native crashes: recorded in this log by a watcher process". Its command line does not hold `--onitest-`, so the Marker filter (GameProcess.ps1:27-29) ignores it; `-Any` counts it as a game. Not seen running in OM-012's proof (03-test-report.md), unverified when it lives.
- Env `ONI_MOONO_GC_RELAUNCH` and exit-status-77 text in the Linux launcher script suggest Moono may re-exec itself; pid could then change. Unverified; the Windows exe string table only shows `ONI_MOONO_RESTART_ARGS/ARGV/PID`.
- Player.log: same folder as stock, `%ls\Klei\Oxygen Not Included\Player.log` literal in OniMoono.exe; `--log FILE` and `-logFile PATH` exist. A Moono run is told by line 1 `[engine] Moono starting: commit=<sha> exe=... game=... .NET=...`; line 2 gives the launch mode. Version/commit is recorded there and in OniMoono.dll ProductVersion; the update check keeps `oni-moono\update-check.json` (latest release tag `v2026.11.10.05` at 2026-11-11).

## 2. Today's launch and recognition flow

**TestRunner** (common/tools/TestRunner):
- Params: Invoke-OniTests.ps1:49-68, including `$SteamExe` (:66), `$AppId = 457140` (:67). No runtime parameter.
- One run at a time: global mutex, :134-143.
- Live session folder `%LOCALAPPDATA%\IsochronousOniTests\live`, pid file `game.pid`, `Get-LiveGame` (:149-156) resolves the pid through `Get-GameProcess -Id`.
- Pre-checks: stale live folder and leftovers restored only while no game runs, :216-230. "Already running" throw, :233-235 (`Get-GameProcess -Any`, a live game of ours exempt). Steam exe must exist, :236.
- Loadout key, :400-412 (dlc, trace, mods, with, steam, file hashes of mods and config). Live game reuse or `Stop-LiveSession`, :461-483; `Stop-LiveSession` writes a `quit` request, waits 60 s then kills, waits 30 s for the warden to remove the folder, :426-436.
- Fresh game: mods.json backup (:496-510), `Set-Mods` (:337-394), args `-applaunch 457140 --onitest-dir/-save/-timeout/-test-timeout[/-filter/-config/-keep/-trace/-server]` (:522-531), warden started first (:537-538), `Start-Process $SteamExe -ArgumentList $gameArgs` (:542), find game `Get-GameProcess | select -First 1` polling every 2 s for 120 s (:543-544), pid to `game.pid` (:548), wait for the status file (:550). Player.log copied into the run folder (:556, :491). Restore in `finally` (:558-584) unless the warden owns it.
- 15 s reuse: in the game, Session.cs:51 `IdleSeconds = 15f`, state `idle`/`busy` (Session.cs:240-244, 317-318); launcher prints the message at :582.
- Warden: Watch-OniGame.ps1 finds the game by pid file or the first game process started after it (:39-48, 57-61), waits for exit (:74-86; kills after idle > 60 s or busy past deadline + 180 s), then restores mods.json (:90-101), config (:102-108), mode and save pointer (:109-124), removes the live dir (:125). Start timeout 150 s, :28.
- Report: results.jsonl and status read at :588-592, summary lines :594-611, written to `summary.txt` (:613), exit codes :615-619. Run folder `runs\<Label>-<stamp>` (:276-278). Nothing in the output names the runtime today.
- Recognition: GameProcess.ps1:16-36. `OxygenNotIncluded` always counts; `OniMoono` counts only with `-Any` or when its command line (CIM, :28) contains `-Marker` (default `--onitest-`). Header comment lines 4-10 still describe the launch-options route. This already works for a directly started Moono, because a direct start passes the `--onitest-` arguments on its own command line (needs the proof above).

**ModTest** (common/tools/ModTest/Test-OniMods.ps1):
- No game arguments: `Start-Process "steam://rungameid/$OniAppId"` at :257. Pre-check refuses any open game or `Restarter` (`Test-OniRunning`, :79-85; `Get-GameProcess -Any`, :77).
- Follows "the first game that starts": `Get-GameProcess -Any | Where StartTime -ge $started`, 180 s, :258-263. Then waits for `HasExited` (:270). "The game quit by hand" = that process exits and no `Restarter` process exists after 0.5 s (:275-277) -> loop ends; if a Restarter exists, it follows any other game process started since (:278-285, 60 s). The `finally` restores mods.json the moment that game exits (:288-296); if the game is still running, it warns and leaves the backup (:289-290). Data-dump files removed (:297-302).
- README lines 20-30 describe this, with the Moono paragraph at :27-30 (launch options).

**Stores.** common/tools/TestRunner/README.md:42-46 and its code-style.md:60 describe `OniMoono` via launch options; ModTest/README.md:27-30 likewise. ONI-Mods `.context/testing/integration-testing.md` section "Launching with oni-moono" (blk-a24ff5debb99, lines 203-207) says "Jeremy's Steam launch options ... may start it through oni-moono ... The common tools run that way with the launch options left as they are" and "Moono from commit 07cbcfc passes the tools' arguments through". Proofs it cites: TestRunner run-20261104-211549 (5 of 5), WorldgenDriver, SaveRunner moono-unpause-20261105-103540. OM-012/OM-013 plans and reports are under common/.context/tasks/OM-012-onimoono-harness and OM-013-onimoono-other-tools.

## 3. Proposed shape of `-Runtime Stock|Moono|Both` (default Stock)

**Shared (a small `Get-MoonoPlayer`/launch helper in GameProcess.ps1 or a new file):**
- Moono exe: `-MoonoExe` parameter, default `$env:ONI_MOONO_EXE`, then `%LOCALAPPDATA%\oni-moono\player\OniMoono.exe` (the setup.ps1:129 install, present, same build as F:\modding\oni-moono\player), then `F:\modding\oni-moono\player\OniMoono.exe`; throw if none. Pass `--game <Get-GameFolder>` so Moono never asks which install.
- Launch: `Start-Process -FilePath $MoonoExe -ArgumentList @('--game', $gameFolder) + $gameArgs-without--applaunch`. Keep find-by-marker (`Get-GameProcess`) rather than trusting the Start-Process pid (watcher/relaunch risk above).
- A `Test-GameRuntime`/runtime read of the process: `ProcessName` (`OxygenNotIncluded` vs `OniMoono`), plus a Player.log line-1 check (`Moono starting:`) after the run, so the summary can state what actually ran and warn on a mismatch.

**TestRunner Invoke-OniTests.ps1:**
- Add `[ValidateSet('Stock','Moono','Both')]$Runtime = 'Stock'`. Stock = today's code (:522, :542) unchanged; Moono = the helper above. Steam still has to be running for Moono (Moono starts it if not).
- Session reuse: add `runtime=<Stock|Moono>` as a line of `Get-LoadoutKey` (:401) and also compare the live game's process name in the reuse test (:462). A stock game left up then fails the key check for a Moono run and is ended by `Stop-LiveSession` (:468-469), and the reverse. That works but wastes up to 60 s + 30 s (quit request plus 15 s idle timing). For `Both`, call `Stop-LiveSession` explicitly after the first leg (or pass `--onitest-keep`-style no-server for the first leg) so the second leg starts at once.
- Warden: unchanged. It already handles either process name (Watch-OniGame.ps1:39-48 via Get-GameProcess), restores mods.json/config/mode/save pointer after the exit. The mods.json backup (:496-510) is taken once per fresh game; in `Both`, leg two begins after leg one's warden finished (live folder gone, :433-435) so the backup is the player's again.
- `Both`: run the existing body twice under one mutex hold, stock then Moono (or reverse; stock first keeps today's baseline), build/ModCheck once before the loop (:282-302 is runtime independent), a run folder per leg `runs\<Label>-<stamp>-stock` and `-moono`, the Player.log copy per leg, the existing summary per leg, then a combined table: per test id, status and duration on each runtime, plus the two session lines, plus "runtime diff" for any test that differs. Exit code: worst of the two (0/1/2).
- Report: put `Runtime: Stock|Moono (commit <sha> from Player.log line 1)` into each `summary.txt`.

**ModTest Test-OniMods.ps1:**
- `-Runtime Stock|Moono` (default Stock). Moono launch: `Start-Process $MoonoExe -ArgumentList '--game', $folder` (no `--onitest-` arguments, so the process is found by `-Any` and StartTime as now, :261). Rest of the follow loop (:265-286) stays valid: a Moono restart goes through `Restarter`, as in `integration-testing.md` / OM-012 report.
- `Both` here is two interactive sessions in a row, the second waiting on the first being closed by hand; propose accepting `Both` but running them in sequence with a printed prompt between (mods.json restored after leg one, rewritten for leg two); see open question 3.

## 4. Risks

1. The direct-launch claim rests on the binary's strings and one Player.log, not on documentation; arguments passing through standalone mode is unproven. One narrow TestRunner run on Moono settles it before more code (matches the plan's Prove step).
2. A directly started Moono needs Steam up; if Steam is down Moono tries to start it and waits (log lines above), which adds boot time inside the 120 s start wait (:543). Raise or keep the wait.
3. "Started directly, not by Steam": Steam Cloud syncs on the next start from Steam; the saves path and `SaveFilenameKey` handling are unaffected, but cloud syncing of the player's saves is deferred. Test runs write no saves outside the run folder (harness), so this is a note, not a blocker.
4. Moono's crash-watcher is a second `OniMoono.exe`; `-Any` pre-checks count it as a game, so a leftover watcher could block a launch; the Marker path ignores it.
5. Pid change if Moono re-execs itself (GC relaunch or restart): the warden holds the first pid. OM-012 review item 5 (02-review-report.md:58) already flagged the one-process watch.
6. Moono and stock share Documents\Klei and the Player.log path; stock writes the same file, so a leg's log must be copied before the next leg starts (:556).
7. Stale doc text: GameProcess.ps1:4-10, TestRunner README:42-46, ModTest README:27-30, code-style.md:60, integration-testing.md:203-207 all say launch options.
8. SaveRunner and WorldgenDriver stay on Steam and use the same `Get-GameProcess`; any change to its signature or a `-Runtime` filter must not break their calls (Invoke-SaveRun.ps1, Invoke-Worldgen.ps1).
9. A Moono install update (the folder is third-party and moving: README still old) can change flags; pin the commit in the summary.

## 5. Open questions for the user

1. Which Moono exe is canonical for the tools: `%LOCALAPPDATA%\oni-moono\player` (setup.ps1's install, currently identical) or `F:\modding\oni-moono\player`; or an env/parameter only?
2. For `Both`, order and failure policy: stock then Moono always, and does a failure on the first leg skip the second?
3. ModTest `Both`: two sequential manual sessions, or only `Stock|Moono` for ModTest (the plan Decisions line says -Runtime Stock|Moono|Both for both tools)?
4. OK to pin `--game <folder>` on every Moono launch (avoids the "which install" dialog), at the cost of ignoring a different choice stored in Moono's launch.json?
5. Cloud-sync deferral from a direct start (risk 3): acceptable for test runs with the player's saves untouched?

## 6. Store notes for the coordinator

- `ks brief` pointers: all applied or were neutral; TestRunner `code-style.md:60` carries stale launch-options text. Nothing governing was missing. No friction with `ks` to record.
