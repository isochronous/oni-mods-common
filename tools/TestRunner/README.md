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
test and `-With` are enabled, launches the game through Steam, waits for the run's results and
prints a line per test plus the session status. Steam must be running. Exit code 0 when every test
passed, 1 when one failed, errored or timed out, 2 when the session never produced a status (launch,
load or crash: read `Player.log` in the run folder), 3 for build or check failures.

**The game stays up between runs.** After a run the game waits 15 s for another. A run whose
loadout matches the running game's (the mode, the mods, `-With`, `-Steam`, `-Trace`, and every file
of each deployed local mod, the harness and their shared configs) is handed to it: it loads the run's
fixture fresh and runs the tests, loading the test DLLs from their bytes, so changed tests need no
restart (about 10 s instead of 40). Any other loadout, a rebuilt mod included, ends the running game
and launches a new one. With no run for 15 s the game quits by itself, and a detached warden
(`Watch-OniGame.ps1`, started with the game, through WMI so it is outside the launcher's process
tree and survives the launcher being killed) puts the player's `mods.json` back once it has exited
and removes the session folder, `%LOCALAPPDATA%\IsochronousOniTests\live` (the warden's log is next
to it). `-KeepGame` launches a game of its own and keeps the old behaviour.

**Mod settings are put back.** The shared config folder of each mod in `-Mods` and `-With`
(`Documents\Klei\OxygenNotIncluded\mods\config\<name>`, PLib's folder named after the mod's assembly,
which is its local folder name here) is backed up by the game when a run starts and put back when
it ends, so a test may write a mod's settings (`ModConfig.cs`). The backup is kept in
`%LOCALAPPDATA%\IsochronousOniTests\config-backup`; one left there by a game that died mid-run is put
back by the warden when the game exits, or else by the next launcher before it starts.

**Nothing is left in the player's folders.** A run writes no save outside its run folder (a
generated colony's first save included), no colony summary and no timelapse image: the game writes
both to `Documents\Klei\OxygenNotIncluded\RetiredColonies\<colony>` on its own, and a generated
colony's random name can be one of the player's colonies, whose summary it would overwrite. The
harness skips those writes rather than deleting them afterwards, so a game that dies mid-run leaves
nothing behind either. The launcher lists `save_files`, `cloud_save_files` and `RetiredColonies`
before and after the run, and the summary says "Player's folders: nothing new" or names each new
entry (it never deletes one).

**One run at a time.** Runs started side by side (several agents at once) queue on a machine-wide
lock and say "waiting for it to finish"; `-BuildOnly` builds and checks without a game session and
takes the lock too, as builds share the harness's output.

The fixture is `fixtures/testbed-so.sav` or `fixtures/testbed-base.sav` by the run's mode
(`-Fixture` overrides); see `fixtures/README.md`. With no fixture for the mode the launcher warns and
the run goes on: Worldgen and NoColony tests need none, and a test that does ends the run as `no-save`.

