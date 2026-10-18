---
doc_id: doc-8c9b5ab0992a
description: "How TestRunner turns problems into results: test statuses, session statuses, launcher exit codes 0 to 3, hand-stepped coroutines, mods.json restore and logging"
applies_to:
  - "*.cs"
  - "Kit/**/*.cs"
  - "*.ps1"
  - "*.csproj"
  - "*.yaml"
---
# Error Handling Conventions

## Principles
**Id:** blk-23a64c67c430

1. **A session always ends with a status.** `Session.Finish` writes `results.jsonl.status` whatever happened, so the launcher never waits on a game that has silently given up.
2. **A test never takes the run down.** Each test's exception, timeout or logged error becomes that test's result and the next test runs.
3. **Never write to the player's data.** Saving is blocked outside the run folder, and the launcher backs up and restores `mods.json`.
4. **Leave a trace.** Whatever ends the process during a session logs its stack; everything goes to `Player.log` under the `[TestRunner] ` tag and into the run folder.
5. **A renamed game member names itself.** `Reflect.SelfCheck` fails the run with the member's name before the first test.

## Test Results
**Id:** blk-5d3610d2392d

One JSON line per test in `results.jsonl`. The driver steps each test by hand (`RunOne` in `TestBehaviour.cs`) inside try/catch and `Classify` maps what it caught:

| Status | Cause |
|--------|-------|
| `pass` | the test ended with no exception and the game logged no error |
| `fail` | `OniAssertException` from `t.Assert`; a `Wait` that timed out (`timed out waiting for ...`); an `Error`, `Exception` or `Assert` the game logged during the test and `t.AllowLogError` did not allow |
| `error` | any other exception (`Type: message` and the stack) |
| `timeout` | the test's real-time deadline passed (`[OniTest(Timeout = n)]`, else `--onitest-test-timeout`) |
| `skipped` | `t.Skip(reason)`, or a `Worldgen` coordinate of the other mode |

Cleanups registered with `t.Cleanup` run after every test, in reverse order; one that throws is recorded in the result's notes and the others still run. `Harness.ResetTools` and `ParkMouse` run after every test.

## Session Status
**Id:** blk-a16ba6352e62

`Session.Finish(status, detail)` writes one JSON line with the counts and quits (unless `--onitest-keep`, or a live session, which goes idle).

| Status | Written by | Meaning |
|--------|-----------|---------|
| `done` | `Session`, `TestBehaviour` | every selected test ran; `detail` is the summary |
| `runner-error` | `Session.Start`, `TestBehaviour` | test discovery failed, the HUD did not appear within 60 s, or the driver threw |
| `no-save` | `Session.Continue` | the fixture file does not exist |
| `timeout` | `App_LateUpdate_Patch` | no end after `--onitest-timeout` real seconds (default 600) |
| `worldgen-hang` | `Worldgen.Watch` | no worldgen progress for 300 s |

A failed world generation is not a session failure: its tests are written off as `fail` (`Session.WriteOff`) and the run goes on.

## Launcher Exit Codes
**Id:** blk-8aed4db67098

`Invoke-OniTests.ps1` ends with:

| Code | Meaning |
|------|---------|
| 0 | session status `done` and every test passed |
| 1 | a test failed, errored or timed out, or no test ran |
| 2 | the session produced no status, or a status other than `done` (launch, load or crash: read `Player.log` in the run folder) |
| 3 | the build or the ModCheck pass failed |

Preconditions use `throw` with a message that names the fix: game already running, Steam not found, fixture not found, fixture of the wrong mode, test project not found, mod not deployed, Workshop mod not installed, local mod folder not found, game did not start within 120 s.

## Handling Patterns
**Id:** blk-0a556e7a86fb

### Hand-stepped coroutines
**Id:** blk-ee72d04f56e6

`TestBehaviour.Guarded` and `RunOne` call `MoveNext` themselves inside try/catch. An exception inside a Unity-driven coroutine cannot be caught by its starter and a timeout cannot stop it. The real-time deadline is checked at every step.

### Driver failures
**Id:** blk-1fbd16bb06a4

An exception the driver did not expect ends the session at once as `runner-error` with the exception, never as an idle game waiting for the session timeout.

### Side files
**Id:** blk-a0ed021080ba

Writing a result, the status file or the session state is wrapped in try/catch that logs a warning (`Session.Write`, `Session.Finish`, `Session.WriteState`); a failed write must not stop the run.

### Crash dialog
**Id:** blk-ed7fda19418a

`OnLoad` sets `KCrashReporter.ignoreAll = true` and `terminateOnError = false`. The game's crash dialog quits the process when the scene change destroys it, which ended runs without a word; exceptions still reach the log.

### Mods.json and the warden
**Id:** blk-73f72ee0a0b7

The launcher copies `mods.json` to `mods.json.onitest-backup` before changing it. A leftover backup means an earlier launcher or warden died; the next run restores it first. `Watch-OniGame.ps1` puts the file back and removes the live-session folder once the game has exited; it ends a game that has been idle in its state file for over a minute, or busy three minutes past its own deadline.

### One run at a time
**Id:** blk-040f34128326

The launcher takes the machine-wide mutex `Global\IsochronousOniTests` and says "waiting for it to finish" when another run holds it. An abandoned mutex is taken over.

## Logging
**Id:** blk-54e6d79709fb

- All harness lines use `Session.Tag` (`[TestRunner] `) through `Debug.Log` or `Debug.LogWarning`.
- `t.Log(text)` writes to the result's notes and to `Player.log` as `[TestRunner] <test id>: <text>`.
- Test code reports failures with `t.Assert.*` and its message argument, which names what was expected; `t.WaitUntil(cond, timeout, what)` fails with `what`.

## Anti-Patterns to Avoid
**Id:** blk-8ac04f4e983a

- Calling `App.Quit()` directly instead of `Session.Finish`: no status is written.
- Reading a private game member by a bare `AccessTools` call in a test or kit file; it goes in `Reflect.cs` with a self-check.
- A test that builds, changes and clears frames apart; the game's sim callbacks arrive after the objects are gone. Wait game time between steps.
- An `Error` logged during a test that is expected (a game warning path): allow it with `t.AllowLogError(regex)`, do not loosen the check.
- Starting a run with the game open: the launcher refuses, except for its own live session.

## Related
**Id:** blk-4d93c98455cc

- See also: [code style](code-style.md)
