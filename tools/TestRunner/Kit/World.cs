using System.Collections.Generic;
using UnityEngine;

namespace Isochronous.TestKit
{
	/// <summary>
	/// Layer 0: the game's own calls, no tool and no UI. For fixture setup and for oracles; a test
	/// about a tool or a screen drives that through Tools instead.
	/// </summary>
	public sealed class World
	{
		private readonly TestContext t;

		internal World(TestContext t)
		{
			this.t = t;
		}

		/// <summary>
		/// A finished building at the cell (the building's origin cell: the middle of a 3x1), the
		/// way an instant build makes one. Fails the test when the game would refuse the placement,
		/// with the game's reason.
		/// </summary>
		public GameObject Build(string prefabId, int cell, Orientation orientation = Orientation.Neutral, SimHashes element = SimHashes.Copper)
		{
			BuildingDef def = Assets.GetBuildingDef(prefabId);
			if (def == null)
				throw new OniAssertException("no building def named " + prefabId);
			var elements = new List<Tag> { element.CreateTag() };
			if (!def.IsValidPlaceLocation(def.BuildingComplete, cell, orientation, out string reason))
				throw new OniAssertException("cannot place " + prefabId + " at " + Cells.Describe(cell) + ": " + reason);
			float temperature = Mathf.Min(def.Temperature, ElementLoader.GetMinMeltingPointAmongElements(elements) - 10f);
			GameObject go = def.Build(cell, orientation, null, elements, temperature, playsound: false, timeBuilt: GameClock.Instance.GetTime());
			t.Log("built " + prefabId + " at " + Cells.Describe(cell) + (orientation != Orientation.Neutral ? " rotated " + orientation : ""));
			return go;
		}

		/// <summary>The object on the layer at the cell, or null.</summary>
		public GameObject At(int cell, ObjectLayer layer)
		{
			return Grid.Objects[cell, (int)layer];
		}

		public void Destroy(GameObject go)
		{
			if (go != null)
				Util.KDestroyGameObject(go);
		}

		/// <summary>The logic network at the cell, or null when no wire or port there belongs to one.</summary>
		public LogicCircuitNetwork LogicNetwork(int cell)
		{
			return Game.Instance.logicCircuitManager.GetNetworkForCell(cell);
		}

		/// <summary>The automation wire bits recorded at the cell (physical grid, what wires and bridge ends draw from).</summary>
		public UtilityConnections LogicConnections(int cell)
		{
			return Game.Instance.logicCircuitSystem.GetConnections(cell, is_physical_building: true);
		}

	}

	/// <summary>Cell helpers for messages; a separate class because the game has a World type of its own that wins in a test's namespace.</summary>
	public static class Cells
	{
		/// <summary>"1234 (x, y)".</summary>
		public static string Describe(int cell)
		{
			Grid.CellToXY(cell, out int x, out int y);
			return cell + " (" + x + ", " + y + ")";
		}
	}
}
