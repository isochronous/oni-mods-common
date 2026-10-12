using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using HarmonyLib;

namespace Isochronous
{
	/// <summary>
	/// Translations for a mod's own strings, loaded from <c>translations/&lt;locale&gt;.po</c> next to
	/// the mod DLL (<c>zh.po</c>, <c>ko.po</c>, <c>ru.po</c>, the game's locale codes). Compiled into
	/// every mod that sets <c>UseTranslations</c> in its csproj.
	///
	/// The mod declares its strings as a <c>ModStrings</c> class whose nested classes mirror the
	/// game's <c>STRINGS</c> tree: <c>ModStrings.BUILDINGS.PREFABS.X.NAME</c> is registered as
	/// <c>STRINGS.BUILDINGS.PREFABS.X.NAME</c>, which is the key the game itself looks a building's
	/// name up by, and the key translators see in the po file. Mod-private text lives under its own
	/// branch, <c>ModStrings.MYMOD.SIDESCREEN.TITLE</c>. Every field is a public static
	/// <see cref="LocString"/>; code reads the field when it needs the text, never at static init.
	///
	/// The language is applied twice: at registration, from the game's saved language preference,
	/// so that strings a mod copies early (logic port descriptions, tag names, action titles) are
	/// already translated when the game reads them, and again after <see cref="Localization.Initialize"/>
	/// with the locale the game actually settled on. Both passes replace the fields and the string
	/// table entries, so key lookups (<c>Strings.Get</c>, <c>LocString.ToString</c>, PLib option
	/// titles given as keys) and direct field reads agree.
	/// </summary>
	internal static class Translations
	{
		private const string Folder = "translations";
		private const string KeyRoot = "STRINGS.";

		private static Type root;
		private static string modFolder;
		private static System.Action afterApply;
		private static readonly List<KeyValuePair<FieldInfo, object>> english = new List<KeyValuePair<FieldInfo, object>>();
		/// <summary>The po file currently applied, "" for English; null before the first pass.</summary>
		private static string applied;

		/// <summary>
		/// Registers the mod's string tree and loads the translation for the game's language.
		/// Call from <c>UserMod2.OnLoad</c>. <paramref name="afterApply"/> runs after every pass,
		/// for strings the game keeps a copy of (a tag's proper name, for one).
		/// </summary>
		public static void Register(Harmony harmony, Type stringsRoot, System.Action afterApply = null)
		{
			root = stringsRoot;
			Translations.afterApply = afterApply;
			modFolder = FindModFolder(stringsRoot.Assembly);
			foreach (Type branch in stringsRoot.GetNestedTypes())
				LocString.CreateLocStringKeys(branch, KeyRoot);
			Snapshot(stringsRoot);
			Apply(GuessLanguageCode());
			harmony.Patch(AccessTools.Method(typeof(Localization), nameof(Localization.Initialize)),
				postfix: new HarmonyMethod(typeof(Translations), nameof(OnLocalizationInitialized)));
		}

		/// <summary>The game has chosen its locale; apply the matching file (or English).</summary>
		private static void OnLocalizationInitialized()
		{
			Localization.Locale locale = Localization.GetLocale();
			string code = locale != null ? locale.Code : null;
			if (string.IsNullOrEmpty(code))
				code = GuessLanguageCode();
			Apply(code);
		}

		/// <summary>The saved language preference, readable before Localization.Initialize runs.</summary>
		private static string GuessLanguageCode()
		{
			try
			{
				return Localization.GetCurrentLanguageCode();
			}
			catch (Exception)
			{
				return "";
			}
		}

		private static void Apply(string code)
		{
			string file = FindFile(code);
			string key = file ?? "";
			if (key == applied)
				return;
			RestoreEnglish();
			int count = 0;
			if (file != null)
			{
				Dictionary<string, string> translated = null;
				try
				{
					translated = Localization.LoadStringsFile(file, isTemplate: false);
				}
				catch (Exception e)
				{
					Debug.LogWarning("[" + root.Assembly.GetName().Name + "] Could not read " + file + ": " + e.Message);
				}
				if (translated != null)
				{
					string parameterErrors = "", linkErrors = "", linkCountErrors = "";
					foreach (Type branch in root.GetNestedTypes())
						Localization.OverloadStrings(translated, KeyRoot + branch.Name, branch, ref parameterErrors, ref linkErrors, ref linkCountErrors);
					ReportErrors(file, parameterErrors, "missing or mismatched parameters");
					ReportErrors(file, linkErrors, "mismatched <link> tags");
					ReportErrors(file, linkCountErrors, "a different number of <link> tags than the English text");
					count = translated.Count;
				}
			}
			applied = key;
			AddToStringTable(root);
			if (file != null)
				Debug.Log("[" + root.Assembly.GetName().Name + "] Loaded " + count + " translated strings from " + Path.GetFileName(file));
			afterApply?.Invoke();
		}

		/// <summary>
		/// translations/&lt;code&gt;.po, else the part of the code before any "_" or "-" (the saved
		/// preference for Klei's Chinese is "zh_klei"; the locale code is "zh").
		/// </summary>
		private static string FindFile(string code)
		{
			if (string.IsNullOrEmpty(code) || modFolder == null)
				return null;
			string folder = Path.Combine(modFolder, Folder);
			foreach (string candidate in new[] { code, code.Split('_', '-')[0] })
			{
				if (candidate.Length == 0)
					continue;
				string path = Path.Combine(folder, candidate.ToLowerInvariant() + ".po");
				if (File.Exists(path))
					return path;
			}
			return null;
		}

		private static void Snapshot(Type type)
		{
			foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.Static))
				if (field.FieldType == typeof(LocString))
					english.Add(new KeyValuePair<FieldInfo, object>(field, field.GetValue(null)));
			foreach (Type nested in type.GetNestedTypes())
				Snapshot(nested);
		}

		private static void RestoreEnglish()
		{
			foreach (KeyValuePair<FieldInfo, object> pair in english)
				pair.Key.SetValue(null, pair.Value);
		}

		/// <summary>Puts every field's current text in the game's string table under its key.</summary>
		private static void AddToStringTable(Type type)
		{
			foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.Static))
			{
				if (field.FieldType != typeof(LocString))
					continue;
				LocString value = (LocString)field.GetValue(null);
				if (value != null && value.key.IsValid())
					Strings.Add(value.key.String, value.text);
			}
			foreach (Type nested in type.GetNestedTypes())
				AddToStringTable(nested);
		}

		private static void ReportErrors(string file, string errors, string what)
		{
			if (!string.IsNullOrEmpty(errors))
				Debug.LogWarning("[" + root.Assembly.GetName().Name + "] " + Path.GetFileName(file) + ": these strings have " + what + " and keep their English text:\n" + errors);
		}

		/// <summary>The folder the mod's content is in: next to the DLL, else from the mod manager.</summary>
		private static string FindModFolder(Assembly assembly)
		{
			try
			{
				string location = assembly.Location;
				if (!string.IsNullOrEmpty(location))
					return Path.GetDirectoryName(location);
			}
			catch (Exception)
			{
			}
			try
			{
				foreach (KMod.Mod mod in Global.Instance.modManager.mods)
					if (mod.loaded_mod_data != null && mod.loaded_mod_data.dlls != null && mod.loaded_mod_data.dlls.Contains(assembly))
						return mod.ContentPath;
			}
			catch (Exception)
			{
			}
			return null;
		}
	}
}
