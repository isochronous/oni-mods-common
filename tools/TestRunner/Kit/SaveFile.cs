using System.IO;
using UnityEngine;

namespace Isochronous.TestKit
{
	/// <summary>
	/// A building through the game's own save and load: what the save file holds for it, written
	/// and read back by SaveLoadRoot, the way a colony save does (templates first, then the object).
	/// For settings a mod keeps on a building; a whole colony's round trip is a save to the run
	/// folder (SaveLoader.Instance.Save under Session.Dir) and a [OniTest(FreshSave = true)] test.
	/// </summary>
	public static class SaveFile
	{
		public static byte[] Capture(GameObject building)
		{
			var objects = new MemoryStream();
			var writer = new BinaryWriter(objects);
			building.GetComponent<SaveLoadRoot>().Save(writer);
			writer.Flush();
			// The directory lists the templates used so far, so it is written after the object.
			var file = new MemoryStream();
			var directory = new BinaryWriter(file);
			KSerialization.Manager.SerializeDirectory(directory);
			directory.Write(objects.ToArray());
			directory.Flush();
			return file.ToArray();
		}

		/// <summary>The building as the load makes it, at the place it was saved; the original must be gone.</summary>
		public static GameObject Restore(Tag prefab, byte[] file)
		{
			var reader = new FastReader(file);
			KSerialization.Manager.DeserializeDirectory(reader);
			return SaveLoadRoot.Load(prefab, reader).gameObject;
		}
	}
}
