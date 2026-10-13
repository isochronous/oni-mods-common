using System;
using System.Collections;
using System.Collections.Generic;
using Isochronous.TestRunner;
using UnityEngine;

namespace Isochronous.TestKit
{
	/// <summary>
	/// Layer 1: the real InterfaceTool fed the same calls PlayerController makes from mouse
	/// events (OnMouseMove while the pointer moves, OnLeftClickDown, OnMouseMove per cell crossed,
	/// OnLeftClickUp), with world positions. What the tool does with them is untouched game code:
	/// the drag path, placement validation, preview colours, the build itself.
	/// </summary>
	public sealed class Tools
	{
		private readonly TestContext t;

		internal Tools(TestContext t)
		{
			this.t = t;
		}

		/// <summary>
		/// The utility build tool (what the build menu activates for wires, pipes and automation
		/// wires) with the def and material selected, as the menu would have left it. Dispose it to
		/// go back to the select tool; the cleanup stack does that anyway.
		/// </summary>
		public UtilityDragSession UtilityTool(string prefabId, SimHashes element = SimHashes.Copper)
		{
			BuildingDef def = Assets.GetBuildingDef(prefabId);
			if (def == null)
				throw new OniAssertException("no building def named " + prefabId);
			if (PlanScreen.Instance == null)
				throw new OniAssertException("no PlanScreen: build tools need the colony HUD");
			var session = new UtilityDragSession(t, def, element);
			t.Cleanup(session.Dispose);
			return session;
		}
	}

	/// <summary>One activation of UtilityBuildTool; its drags are coroutines to yield.</summary>
	public sealed class UtilityDragSession : IDisposable
	{
		private readonly TestContext t;
		private readonly Dictionary<int, Color> previewTints = new Dictionary<int, Color>();
		private readonly Dictionary<int, bool> previewValid = new Dictionary<int, bool>();
		private bool disposed;

		public BuildingDef Def { get; }
		public BaseUtilityBuildTool Tool { get; }

		internal UtilityDragSession(TestContext t, BuildingDef def, SimHashes element)
		{
			this.t = t;
			Def = def;
			Tool = UtilityBuildTool.Instance;
			Tool.Activate(def, new List<Tag> { element.CreateTag() });
			Harness.ForceFocus();
			t.Log("activated " + Tool.GetType().Name + " for " + def.PrefabID + " of " + element);
		}

		/// <summary>
		/// A careful drag: the pointer visits each listed cell in turn, one frame per cell, from
		/// the first (button down) to the last (button up). Consecutive cells should be neighbours,
		/// as on a player's path. One frame passes before the up event so the tool's preview
		/// coroutine has run; the preview tints are snapshotted then (see PreviewTint).
		/// </summary>
		public IEnumerator Path(params int[] cells)
		{
			if (cells == null || cells.Length == 0)
				throw new ArgumentException("a path needs at least one cell");
			yield return PrimeBuildMenu(t);
			Tool.OnMouseMove(Pos(cells[0]));
			yield return null;
			Tool.OnLeftClickDown(Pos(cells[0]));
			t.Log("drag down at " + Cells.Describe(cells[0]));
			for (int i = 1; i < cells.Length; i++)
			{
				yield return null;
				Tool.OnMouseMove(Pos(cells[i]));
			}
			yield return null;
			yield return null;
			SnapshotPreview();
			t.Log("before the up at " + Cells.Describe(cells[cells.Length - 1]) + ": " + DescribePreview());
			try
			{
				Tool.OnLeftClickUp(Pos(cells[cells.Length - 1]));
			}
			catch (Exception)
			{
				t.Log("the up threw; cells now: " + DescribePreview() + "; instant build " + DebugHandler.InstantBuildMode + ", sandbox " + Game.Instance.SandboxModeActive);
				foreach (int cell in cells)
				{
					GameObject onTile = Def.TileLayer == ObjectLayer.NumLayers ? null : Grid.Objects[cell, (int)Def.TileLayer];
					if (onTile == null)
						continue;
					PrimaryElement pe = onTile.GetComponent<PrimaryElement>();
					t.Log("tile occupant at " + Cells.Describe(cell) + ": " + onTile.name + ", PrimaryElement " + (pe == null ? "missing" : pe.ElementID + " -> " + (pe.Element == null ? "no Element" : pe.Element.id.ToString()))
						+ ", BuildingComplete " + (onTile.GetComponent<BuildingComplete>() != null) + ", Constructable " + (onTile.GetComponent<Constructable>() != null));
				}
				throw;
			}
			t.Log("drag up at " + Cells.Describe(cells[cells.Length - 1]) + "; preview " + DescribePreview());
			yield return null;
		}

