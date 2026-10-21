using System;
using System.Collections;
using System.Collections.Generic;
using Isochronous.TestKit;
using UnityEngine;

namespace Isochronous.TestRunner
{
	/// <summary>
	/// Lives on the Game object for the session: waits for the HUD, throws the harness switches,
	/// finds and clears the fixture area, then runs every selected test as a hand-stepped coroutine.
	/// Hand-stepped because an exception inside a Unity-driven coroutine cannot be caught by its
	/// starter and a timeout cannot stop it: the driver calls MoveNext itself, interprets what the
	/// test yielded (null for a frame, a nested IEnumerator, a kit Wait, or a Unity yield
	/// instruction) and checks the real-time deadline at every step.
	/// </summary>
	public sealed class TestBehaviour : MonoBehaviour
	{
		private const float HudWaitSeconds = 60f;

		/// <summary>
		/// Game seconds a loaded colony runs before it is loaded again. Torn down at once (a test that
		/// skips straight after a load, then another fresh-save test), the game's own start-up was
		/// still going: Game.StopBE threw and the scheduler threw every frame after, a hung session.
		/// GameFlowManager alone waits four seconds after a load before it starts.
		/// </summary>
		private const double SettleBeforeReloadSeconds = 5;

		private bool started;
		private double gameSeconds;

		/// <summary>
		/// Set on the driver Session.Start makes for a run of [OniTest(NoColony = true)] tests: it runs
		/// them where the game is (the main menu after a launch) with no fixture, then goes away.
		/// </summary>
		public bool NoColony;

		/// <summary>Scaled game time since the colony loaded, for the per-test gameSeconds figure.</summary>
		public double GameSeconds => gameSeconds;

		private void Update()
		{
			if (Session.Finished)
				return;
			gameSeconds += Time.deltaTime;
			if (started)
				return;
			started = true;
			Debug.Log(Session.Tag + "first update, time scale " + Time.timeScale + (NoColony ? ", no colony" : ""));
			StartCoroutine(Guarded(NoColony ? RunWithoutColony() : RunAll()));
		}

		/// <summary>
		/// Steps the driver's own coroutine by hand, nested ones included, so an exception the driver
		/// did not expect ends the run at once ("runner-error" with the exception) instead of silently
		/// ending a Unity coroutine and leaving the game idle until the session timeout.
		/// </summary>
		private static IEnumerator Guarded(IEnumerator root)
		{
			var stack = new Stack<IEnumerator>();
			stack.Push(root);
			while (stack.Count > 0)
			{
				bool more;
				object current = null;
				try
				{
					more = stack.Peek().MoveNext();
					if (more)
						current = stack.Peek().Current;
				}
				catch (Exception e)
				{
					Debug.LogWarning(Session.Tag + "the driver failed: " + e);
					Session.Finish("runner-error", "the driver failed: " + e.GetType().Name + ": " + e.Message);
					yield break;
				}
				if (!more)
				{
					stack.Pop();
					continue;
				}
				if (current is IEnumerator nested)
				{
					stack.Push(nested);
					continue;
				}
				yield return current;
			}
		}

		private IEnumerator RunWithoutColony()
		{
			yield return null;
			Harness.ForceFocus();
			for (int i = Session.NextIndex; i < Session.Tests.Count && Session.Tests[i].NoColony; i++)
			{
				if (Session.Finished)
					break;
				Session.NextIndex = i + 1;
				yield return RunOne(Session.Tests[i]);
			}
			if (!Session.Finished && Session.NextIndex < Session.Tests.Count)
			{
				// The rest of a mixed run: the next world to generate, or the fixture to load.
				Session.Continue();
				Destroy(gameObject);
				yield break;
			}
			if (!Session.Finished)
				Session.Finish("done", Session.Passed + " passed, " + Session.Failed + " failed, " + Session.Errors + " errors, " + Session.Timeouts + " timeouts, " + Session.Skipped + " skipped");
			Destroy(gameObject);
		}

