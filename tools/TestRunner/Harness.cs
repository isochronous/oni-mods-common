using System;
using UnityEngine;

namespace Isochronous.TestRunner
{
	/// <summary>
	/// The global switches a run throws so that synthetic input reaches the game's tools and real
	/// input does not: window focus forced on, the mouse position locked, the physical keyboard and
	/// mouse ignored, instant build on, the sim running at speed 1. Each is undone by the game
	/// quitting; nothing here is saved, since saving is blocked for the session.
	/// </summary>
	public static class Harness
	{
		/// <summary>Focus flags on (see Patches: KInputManager_OnApplicationFocus_Patch for why).</summary>
		public static void ForceFocus()
		{
			Reflect.KInputManagerHasFocus?.SetValue(null, true);
			if (PlayerController.Instance != null && PlayerController.Instance.tools != null)
				foreach (InterfaceTool tool in PlayerController.Instance.tools)
					Reflect.SetAppFocused(tool, true);
		}

		/// <summary>
		/// The physical keyboard and mouse stop producing events (KInputController's ignore flags;
		/// the parameter is the ignore state despite the name). Queued controller events, the L3a
		/// path, bypass these flags.
		/// </summary>
		public static void IgnorePhysicalInput()
		{
			KInputController controller = Global.GetInputManager().GetDefaultController();
			controller.ToggleKeyboard(true);
			controller.ToggleMouse(true);
		}

		/// <summary>
		/// KInputManager.GetMousePos returns this screen point for the rest of the session, so a hand
		/// on the mouse cannot move a tool's visualizer under a running test (PlayerController.Update
		/// sends OnMouseMove whenever the position changes).
		/// </summary>
		public static void LockMouse(int cell)
		{
			KInputManager.lockedMousePos = Camera.main.WorldToScreenPoint(Grid.CellToPosCCC(cell, Grid.SceneLayer.Building));
			KInputManager.isMousePosLocked = true;
		}

		/// <summary>
		/// The locked mouse in the bottom right corner of the screen, where no hover card or tool
		/// visualizer it raises covers what a test looks at or captures. Tests that need the cursor
		/// somewhere call TestContext.PointAt.
		/// </summary>
		public static void ParkMouse()
		{
			KInputManager.lockedMousePos = new Vector3(Screen.width - 1, 0f, 0f);
			KInputManager.isMousePosLocked = true;
		}

		/// <summary>Instant build the way the debug key does it, so the tools' configs and the plan screen follow.</summary>
		public static void EnsureInstantBuild()
		{
			DebugHandler.SetDebugEnabled(true);
			if (!DebugHandler.InstantBuildMode)
				DebugHandler.ToggleInstantBuildMode();
		}

		public static void Unpause()
		{
			SpeedControlScreen speed = SpeedControlScreen.Instance;
			if (speed == null)
				return;
			speed.SetSpeed(0);
			for (int i = 0; i < 8 && speed.IsPaused; i++)
				speed.Unpause(playSound: false);
		}

		/// <summary>
		/// Back to the select tool with no drag in progress, whatever a test left behind. A DragTool
		/// turns the UI event system off between its down and up; a test that died in between would
		/// leave the UI dead, so the event system is switched on here unconditionally.
		/// </summary>
		public static void ResetTools()
		{
			PlayerController controller = PlayerController.Instance;
			if (controller == null)
				return;
			InterfaceTool active = controller.ActiveTool;
			if (active is DragTool drag && drag.Dragging)
				drag.CancelDragging();
			if (KScreenManager.Instance != null)
				KScreenManager.Instance.SetEventSystemEnabled(true);
			if (SelectTool.Instance != null && active != SelectTool.Instance)
				controller.ActivateTool(SelectTool.Instance);
		}

		/// <summary>Camera on the fixture area, at a size where the whole area is on screen.</summary>
		public static void LookAt(int cell, float orthographicSize)
		{
			try
			{
				CameraController.Instance.SnapTo(Grid.CellToPos(cell), orthographicSize);
			}
			catch (Exception e)
			{
				Debug.LogWarning(Session.Tag + "cannot move the camera: " + e.Message);
			}
		}
	}
}
