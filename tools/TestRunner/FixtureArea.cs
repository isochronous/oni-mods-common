using System.Collections;
using System.Collections.Generic;
using Isochronous.TestKit;
using UnityEngine;

namespace Isochronous.TestRunner
{
	/// <summary>
	/// The rectangle of the fixture world that tests build in. Chosen once per session: the window
	/// of the active world, all of it revealed and buildable, with the most cells that are already
	/// empty (not solid, no object on any layer), underground cells counting double so a cave beats
	/// open space when one is big enough. Cleared before the first test and after every test:
	/// every object and loose item in it destroyed (Duplicants excepted), every building whose
	/// placement cells touch it too (built or under construction, on a layer or not), and every
	/// solid cell turned to vacuum.
	/// </summary>
	public static class FixtureArea
	{
		public const int Width = 40;
		public const int Height = 10;

		public static CellRect Current;

		/// <summary>True while the Duplicants' box stands over the whole area (TestKit.Dupes.Box).</summary>
		public static bool BoxStanding;

		/// <summary>The box's dev life support, kept when only the inside is cleared.</summary>
		public static GameObject LifeSupport;

		/// <summary>The cells inside the box's walls.</summary>
		public static CellRect Inside => new CellRect(Current.X + 1, Current.Y + 1, Current.W - 2, Current.H - 2);

		/// <summary>What World.Build made, checked after each clear for anything it left standing.</summary>
		private static readonly List<GameObject> built = new List<GameObject>();

		/// <summary>Records a building World.Build made, for the check after each clear.</summary>
		internal static void Track(GameObject go)
		{
			if (go != null)
				built.Add(go);
		}

		public static CellRect Find(int w, int h)
		{
			WorldContainer world = ClusterManager.Instance.activeWorld;
			int minX = (int)world.minimumBounds.x, minY = (int)world.minimumBounds.y;
			int maxX = (int)world.maximumBounds.x, maxY = (int)world.maximumBounds.y;
			int cols = maxX - minX + 1, rows = maxY - minY + 1;
			if (cols < w || rows < h)
				throw new System.InvalidOperationException("the active world (" + cols + "x" + rows + ") is smaller than the fixture area (" + w + "x" + h + ")");
			// Prefix sums: usable[y][x] counts revealed buildable cells of this world, score[y][x] the empty ones (weighted).
			int[,] usable = new int[rows + 1, cols + 1];
			int[,] score = new int[rows + 1, cols + 1];
			for (int y = 0; y < rows; y++)
			{
				for (int x = 0; x < cols; x++)
				{
					int cell = Grid.XYToCell(minX + x, minY + y);
					int u = 0, s = 0;
					if (Grid.IsValidBuildingCell(cell) && Grid.IsVisible(cell) && Grid.WorldIdx[cell] == world.id)
					{
						u = 1;
						if (!Grid.Solid[cell] && !HasObjects(cell))
							s = Grid.ExposedToSunlight[cell] > 0 ? 1 : 2;
					}
					usable[y + 1, x + 1] = usable[y, x + 1] + usable[y + 1, x] - usable[y, x] + u;
					score[y + 1, x + 1] = score[y, x + 1] + score[y + 1, x] - score[y, x] + s;
				}
			}
			int bestScore = -1, bestX = 0, bestY = 0;
			float bestDistance = float.MaxValue;
			float centreX = cols / 2f, centreY = rows / 2f;
			for (int y = 0; y + h <= rows; y++)
			{
				for (int x = 0; x + w <= cols; x++)
				{
					int u = usable[y + h, x + w] - usable[y, x + w] - usable[y + h, x] + usable[y, x];
					if (u != w * h)
						continue;
					int s = score[y + h, x + w] - score[y, x + w] - score[y + h, x] + score[y, x];
					float distance = Mathf.Abs(x + w / 2f - centreX) + Mathf.Abs(y + h / 2f - centreY);
					if (s > bestScore || (s == bestScore && distance < bestDistance))
					{
						bestScore = s;
						bestDistance = distance;
						bestX = x;
						bestY = y;
					}
				}
			}
			if (bestScore < 0)
				throw new System.InvalidOperationException("no " + w + "x" + h + " window of the active world is fully revealed and buildable");
			return new CellRect(minX + bestX, minY + bestY, w, h);
		}

		private static bool HasObjects(int cell)
		{
			for (int layer = 0; layer < (int)ObjectLayer.NumLayers; layer++)
				if (Grid.Objects[cell, layer] != null)
					return true;
			return false;
		}

		/// <summary>The inside of the box cleared for the next boxed test; the walls, the life support and the air stay.</summary>
		public static IEnumerator ClearInside()
		{
			return Clear(Inside, LifeSupport);
		}

		/// <summary>Everything out of the rectangle: objects destroyed, solids turned to vacuum, then two frames for the sim and the spawners to settle.</summary>
		public static IEnumerator Clear(CellRect rect)
		{
			BoxStanding = false;
			LifeSupport = null;
			return Clear(rect, null);
		}

