using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

namespace Isochronous.TestRunner
{
	/// <summary>
	/// Every private game member the harness and the kit touch, by name, in one place, with a
	/// self-check the run performs before its first test so that a game update that renames one
	/// fails the whole run with the name rather than one test with a NullReferenceException.
	/// FieldInfo rather than FieldRefAccess: these are cold paths, and a FieldRef's dynamic method
	/// cannot be built on the CoreCLR host that ModCheck runs on.
	/// </summary>
	public static class Reflect
	{
		/// <summary>KInputManager.hasFocus: set only by OnApplicationFocus; false while the window is behind another.</summary>
		public static readonly FieldInfo KInputManagerHasFocus = AccessTools.Field(typeof(KInputManager), "hasFocus");
		/// <summary>InterfaceTool.isAppFocused: gates the visualizer move in OnMouseMove.</summary>
		public static readonly FieldInfo InterfaceToolIsAppFocused = AccessTools.Field(typeof(InterfaceTool), "isAppFocused");
		/// <summary>BaseUtilityBuildTool.path: the drag's nodes with their preview visualizers.</summary>
		public static readonly FieldInfo BaseUtilityBuildToolPath = AccessTools.Field(typeof(BaseUtilityBuildTool), "path");
		public static readonly Type PathNodeType = AccessTools.Inner(typeof(BaseUtilityBuildTool), "PathNode");
		public static readonly FieldInfo PathNodeCell = PathNodeType == null ? null : AccessTools.Field(PathNodeType, "cell");
		public static readonly FieldInfo PathNodeVisualizer = PathNodeType == null ? null : AccessTools.Field(PathNodeType, "visualizer");
		public static readonly FieldInfo PathNodeValid = PathNodeType == null ? null : AccessTools.Field(PathNodeType, "valid");
		/// <summary>DragTool.downPos: where the current drag started.</summary>
		public static readonly FieldInfo DragToolDownPos = AccessTools.Field(typeof(DragTool), "downPos");
		/// <summary>Timelapser.previewScreenshot: true while the image being written is a save's preview, not a timelapse frame.</summary>
		public static readonly FieldInfo TimelapserPreviewScreenshot = AccessTools.Field(typeof(Timelapser), "previewScreenshot");
		/// <summary>Timelapser.previewSaveGamePath: the save whose preview image is being written, next to it.</summary>
		public static readonly FieldInfo TimelapserPreviewSaveGamePath = AccessTools.Field(typeof(Timelapser), "previewSaveGamePath");

		/// <summary>The names that did not resolve; empty when the game still has every member.</summary>
		public static List<string> SelfCheck()
		{
			var missing = new List<string>();
			if (KInputManagerHasFocus == null) missing.Add("KInputManager.hasFocus");
			if (InterfaceToolIsAppFocused == null) missing.Add("InterfaceTool.isAppFocused");
			if (BaseUtilityBuildToolPath == null) missing.Add("BaseUtilityBuildTool.path");
			if (PathNodeType == null) missing.Add("BaseUtilityBuildTool.PathNode");
			if (PathNodeCell == null) missing.Add("BaseUtilityBuildTool.PathNode.cell");
			if (PathNodeVisualizer == null) missing.Add("BaseUtilityBuildTool.PathNode.visualizer");
			if (PathNodeValid == null) missing.Add("BaseUtilityBuildTool.PathNode.valid");
			if (DragToolDownPos == null) missing.Add("DragTool.downPos");
			if (TimelapserPreviewScreenshot == null) missing.Add("Timelapser.previewScreenshot");
			if (TimelapserPreviewSaveGamePath == null) missing.Add("Timelapser.previewSaveGamePath");
			return missing;
		}

		public static void SetAppFocused(InterfaceTool tool, bool value)
		{
			if (tool != null && InterfaceToolIsAppFocused != null)
				InterfaceToolIsAppFocused.SetValue(tool, value);
		}
	}
}
