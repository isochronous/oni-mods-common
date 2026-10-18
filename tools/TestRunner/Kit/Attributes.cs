using System;

namespace Isochronous.TestKit
{
	/// <summary>
	/// Marks a test: <c>public static IEnumerator M(TestContext t)</c>. The name is what the
	/// results show; the timeout is real seconds (0 = the session default).
	/// </summary>
	[AttributeUsage(AttributeTargets.Method)]
	public sealed class OniTestAttribute : Attribute
	{
		public string Name { get; }
		public double Timeout { get; set; }

		/// <summary>
		/// The test needs the fixture as it was saved: the runner loads the save again before it,
		/// in the same game, unless no test has run since the last load. For tests that change the
		/// colony outside the fixture area (the area itself is cleared before every test).
		/// </summary>
		public bool FreshSave { get; set; }

		/// <summary>
		/// The test reads the save as it was loaded: the fixture area is not cleared before it (when it
		/// runs first after a load) nor after the test before it. For a save made to be checked,
		/// such as one written by another test and loaded with -Fixture. Run it first after a load:
		/// alone, or with FreshSave.
		/// </summary>
		public bool KeepSave { get; set; }

		/// <summary>
		/// Run only when a -Filter selects the test; a run with no filter leaves it out. For long
		/// tests kept for when they are needed.
		/// </summary>
		public bool Explicit { get; set; }

		/// <summary>
		/// The test works in the Duplicants' box over the fixture area (t.Dupes.Box()): when the next
		/// test is boxed too, only the inside is cleared after it, and the walls, the life support and
		/// the air stay for that test.
		/// </summary>
		public bool Boxed { get; set; }

		/// <summary>
		/// The test needs no colony (a mod's options dialog, its strings, its registrations): a run
		/// whose tests are all marked so loads no save and runs them where the game is, at the main
		/// menu after a launch. t.Area, t.World, t.Tools and t.Dupes are not for such a test.
		/// </summary>
		public bool NoColony { get; set; }

		/// <summary>
		/// The test runs in a world freshly generated from this coordinate (as the destination screen
		/// takes it, e.g. "SNDST-C-100-0-0-0") instead of the fixture: from the main menu the game
		/// generates it with every mod of the run active and loads the colony. Consecutive tests with
		/// one coordinate share the world. Nothing in it is cleared or boxed: t.Area is not for such a
		/// test; t.Generated tells what was generated. A coordinate of the other mode skips the test.
		/// </summary>
		public string Worldgen { get; set; }

		public OniTestAttribute(string name = null)
		{
			Name = name;
		}
	}

	/// <summary>
	/// Which layer the test drives the game at: 0 API, 1 tool, 2 UI, 3 input. Recorded in the
	/// results; a reminder at the top of each test of where the bug it guards would live.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
	public sealed class LayerAttribute : Attribute
	{
		public int Layer { get; }

		public LayerAttribute(int layer)
		{
			Layer = layer;
		}
	}

	/// <summary>Thrown by t.Assert; the driver records the message as the test's failure.</summary>
	public sealed class OniAssertException : Exception
	{
		public OniAssertException(string message) : base(message)
		{
		}
	}

	/// <summary>Thrown by t.Skip; the test is recorded as skipped with the reason.</summary>
	public sealed class SkipException : Exception
	{
		public SkipException(string reason) : base(reason)
		{
		}
	}
}
