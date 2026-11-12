# Unit 03 report: why `OniMoono.exe --game` also starts the stock game

Read only. Nothing launched. Third-party files and logs read as data. Decompile output went to the session scratchpad (`...\scratchpad\dec`), not the repo.

## Cause

The game's own SteamManager calls `SteamAPI.RestartAppIfNecessary(457140)`. Moono's `--game` mode does not set `SteamAppId`, so Steam's API sees a process not started by Steam and asks Steam to start the game itself. Steam starts the stock `OxygenNotIncluded.exe` (with the process's command-line arguments, which is what raises the "launch with custom arguments" prompt).

Evidence:

1. Decompile of `OxygenNotIncluded_Data\Managed\Assembly-CSharp-firstpass.dll`, type `SteamManager`, lines 78-85:
   `if (SteamAPI.RestartAppIfNecessary(new AppId_t(457140u))) { App.Quit(); return; }`
2. Hand-launch log `...\scratchpad\moono-launch.log`, lines 1-2: `[engine] Moono starting: ... exe=F:\modding\oni-moono\player\OniMoono.exe game=F:\SteamLibrary\...\OxygenNotIncluded` and `[engine] launch: direct (--game)`. Lines 103-126: `Creating SteamManager.` then `Native steam_api64: ...` then
   `[engine] Application.Quit(0) requested by ... App:Quit() SteamManager:Awake() ... DistributionPlatform:Initialize() LaunchInitializer:Update()`
   then `[ERROR] Steam not initialized in time.` That is the RestartAppIfNecessary branch returning true (App.Quit from SteamManager.Awake). Moono's Quit is logged but loading carried on to the main-menu/mod phase, so Moono's window kept loading while Steam started the stock game (stock exe at 18:18:26, seven seconds after the Moono start).
3. Strings in `F:\modding\oni-moono\player\OniMoono.dll` (UTF-16, offsets): `launch: from Steam's launch options` 0xb6da0, `launch: direct (--game)` 0xb6e06, the standalone group 0xb6e36-0xb7244, then `SteamAppId` 0xb75eb and `457140` 0xb7601 sitting right after the standalone strings (0xb701f `launch: standalone: `, 0xb7196 `the game at `, 0xb743b `Steam isn't running`). No `SteamAppId`/`457140` literal belongs to the direct or Steam-options groups; the only other 457140 literals are `appmanifest_457140.acf` (0xb7d7c) and `steam://nav/games/details/457140` (0xb73f9, 0xda1d2). No `steam://run`, `-applaunch`, or `steam_appid.txt` string in OniMoono.dll, OniMoono.Desktop.dll or OniMoono.exe. So only standalone mode sets SteamAppId itself; Steam-options mode is started by Steam (Steam sets it); `--game` mode has neither.
4. No `steam_appid.txt` in the game folder (listing of `F:\SteamLibrary\steamapps\common\OxygenNotIncluded` shows no .txt) or the player folder.
5. Unit 01 found Player.log showing `launch: standalone: the game at F:\SteamLibrary\...` and `started directly, not by Steam`, i.e. standalone mode has already run on this machine (that session's way of starting it is not recorded).

Inferred, not seen: that the Steam custom-arguments prompt comes from RestartAppIfNecessary relaunching with the arguments (`--game`, `--log`, `--onitest-*`). It fits the symptoms (prompt plus stock exe plus the runner's 120 s of nothing while the prompt waits for a click), but no Steam log was read. The runner's silent run is consistent: stock was just closed, RestartAppIfNecessary handed off, no Player.log from Moono.

## Recommended runner launch

Start Moono with `SteamAppId=457140` in its environment, which is exactly what standalone mode does internally:

- Option A (preferred, matches Moono's own design): no `--game`, no `--`: `OniMoono.exe --onitest-dir=... --onitest-save=...`. Standalone finds the game, sets SteamAppId, starts Steam if needed. Risks: the "which install" dialog if several installs exist (env `ONI_MOONO_CHOOSE_GAME`, `launch.json`); whether `--onitest-*` reach the game in this mode is unproven (usage line says unknown arguments are passed to the game).
- Option B: keep `--game <folder>` and set the environment variable on the child: in PowerShell set `$env:SteamAppId = '457140'` before `Start-Process`, restore after (or `ProcessStartInfo.Environment`). `RestartAppIfNecessary` is documented to return false when the SteamAppId environment variable (or steam_appid.txt) is present, so the game should initialise against the running Steam client and not relaunch. Pins the folder, avoids the dialog. Unproven for Moono: it may pass the variable on to steam_api64 unchanged (the log shows the real steam_api64.dll is loaded).
- Do not write a `steam_appid.txt` into the game or player folder (changes third-party/game files).
- Steam must be running and logged in (Moono waits and can start it; `ONI_MOONO_STEAM_START` is a test switch for its own tests).

## Runner changes this implies (Invoke-OniTests.ps1, GameProcess.ps1)

- Moono leg: `Start-Process $MoonoExe` with `--game <Get-GameFolder>` (Option B) plus `SteamAppId=457140` in the environment, and the `--onitest-*` arguments without `-applaunch 457140`. Scope the env change to the launch (set, start, restore in `finally`).
- Stock leg stays `steam.exe -applaunch 457140 ...` (:522, :542).
- Detection: stay with `Get-GameProcess` by the `--onitest-` marker; after launch check that no `OxygenNotIncluded` process appeared within the start wait. If one appears for a Moono run, fail with "Steam relaunched the stock game (SteamAppId missing)" rather than testing the wrong runtime. A stock exe of ours is recognised by `Get-GameProcess` as a game anyway, so add a runtime check (process name) to avoid it being taken for the Moono session.
- Start wait (:543-544, 120 s): keep or raise; Moono waits for Steam when it is down.
- Update stale text: GameProcess.ps1:4-10, TestRunner README:42-46, code-style.md:60, ModTest README:27-30.

## Open questions

1. Does `SteamAppId` in the child environment really suppress the relaunch under Moono's `--game` mode (Option B)? Unproven; decompile shows only that standalone sets it.
2. Do `--onitest-*` arguments reach the game in standalone (Option A) and in `--game` mode? The hand log shows `game arguments: none` because none were passed.
3. Does the prompt appear in standalone mode? Unit 01's evidence says standalone ran without it, not recorded either way.

## Narrowest launch to settle it (Jeremy must approve)

One TestRunner run through the runner with a minimal loadout (tool plus needed mods only, `-Only`-style, the smallest `-Filter` test, `-Runtime Moono` prototype using Option B): `OniMoono.exe --game <folder> --onitest-...` with `SteamAppId=457140` set. Pass criteria: Moono's Player.log line 2 reads `launch: direct (--game)`, `game arguments:` lists the `--onitest-` arguments, no `OxygenNotIncluded` process appears, no Steam prompt, the test result file is written. If the stock exe or the prompt still appears, fall back to Option A with the same run. Do not test with a full ~150-mod loadout (boots take minutes).

## ICON notes for the coordinator

- Handoff packet pointers: neither opened nor needed; the question is about third-party launch behaviour and TestRunner records add nothing beyond Unit 01's findings. Nothing governing was missing from the brief that I could see.
- ICON tooling friction: none (I did not call `ks`).
