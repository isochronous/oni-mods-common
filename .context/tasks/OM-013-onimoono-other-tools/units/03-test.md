Task: OM-013 — in-game checks through oni-moono for SaveRunner and WorldgenDriver
Lens: test. Repo C:\dev\ONI-Mods\common, branch OM-013-onimoono-other-tools. Do not commit, do not edit code, never edit Steam settings (launch options start the game through oni-moono; leave them).
Jeremy approved these checks 2026-11-04 and is asleep: there is no one to ask. Do NOT run ModTest (it needs a human to quit the game).

Rules (Jeremy's, hard):
- Sequential, one tool at a time. Exactly ONE run per tool. A second run of a tool only if the first clearly failed for an environmental reason you can name (not a harness bug) — at most one, and never a loop. A tool whose run fails for a reason you can't name: stop that tool, collect logs/screenshots, report.
- SaveRunner launches have been closed from outside before: if a SaveRunner run is closed externally, do not relaunch.
- Never foreground or poll the game window. Never kill a game you did not start; if a game is already open before a run, stop and report.
- After each run, before the next: confirm no OxygenNotIncluded/OniMoono process is left, and the player's mods.json hash equals its pre-run hash (record the pre-run hash first; the runner's backup is the reference). If mods.json differs and the tool did not restore it, STOP all runs and report; do not try to repair it yourself beyond what the tool's documented leftover-backup restore does.
- Kill nothing mid-run; a PowerShell call is capped at 10 min, so start each run with run_in_background and redirect output to a file in C:\Users\thegr\AppData\Local\Temp\claude\C--dev-ONI-Mods\7b932c4a-6569-4783-a8c2-8a995ac90316\scratchpad, then wait for it to finish.

Runs (from common/.context/tasks/OM-013-onimoono-other-tools/units/01-implement-report.md, "The in-game check that proves each tool"); read each tool's README for exact parameters first:
1. SaveRunner: `tools/SaveRunner/Invoke-SaveRun.ps1 <a small save> -Seconds 10 -Without 2854869130,3281716506 -Label moono`. Pick a small save: prefer a fixture save in common/tools/TestRunner/fixtures (never one of Jeremy's colony saves unless no fixture works; the tool copies the save, never modifies the original). Proof: no "did not start within 120 s"; closing line "Game exited ...; status done" with probe rows > 0 (killed-after-status also proves following); mods.json restored.
2. WorldgenDriver: build it first if needed (`dotnet build tools/WorldgenDriver -c Release`), then `tools/WorldgenDriver/Invoke-Worldgen.ps1 SNDST-A-100-0-0-0 -PerSession 1 -Label moono`. Proof: "session 1 exited: 1 result(s)", not "no-process"; config and mods.json restored.
For each: confirm from the copied Player.log that line 1 is "Moono starting ...".

Report: C:\dev\ONI-Mods\common\.context\tasks\OM-013-onimoono-other-tools\units\03-test-report.md — per tool: exact command, verbatim runner output tail, Moono log line, mods.json hashes before/after, leftover processes, verdict. Reply with a 3-line summary.
Model tier: default
