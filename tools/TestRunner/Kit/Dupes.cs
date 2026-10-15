using System.Collections.Generic;
using UnityEngine;

namespace Isochronous.TestKit
{
	/// <summary>
	/// Duplicants for a test that needs work done by hand: a sealed box of tiles with air and a dev
	/// life support in it, Duplicants spawned inside who only work, the materials they need, and
	/// skills when a job asks for one. The fixture has no Duplicants, so nothing else competes for
	/// their errands. Every Duplicant spawned is removed at cleanup, before the area is cleared.
	/// </summary>
	public sealed class Dupes
	{
		/// <summary>Air pressure the box is filled to, in kg per cell.</summary>
		private const float AirKg = 1f;

		private const float RoomTemperature = 293.15f;

		/// <summary>The highest attribute level a Duplicant reaches by training.</summary>
		private const int MaxLevel = 20;

		private readonly TestContext t;
		private readonly List<MinionIdentity> spawned = new List<MinionIdentity>();
		private Schedule schedule;

		internal Dupes(TestContext t)
		{
			this.t = t;
		}

		/// <summary>The Duplicants this test spawned and has not removed.</summary>
		public IReadOnlyList<MinionIdentity> All => spawned;

		/// <summary>
		/// The whole fixture area as a box: its edge cells the walls, the rest inside. See
		/// <see cref="Box(CellRect, SimHashes)"/>. A test marked <c>[OniTest(Boxed = true)]</c> finds
		/// it standing when the test before it was boxed too; it is made good again here either way.
		/// </summary>
		public CellRect Box(SimHashes wall = SimHashes.IgneousRock)
		{
			CellRect inside = Box(t.Area, wall);
			Isochronous.TestRunner.FixtureArea.BoxStanding = true;
			Isochronous.TestRunner.FixtureArea.LifeSupport = Grid.Objects[inside.Cell(0, inside.H - 1), (int)ObjectLayer.Building];
			return inside;
		}

		/// <summary>
		/// Tiles of <paramref name="wall"/> round the edge of <paramref name="outer"/>, the cells
		/// inside filled with oxygen at room temperature, and a dev life support in the top left
		/// corner inside, which keeps the air breathable and the temperature even. Returns the inside,
		/// whose bottom row is the floor Duplicants stand on. Walls and life support already there are
		/// kept, and the air is filled again. Give the sim a game second before spawning.
		/// </summary>
		public CellRect Box(CellRect outer, SimHashes wall = SimHashes.IgneousRock)
		{
			if (outer.W < 3 || outer.H < 4)
				throw new OniAssertException("a box needs at least 3x4 cells for walls round a space two high: " + outer);
			BuildingDef tile = Assets.GetBuildingDef("Tile");
			var elements = new List<Tag> { wall.CreateTag() };
			int walls = 0;
			for (int y = 0; y < outer.H; y++)
				for (int x = 0; x < outer.W; x++)
				{
					if (x != 0 && y != 0 && x != outer.W - 1 && y != outer.H - 1)
						continue;
					int cell = outer.Cell(x, y);
					if (Grid.Objects[cell, (int)ObjectLayer.FoundationTile] != null)
						continue;
					if (!tile.IsValidPlaceLocation(null, cell, Orientation.Neutral, out string reason))
						throw new OniAssertException("cannot wall the box at " + Cells.Describe(cell) + ": " + reason);
					tile.Build(cell, Orientation.Neutral, null, elements, RoomTemperature, playsound: false, timeBuilt: GameClock.Instance.GetTime());
					walls++;
				}
			var inside = new CellRect(outer.X + 1, outer.Y + 1, outer.W - 2, outer.H - 2);
			foreach (int cell in inside.Cells)
				if (!Grid.Solid[cell])
					SimMessages.ReplaceElement(cell, SimHashes.Oxygen, CellEventLogger.Instance.SandBoxTool, AirKg, RoomTemperature);
			int corner = inside.Cell(0, inside.H - 1);
			GameObject there = Grid.Objects[corner, (int)ObjectLayer.Building];
			if (there == null || there.PrefabID() != "DevLifeSupport")
				t.World.Build("DevLifeSupport", corner, element: SimHashes.IgneousRock);
			t.Log((walls > 0 ? "boxed " + inside + " in " + walls + " tiles of " + wall : "the box round " + inside + " stands") + "; air filled, a dev life support in the corner");
			return inside;
		}

		/// <summary>
		/// <paramref name="count"/> Duplicants standing at the cell, as the sandbox spawner makes them,
		/// with only their model's base trait (no trait that forbids a kind of work, falls asleep or
		/// gets in the way), every attribute at 20, and a schedule of work at every hour. They are kept fed, rested and calm
		/// while the test runs, and removed at cleanup.
		/// </summary>
		public List<MinionIdentity> Spawn(int count, int cell)
		{
			if (spawned.Count == 0)
				t.Cleanup(RemoveAll);
			var made = new List<MinionIdentity>();
			for (int i = 0; i < count; i++)
			{
				GameObject prefab = Assets.GetPrefab(MinionConfig.ID);
				GameObject go = Util.KInstantiate(prefab);
				go.name = prefab.name;
				Immigration.Instance.ApplyDefaultPersonalPriorities(go);
				go.transform.SetLocalPosition(Grid.CellToPosCBC(cell, Grid.SceneLayer.Move));
				go.SetActive(true);
				var stats = new MinionStartingStats(MinionConfig.MODEL, is_starter_minion: false, isDebugMinion: true);
				stats.Traits.RemoveRange(1, stats.Traits.Count - 1);
				stats.congenitaltrait = null;
				// Every attribute as high as a Duplicant's own levels go, for the fastest work vanilla allows.
				foreach (string attribute in new List<string>(stats.StartingLevels.Keys))
					stats.StartingLevels[attribute] = MaxLevel;
				stats.Apply(go);
				go.GetMyWorld().SetDupeVisited();
				MinionIdentity dupe = go.GetComponent<MinionIdentity>();
				AlwaysWork(dupe);
				go.AddComponent<KeptFit>();
				spawned.Add(dupe);
				made.Add(dupe);
			}
			t.Log("spawned " + count + " Duplicant(s) at " + Cells.Describe(cell) + ": " + string.Join(", ", made.ConvertAll(d => d.GetProperName())));
			return made;
		}

