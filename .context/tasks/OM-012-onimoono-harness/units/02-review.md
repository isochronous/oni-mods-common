Feature: TestRunner follows a game started through oni-moono, and an open Moono game blocks a run
Ticket: OM-012
Lens: review (read-only; write only your report). Do not launch the game.
Diff: in C:\dev\ONI-Mods\common, `git diff 5008da9..HEAD -- tools/`
Changed files:
  - tools/TestRunner/GameProcess.ps1 (new): Get-GameProcess (OxygenNotIncluded, or OniMoono with --onitest- in its command line; -Id; -Any = any OniMoono too)
  - tools/TestRunner/Invoke-OniTests.ps1: every game lookup uses it; -Any at the pre-launch "already running" checks (:216 :219 :221 :233) and :577
  - tools/TestRunner/Watch-OniGame.ps1: Find-Game uses it
  - tools/TestRunner/README.md: launching/warden sentences
Context: plan common/.context/tasks/OM-012-onimoono-harness/plan.md; implement report units/01-implement-report.md; evidence C:\dev\ONI-Mods\.context\tasks\OM-011-fridge-thresholds-patch\units\04-test-report.md "Run 5 (oni-moono)".
Review focus:
  - Each call site uses the right mode: pre-launch refusal and leftover-file decisions use -Any; the start wait, live-game lookups and warden follow only the test game. Check :577 specifically: what decision is it, and is -Any right there (e.g. would it make the warden or end-of-run wait on a hand-started Moono)?
  - A plain OxygenNotIncluded launch behaves exactly as before (same objects, same timing).
  - The warden restores mods.json after whichever process exits; no path where it stops while the test game is still running.
  - WMI CommandLine access: failure modes (access denied, process gone between list and query, null CommandLine) handled without throwing or misclassifying; cost if called in a tight loop.
  - Nothing foregrounds or polls the game window.
Report: C:\dev\ONI-Mods\common\.context\tasks\OM-012-onimoono-harness\units\02-review-report.md — findings with severity and file:line, each with the fix expected; "no findings" only with the passes run.
Model tier: default