		/// <summary>
		/// A player reaches a build tool through the build menu, whose product info screen has
		/// then been shown once; that first showing runs its Awake, which creates the material
		/// selection panel that BuildPath reads the build priority from after queuing an order
		/// (PlanScreen.GetBuildingPriority, a one-liner Mono inlines, so the NullReferenceException
		/// it throws otherwise is reported inside BuildPath). Activating a tool directly skips the
		/// menu, so the screen is woken once here: shown for a frame, so its spawn runs as it would
		/// behind the menu, then hidden again.
		/// </summary>
		internal static IEnumerator PrimeBuildMenu(TestContext t)
		{
			ProductInfoScreen screen = PlanScreen.Instance != null ? PlanScreen.Instance.ProductInfoScreen : null;
			if (screen == null || screen.materialSelectionPanel != null)
			{
				yield return null;
				yield break;
			}
			screen.gameObject.SetActive(true);
			yield return null;
			screen.gameObject.SetActive(false);
			t.Log("woke the product info screen once, as the build menu would have (material panel " + (screen.materialSelectionPanel != null) + ")");
		}

		/// <summary>A player's fast swipe: one mouse move from the first cell to the last; the tool samples the cells in between itself.</summary>
		public IEnumerator Sweep(int from, int to)
		{
			yield return PrimeBuildMenu(t);
			Tool.OnMouseMove(Pos(from));
			yield return null;
			Tool.OnLeftClickDown(Pos(from));
			yield return null;
			Tool.OnMouseMove(Pos(to));
			yield return null;
			yield return null;
			SnapshotPreview();
			Tool.OnLeftClickUp(Pos(to));
			t.Log("sweep from " + Cells.Describe(from) + " to " + Cells.Describe(to) + "; preview " + DescribePreview());
			yield return null;
		}

		/// <summary>The tint the preview at the cell had just before the button went up (white: the game would build there, red: not), or null when the cell was not on the path.</summary>
		public Color? PreviewTint(int cell)
		{
			return previewTints.TryGetValue(cell, out Color c) ? c : (Color?)null;
		}

		/// <summary>Whether the tool judged the cell a valid path piece when the drag reached it.</summary>
		public bool? PreviewValid(int cell)
		{
			return previewValid.TryGetValue(cell, out bool v) ? v : (bool?)null;
		}

		private void SnapshotPreview()
		{
			previewTints.Clear();
			previewValid.Clear();
			var path = (IList)Reflect.BaseUtilityBuildToolPath.GetValue(Tool);
			foreach (object node in path)
			{
				int cell = (int)Reflect.PathNodeCell.GetValue(node);
				previewValid[cell] = (bool)Reflect.PathNodeValid.GetValue(node);
				var visualizer = Reflect.PathNodeVisualizer.GetValue(node) as GameObject;
				KBatchedAnimController controller = visualizer != null ? visualizer.GetComponent<KBatchedAnimController>() : null;
				if (controller != null)
					previewTints[cell] = controller.TintColour;
			}
		}

		/// <summary>The preview tints plus what sits on the def's layers at each path cell, for the log.</summary>
		private string DescribePreview()
		{
			var parts = new List<string>();
			foreach (KeyValuePair<int, Color> pair in previewTints)
			{
				GameObject onLayer = Grid.Objects[pair.Key, (int)Def.ObjectLayer];
				GameObject onTile = Def.TileLayer == ObjectLayer.NumLayers ? null : Grid.Objects[pair.Key, (int)Def.TileLayer];
				parts.Add(pair.Key + "=" + (pair.Value == Color.white ? "white" : pair.Value == Color.red ? "red" : pair.Value.ToString())
					+ (onLayer != null ? " layer:" + onLayer.name : "") + (onTile != null ? " tile:" + onTile.name + (onTile.GetComponent<PrimaryElement>() == null ? "(no PrimaryElement)" : "") : ""));
			}
			return parts.Count == 0 ? "(no nodes)" : string.Join(", ", parts);
		}

		private static Vector3 Pos(int cell)
		{
			return Grid.CellToPosCCC(cell, Grid.SceneLayer.Building);
		}

		public void Dispose()
		{
			if (disposed)
				return;
			disposed = true;
			if (Tool != null && Tool.Dragging)
				Tool.CancelDragging();
			if (PlayerController.Instance != null && PlayerController.Instance.ActiveTool == Tool && SelectTool.Instance != null)
				PlayerController.Instance.ActivateTool(SelectTool.Instance);
		}
	}
}
