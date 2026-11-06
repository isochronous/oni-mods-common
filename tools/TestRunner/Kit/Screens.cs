using System.Collections;
using System.Collections.Generic;
using Isochronous.TestRunner;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Isochronous.TestKit
{
	/// <summary>
	/// Layer 2: the game's screens driven the way a player's clicks drive them. Selecting goes
	/// through the select tool, as a click on the building does; a click on a widget is the pointer
	/// events Unity's event system would send it. What the screens do with them is untouched game
	/// code. The selection is cleared at cleanup.
	/// </summary>
	public sealed class Screens
	{
		private readonly TestContext t;
		private bool selected;

		internal Screens(TestContext t)
		{
			this.t = t;
		}

		/// <summary>
		/// Selects the object as a click on it does (SelectTool.Select), waits until the details
		/// screen shows it, then 3 frames for its side screens to lay out. Yield it. A build tool
		/// clears the selection, as it does for a player, so select again after a drag.
		/// </summary>
		public IEnumerator Select(GameObject go)
		{
			if (go == null)
				throw new OniAssertException("nothing to select");
			KSelectable selectable = go.GetComponent<KSelectable>();
			if (selectable == null)
				throw new OniAssertException(go.name + " has no KSelectable to select");
			if (!selected)
			{
				selected = true;
				t.Cleanup(() =>
				{
					if (SelectTool.Instance != null)
						SelectTool.Instance.Select(null, true);
				});
			}
			SelectTool.Instance.Select(selectable);
			t.Log("selected " + go.name);
			yield return t.WaitUntil(() => DetailsScreen.Instance != null && DetailsScreen.Instance.target == go
				&& DetailsScreen.Instance.gameObject.activeInHierarchy, 10, "the details screen to show " + go.name);
			yield return t.Frames(3);
		}

		/// <summary>Clears the selection, as a click on nothing does; the details screen closes.</summary>
		public void Deselect()
		{
			SelectTool.Instance.Select(null);
			t.Log("deselected");
		}

		/// <summary>
		/// The side screen of that type the details screen is showing, or null when it shows none
		/// (DetailsScreen.sideScreens, the instances it made, active in the hierarchy).
		/// </summary>
		public T SideScreen<T>() where T : SideScreenContent
		{
			if (DetailsScreen.Instance == null)
				return null;
			if (!(Reflect.DetailsScreenSideScreens.GetValue(DetailsScreen.Instance) is List<DetailsScreen.SideScreenRef> refs))
				return null;
			foreach (DetailsScreen.SideScreenRef sideScreen in refs)
				if (sideScreen.screenInstance is T screen && screen.gameObject.activeInHierarchy)
					return screen;
			return null;
		}

		/// <summary>
		/// The first object named so under the root, nearest first (breadth first, inactive ones
		/// included, the root itself too), or null. Fails the test when the root is null, as when an
		/// outer Find found nothing.
		/// </summary>
		public GameObject Find(GameObject root, string name)
		{
			if (root == null)
				throw new OniAssertException("no object to look for " + name + " under");
			var queue = new Queue<Transform>();
			queue.Enqueue(root.transform);
			while (queue.Count > 0)
			{
				Transform current = queue.Dequeue();
				if (current.name == name)
					return current.gameObject;
				foreach (Transform child in current)
					queue.Enqueue(child);
			}
			return null;
		}

		/// <summary>
		/// A left click on the widget: pointer down, pointer up and click, the events Unity's event
		/// system sends a widget under the mouse, so KButton, MultiToggle and KToggle each react as
		/// they do to a player's click (a button that is not interactable ignores it). Fails the test
		/// when the widget is null or not showing. Wait half a second or so before the next click.
		/// </summary>
		public void Click(GameObject go)
		{
			if (go == null)
				throw new OniAssertException("nothing to click");
			if (!go.activeInHierarchy)
				throw new OniAssertException(go.name + " is not showing, so it cannot be clicked");
			var pointer = new PointerEventData(UnityEngine.EventSystems.EventSystem.current)
			{
				button = PointerEventData.InputButton.Left,
				pointerPress = go,
				rawPointerPress = go,
			};
			ExecuteEvents.Execute(go, pointer, ExecuteEvents.pointerDownHandler);
			ExecuteEvents.Execute(go, pointer, ExecuteEvents.pointerUpHandler);
			ExecuteEvents.Execute(go, pointer, ExecuteEvents.pointerClickHandler);
			t.Log("clicked " + go.name);
		}
	}
}
