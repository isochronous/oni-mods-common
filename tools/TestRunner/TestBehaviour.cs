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

		private bool started;
		private double gameSeconds;

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
			Debug.Log(Session.Tag + "first update, time scale " + Time.timeScale);
			StartCoroutine(RunAll());
		}

		private IEnumerator RunAll()
		{
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
			yield return FixtureArea.Clear(FixtureArea.Current);
			for (int i = Session.NextIndex; i < Session.Tests.Count; i++)
			{
				if (Session.Finished)
					yield break;
				Session.NextIndex = i + 1;
				yield return RunOne(Session.Tests[i]);
				yield return FixtureArea.Clear(FixtureArea.Current);
			}
			Session.Finish("done", Session.Passed + " passed, " + Session.Failed + " failed, " + Session.Errors + " errors, " + Session.Timeouts + " timeouts, " + Session.Skipped + " skipped");
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
			FixtureArea.Current = FixtureArea.Find(FixtureArea.Width, FixtureArea.Height);
			int centre = FixtureArea.Current.Cell(FixtureArea.Width / 2, FixtureArea.Height / 2);
			Harness.LookAt(centre, 14f);
			Harness.LockMouse(centre);
			Harness.ForceFocus();
			Debug.Log(Session.Tag + "fixture area " + FixtureArea.Current + ", focus " + KInputManager.isFocused
				+ ", instant build " + DebugHandler.InstantBuildMode + ", paused " + SpeedControlScreen.Instance.IsPaused
				+ ", mouse locked at " + KInputManager.lockedMousePos);
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
