# Test Runner

A development-only mod plus a launcher that run a mod's in-game tests in the real game with nobody
at the keyboard. Started with `--onitest-dir=DIR --onitest-save=FILE`, the mod loads the fixture
save from the main menu, blocks every save to disk outside `DIR`, runs every `[OniTest]` coroutine
found in `DIR/tests/*.dll`, writes one JSON line per test to `DIR/results.jsonl` and a status line
to `DIR/results.jsonl.status`, and quits. Without the arguments the mod does nothing, so it can stay
installed.

The tests drive the game the way a player does where that is where the bugs live: the real
`InterfaceTool` fed the same `OnMouseMove`/`OnLeftClickDown`/`OnLeftClickUp` calls the player
controller makes (layer 1), with the game's own API for setup and oracles (layer 0). The design,
including the UI and input layers still to come, is in `incubator/test-automation/PLAN.md`.

## Running

```
pwsh common/tools/TestRunner/Invoke-OniTests.ps1 -Mods DirectAutomationBridges
pwsh common/tools/TestRunner/Invoke-OniTests.ps1 -Mods DirectAutomationBridges -Filter "*DragFromEnd*" -KeepGame
```

The launcher builds `<repo>/src/<Mod>.Tests/<Mod>.Tests.csproj` (which builds and deploys the mod
and this harness), checks the test DLL against the game assemblies with `tools/ModCheck`, copies it
into `runs/<label>-<stamp>/tests/`, rewrites `mods.json` so that only TestRunner, the mods under
test and `-With` are enabled, launches the game through Steam, waits for it to quit, restores
`mods.json` and prints a line per test plus the session status. Steam must be running. Exit code 0
when every test passed, 1 when one failed, errored or timed out, 2 when the session never produced a
status (launch, load or crash: read `Player.log` in the run folder), 3 for build or check failures.

The fixture is `fixtures/testbed-so.sav` or `fixtures/testbed-base.sav` by the game's current mode
(`-Fixture` overrides); see `fixtures/README.md`. The harness picks a 40x10 test area in the fixture
world itself and clears it before the first test and after every test.

## Writing tests

```
src/<Mod>.Tests/<Mod>.Tests.csproj     references the mod and common/tools/TestRunner/TestRunner.csproj, ModDeployFolder none
src/<Mod>.Tests/*.cs                   public static classes with [OniTest] methods
```

```csharp
using System.Collections;
using Isochronous.TestKit;

public static class WireDrags
{
    [OniTest("DAB: wire dragged from a bridge end to its neighbour connects both", Timeout = 30), Layer(1)]
    public static IEnumerator DragFromEnd(TestContext t)
    {
        CellRect r = t.Rect(6, 1);                                   // clear cells, r.Cell(x, y) from its bottom-left
        t.World.Build("LogicWireBridge", r.Cell(3, 0));              // layer 0: BuildingDef.Build, instant
        yield return t.WaitUntil(() => BridgeEndNode.TryGet(r.Cell(2, 0), out _), 3, "the bridge's end node");
        UtilityDragSession wire = t.Tools.UtilityTool("LogicWire");  // layer 1: UtilityBuildTool.Activate
        yield return wire.Path(r.Cell(2, 0), r.Cell(1, 0));          // down, move, up, one frame per step
        t.Assert.Eq(Color.white, wire.PreviewTint(r.Cell(1, 0)), "the neighbour previews white");
        yield return t.Frames(2);
        t.Assert.Has(UtilityConnections.Right, t.World.LogicConnections(r.Cell(1, 0)), "the wire draws into the end");
    }
}
```

- A test is `public static IEnumerator M(TestContext t)` with `[OniTest]`; `[Layer(n)]` records
  which layer it drives. Tests run in declaration order, each with a real-time timeout (the
  attribute's `Timeout`, else `-TestTimeoutSec`).
- Yield `t.Frames(n)`, `t.WaitGameSeconds(s)` (scaled sim time), `t.WaitRealSeconds(s)`,
  `t.WaitUntil(cond, timeout, what)` (a timeout fails the test with `what`), or a kit coroutine
  such as `wire.Path(...)`. Plain `yield return null` is one frame.
- `t.World`: `Build(id, cell, orientation, element)`, `At(cell, layer)`, `Destroy(go)`,
  `LogicNetwork(cell)`, `LogicConnections(cell)`.
- `t.Tools.UtilityTool(id, element)`: the real utility build tool; `Path(cells...)` visits each
  cell with a frame between (a careful drag), `Sweep(from, to)` is one move (a fast swipe);
  `PreviewTint(cell)` is the preview's colour just before the button went up (white: buildable).
  Disposed, or at cleanup, the select tool is back and any drag is cancelled.
- `t.Assert`: `True`, `False`, `Eq`, `NotEq`, `Null`, `NotNull`, `Has(flag, bits)`, `Lacks`, `Fail`.
- `t.Log(text)` goes into the result's notes and `Player.log`; `t.Cleanup(action)` runs after the
  test whatever happened; `t.AllowLogError(regex)` for an error the game is expected to log,
  because any `Error`, `Exception` or `Assert` logged during a test fails it otherwise;
  `t.Skip(reason)`.

The harness forces the game's focus flags on (unfocused, the game drops every click), locks the
mouse position and ignores the physical keyboard and mouse for the session, turns instant build on
the way the debug key does, and runs the sim unpaused at speed 1. Every private game member it reads
is listed in `Reflect.cs` with a self-check that fails the run with the member's name after a game
update renames it.

## Output

`runs/<label>-<stamp>/`: `results.jsonl` (per test: `id`, `name`, `layer`, `status` pass/fail/
error/timeout/skipped, `message`, `stack`, `durationSeconds`, `gameSeconds`, `logErrors`, `notes`),
`results.jsonl.status`, `summary.txt`, `Player.log`, `mods.json.used`, the fixture copy and the
checked DLLs.

## How it works

`Patches.cs`: `MainMenu.OnSpawn` notes the time and `App.LateUpdate` calls `LoadScreen.DoLoad`
four seconds later (SaveRunner's lesson: loading straight from OnSpawn tore the menu down under a
popup still laying out); `Game.OnSpawn` adds `TestBehaviour` to the Game object; `SaveLoader.Save`
is blocked outside the run folder; `KInputManager.OnApplicationFocus` and
`InterfaceTool.OnApplicationFocus` re-force the focus flags. `TestBehaviour` waits for the HUD,
throws the switches (`Harness.cs`), finds and clears the area (`FixtureArea.cs`), then steps each
test's enumerator by hand: an exception inside a Unity-driven coroutine cannot be caught by its
starter and a timeout cannot stop it, so the driver calls `MoveNext` itself inside try/catch,
interprets the yielded value (a nested enumerator, a kit `Wait` polled per frame, a Unity yield
instruction, or null for a frame) and checks the deadline at every step. Errors the game logs during
a test are captured through `Application.logMessageReceivedThreaded`.
