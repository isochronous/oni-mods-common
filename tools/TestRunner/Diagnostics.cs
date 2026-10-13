using System.Diagnostics;
using HarmonyLib;
using UnityEngine;

namespace Isochronous.TestRunner
{
	/// <summary>
	/// Opt-in tracing (--onitest-trace=1) of the grid marks the game makes while a test runs: who
	/// puts what on which layer, with the caller. For chasing a test that dies inside game code
	/// with nothing but an IL offset to go on.
	/// </summary>
	[HarmonyPatch(typeof(BuildingDef), nameof(BuildingDef.MarkArea))]
	public static class BuildingDef_MarkArea_Trace
	{
		public static void Postfix(BuildingDef __instance, int cell, ObjectLayer layer, GameObject go)
		{
			if (!Session.Active || !Session.Trace)
				return;
			var stack = new StackTrace(2, false);
			string callers = "";
			for (int i = 0; i < 4 && i < stack.FrameCount; i++)
			{
				var m = stack.GetFrame(i).GetMethod();
				callers += (i > 0 ? " < " : "") + m.DeclaringType?.Name + "." + m.Name;
			}
			UnityEngine.Debug.Log(Session.Tag + "trace MarkArea " + __instance.PrefabID + " at " + cell + " on " + layer + " with " + (go != null ? go.name : "null") + " from " + callers);
		}
	}

	[HarmonyPatch(typeof(BuildingDef), nameof(BuildingDef.TryPlace), typeof(GameObject), typeof(Vector3), typeof(Orientation), typeof(System.Collections.Generic.IList<Tag>), typeof(string), typeof(bool), typeof(int))]
	public static class BuildingDef_TryPlace_Trace
	{
		public static void Postfix(BuildingDef __instance, Vector3 pos, GameObject __result)
		{
			if (Session.Active && Session.Trace)
				UnityEngine.Debug.Log(Session.Tag + "trace TryPlace " + __instance.PrefabID + " at " + Grid.PosToCell(pos) + " -> " + (__result != null ? __result.name + " tile:" + (Grid.Objects[Grid.PosToCell(pos), (int)__instance.TileLayer]?.name ?? "null") : "null"));
		}
	}
}
