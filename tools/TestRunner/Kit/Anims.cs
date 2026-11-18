using Isochronous.TestRunner;
using UnityEngine;

namespace Isochronous.TestKit
{
	/// <summary>
	/// Layer 0 oracles on a building's animation: the anim it plays, whether a symbol of its build
	/// shows and the tint the game gave it, read from the controller as the renderer gets them. For
	/// lamps, gauges and anything else a building draws by playing anims, hiding or tinting symbols.
	/// </summary>
	public static class Anims
	{
		/// <summary>
		/// The name of the anim the controller is playing (the last one Play or Queue started), or
		/// null when it plays none. Fails the test when there is no controller.
		/// </summary>
		public static string Current(KBatchedAnimController kbac)
		{
			if (kbac == null)
				throw new OniAssertException("no animation controller to read the current anim from");
			KAnim.Anim anim = kbac.GetCurrentAnim();
			return anim?.name;
		}

		/// <summary>
		/// The tint of the symbol, as KBatchedAnimController.SetSymbolTint last set it (white when
		/// nothing has). Fails the test, naming the symbol, when the build has no such symbol.
		/// </summary>
		public static Color SymbolTint(KBatchedAnimController kbac, KAnimHashedString symbol)
		{
			KAnim.Build.Symbol found = Symbol(kbac, symbol);
			// SymbolInstanceGpuData has a setter for the tint and no getter; the instances sit in its private converter.
			var converter = (SymbolInstanceGpuData.SymbolInstanceToByteConverter)Reflect.SymbolInstanceGpuDataConverter.GetValue(kbac.symbolInstanceGpuData);
			return converter.symbolInstances[found.symbolIndexInSourceBuild].color;
		}

		/// <summary>
		/// Whether the symbol shows (SetSymbolVisiblity has not hidden it). Fails the test, naming
		/// the symbol, when the build has no such symbol, where the game would answer true.
		/// </summary>
		public static bool SymbolVisible(KBatchedAnimController kbac, KAnimHashedString symbol)
		{
			Symbol(kbac, symbol);
			return kbac.GetSymbolVisiblity(symbol);
		}

		private static KAnim.Build.Symbol Symbol(KBatchedAnimController kbac, KAnimHashedString symbol)
		{
			if (kbac == null)
				throw new OniAssertException("no animation controller to read " + symbol + " from");
			KAnim.Build.Symbol found = KAnimBatchManager.Instance().GetBatchGroupData(kbac.GetBatchGroupID()).GetSymbol(symbol);
			if (found == null)
				throw new OniAssertException(kbac.name + "'s build has no symbol " + symbol);
			return found;
		}
	}
}