		/// <summary>The skills, by id (Db.Get().Skills), mastered by every Duplicant of the test, without skill points or morale.</summary>
		public void Grant(params string[] skillIds)
		{
			foreach (MinionIdentity dupe in spawned)
				Grant(dupe, skillIds);
		}

		public void Grant(MinionIdentity dupe, params string[] skillIds)
		{
			MinionResume resume = dupe.GetComponent<MinionResume>();
			foreach (string id in skillIds)
			{
				if (Db.Get().Skills.TryGet(id) == null)
					throw new OniAssertException("no skill named " + id);
				resume.GrantSkill(id);
			}
			t.Log(dupe.GetProperName() + " granted " + string.Join(", ", skillIds));
		}

		/// <summary>
		/// A loose pile of the material at the cell, for Duplicants to fetch: an element in kg, or
		/// an item (reed fiber, say) of that total mass. Solids come at room temperature or just
		/// under their melting point, whichever is lower.
		/// </summary>
		public GameObject Drop(Tag material, float kg, int cell)
		{
			Vector3 pos = Grid.CellToPosCCC(cell, Grid.SceneLayer.Ore);
			Element element = ElementLoader.GetElement(material);
			GameObject go;
			if (element != null)
			{
				float temperature = Mathf.Min(RoomTemperature, element.highTemp - 10f);
				go = element.substance.SpawnResource(pos, kg, temperature, byte.MaxValue, 0);
			}
			else
			{
				GameObject prefab = Assets.TryGetPrefab(material);
				if (prefab == null)
					throw new OniAssertException("no element or item named " + material);
				go = GameUtil.KInstantiate(prefab, pos, Grid.SceneLayer.Ore);
				PrimaryElement primary = go.GetComponent<PrimaryElement>();
				if (primary != null)
					primary.Mass = kg;
				go.SetActive(true);
			}
			t.Log("dropped " + kg + " kg of " + material.Name + " at " + Cells.Describe(cell));
			return go;
		}

		public GameObject Drop(SimHashes element, float kg, int cell)
		{
			return Drop(element.CreateTag(), kg, cell);
		}

		/// <summary>The Duplicant out of the colony now, before the test ends.</summary>
		public void Remove(MinionIdentity dupe)
		{
			if (dupe == null)
				return;
			spawned.Remove(dupe);
			schedule?.Unassign(dupe.GetComponent<Schedulable>());
			Util.KDestroyGameObject(dupe.gameObject);
		}

		/// <summary>Every Duplicant of the test removed, and the schedule they were on.</summary>
		public void RemoveAll()
		{
			int count = spawned.Count;
			foreach (MinionIdentity dupe in spawned.ToArray())
				Remove(dupe);
			if (schedule != null && ScheduleManager.Instance != null)
				ScheduleManager.Instance.DeleteSchedule(schedule);
			schedule = null;
			if (count > 0)
				t.Log("removed " + count + " Duplicant(s)");
		}

		/// <summary>
		/// On a schedule of work at every hour, so the time of day the test runs at does not send
		/// them to bed or to their break.
		/// </summary>
		private void AlwaysWork(MinionIdentity dupe)
		{
			Schedulable schedulable = dupe.GetComponent<Schedulable>();
			if (schedule == null)
			{
				// Made the way the schedule screen makes one (each group its default hours, 24 in all),
				// then every hour turned to work.
				schedule = ScheduleManager.Instance.AddSchedule(Db.Get().ScheduleGroups.allGroups, "Test crew");
				for (int hour = 0; hour < schedule.GetBlocks().Count; hour++)
					schedule.SetBlockGroup(hour, Db.Get().ScheduleGroups.Worktime);
			}
			ScheduleManager.Instance.GetSchedule(schedulable)?.Unassign(schedulable);
			schedule.Assign(schedulable);
		}
	}

	/// <summary>
	/// Keeps a test's Duplicant from wanting anything but work: fed, rested, calm, breathing and
	/// whole, topped up once a real second. A test lasts minutes; what a Duplicant needs over a
	/// cycle is no part of what it tests.
	/// </summary>
	internal sealed class KeptFit : MonoBehaviour
	{
		private float next;

		private void Update()
		{
			if (Time.realtimeSinceStartup < next)
				return;
			next = Time.realtimeSinceStartup + 1f;
			Database.Amounts amounts = Db.Get().Amounts;
			Full(amounts.Calories);
			Full(amounts.Stamina);
			Full(amounts.Breath);
			Full(amounts.HitPoints);
			Empty(amounts.Bladder);
			Empty(amounts.Stress);
		}

		private void Full(Klei.AI.Amount amount)
		{
			Klei.AI.AmountInstance instance = amount.Lookup(gameObject);
			if (instance != null)
				instance.SetValue(instance.GetMax());
		}

		private void Empty(Klei.AI.Amount amount)
		{
			Klei.AI.AmountInstance instance = amount.Lookup(gameObject);
			if (instance != null)
				instance.SetValue(instance.GetMin());
		}
	}
}
