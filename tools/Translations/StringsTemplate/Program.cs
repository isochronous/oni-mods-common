using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

// Writes a mod's translation template and checks its translations, from the built DLL, without
// launching the game. Loads the DLL against the real game assemblies (on .NET 8, like ModCheck),
// runs the ModStrings initializers, and walks the tree the way Isochronous.Translations registers
// it: ModStrings.A.B.C is the key STRINGS.A.B.C, its English text the field's value.
//
//   StringsTemplate <game Managed dir> <mod dll> <translations dir>
//
// Writes <translations dir>/<Mod>.pot (created when missing), then for every <locale>.po there
// reports entries that are missing, that translate text the English no longer says (stale),
// that are left empty, that no longer exist (obsolete), and that would be refused by the game
// (different {0} parameters or <link> counts than the English). Also checks that every PLib
// [Option] title, tooltip and category written as a STRINGS. key is a key in the tree.
// Exit code 1 when a po has missing, stale or refused entries, or an option key is unknown.
internal static class Program
{
	private sealed class Entry
	{
		public string Key;
		public string English;
	}

	private static int Main(string[] args)
	{
		if (args.Length < 3)
		{
			Console.WriteLine("usage: StringsTemplate <game Managed dir> <mod dll> <translations dir>");
			return 2;
		}
		string managedDir = args[0];
		string modDll = Path.GetFullPath(args[1]);
		string translationsDir = Path.GetFullPath(args[2]);
		Console.OutputEncoding = new UTF8Encoding(false);

		AppDomain.CurrentDomain.AssemblyResolve += (_, e) =>
		{
			string path = Path.Combine(managedDir, new AssemblyName(e.Name).Name + ".dll");
			return File.Exists(path) ? Assembly.LoadFrom(path) : null;
		};

		Assembly asm = Assembly.LoadFrom(modDll);
		Type root = asm.GetTypes().FirstOrDefault(t => t.Name == "ModStrings" && !t.IsNested);
		if (root == null)
		{
			Console.WriteLine("FAIL no ModStrings class in " + asm.GetName().Name);
			return 1;
		}
		var entries = new List<Entry>();
		foreach (Type branch in root.GetNestedTypes())
			Collect(branch, "STRINGS." + branch.Name, entries);
		entries.Sort((a, b) => string.CompareOrdinal(a.Key, b.Key));
		if (entries.Count == 0)
		{
			Console.WriteLine("FAIL ModStrings has no LocString fields");
			return 1;
		}

		Directory.CreateDirectory(translationsDir);
		string pot = Path.Combine(translationsDir, asm.GetName().Name + ".pot");
		WritePot(pot, entries);
		Console.WriteLine("Wrote " + entries.Count + " strings to " + pot);

		int problems = CheckOptionKeys(asm, entries);
		foreach (string po in Directory.GetFiles(translationsDir, "*.po").OrderBy(p => p))
			problems += CheckPo(po, entries);
		Console.WriteLine(problems == 0 ? "ALL CHECKS PASSED" : problems + " PROBLEM(S)");
		return problems == 0 ? 0 : 1;
	}