**The mode.** A run is in the mode the player left the game in unless `-Mode base` or `-Mode spacedout`
says otherwise. Another mode is set in `kplayerprefs.yaml` (`EXPANSION1_ID.ENABLED`) before the game
starts, with the player's value kept in `kplayerprefs.yaml.onitest-mode`, and put back once the game
exits: by the warden, by the launcher when there is no warden, or by the next launcher after a run
that died (`GameMode.ps1`). The last-save pointer `SaveFilenameKey/` in the same file, which the game
points at a run's last test save, is kept in `kplayerprefs.yaml.onitest-savekey` and put back at the
same places. The mode is part of the loadout, so a run in the other mode ends a live
game and launches a new one. Base-game coordinates (`SNDST-A-100-0-0-0`) need `-Mode base`. The harness picks a 40x10 test area in the fixture
world itself and clears it (objects, loose items and solid cells; Duplicants are a test's to remove) before
the first test and after every test.

Launcher options:

| Option | What it does |
|---|---|
| `-Filter "glob[;glob]"` | Only tests whose `Assembly/Class.Method` id or display name matches. One word, no spaces (it travels through Steam's command line); `*` and `?` wildcards. Also the only way to run an `Explicit` test. |
| `-Fixture path.sav` | Load this save instead of the testbed, e.g. a player's colony for a bug only seen there. The save is copied into the run folder and never written back. |
| `-With Mod1,Mod2` | Enable more local mods (folder names under `mods/local`). |
| `-Steam id1,id2` | Enable installed Workshop mods by item id, to test alongside another author's mod. An item downloaded to `mods\Steam\<id>` that the game has not registered yet (no start since the subscription) is registered for the run, with Steam's time-updated as its version. |
| `-TimeoutSec n` | Real seconds the whole session may take (default 600). |
| `-TestTimeoutSec n` | Default real seconds per test (default 60); `[OniTest(Timeout = n)]` overrides it. |
| `-KeepGame` | Leave the colony open after the tests; the launcher waits until the game is closed. |
| `-BuildOnly` | Build the test project and check it against the game assemblies, then stop. |
| `-Label name` | The run folder's prefix (`runs/<label>-<stamp>`). |
| `-Mode auto\|base\|spacedout` | The game's mode for the run (default: the player's); see "The mode" above. |
| `-Trace` | Log the grid marks the game makes during tests (`Diagnostics.cs`). |
| `-NoBuild`, `-NoCheck`, `-BuildArgs` | Skip the build or the ModCheck pass, or pass extra arguments to `dotnet build`. |

The game must not be running, other than the live session's own (the launcher refuses), and Steam
must be. If a run or its warden is killed before the end, `mods.json.onitest-backup` holds the
player's mod list: the next run restores it first, or copy it over `mods.json` by hand.

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
- `[OniTest(FreshSave = true)]`: the fixture is loaded again before the test, in the same game (a
  few seconds, not a restart), unless no test has run since the last load. For tests that change
  the colony outside the test area, such as one run on a player's save. Only a change of mods or
  mod settings needs a new launch. The status line counts the reloads.
- `[OniTest(KeepSave = true)]`: the test reads the save as it was loaded, so the fixture area is
  not cleared before it. For checking a save made for the purpose, such as one another test wrote
  and `-Fixture` loads (Ovagro Overhead's removal check loads a save without the mod that made it).
  Run it alone or first after a load (with `FreshSave`).
- Each mod's tests start on a fresh load of the fixture: when a run moves on to the next test
  assembly, the fixture is loaded again, so nothing one mod's tests left (liquid, heat, a setting)
  reaches the next mod's.
- `[OniTest(Explicit = true)]`: left out of a run with no `-Filter`, run only when a filter selects
  it. For long tests kept for when they are needed.
- `[OniTest(NoColony = true)]`: the test needs no colony (an options dialog, strings,
  registrations). A run whose tests are all marked so loads no save: after a launch they run at the
  main menu, in a live game wherever it is. In a run with other tests too, the NoColony tests run first,
  at the main menu (a game in a colony goes back to the menu for them), before any world is generated
  or the fixture loaded. `t.Area`, `t.World`, `t.Tools` and `t.Dupes` are not for
  such a test.
- `[OniTest(Worldgen = "SNDST-C-100-0-0-0")]`: the test runs in a world freshly generated from that
  coordinate (as the destination screen takes it) with every mod of the run active, instead of the
  fixture. The harness goes to the main menu, opens the game's destination screen with the cluster's
  category chosen (as a player's New Game does, so a mod that sets its clusters up there, such as
  MiniBase, does so), applies the coordinate, checks the game took it, runs the game's own worldgen
  screen (about 20 s for a Spaced Out cluster), lets the colony load, embarks with the Duplicants
  offered and presses Begin on the opening message, then runs the test. Consecutive tests with one
  coordinate share the world; a later fixture test loads the fixture again. Nothing in a generated
  world is cleared, and `t.Area` is not for such a test. `t.Generated` tells what was generated (the
  applied coordinate, the cluster, worldgen errors, seconds). A coordinate of the other mode skips
  the test (`-Mode` picks the run's mode). The game's prefixes end in `A` or `C` for the mode; a
  Workshop cluster's prefix (`MINI-B`, `BTOR-L`) is looked up in the clusters the game loaded, and a
  prefix the mode has no cluster for skips the test; a failed generation fails its tests and the run goes on; a worldgen thread silent for
  300 s ends the session. Generation is real game time: raise `-TimeoutSec` for runs with several worlds.
- `[OniTest(Worldgen = "...", Prepare = nameof(Setup))]`: `Setup` is a `public static` method of the
  test's class, taking nothing and returning `void` or `System.Action`. It runs once, at the main
  menu, just before the coordinate is applied: write the mod's config for this world, or unpatch
  something for a vanilla baseline. The action it returns undoes that; the harness calls it before
  the next world is generated or the fixture loaded, and when the run ends. Tests with different
  `Prepare` methods never share a world, the coordinate the same or not. A `Prepare` that throws
  fails its world's tests ("Prepare threw: ...") and the run goes on; a missing or ill-shaped method,
  or a `Prepare` without `Worldgen`, stops the run before any test with a message naming the test.
  A config a `Prepare` writes is put back at the end of the run anyway (see "Mod settings are put back").
- `t.LookAt(cell, size)` points the camera at a cell (orthographic size: 14 shows the fixture area,
  8 a close-up), for a `t.Screenshot` of something particular. In a generated world, lift the fog of
  war first (`t.World.RevealAsteroid(world)` or `RevealActiveAsteroid()`) or the picture is black. Natural Backwalls' `Generated.Overview`
  (explicit) photographs the start area and one spot per biome; its `StartingAsteroid` photographs
  each hole it finds before it fails.
- Yield `t.Frames(n)`, `t.WaitGameSeconds(s)` (scaled sim time), `t.WaitRealSeconds(s)`,
  `t.WaitUntil(cond, timeout, what)` (a timeout fails the test with `what`), or a kit coroutine
  such as `wire.Path(...)`. Plain `yield return null` is one frame.
- `t.World`: `Build(id, cell, orientation, element)`, `At(cell, layer)`, `Destroy(go)`,
  `LogicNetwork(cell)`, `LogicConnections(cell)`, `RevealAsteroid(world)` and `RevealActiveAsteroid()`
  (every cell of the asteroid out of the fog of war, as the game's debug reveal does; test-only).
- `UtilityDragSession.PrimeBuildMenu(t)`: wakes the build menu's product screen once, as the menu
  would have, before a test drives a build tool directly (the BuildTool, say).
- `t.Tools.UtilityTool(id, element)`: the real utility build tool; `Path(cells...)` visits each
  cell with a frame between (a careful drag), `Sweep(from, to)` is one move (a fast swipe);
  `PreviewTint(cell)` is the preview's colour just before the button went up (white: buildable).
  Disposed, or at cleanup, the select tool is back and any drag is cancelled.
- `t.Assert`: `True`, `False`, `Eq`, `NotEq`, `Null`, `NotNull`, `Has(flag, bits)`, `Lacks`, `Fail`.
- `t.Log(text)` goes into the result's notes and `Player.log`; `t.Cleanup(action)` runs after the
  test whatever happened; `t.AllowLogError(regex)` for an error the game is expected to log,
  because any `Error`, `Exception` or `Assert` logged during a test fails it otherwise;
  `t.Skip(reason)`.
- `t.InstantBuild(on)` turns the debug instant build off or on for the test (restored at cleanup);
  sandbox's own Instant Build setting is a separate switch, `SandboxToolParameterMenu.instance.settings.InstantBuild`.
- `t.Dupes`, for a test that needs work done by hand (the fixture has no Duplicants):
  `Box()` walls the fixture area (or `Box(rect)`) with tiles, fills the inside with air and puts a
  dev life support in its top left corner, and returns the inside, whose bottom row is the floor;
  `Spawn(n, cell)` makes Duplicants there as the sandbox spawner does, with only their base trait,
  every attribute at 20, a schedule of work at every hour, and their needs topped up while the test
  runs; `Grant(skillIds...)` masters skills without points, `GrantAll()` every skill a standard
  Duplicant can learn (some buildings ask one of whoever builds them); `Drop(material, kg, cell)` leaves a
  loose pile to fetch (an element or an item); `Remove(dupe)` and `RemoveAll()`. Every Duplicant
  spawned is removed at cleanup. Wait a game second after `Box()` before spawning. Walls and life
  support already standing are kept and the air filled again, so `Box()` is cheap the second time.
  `ClearBox()` (yield it) clears the inside within a test, as between two boxed tests, for a test
  that works in batches; remove the Duplicants first.
- `[OniTest(Boxed = true)]`: the test works in the box over the whole area. When the next test is
  boxed too, only the inside is cleared between them (the life support stays); otherwise the whole
  area is, as usual.
- `t.ResearchAll()` researches every tech the way the game finishes one, so tools and menus offer
  everything without instant build. It lasts until the fixture is loaded again.
- `t.Speed(n)` sets the game speed, 1 to 3, for the rest of the test (restored at cleanup); speed 3
  for tests that wait on Duplicants.
- `t.Screenshot(name)` (yield it) writes `artifacts/<Class>.<Method>-<name>.png` and lists it in the
  result; something to look at, never an oracle.
- `yield return t.Press(Action.Escape)` presses a key as the game hands one to its screens (KScreenManager,
  topmost first, until one takes it); the physical keyboard stays ignored.
- `t.PointAt(cell)` puts the locked mouse over a cell, for a test that needs the cursor or its hover
  card there.

**Pace tests to the game.** The game is built for a player's speed: build, change, rebuild and clear
frames apart, and its sim callbacks arrive after the objects they belong to are gone (in one case an
endless exception every tick, the game at 2 FPS). Between steps, wait game time
(`t.WaitGameSeconds`, a second or two after building or clearing), wait for conditions rather than
a frame count, give a drag a moment between its steps, and let a door finish opening before acting on
it. Watch `Player.log` for a repeating exception during long runs.

The harness skips the game's autosave during a run (it cancels a drag the player controller does not
know of, which is every test's), forces the game's focus flags on (unfocused, the game drops every click), locks the
mouse position in the screen's bottom right corner, where its hover card and tool visualizers cover
nothing (back there after every test), ignores the physical keyboard and mouse for the session,
turns instant build on the way the debug key does, and runs the sim unpaused at speed 1. Every private game member it reads
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
is blocked outside the run folder, and `RetireColonyUtility.SaveColonySummaryData` and
`Timelapser.WriteToPng` (but for a save's preview in the run folder) are skipped; `KInputManager.OnApplicationFocus` and
`InterfaceTool.OnApplicationFocus` re-force the focus flags. `TestBehaviour` waits for the HUD,
throws the switches (`Harness.cs`), finds and clears the area (`FixtureArea.cs`), then steps each
test's enumerator by hand: an exception inside a Unity-driven coroutine cannot be caught by its
starter and a timeout cannot stop it, so the driver calls `MoveNext` itself inside try/catch,
interprets the yielded value (a nested enumerator, a kit `Wait` polled per frame, a Unity yield
instruction, or null for a frame) and checks the deadline at every step. Errors the game logs during
a test are captured through `Application.logMessageReceivedThreaded`.

A live session (`--onitest-server=DIR`, given by the launcher unless `-KeepGame`): when a run ends,
`Session.Finish` writes `DIR/state` (`idle <since>`) instead of quitting, and `App.LateUpdate` polls
`DIR/queue` each frame. A launcher writes `<run>.req` (key=value lines: the run folder, fixture,
filter and timeouts) and waits for the game to rename it `.taken`; the game resets the session's
counts, finds the tests again and loads the fixture (or, for a run of no-colony tests, adds a
driver where it is). `quit.req`, or no request for 15 s, ends the session with `App.Quit`. The
warden ends a game idle for over a minute or busy three minutes past its deadline.