		private IEnumerator RunAll()
		{
			if (Worldgen.Current != null)
				yield return Worldgen.Embark();
			float deadline = Time.realtimeSinceStartup + HudWaitSeconds;
			while (PlayerController.Instance == null || UtilityBuildTool.Instance == null || SelectTool.Instance == null
				|| SpeedControlScreen.Instance == null || ClusterManager.Instance == null || Camera.main == null)
			{
				if (Time.realtimeSinceStartup > deadline)
				{
					Session.Finish("runner-error", "the HUD did not appear within " + HudWaitSeconds + " s");
					yield break;
				}
				yield return null;
			}
			yield return null;
			yield return null;
			try
			{
				Setup();
			}
			catch (Exception e)
			{
				Debug.LogWarning(Session.Tag + e);
				Session.Finish("runner-error", e.ToString());
				yield break;
			}
			// A generated world is the thing under test: nothing in it is cleared; nor is a save the
			// next test reads as it was loaded.
			bool generated = Worldgen.Current != null;
			bool keepFirst = Session.NextIndex < Session.Tests.Count && Session.Tests[Session.NextIndex].KeepSave;
			if (!generated && !keepFirst)
				yield return FixtureArea.Clear(FixtureArea.Current);
			for (int i = Session.NextIndex; i < Session.Tests.Count; i++)
			{
				if (Session.Finished)
					yield break;
				TestCase test = Session.Tests[i];
				// Each mod's tests start on a fresh load (or a fresh generation), so nothing one mod's
				// tests left behind (liquid in the area, heat, a changed setting) reaches the next mod's;
				// and a test in a generated world needs that world, one on the fixture the fixture.
				bool nextMod = Session.LastAssembly != null && test.AssemblyName != Session.LastAssembly;
				bool otherWorld = test.WorldKey != (generated ? Worldgen.Current.Key : null);
				if (otherWorld || ((test.FreshSave || nextMod) && !Session.SaveIsFresh))
				{
					// The scene change destroys this driver; Game_OnSpawn_Patch attaches a new one,
					// which resumes at this test.
					while (gameSeconds < SettleBeforeReloadSeconds)
						yield return null;
					Session.NextIndex = i;
					Session.Reloads++;
					Debug.Log(Session.Tag + (test.Worldgen != null ? "generating a world" : "loading the fixture") + " again for " + test.Id
						+ (otherWorld ? " (it needs " + (test.Worldgen ?? "the fixture") + ")" : nextMod ? " (the next mod's tests)" : ""));
					Session.Continue();
					yield break;
				}
				Session.NextIndex = i + 1;
				if (test.KeepSave && !Session.SaveIsFresh)
					Debug.LogWarning(Session.Tag + test.Id + " keeps the save, but a test ran since the last load: it reads what that test left");
				Session.SaveIsFresh = false;
				Session.LastAssembly = test.AssemblyName;
				yield return RunOne(test);
				if (generated)
					continue;
				// A box the next test works in too stays up; only its inside is cleared.
				TestCase next = i + 1 < Session.Tests.Count ? Session.Tests[i + 1] : null;
				if (next != null && next.KeepSave && !next.FreshSave)
					continue;
				if (FixtureArea.BoxStanding && test.Boxed && next != null && next.Boxed && !next.FreshSave)
					yield return FixtureArea.ClearInside();
				else
					yield return FixtureArea.Clear(FixtureArea.Current);
			}
			Session.Finish("done", Session.Summary());
		}

		private void Setup()
		{
			List<string> missing = Reflect.SelfCheck();
			if (missing.Count > 0)
				throw new InvalidOperationException("game members not found (the game changed): " + string.Join(", ", missing));
			Session.Discover();
			Harness.ForceFocus();
			Harness.IgnorePhysicalInput();
			Harness.Unpause();
			Harness.EnsureInstantBuild();
			FixtureArea.BoxStanding = false;
			FixtureArea.LifeSupport = null;
			if (Worldgen.Current == null)
			{
				FixtureArea.Current = FixtureArea.Find(FixtureArea.Width, FixtureArea.Height);
				int centre = FixtureArea.Current.Cell(FixtureArea.Width / 2, FixtureArea.Height / 2);
				Harness.LookAt(centre, 14f);
			}
			else
				FixtureArea.Current = default;
			Harness.ParkMouse();
			Harness.ForceFocus();
			Debug.Log(Session.Tag + (Worldgen.Current != null ? "generated world " + Worldgen.Current.Applied : "fixture area " + FixtureArea.Current) + ", focus " + KInputManager.isFocused
				+ ", instant build " + DebugHandler.InstantBuildMode + ", paused " + SpeedControlScreen.Instance.IsPaused
				+ ", mouse locked at " + KInputManager.lockedMousePos
				+ "; sandbox " + Game.Instance.SandboxModeActive + ", sandbox menu " + (SandboxToolParameterMenu.instance != null)
				+ ", plan screen " + (PlanScreen.Instance != null) + ", priority screen " + (ToolMenu.Instance != null && ToolMenu.Instance.PriorityScreen != null)
				+ ", pop fx " + (PopFXManager.Instance != null)
				+ "; dialogs open: " + UnityEngine.Object.FindObjectsOfType<ConfirmDialogScreen>().Length);
		}

