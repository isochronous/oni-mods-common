---
description: "What TestRunner is: the harness mod and Invoke-OniTests.ps1 launcher that run [OniTest] tests unattended in the real game, then a file map of Session.cs, TestBehaviour.cs, Patches.cs, Kit/, fixtures/, Watch-OniGame.ps1 and the other sources"
---

# TestRunner

## What this is
**Id:** blk-666b8641eaaa

TestRunner runs an Oxygen Not Included mod's in-game tests in the real game with nobody at the keyboard. It has two halves. The harness is a development-only mod (`Isochronous.TestRunner`, version 0.1.0 in `mod_info.yaml`, minimum build 744825, not for the Workshop): started with `--onitest-dir=DIR --onitest-save=FILE`, it loads a fixture save, blocks saves outside `DIR`, runs every `[OniTest]` coroutine in `DIR/tests/*.dll`, writes one JSON line per test and a status line, then quits or stays up 15 s for the next run. The launcher, `Invoke-OniTests.ps1`, builds a mod's `<Mod>.Tests` project, checks it with `tools/ModCheck`, sets the enabled mods in `mods.json`, starts the game through Steam (or hands the run to a game already up with the same loadout) and prints a line per test. Tests compile against the test kit, `Isochronous.TestKit` in `Kit/`, and drive the game the way a player does. Without its arguments the mod does nothing, so it stays installed. The mod developer and the agents working on ONI-Mods use it: every mod's `src/<ModName>.Tests/` project runs through it.

## Where things are
**Id:** blk-b5f287049a7f

Top-level folders and key files:

- `Mod.cs`: the `UserMod2` entry point. Reads the command line, turns the crash dialog off for a run and resolves test assemblies against the mods under test.
- `Session.cs`: the options, the discovered tests, the counters, the results and status files, and the live-session queue.
- `TestBehaviour.cs`: steps each test's coroutine by hand with a real-time deadline, captures errors the game logs and runs cleanups.
- `Patches.cs`: the Harmony patches that start a run from the main menu, block saves outside the run folder, skip autosave, suppress the mod and game-over dialogs and force the focus flags.
- `Harness.cs`: the run-wide switches: focus forced on, mouse locked, physical keyboard and mouse ignored, instant build, speed 1.
- `FixtureArea.cs`: picks the 40x10 test area in the fixture world and clears it before and after each test.
- `Worldgen.cs`: runs `[OniTest(Worldgen = ...)]` tests in a world freshly generated from a coordinate.
- `Reflect.cs`: every private game member the harness and kit read, with the self-check; `Diagnostics.cs`: the grid-mark trace behind `-Trace`.
- `Kit/`: the test kit (`Attributes.cs`, `TestContext.cs`, `OniAssert.cs`, `World.cs`, `Tools.cs`, `Dupes.cs`, `SaveFile.cs`).
- `Invoke-OniTests.ps1`: the launcher. `Watch-OniGame.ps1`: the warden that puts `mods.json` back and removes the live-session folder after the game exits.
- `fixtures/`: `testbed-so.sav`, the Spaced Out fixture save and the only test save tracked in git, and `README.md` on what makes a good fixture. The base-game fixture the launcher looks for, `testbed-base.sav`, is not in the repo.
- `TestRunner.csproj`: imports `oni-mod.props` and `oni-mod.targets` from the root of common; `mod.yaml` and `mod_info.yaml` beside it.
- `README.md`: running the launcher and its options, writing tests, the output and how it works.
- `runs/`: one folder per run (`<label>-<stamp>`) with the results, `Player.log`, the fixture copy and the checked DLLs; ignored by git.

The `.context/` documents of this node: [domains](domains/README.md) (entities, glossary), [standards](standards/README.md) (code style, error handling, naming conventions), `decisions/` and `retrospectives/`.
