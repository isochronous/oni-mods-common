using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Isochronous.TestKit
{
	/// <summary>
	/// What a test gets: waits (yield them), a clear rectangle to build in, the API layer (World),
	/// the tool layer (Tools), assertions, a cleanup stack and a note log. Every wait is a
	/// <see cref="Wait"/> the driver polls once per frame with a deadline and a description, so a
	/// test that stalls fails with what it was waiting for.
	/// </summary>
	public sealed class TestContext
	{
		public string Name { get; }
		public string Id { get; }
		public OniAssert Assert { get; }
		public World World { get; }
		public Tools Tools { get; }

		private readonly List<System.Action> cleanups = new List<System.Action>();
		private readonly List<Regex> allowedLogErrors = new List<Regex>();
		internal readonly List<string> Notes = new List<string>();

		public TestContext(string name, string id)
		{
			Name = name;
			Id = id;
			Assert = new OniAssert();
			World = new World(this);
			Tools = new Tools(this);
		}

		/// <summary>The whole fixture area (cleared before the test).</summary>
		public CellRect Area => Isochronous.TestRunner.FixtureArea.Current;

		/// <summary>
		/// A w x h rectangle inside the fixture area with a cell of margin on every side, clear
		/// when the test starts; r.Cell(x, y) counts from its bottom-left.
		/// </summary>
		public CellRect Rect(int w, int h)
		{
			CellRect area = Area;
			if (w + 2 > area.W || h + 2 > area.H)
				throw new OniAssertException("a " + w + "x" + h + " rectangle does not fit the fixture area " + area + " with its margin");
			return new CellRect(area.X + 1, area.Y + 1, w, h);
		}

		/// <summary>Runs after the test, pass or fail, in reverse order of registration.</summary>
		public void Cleanup(System.Action action)
		{
			cleanups.Add(action);
		}

		internal List<System.Action> TakeCleanups()
		{
			var list = new List<System.Action>(cleanups);
			list.Reverse();
			cleanups.Clear();
			return list;
		}

		/// <summary>A line in the result's notes and in Player.log.</summary>
		public void Log(string message)
		{
			Notes.Add(message);
			Debug.Log("[TestRunner] " + Id + ": " + message);
		}

		/// <summary>An error the game is expected to log during this test (regex on the message).</summary>
		public void AllowLogError(string pattern)
		{
			allowedLogErrors.Add(new Regex(pattern));
		}

		internal bool IsLogErrorAllowed(string error)
		{
			foreach (Regex regex in allowedLogErrors)
				if (regex.IsMatch(error))
					return true;
			return false;
		}

		public void Skip(string reason)
		{
			throw new SkipException(reason);
		}

		/// <summary>
		/// Instant build on or off for the rest of the test (the harness runs with it on; off, a
		/// tool queues construction orders the way a player's does). Restored at cleanup.
		/// </summary>
		public void InstantBuild(bool on)
		{
			bool was = DebugHandler.InstantBuildMode;
			if (was == on)
				return;
			DebugHandler.ToggleInstantBuildMode();
			Cleanup(() =>
			{
				if (DebugHandler.InstantBuildMode != was)
					DebugHandler.ToggleInstantBuildMode();
			});
		}

		/// <summary>
		/// A PNG of the whole screen, written to the run's artifacts folder as
		/// &lt;class&gt;.&lt;method&gt;-&lt;name&gt;.png and listed in the result; never an oracle, something
		/// to look at. Yield it: the capture happens at the end of the frame.
		/// </summary>
		public Wait Screenshot(string name)
		{
			string dir = System.IO.Path.Combine(Isochronous.TestRunner.Session.Dir, "artifacts");
			System.IO.Directory.CreateDirectory(dir);
			string file = System.IO.Path.Combine(dir, Id.Replace('/', '.') + "-" + name + ".png");
			ScreenCapture.CaptureScreenshot(file, 1);
			Artifacts.Add(file);
			Log("screenshot " + file);
			return new FrameWait(3);
		}

		internal readonly List<string> Artifacts = new List<string>();

		/// <summary>n frames (Frames(1) resumes next frame).</summary>
		public Wait Frames(int n)
		{
			return new FrameWait(n);
		}

		/// <summary>Scaled game time, as the sim experiences it at the current speed.</summary>
		public Wait WaitGameSeconds(double seconds)
		{
			return new GameSecondsWait(seconds);
		}

		public Wait WaitRealSeconds(double seconds)
		{
			return new UntilWait(null, seconds, seconds + " s of real time");
		}

		/// <summary>Polls the condition each frame until true; a timeout fails the test with "timed out waiting for {what}".</summary>
		public Wait WaitUntil(Func<bool> condition, double timeoutRealSeconds, string what)
		{
			return new UntilWait(condition, timeoutRealSeconds, what);
		}
	}

	/// <summary>Something a test yields and the driver polls once per frame.</summary>
	public abstract class Wait
	{
		public bool TimedOut { get; protected set; }
		public abstract string Description { get; }
		/// <summary>True when the wait is over (because its condition holds, or because it timed out).</summary>
		public abstract bool Poll();
	}

	internal sealed class FrameWait : Wait
	{
		private int remaining;

		public FrameWait(int frames)
		{
			remaining = frames;
		}

		public override string Description => remaining + " frame(s)";

		public override bool Poll()
		{
			remaining--;
			return remaining <= 0;
		}
	}

	internal sealed class GameSecondsWait : Wait
	{
		private readonly double seconds;
		private double elapsed;
		private readonly float realDeadline;

		public GameSecondsWait(double seconds)
		{
			this.seconds = seconds;
			// Paused or crawling, the game would never get there; 30 s real per game second is a generous bound.
			realDeadline = Time.realtimeSinceStartup + (float)Math.Max(10.0, seconds * 30.0);
		}

		public override string Description => seconds + " game second(s)";

		public override bool Poll()
		{
			elapsed += Time.deltaTime;
			if (elapsed >= seconds)
				return true;
			if (Time.realtimeSinceStartup > realDeadline)
			{
				TimedOut = true;
				return true;
			}
			return false;
		}
	}

	internal sealed class UntilWait : Wait
	{
		private readonly Func<bool> condition;
		private readonly float deadline;
		private readonly string what;

		public UntilWait(Func<bool> condition, double timeoutRealSeconds, string what)
		{
			this.condition = condition;
			this.what = what;
			deadline = Time.realtimeSinceStartup + (float)timeoutRealSeconds;
		}

		public override string Description => what;

		public override bool Poll()
		{
			if (condition != null && condition())
				return true;
			if (Time.realtimeSinceStartup >= deadline)
			{
				TimedOut = condition != null;
				return true;
			}
			return false;
		}
	}

	/// <summary>A rectangle of cells; (x, y) from its bottom-left corner.</summary>
	public readonly struct CellRect
	{
		public readonly int X, Y, W, H;

		public CellRect(int x, int y, int w, int h)
		{
			X = x;
			Y = y;
			W = w;
			H = h;
		}

		public int Cell(int x, int y)
		{
			if (x < 0 || y < 0 || x >= W || y >= H)
				throw new ArgumentOutOfRangeException("(" + x + ", " + y + ") is outside the " + W + "x" + H + " rectangle");
			return Grid.XYToCell(X + x, Y + y);
		}

		public bool Contains(int cell)
		{
			Grid.CellToXY(cell, out int cx, out int cy);
			return cx >= X && cy >= Y && cx < X + W && cy < Y + H;
		}

		public IEnumerable<int> Cells
		{
			get
			{
				for (int y = 0; y < H; y++)
					for (int x = 0; x < W; x++)
						yield return Grid.XYToCell(X + x, Y + y);
			}
		}

		public override string ToString()
		{
			return W + "x" + H + " at (" + X + ", " + Y + ")";
		}
	}
}