		private IEnumerator RunOne(TestCase test)
		{
			var result = new TestResult(test);
			var t = new TestContext(test.Name, test.Id);
			var capture = new LogCapture();
			capture.Begin();
			float start = Time.realtimeSinceStartup;
			double gameStart = gameSeconds;
			float deadline = start + (float)test.TimeoutSeconds;
			var stack = new Stack<IEnumerator>();
			Wait waiting = null;
			Debug.Log(Session.Tag + "running " + test.Id + " (" + test.Name + ")");
			try
			{
				stack.Push(test.Invoke(t));
			}
			catch (Exception e)
			{
				Classify(Unwrap(e), result);
			}
			while (result.Status == "pass" && stack.Count > 0)
			{
				if (Time.realtimeSinceStartup > deadline)
				{
					result.Status = "timeout";
					result.Message = "no end after " + test.TimeoutSeconds + " s real time" + (waiting != null ? " (waiting for " + waiting.Description + ")" : "");
					break;
				}
				if (waiting != null)
				{
					bool done;
					try
					{
						done = waiting.Poll();
					}
					catch (Exception e)
					{
						Classify(e, result);
						break;
					}
					if (!done)
					{
						yield return null;
						continue;
					}
					if (waiting.TimedOut)
					{
						result.Status = "fail";
						result.Message = "timed out waiting for " + waiting.Description;
						break;
					}
					waiting = null;
					continue;
				}
				bool more;
				object yielded = null;
				try
				{
					more = stack.Peek().MoveNext();
					if (more)
						yielded = stack.Peek().Current;
				}
				catch (Exception e)
				{
					Classify(e, result);
					break;
				}
				if (!more)
				{
					stack.Pop();
					continue;
				}
				if (yielded is IEnumerator nested)
				{
					stack.Push(nested);
					continue;
				}
				if (yielded is Wait wait)
				{
					waiting = wait;
					yield return null;
					continue;
				}
				yield return yielded;
			}
			foreach (System.Action cleanup in t.TakeCleanups())
			{
				try
				{
					cleanup();
				}
				catch (Exception e)
				{
					result.Notes.Add("cleanup threw: " + e.Message);
					Debug.LogWarning(Session.Tag + "cleanup of " + test.Id + " threw: " + e);
				}
			}
			try
			{
				Harness.ResetTools();
				Harness.ParkMouse();
			}
			catch (Exception e)
			{
				result.Notes.Add("tool reset threw: " + e.Message);
			}
			capture.End();
			foreach (string error in capture.Errors)
			{
				if (t.IsLogErrorAllowed(error))
					continue;
				result.LogErrors.Add(error);
			}
			if (result.Status == "pass" && result.LogErrors.Count > 0)
			{
				result.Status = "fail";
				result.Message = "error logged during the test: " + result.LogErrors[0];
			}
			result.Notes.AddRange(t.Notes);
			result.DurationSeconds = Time.realtimeSinceStartup - start;
			result.GameSeconds = gameSeconds - gameStart;
			Session.Write(result);
		}

		private static Exception Unwrap(Exception e)
		{
			return e is System.Reflection.TargetInvocationException tie && tie.InnerException != null ? tie.InnerException : e;
		}

		private static void Classify(Exception e, TestResult result)
		{
			if (e is OniAssertException)
			{
				result.Status = "fail";
				result.Message = e.Message;
			}
			else if (e is SkipException)
			{
				result.Status = "skipped";
				result.Message = e.Message;
			}
			else
			{
				result.Status = "error";
				result.Message = e.GetType().Name + ": " + e.Message;
			}
			result.Stack = e.StackTrace;
		}

		private void OnDestroy()
		{
			Debug.Log(Session.Tag + "driver destroyed");
		}
	}

	/// <summary>Errors, exceptions and assertions the game logs while a test runs; any one of them fails the test unless allowed.</summary>
	internal sealed class LogCapture
	{
		private readonly object sync = new object();
		public readonly List<string> Errors = new List<string>();

		public void Begin()
		{
			Application.logMessageReceivedThreaded += OnLog;
		}

		public void End()
		{
			Application.logMessageReceivedThreaded -= OnLog;
		}

		private void OnLog(string condition, string stackTrace, LogType type)
		{
			if (type != LogType.Error && type != LogType.Exception && type != LogType.Assert)
				return;
			string first = stackTrace == null ? "" : stackTrace.Split('\n')[0].Trim();
			lock (sync)
				Errors.Add(type + ": " + condition + (first.Length > 0 ? " @ " + first : ""));
		}
	}
}