		private static IEnumerator Clear(CellRect rect, GameObject keep)
		{
			int destroyed = 0;
			try
			{
				var objects = new HashSet<GameObject>();
				foreach (int cell in rect.Cells)
				{
					for (int layer = 0; layer < (int)ObjectLayer.NumLayers; layer++)
					{
						GameObject go = Grid.Objects[cell, layer];
						if (go != null && go != keep && go.GetComponent<MinionIdentity>() == null)
							objects.Add(go);
					}
				}
				// A building need not sit on any layer: a mod may leave a cell's slot to another owner
				// (a bridge whose ends both went to other buildings), and then Grid.Objects never names it.
				// Build-tool previews are Buildings too, and belong to the tool.
				foreach (Building building in Object.FindObjectsByType<Building>(FindObjectsSortMode.None))
				{
					if (building is BuildingPreview || building.gameObject == keep)
						continue;
					try
					{
						if (Touches(building, rect))
							objects.Add(building.gameObject);
					}
					catch (System.Exception e)
					{
						Debug.LogWarning(Session.Tag + "clearing " + rect + ": skipped " + building.name + ", whose cells threw: " + e.Message);
					}
				}
				foreach (GameObject go in objects)
				{
					Util.KDestroyGameObject(go);
					destroyed++;
				}
			}
			catch (System.Exception e)
			{
				Debug.LogWarning(Session.Tag + "clearing " + rect + " threw: " + e);
			}
			// The buildings go at the end of the frame. A tile taken away while its neighbours still
			// stand leaves its mass in the cell as a natural tile (the game's rule for mass with nowhere
			// to fall), a moment after it goes; the solids are turned to vacuum after that, and again
			// for any that turned solid since.
			yield return null;
			yield return null;
			int replaced = 0;
			float deadline = Time.realtimeSinceStartup + 10f;
			float nextSend = 0f;
			while (true)
			{
				bool solid = false;
				try
				{
					// Loose items: what Duplicants dropped or left, what a test dropped for them, and what
					// the buildings dropped as they went. Only the top item of a cell is on its object
					// layer; the partitioner has them all.
					foreach (int cell in rect.Cells)
						GameUtil.ClearCell(cell, deleteMinions: false);
					bool send = Time.realtimeSinceStartup >= nextSend;
					foreach (int cell in rect.Cells)
					{
						if (!Grid.Solid[cell])
							continue;
						solid = true;
						if (send)
						{
							SimMessages.ReplaceElement(cell, SimHashes.Vacuum, CellEventLogger.Instance.SandBoxTool, 0f);
							replaced++;
						}
					}
					if (send)
						nextSend = Time.realtimeSinceStartup + 1f;
				}
				catch (System.Exception e)
				{
					Debug.LogWarning(Session.Tag + "clearing " + rect + " threw: " + e);
				}
				if (!solid)
					break;
				if (Time.realtimeSinceStartup > deadline)
				{
					Debug.LogWarning(Session.Tag + "solid cells remain in " + rect + " after 10 s: " + DescribeSolid(rect));
					break;
				}
				yield return null;
			}
			yield return null;
			yield return null;
			if (destroyed > 0 || replaced > 0)
				Debug.Log(Session.Tag + "cleared " + rect + ": " + destroyed + " object(s) destroyed, " + replaced + " solid cell(s) replaced");
			WarnStanding(rect, keep);
		}

		/// <summary>True when one of the building's placement cells is in the rectangle.</summary>
		private static bool Touches(Building building, CellRect rect)
		{
			foreach (int cell in building.PlacementCells)
				if (rect.Contains(cell))
					return true;
			return false;
		}

		/// <summary>A warning naming what World.Build made in the rectangle that the clear left standing.</summary>
		private static void WarnStanding(CellRect rect, GameObject keep)
		{
			try
			{
				built.RemoveAll(go => go == null);
				var standing = new List<string>();
				foreach (GameObject go in built)
				{
					if (go == keep)
						continue;
					Building building = go.GetComponent<Building>();
					if (building != null ? Touches(building, rect) : rect.Contains(Grid.PosToCell(go)))
						standing.Add(go.name + " at " + Cells.Describe(Grid.PosToCell(go)));
				}
				if (standing.Count > 0)
					Debug.LogWarning(Session.Tag + "still standing in " + rect + " after the clear: " + string.Join("; ", standing));
			}
			catch (System.Exception e)
			{
				Debug.LogWarning(Session.Tag + "checking " + rect + " after the clear threw: " + e);
			}
		}

		/// <summary>The solid cells of the rectangle, with their element, mass and what is built there, for the log.</summary>
		private static string DescribeSolid(CellRect rect)
		{
			var parts = new List<string>();
			foreach (int cell in rect.Cells)
			{
				if (!Grid.Solid[cell])
					continue;
				GameObject tile = Grid.Objects[cell, (int)ObjectLayer.FoundationTile];
				GameObject building = Grid.Objects[cell, (int)ObjectLayer.Building];
				parts.Add(Cells.Describe(cell) + " " + Grid.Element[cell].id + " " + Grid.Mass[cell].ToString("0.#") + " kg"
					+ (tile != null ? " tile " + tile.name : "") + (building != null ? " building " + building.name : ""));
			}
			return string.Join("; ", parts);
		}
	}
}
