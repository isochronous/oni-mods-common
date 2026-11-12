### OM-014: TestRunner and ModTest on stock, oni-moono or both
**Id:** blk-8b9ccc27a1b6

- **Avoid**: launching an external program from a runner without capturing its exit code, stdout/stderr and own log from the first attempt: the first real Moono run said only "Game exited after 2 s" and blamed a stale Player.log, and it took a second launch to see exit code 2 ("Unknown option", printed to a stderr a GUI-subsystem exe has nowhere to show). Never read a log older than the launch. Don't commit in `common/` between runs that should share a live game (the build stamps the commit into TestRunner.dll), and don't use a console-subsystem stand-in for a GUI program (where.exe left Windows Terminal windows open on exit code 2). A bug found in the task's own diff is fixed in the task, not noted as a minor (Jeremy, 2026-11-12).
- **Repeat**: make the failing path explain itself before guessing a fix (exit code + log tail + stderr turned an unknown 2-second exit into one line), then read the program's own code for the cause (OniMoono's option parser; the source is on GitHub). Prove each path with the narrowest in-game run, and failure paths with a stand-in exe through `-MoonoExe`; check mods.json by hash before and after.
- **Updated**: ONI-Mods `testing/integration-testing.md` ("Launching with oni-moono"), common tools/TestRunner `domains/entities.md` (Invoke-OniTests.ps1, Business Rules) and `standards/code-style.md` (Layout: GameProcess.ps1), common `domains/tooling-friction.md` (ks read on a packet)


### OM-013: SaveRunner, WorldgenDriver and ModTest follow a game started through oni-moono
**Id:** blk-062fcdde7835

- **Avoid**: Copying a proof command into a test spec without reading the tool's parameters: the SaveRunner check had no -Probe, so its 'probe rows > 0' proof could never pass. Also: SaveRunner on Moono ran game time at about 1/100 speed (first update logs time scale 0), cause unknown; it was left for Jeremy instead of chasing it unattended.
- **Repeat**: Reusing one helper with a marker parameter instead of a copy per tool, proven by stand-in processes per marker plus the earlier tool's stand-in script run unchanged. An unattended run spec with hard stop rules (one run per tool, hash mods.json before and after, stop on an unnamed failure) ran clean while Jeremy slept.
- **Updated**: standards/code-style.md in tools/TestRunner (GameProcess.ps1 row: -Marker, shared with the other tools), tools/SaveRunner (Layout: Invoke-SaveRun.ps1 uses Get-GameProcess -Marker '--saverun') and tools/WorldgenDriver (Imports/Includes: Get-GameProcess -Marker '--worldgen-'). In the parent store, testing/integration-testing.md 'Launching with oni-moono' drops the caveat that the other tools need the launch options cleared.


### OM-012: TestRunner follows a game started through oni-moono
**Id:** blk-aa6fccf8d4a0

- **Avoid**: Nothing went wrong in this task; the failure it fixed came from OM-011 assuming the game always runs as OxygenNotIncluded. SaveRunner, WorldgenDriver and ModTest still make that assumption (found in review, out of scope; follow-up).
- **Repeat**: Proving a process-detection change without the game first: a parse check of every script plus renamed cmd.exe stand-ins for both process names, with and without the test arguments. Then one approved in-game run that checks each claim (launcher found the game, Player.log copied, warden restored mods.json when OniMoono exited, nothing left running).
- **Updated**: tools/TestRunner standards/code-style.md: Layout lists ModConfig.ps1, GameMode.ps1 and GameProcess.ps1 (Get-GameProcess and its -Any mode); Indentation covers every .ps1 in the tool. In the parent ONI-Mods store, testing/integration-testing.md 'Launching with oni-moono' now says runs work through Moono.


### OM-005: overviews and the description, coverage and tag backfills over common
**Id:** blk-2fd7612e80b0

- **Avoid**: Writing an aggregator overview's paths relative to its own folder (`Release/release.py`): source-mention matches store-relative paths, so `tools/.context/overview.md` was unreached by every brief until review 1.1 rewrote them. Also deciding a whole-tool term per node during vocabulary checking: in-game-testing was dropped from TestRunner as a place while steam-workshop, translations and generated-worlds stayed elsewhere, and the review had to impose one rule afterwards.
- **Repeat**: Driving the six nodes through OM-002's tagwork driver retargeted at the common store (survey, chunks, begin/complete passes, check, write): 10 chunks validated and written in 10-17 rounds per node with no refusal. A review pass, a fix unit, and a confirmation review before closing.
- **Updated**: Decisions in OM-005's plan.md: whole-tool terms sit at document level on overview, domains/README, entities and glossary of each tool node; a term with one carrier is retired. Six friction entries in the ONI-Mods log.


### OM-004: populate-context over common's six tool nodes
**Id:** blk-a823ce0beb55

- **Avoid**: Drafting units writing claims from READMEs and from one example instead of the code: the review found 14 statements false against the code across 30 documents, among them usage errors said to skip SteamAPI.Init, a Reflect.cs-only rule that Worldgen.cs breaks, and testbed-base.sav presented as a tracked fixture. That last one is the same mistake OM-001's review caught in the ONI-Mods testing documents. A spec for documents about a test harness names the tracked fixtures (only testbed-so.sav is in the repository) up front.
- **Repeat**: Planning and applying every change-set with --at common, since the engine treats the submodule as its own store: 6 creates and the fix change-sets landed with no refusal. A review pass that checks every concrete claim against the code before anything is called done.
- **Updated**: Nothing promoted: the fixture fact is now stated in the TestRunner documents themselves and in the ONI-Mods testing documents.
