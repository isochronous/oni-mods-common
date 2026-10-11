using HarmonyLib;
using KMod;

namespace WorldgenDriver
{
	/// <summary>
	/// Development-only mod: when the game is started with --worldgen=COORD it generates that
	/// cluster unattended, writes a result file and quits. Without the argument it does nothing.
	/// </summary>
	public sealed class WorldgenDriverMod : UserMod2
	{
		public override void OnLoad(Harmony harmony)
		{
			Driver.ReadCommandLine();
			if (!Driver.Active)
			{
				Debug.Log(Driver.Tag + "no --worldgen argument; idle");
				return;
			}
			base.OnLoad(harmony);
		}
	}
}
