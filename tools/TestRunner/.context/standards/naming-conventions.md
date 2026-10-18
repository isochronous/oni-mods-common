---
tags: code-style
doc_id: doc-e555d0ee573e
description: "Naming in TestRunner: files and run folders, the two namespaces, patch classes, Reflect handles, Wait classes, attributes, test classes and ids, the --onitest-* options, Invoke-OniTests.ps1 parameters, live-session request files and result fields"
applies_to:
  - "*.cs"
  - "Kit/**/*.cs"
  - "*.ps1"
  - "*.csproj"
  - "*.yaml"
---

# Naming Conventions

## Files and Directories
**Id:** blk-dbf5ebc0be89  
**Tags:** game-mode, save-files, live-session, mod-build

- C# files are PascalCase and named for the main type they hold (`Session.cs`, `Harness.cs`, `FixtureArea.cs`). `Patches.cs` holds every patch class; `Kit/` holds one file per kit area (`Dupes.cs`, `Tools.cs`, `World.cs`, `OniAssert.cs`, `TestContext.cs`, `Attributes.cs`, `SaveFile.cs`).
- Scripts use PowerShell Verb-Noun names: `Invoke-OniTests.ps1`, `Watch-OniGame.ps1`.
- The mod's folder, assembly (`TestRunner`) and deployed folder share one name. `mod.yaml` carries `staticID: "Isochronous.TestRunner"`; the title is "Test Runner (dev tool)".
- The launcher picks the fixture by the game's current mode: `fixtures/testbed-so.sav` for Spaced Out, `fixtures/testbed-base.sav` for base game. Only `testbed-so.sav` is supplied (it is tracked in common). A base-game run needs `testbed-base.sav` copied into `fixtures/` or a `-Fixture <path>` argument; without one the launcher throws `Fixture save not found`.
- Run folders are `runs/<label>-<yyyyMMdd-HHmmss>/` (ignored by `common/.gitignore`) holding `tests/`, `check/<Mod>/`, `fixture.sav`, `results.jsonl`, `results.jsonl.status`, `summary.txt`, `Player.log`, `mods.json.used`, `mods.json.real` and `artifacts/`. The label is short and names the work (`bs-cleanup`, `bs-bao`).
- The live session folder is `%LOCALAPPDATA%\IsochronousOniTests\live` (with `queue`, `state`, `game.pid`); the warden's log sits next to it.
- A mod's test project is `src/<Mod>.Tests/<Mod>.Tests.csproj`; its assembly is `<Mod>.Tests`.

## Code Naming
**Id:** blk-81d3dab6af5e  
**Tags:** harmony-patching, reflection, test-discovery, test-assertions, mod-entry-point

- Namespaces `Isochronous.TestRunner` (harness) and `Isochronous.TestKit` (kit).
- Types PascalCase, locals and parameters camelCase, private fields camelCase with no prefix. The harness's state lives in public static fields of static classes named for the concern (`Session`, `Harness`, `FixtureArea`, `Worldgen`, `Reflect`).
- The mod class is `TestRunnerMod : UserMod2`; the driver is `TestBehaviour`.
- Patch classes are `<Type>_<Method>_Patch`; trace patches end in `_Trace`.
- Reflection handles in `Reflect` are named `<Type><Member>` (`KInputManagerHasFocus`, `InterfaceToolIsAppFocused`, `PathNodeCell`); the self-check message uses the game's own `Type.member`.
- Waits are classes ending in `Wait` deriving from `Wait` (`FrameWait`, `GameSecondsWait`, `UntilWait`); the `TestContext` factory methods that make them are `Frames`, `WaitGameSeconds`, `WaitRealSeconds`, `WaitUntil`.
- Attributes end in `Attribute` and are used without the suffix: `[OniTest]`, `[Layer]`. `OniTest` options are PascalCase properties: `Timeout`, `FreshSave`, `KeepSave`, `Explicit`, `Boxed`, `NoColony`, `Worldgen`.
- Test classes are `public static` classes named for the area under test (`WireDrags`); test methods are `public static IEnumerator <Name>(TestContext t)` named for what is checked (`DragFromEnd`). The test id is `<Assembly>/<Class>.<Method>`. The attribute's display name starts with a short mod prefix (`"DAB: wire dragged from a bridge end to its neighbour connects both"`).
- Assertion methods are short (`True`, `False`, `Eq`, `NotEq`, `Null`, `NotNull`, `Has`, `Lacks`, `Fail`) and take the message last.

## Command Line and Launcher
**Id:** blk-afb883826043  
**Tags:** command-line, live-session, game-launch

- Game arguments are `--onitest-<option>=value`, lower case, hyphen-separated: `--onitest-dir`, `--onitest-save`, `--onitest-filter`, `--onitest-timeout`, `--onitest-test-timeout`, `--onitest-keep`, `--onitest-trace`, `--onitest-server`.
- Launcher parameters are PowerShell PascalCase (`-Mods`, `-Filter`, `-Fixture`, `-With`, `-Steam`, `-TimeoutSec`, `-TestTimeoutSec`, `-KeepGame`, `-BuildOnly`, `-NoBuild`, `-NoCheck`, `-BuildArgs`, `-Label`, `-Trace`).
- `-Filter` is one word with no spaces (it travels through Steam's command line); globs use `*` and `?`, several separated by `;`.
- Run folder paths contain no spaces for the same reason.
- Live-session request files are `<run>.req` with `key=value` lines, renamed `.taken` when the game takes them; `quit.req` ends the session.

## Results
**Id:** blk-982a2140c0af  
**Tags:** run-result

`results.jsonl` fields are lower camel case: `id`, `assembly`, `class`, `method`, `name`, `layer`, `status`, `message`, `stack`, `durationSeconds`, `gameSeconds`, `logErrors`, `notes`. Statuses are lower case words: `pass`, `fail`, `error`, `timeout`, `skipped`. Screenshots are `artifacts/<Assembly>.<Class>.<Method>-<name>.png`.

## Related
**Id:** blk-86f3ccc4f182

- See also: [code style](code-style.md)
