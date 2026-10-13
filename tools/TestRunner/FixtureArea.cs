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
	/// every object in it destroyed (Duplicants excepted) and every solid cell turned to vacuum.
	/// </summary>
	public static class FixtureArea
	{
		public const int Width = 40;
		public const int Height = 10;

		public static CellRect Current;

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

		/// <summary>Everything out of the rectangle: objects destroyed, solids turned to vacuum, then two frames for the sim and the spawners to settle.</summary>
		public static IEnumerator Clear(CellRect rect)
		{
			int destroyed = 0, replaced = 0;
			try
			{
				var objects = new HashSet<GameObject>();
				foreach (int cell in rect.Cells)
				{
					for (int layer = 0; layer < (int)ObjectLayer.NumLayers; layer++)
					{
						GameObject go = Grid.Objects[cell, layer];
						if (go != null && go.GetComponent<MinionIdentity>() == null)
							objects.Add(go);
					}
				}
				foreach (GameObject go in objects)
				{
					Util.KDestroyGameObject(go);
					destroyed++;
				}
				foreach (int cell in rect.Cells)
				{
					if (Grid.Solid[cell])
					{
						SimMessages.ReplaceElement(cell, SimHashes.Vacuum, CellEventLogger.Instance.SandBoxTool, 0f);
						replaced++;
					}
				}
			}
			catch (System.Exception e)
			{
				Debug.LogWarning(Session.Tag + "clearing " + rect + " threw: " + e);
			}
			if (replaced > 0)
			{
				float deadline = Time.realtimeSinceStartup + 10f;
				while (Time.realtimeSinceStartup < deadline && AnySolid(rect))
					yield return null;
				if (AnySolid(rect))
					Debug.LogWarning(Session.Tag + "solid cells remain in " + rect + " after 10 s");
			}
			yield return null;
			yield return null;
			if (destroyed > 0 || replaced > 0)
				Debug.Log(Session.Tag + "cleared " + rect + ": " + destroyed + " object(s) destroyed, " + replaced + " solid cell(s) replaced");
		}

		private static bool AnySolid(CellRect rect)
		{
			foreach (int cell in rect.Cells)
				if (Grid.Solid[cell])
					return true;
			return false;
		}
	}
}