	private static void Collect(Type type, string path, List<Entry> entries)
	{
		foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.Static))
		{
			if (field.FieldType.Name != "LocString")
				continue;
			object value = field.GetValue(null);
			string text = value == null ? null : (string)value.GetType().GetProperty("text").GetValue(value);
			entries.Add(new Entry { Key = path + "." + field.Name, English = text ?? "" });
		}
		foreach (Type nested in type.GetNestedTypes())
			Collect(nested, path + "." + nested.Name, entries);
	}

	private static void WritePot(string path, List<Entry> entries)
	{
		var w = new StringBuilder();
		w.Append("msgid \"\"\nmsgstr \"\"\n");
		w.Append("\"Application: Oxygen Not Included\\n\"\n");
		w.Append("\"POT Version: 2.0\\n\"\n");
		w.Append("\"MIME-Version: 1.0\\n\"\n");
		w.Append("\"Content-Type: text/plain; charset=UTF-8\\n\"\n");
		w.Append("\"Content-Transfer-Encoding: 8bit\\n\"\n\n");
		foreach (Entry e in entries)
		{
			w.Append("#. ").Append(e.Key).Append('\n');
			w.Append("msgctxt \"").Append(e.Key).Append("\"\n");
			w.Append("msgid \"").Append(Escape(e.English)).Append("\"\n");
			w.Append("msgstr \"\"\n\n");
		}
		File.WriteAllText(path, w.ToString(), new UTF8Encoding(false));
	}

	/// <summary>The escaping the game's reader undoes: quotes and newlines only.</summary>
	private static string Escape(string text)
	{
		return text.Replace("\"", "\\\"").Replace("\r", "").Replace("\n", "\\n");
	}

	private static string Unescape(string text)
	{
		return text.Replace("\\n", "\n").Replace("\\\"", "\"");
	}

	private static int CheckOptionKeys(Assembly asm, List<Entry> entries)
	{
		var keys = new HashSet<string>(entries.Select(e => e.Key));
		int problems = 0;
		foreach (Type type in asm.GetTypes())
		{
			IEnumerable<MemberInfo> members = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
				.Cast<MemberInfo>()
				.Concat(type.GetFields(BindingFlags.Public | BindingFlags.Static))
				.Concat(new MemberInfo[] { type });
			foreach (MemberInfo member in members)
			{
				foreach (object attribute in member.GetCustomAttributes(false))
				{
					string name = attribute.GetType().Name;
					if (name != "OptionAttribute" && name != "ConfigFileAttribute")
						continue;
					foreach (string property in new[] { "Title", "Tooltip", "Category" })
					{
						PropertyInfo p = attribute.GetType().GetProperty(property);
						string value = p?.GetValue(attribute) as string;
						if (value != null && value.StartsWith("STRINGS.") && !keys.Contains(value))
						{
							Console.WriteLine("FAIL option key " + value + " on " + type.Name + "." + member.Name + " is not in ModStrings");
							problems++;
						}
					}
				}
			}
		}
		return problems;
	}

	private static int CheckPo(string path, List<Entry> entries)
	{
		Dictionary<string, KeyValuePair<string, string>> translated = ReadPo(path);
		string name = Path.GetFileName(path);
		int missing = 0, stale = 0, empty = 0, refused = 0, obsolete = 0;
		var seen = new HashSet<string>();
		foreach (Entry e in entries)
		{
			if (!translated.TryGetValue(e.Key, out KeyValuePair<string, string> pair))
			{
				Console.WriteLine("MISSING " + name + ": " + e.Key);
				missing++;
				continue;
			}
			seen.Add(e.Key);
			if (pair.Value.Length == 0)
			{
				Console.WriteLine("EMPTY " + name + ": " + e.Key);
				empty++;
				continue;
			}
			if (pair.Key != e.English)
			{
				Console.WriteLine("STALE " + name + ": " + e.Key + "\n    po:  " + Escape(pair.Key) + "\n    now: " + Escape(e.English));
				stale++;
			}
			if (!SameParameters(e.English, pair.Value))
			{
				Console.WriteLine("REFUSED " + name + ": " + e.Key + " does not keep the English {n} parameters");
				refused++;
			}
			else if (LinkCount(pair.Value) > LinkCount(e.English) || !LinksMatch(pair.Value))
			{
				Console.WriteLine("REFUSED " + name + ": " + e.Key + " has more <link> tags than the English text, or unmatched ones");
				refused++;
			}
		}
		foreach (string key in translated.Keys)
		{
			if (!seen.Contains(key))
			{
				Console.WriteLine("OBSOLETE " + name + ": " + key);
				obsolete++;
			}
		}
		Console.WriteLine(name + ": " + (entries.Count - missing - empty) + "/" + entries.Count + " translated"
			+ (missing > 0 ? ", " + missing + " missing" : "")
			+ (empty > 0 ? ", " + empty + " empty" : "")
			+ (stale > 0 ? ", " + stale + " stale" : "")
			+ (refused > 0 ? ", " + refused + " refused by the game" : "")
			+ (obsolete > 0 ? ", " + obsolete + " obsolete" : ""));
		return missing + stale + refused;
	}

	/// <summary>msgctxt -> (msgid, msgstr), parsed the way the game's reader does it: a keyword line plus any following lines that start with a quote.</summary>
	private static Dictionary<string, KeyValuePair<string, string>> ReadPo(string path)
	{
		var result = new Dictionary<string, KeyValuePair<string, string>>();
		string[] lines = File.ReadAllLines(path, Encoding.UTF8);
		string ctx = null, id = null, str = null;
		for (int i = 0; i <= lines.Length; i++)
		{
			string line = i < lines.Length ? lines[i].TrimEnd('\r') : "";
			if (line.Length == 0)
			{
				if (ctx != null && id != null && str != null)
					result[ctx] = new KeyValuePair<string, string>(id, str);
				ctx = id = str = null;
				continue;
			}
			if (line.StartsWith("msgctxt "))
				ctx = Parameter(lines, ref i);
			else if (line.StartsWith("msgid "))
				id = Parameter(lines, ref i);
			else if (line.StartsWith("msgstr "))
				str = Parameter(lines, ref i);
		}
		return result;
	}

	private static string Parameter(string[] lines, ref int i)
	{
		string first = lines[i].TrimEnd('\r');
		var text = new StringBuilder(Quoted(first.Substring(first.IndexOf(' ') + 1)));
		while (i + 1 < lines.Length && lines[i + 1].TrimEnd('\r').StartsWith("\""))
		{
			i++;
			text.Append(Quoted(lines[i].TrimEnd('\r')));
		}
		return Unescape(text.ToString());
	}

	private static string Quoted(string line)
	{
		line = line.Trim();
		return line.Length >= 2 && line[0] == '"' && line[line.Length - 1] == '"' ? line.Substring(1, line.Length - 2) : line;
	}

	/// <summary>The game's rule: the set of distinct {…} tokens must be the same.</summary>
	private static readonly Regex Parameters = new Regex(@"\{[^}]*\}");

	private static bool SameParameters(string english, string translated)
	{
		var a = new HashSet<string>(Parameters.Matches(english).Cast<Match>().Select(m => m.Value));
		var b = new HashSet<string>(Parameters.Matches(translated).Cast<Match>().Select(m => m.Value));
		return a.SetEquals(b);
	}

	private static int LinkCount(string text)
	{
		return Regex.Matches(text, "<link=").Count;
	}

	private static bool LinksMatch(string text)
	{
		return Regex.Matches(text, "<link=").Count == Regex.Matches(text, "</link>").Count;
	}
}
