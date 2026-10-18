---
doc_id: doc-4d8a297d3589
description: "TestRunner glossary: the abbreviations ONI, SO, HUD and DLL, and definitions of harness, kit, fixture, fixture area, layer, run and live session, warden, wait, box, loadout, generated world and self-check"
---

# TestRunner glossary

Terms a newcomer to TestRunner needs. Entity detail is in [entities](entities.md).

## Abbreviations and Acronyms
**Id:** blk-f88dca2a1b27  
**Tags:** game-mode

| Abbreviation | Full Term | Context |
|--------------|-----------|---------|
| ONI | Oxygen Not Included | the game |
| SO | Spaced Out | the expansion; its mode loads `testbed-so.sav`, the only fixture supplied |
| HUD | heads-up display | the driver waits up to 60 s for it before the first test |
| DLL | test assembly | `<Mod>.Tests.dll`, copied to `DIR/tests` |

## Cross-Domain Terms
**Id:** blk-d5cc7906d14d

### Harness
**Id:** blk-0c81d2ef6d47  
**Definition**: the TestRunner mod itself: the code in the game that loads the fixture, steps tests and reports. Versus the kit, which is what tests call.  
**Used in**: `Harness.cs`, `README.md`.

### Kit
**Id:** blk-cddba398d1da  
**Definition**: `Isochronous.TestKit` in `Kit/`: the test context, assertions, attributes and helpers a test project references.  
**Used in**: every test project.

### Fixture
**Id:** blk-4199508c4e2e  
**Tags:** save-files, game-mode

**Definition**: the save a run loads: `fixtures/testbed-so.sav` in Spaced Out mode (the only fixture supplied), `fixtures/testbed-base.sav` in base-game mode (copy it in first), or the `-Fixture` path: sandbox on, no Duplicants, revealed map. Tests never change the file; the launcher copies it into the run folder.  
**Used in**: `Session.Continue`, the launcher.

### Fixture area
**Id:** blk-a0cfe36dee6e  
**Tags:** test-cleanup  
**Definition**: the 40 by 10 window of the active world that the harness picks and clears for tests (`FixtureArea`); `t.Rect(w, h)` gives a clear rectangle inside it with a one-cell margin.  
**Used in**: `FixtureArea.cs`, `TestContext`.

### Layer
**Id:** blk-46885449ff2a  
**Tags:** simulated-input  
**Definition**: where a test drives the game: 0 the game's API, 1 the tool (real `InterfaceTool` input), 2 UI, 3 input. Recorded with `[Layer(n)]` and in the results; layers 2 and 3 are not built.  
**Used in**: `LayerAttribute`, `TestResult`.

### Run, session and live session
**Id:** blk-be133b9156d5  
**Tags:** live-session  
**Definition**: a run is one launcher invocation's tests. A session is one game process taking runs. A live session is a game that stays up 15 s after a run and takes the next one with the same loadout from its queue.  
**Used in**: `Session.cs`, the launcher, `Watch-OniGame.ps1`.

### Warden
**Id:** blk-17051b9bdfa7  
**Tags:** mod-loadout, live-session  
**Definition**: `Watch-OniGame.ps1`, the detached process that restores `mods.json` and clears the live-session folder when the game exits.  
**Used in**: the launcher, `Watch-OniGame.ps1`.

### Wait
**Id:** blk-12fc1ae73c38  
**Tags:** test-pacing, watchdog  
**Definition**: an object a test yields (`t.Frames`, `t.WaitGameSeconds`, `t.WaitRealSeconds`, `t.WaitUntil`); the driver polls it once per frame against the test's deadline and fails the test with its description when it times out.  
**Used in**: `Kit/TestContext.cs`, `TestBehaviour.cs`.

### Box
**Id:** blk-40af48693237  
**Definition**: the Duplicants' box: tiles walling the fixture area with air inside and a dev life support, made by `t.Dupes.Box()`; `[OniTest(Boxed = true)]` keeps it between consecutive tests.  
**Used in**: `Kit/Dupes.cs`, `FixtureArea.cs`.

### Loadout
**Id:** blk-90066b0e7a52  
**Tags:** mod-loadout  
**Definition**: the list of enabled mods in `mods.json`. A run enables only TestRunner, the mods under test, `-With` and `-Steam`; the player's is backed up and restored.  
**Used in**: the launcher, the warden.

### Generated world
**Id:** blk-4d6d5f72d7c2  
**Tags:** generated-worlds, game-mode  
**Definition**: a world made fresh from a coordinate such as `SNDST-C-100-0-0-0` (the letter before the seed is `C` for Spaced Out, `A` for base game) for tests with `Worldgen = ...`.  
**Used in**: `Worldgen.cs`.

### Self-check
**Id:** blk-14e9c9072d5c  
**Tags:** reflection, game-update-compatibility  
**Definition**: `Reflect.SelfCheck()`, run before the first test, listing the private game members that no longer resolve.  
**Used in**: `Reflect.cs`.

## Related
**Id:** blk-db910f4597a3

- See also: [entities](entities.md)
