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
		public Dupes Dupes { get; }

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
			Dupes = new Dupes(this);
		}

		/// <summary>
		/// For a test with [OniTest(Worldgen = ...)]: the world it runs in, as generated (the coordinate
		/// the game applied, the cluster, errors the worldgen reported, how long it took). Null on the fixture.
		/// </summary>
		public Isochronous.TestRunner.GeneratedWorld Generated => Isochronous.TestRunner.Worldgen.Current;

		/// <summary>
		/// The camera on the cell, zoomed to the orthographic size (smaller is closer; 14 shows the
		/// whole fixture area), a few frames given to settle. For a screenshot of something particular,
		/// such as a problem spot in a generated world.
		/// </summary>
		public Wait LookAt(int cell, float size = 10f)
		{
			Isochronous.TestRunner.Harness.LookAt(cell, size);
			return new FrameWait(5);
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
		/// Every tech researched, the way the game finishes one when its points are in: each marked
		/// done and announced, so build menus and tools see it. Lasts until the fixture is loaded again
		/// (FreshSave), so later tests see the colony researched too.
		/// </summary>
		public void ResearchAll()
		{
			int done = 0;
			foreach (Tech tech in Db.Get().Techs.resources)
			{
				TechInstance instance = Research.Instance.Get(tech);
				if (instance == null || instance.IsComplete())
					continue;
				instance.Purchased();
				Game.Instance.Trigger((int)GameHashes.ResearchComplete, tech);
				done++;
			}
			if (done > 0)
				Log("researched " + done + " techs");
		}

		/// <summary>
		/// The game's speed for the rest of the test, 1 to 3 as the speed buttons set it (the harness
		/// runs at 1). For tests that wait on Duplicants. Restored at cleanup.
		/// </summary>
		public void Speed(int speed)
		{
			SpeedControlScreen screen = SpeedControlScreen.Instance;
			int was = screen.GetSpeed();
			screen.SetSpeed(Mathf.Clamp(speed, 1, 3) - 1);
			Cleanup(() => screen.SetSpeed(was));
		}

		/// <summary>
		/// The locked mouse over the cell, for a test that needs the cursor or its hover card there.
		/// The harness parks it in the screen's bottom right corner again after the test.
		/// </summary>
		public void PointAt(int cell)
		{
			Isochronous.TestRunner.Harness.LockMouse(cell);
		}

		/// <summary>
		/// A key press as the game hands one to its screens: the action's key down, then up, through
		/// KScreenManager, the topmost screen first, until one takes it (the physical keyboard stays
		/// ignored). Yield it: the screens act on it over the next frames.
		/// </summary>
		public Wait Press(global::Action action)
		{
			var down = new KButtonEvent(null, InputEventType.KeyDown, action);
			KScreenManager.Instance.OnKeyDown(down);
			KScreenManager.Instance.OnKeyUp(new KButtonEvent(null, InputEventType.KeyUp, action));
			Log("pressed " + action + (down.Consumed ? "" : ", which no screen took"));
			return new FrameWait(2);
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
