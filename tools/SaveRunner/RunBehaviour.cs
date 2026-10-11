using System;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;

namespace SaveRunner
{
	/// <summary>
	/// Lives on the Game object for one run: unpauses, sets the speed, samples the probe once per
	/// game second and ends the run when the time is up. Game seconds come from the scaled frame
	/// time, so speed 3 gets through the run three times faster.
	/// </summary>
	public sealed class RunBehaviour : MonoBehaviour
	{
		private double elapsed;
		private int samples;
		private bool started;
		private StreamWriter writer;

		private void Update()
		{
			if (Runner.Finished)
				return;
			try
			{
				if (!started)
				{
					Debug.Log(Runner.Tag + "first update, time scale " + Time.timeScale);
					Begin();
				}
				elapsed += Time.deltaTime;
				if (elapsed >= samples)
				{
					Sample(samples);
					samples++;
				}
				if (elapsed >= Runner.Seconds)
				{
					Close();
					Runner.Finish("done", samples + " samples");
				}
			}
			catch (Exception e)
			{
				Debug.LogWarning(Runner.Tag + e);
				Close();
				Runner.Finish("runner-error", e.ToString());
			}
		}

		private void Begin()
		{
			started = true;
			SpeedControlScreen speed = SpeedControlScreen.Instance;
			if (speed != null)
			{
				speed.SetSpeed(Runner.Speed - 1);
				for (int i = 0; i < 8 && speed.IsPaused; i++)
					speed.Unpause(playSound: false);
			}
			if (Runner.OutFile != null && Runner.Probe != null)
			{
				writer = new StreamWriter(Runner.OutFile, append: false, Encoding.UTF8);
				if (Runner.Probe == "fridges")
					writer.WriteLine("t,fridge,fridgeId,cell,state,active,operational,thermostat,setpointC,cellTempC,cellElement,cellMassKg,item,itemId,itemElement,massKg,tempC,surfaceArea,groundTransferScale,sttEnabled,simHandle,deltaKJ");
				writer.Flush();
			}
			Debug.Log(Runner.Tag + "running " + Runner.Seconds + " s at speed " + Runner.Speed);
		}

		private void Sample(int t)
		{
			if (Runner.Probe == "fridges")
				FridgeProbe.Sample(t, writer, t % 10 == 0);
			writer?.Flush();
		}

		private void Close()
		{
			writer?.Flush();
			writer?.Dispose();
			writer = null;
		}

		private void OnDisable()
		{
			Debug.Log(Runner.Tag + "runner disabled after " + samples + " samples");
		}

		private void OnDestroy()
		{
			Debug.Log(Runner.Tag + "runner destroyed after " + samples + " samples");
			Close();
		}
	}

	/// <summary>
	/// Every building with a fridge controller (Refrigerator, Mini Fridge, modded fridges) and every
	/// item in it: the item's temperature as the game reads it, its heat-exchange settings, the
	/// cell it sits in, and whether No Magic Fridges' thermostat is on the building.
	/// </summary>
	public static class FridgeProbe
	{
		private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

		public static void Sample(int t, StreamWriter writer, bool toLog)
		{
			int fridges = 0, items = 0;
			foreach (BuildingComplete building in Components.BuildingCompletes.Items)
			{
				if (building == null)
					continue;
				GameObject go = building.gameObject;
				RefrigeratorController.Def def = go.GetDef<RefrigeratorController.Def>();
				if (def == null)
					continue;
				fridges++;
				Storage storage = go.GetComponent<Storage>();
				Operational operational = go.GetComponent<Operational>();
				RefrigeratorController.StatesInstance smi = go.GetSMI<RefrigeratorController.StatesInstance>();
				int cell = Grid.PosToCell(go);
				bool valid = Grid.IsValidCell(cell);
				string head = string.Join(",",
					t.ToString(Inv),
					Csv(go.GetProperName()),
					go.GetInstanceID().ToString(Inv),
					cell.ToString(Inv),
					Csv(smi != null ? smi.GetCurrentState().name : ""),
					operational != null && operational.IsActive ? "1" : "0",
					operational != null && operational.IsOperational ? "1" : "0",
					go.GetComponent("FridgeThermostat") != null ? "1" : "0",
					F(def.simulatedInternalTemperature - 273.15f),
					valid ? F(Grid.Temperature[cell] - 273.15f) : "",
					valid ? Grid.Element[cell].id.ToString() : "",
					valid ? F(Grid.Mass[cell]) : "");
				if (storage == null || storage.items.Count == 0)
				{
					writer?.WriteLine(head + ",,,,,,,,,,");
					continue;
				}
				foreach (GameObject item in storage.items)
				{
					if (item == null)
						continue;
					items++;
					PrimaryElement pe = item.GetComponent<PrimaryElement>();
					SimTemperatureTransfer stt = item.GetComponent<SimTemperatureTransfer>();
					string row = head + "," + string.Join(",",
						Csv(item.GetProperName()),
						item.GetInstanceID().ToString(Inv),
						pe != null ? pe.Element.id.ToString() : "",
						pe != null ? F(pe.Mass) : "",
						pe != null ? F(pe.Temperature - 273.15f) : "",
						stt != null ? F(stt.SurfaceArea) : "",
						stt != null ? F(stt.GroundTransferScale) : "",
						stt != null ? (stt.enabled ? "1" : "0") : "",
						stt != null ? stt.SimHandle.ToString(Inv) : "",
						stt != null ? F(stt.deltaKJ) : "");
					writer?.WriteLine(row);
					if (toLog)
						Debug.Log(Runner.Tag + row);
				}
			}
			if (toLog)
				Debug.Log(Runner.Tag + "t=" + t + ": " + fridges + " fridge(s), " + items + " item(s)");
		}

		private static string F(float v)
		{
			return v.ToString("F3", Inv);
		}

		private static string Csv(string s)
		{
			s = s ?? "";
			return s.IndexOfAny(new[] { ',', '"', '\n' }) >= 0 ? "\"" + s.Replace("\"", "\"\"") + "\"" : s;
		}
	}
}
