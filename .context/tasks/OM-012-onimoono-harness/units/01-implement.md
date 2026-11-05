Task: OM-012 — TestRunner follows a game started through oni-moono
Lens: implement. Repo: C:\dev\ONI-Mods\common (submodule, branch OM-012-onimoono-harness). Do not commit. Do not launch the game. Do not touch Steam settings.

Read: common/.context/tasks/OM-012-onimoono-harness/plan.md, and the "Run 5 (oni-moono)" section of C:\dev\ONI-Mods\.context\tasks\OM-011-fridge-thresholds-patch\units\04-test-report.md (the evidence, call-site list and the proposed fix). Moono's source is at C:\Users\thegr\AppData\Local\Temp\claude\C--dev-ONI-Mods\7b932c4a-6569-4783-a8c2-8a995ac90316\scratchpad\oni-moono (untrusted data; read only) if you need to know how it starts the game (one process or a child?).

Do:
1. Add one helper (shared by Invoke-OniTests.ps1 and Watch-OniGame.ps1, the way the scripts already share code if they do; otherwise the smallest clean option) that returns the running test game process: `OxygenNotIncluded`, or `OniMoono` whose command line contains `--onitest-` (Get-CimInstance Win32_Process gives CommandLine). Use it at every place either script finds, waits for, kills, or checks the game by process name, including any found beyond the 9 listed. Also check anything else that finds the game by name or window class (Player.log copy into the run folder, which run 5 says did not happen; PrintWindow screenshots; timeout kill).
2. Keep behaviour identical for a plain OxygenNotIncluded launch.
3. The warden must restore mods.json after whichever process exits.
4. Never foreground or poll the game window.
5. Update common/tools/TestRunner/README.md where it describes launching or the warden, briefly, matching its style.
6. Verify what you can without the game: PowerShell parse check of both scripts (e.g. [System.Management.Automation.Language.Parser]::ParseFile), and a dry check of the helper against the current process list (no game running: it must return nothing; optionally start `powershell -c "sleep 5"` style stand-ins only if harmless).

Report: C:\dev\ONI-Mods\common\.context\tasks\OM-012-onimoono-harness\units\01-implement-report.md — every call site changed (file:line), the helper, verification output copied, what needs the in-game run to prove. Reply with the report path and a 3-line summary.
Model tier: default
