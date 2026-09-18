using Steamworks;

namespace WorkshopUpload;

/// <summary>
/// Legacy (ISteamRemoteStorage) Workshop publisher for Oxygen Not Included mods.
///
///   WorkshopUpload info &lt;itemId&gt;
///       Print the item's details as the legacy API sees them (file name, size, handle).
///   WorkshopUpload list
///       List the logged-in user's published items for app 457140.
///   WorkshopUpload update &lt;itemId&gt; &lt;mod.zip&gt; [preview.png] [--title "..."] [--description-file path] [--changenote "..."]
///       Replace an existing item's content with the zip (and optionally its preview).
///   WorkshopUpload publish &lt;mod.zip&gt; &lt;preview.png&gt; --title "..." [--description-file path] [--visibility public|friends|private]
///       Create a new item. Prints the new item id.
///
/// The zip must contain mod.yaml, mod_info.yaml, the DLL, and preview.png at its root,
/// exactly like the items Klei's own uploader produces.
/// </summary>
internal static class Program
{
	/// <summary>The game: consumer app of every item.</summary>
	private const uint AppId = 457140;
	/// <summary>Klei's "Oxygen Not Included Uploader" tool: the app legacy items are published from
	/// (creator_app_id of every working item). steam_appid.txt selects it; sharing cloud files
	/// under the game's own app id fails with FileNotFound.</summary>
	private const uint UploaderAppId = 636750;

	private static int Main(string[] args)
	{
		if (args.Length == 0)
		{
			Console.Error.WriteLine("usage: WorkshopUpload info <id> | list | update <id> <zip> [preview] [options] | publish <zip> <preview> --title T [options]");
			return 2;
		}
		if (!SteamAPI.Init())
		{
			Console.Error.WriteLine("SteamAPI.Init failed. Is the Steam client running and logged in? Is steam_appid.txt next to the exe?");
			return 1;
		}
		try
		{
			Console.WriteLine($"running as app {SteamUtils.GetAppID().m_AppId} (uploader is {UploaderAppId}, game is {AppId})");
			Console.WriteLine($"Steam user: {SteamFriends.GetPersonaName()} ({SteamUser.GetSteamID()})");
			switch (args[0])
			{
				case "info": return Info(ulong.Parse(args[1]));
				case "list": return List();
				case "update": return Update(args);
				case "publish": return Publish(args);
				case "cloud": return Cloud();
				case "ugc-update": return UgcUpdate(args);
				case "ugc-describe": return UgcDescribe(args);
				case "download": return Download(ulong.Parse(args[1]));
				default:
					Console.Error.WriteLine("unknown command " + args[0]);
					return 2;
			}
		}
		finally
		{
			SteamAPI.Shutdown();
		}
	}

	// ---- commands ----

	private static int Info(ulong id)
	{
		var details = Await<RemoteStorageGetPublishedFileDetailsResult_t>(
			SteamRemoteStorage.GetPublishedFileDetails(new PublishedFileId_t(id), 0));
		Console.WriteLine($"result:   {details.m_eResult}");
		Console.WriteLine($"title:    {details.m_rgchTitle}");
		Console.WriteLine($"file:     '{details.m_pchFileName}' size {details.m_nFileSize} handle {details.m_hFile}");
		Console.WriteLine($"preview:  handle {details.m_hPreviewFile} size {details.m_nPreviewFileSize}");
		Console.WriteLine($"visible:  {details.m_eVisibility}  type {details.m_eFileType}  updated {DateTimeOffset.FromUnixTimeSeconds(details.m_rtimeUpdated)}");
		Console.WriteLine(details.m_hFile == UGCHandle_t.Invalid
			? "NOT a legacy single-file item: the game cannot download this."
			: "Legacy single-file item: downloadable by the game.");
		return details.m_eResult == EResult.k_EResultOK ? 0 : 1;
	}

	private static int List()
	{
		uint start = 0;
		while (true)
		{
			var page = Await<RemoteStorageEnumerateUserPublishedFilesResult_t>(SteamRemoteStorage.EnumerateUserPublishedFiles(start));
			if (page.m_eResult != EResult.k_EResultOK)
			{
				Console.Error.WriteLine("enumerate failed: " + page.m_eResult);
				return 1;
			}
			for (int i = 0; i < page.m_nResultsReturned; i++)
			{
				var details = Await<RemoteStorageGetPublishedFileDetailsResult_t>(
					SteamRemoteStorage.GetPublishedFileDetails(page.m_rgPublishedFileId[i], 0));
				string kind = details.m_hFile == UGCHandle_t.Invalid ? "NON-LEGACY (broken)" : "legacy";
				Console.WriteLine($"{page.m_rgPublishedFileId[i]}  {details.m_rgchTitle,-40} {details.m_eVisibility,-45} {kind}");
			}
			start += (uint)page.m_nResultsReturned;
			if (start >= page.m_nTotalResultCount || page.m_nResultsReturned == 0)
				return 0;
		}
	}

	private static int Update(string[] args)
	{
		if (args.Length < 3)
		{
			Console.Error.WriteLine("usage: update <id> [zip [preview]] [--title T] [--description-file F] [--changenote N] [--visibility public|friends|unlisted|private]");
			return 2;
		}
		ulong id = ulong.Parse(args[1]);
		string zipPath = !args[2].StartsWith("--") ? args[2] : null;
		string previewPath = zipPath != null && args.Length > 3 && !args[3].StartsWith("--") ? args[3] : null;
		var opts = ParseOptions(args);

		string cloudZip = zipPath != null ? UploadToCloud(zipPath) : null;
		string cloudPreview = previewPath != null ? UploadToCloud(previewPath) : null;

		PublishedFileUpdateHandle_t handle = SteamRemoteStorage.CreatePublishedFileUpdateRequest(new PublishedFileId_t(id));
		if (cloudZip != null)
			Check(SteamRemoteStorage.UpdatePublishedFileFile(handle, cloudZip), "UpdatePublishedFileFile");
		if (cloudPreview != null)
			Check(SteamRemoteStorage.UpdatePublishedFilePreviewFile(handle, cloudPreview), "UpdatePublishedFilePreviewFile");
		if (opts.TryGetValue("title", out string title))
			Check(SteamRemoteStorage.UpdatePublishedFileTitle(handle, title), "UpdatePublishedFileTitle");
		if (opts.TryGetValue("description-file", out string descFile))
			Check(SteamRemoteStorage.UpdatePublishedFileDescription(handle, File.ReadAllText(descFile)), "UpdatePublishedFileDescription");
		if (opts.TryGetValue("changenote", out string note))
			Check(SteamRemoteStorage.UpdatePublishedFileSetChangeDescription(handle, note), "UpdatePublishedFileSetChangeDescription");
		if (opts.TryGetValue("visibility", out string vis))
			Check(SteamRemoteStorage.UpdatePublishedFileVisibility(handle, ParseVisibility(vis)), "UpdatePublishedFileVisibility");

		Console.WriteLine($"Committing update to item {id} ...");
		var result = Await<RemoteStorageUpdatePublishedFileResult_t>(SteamRemoteStorage.CommitPublishedFileUpdate(handle));
		Console.WriteLine("result: " + result.m_eResult + (result.m_bUserNeedsToAcceptWorkshopLegalAgreement ? " (user must accept the Workshop legal agreement)" : ""));
		if (result.m_eResult != EResult.k_EResultOK)
			return 1;
		return Info(id);
	}

	private static int Publish(string[] args)
	{
		if (args.Length < 3)
		{
			Console.Error.WriteLine("usage: publish <zip> <preview> --title T [--description-file F] [--visibility public|friends|private]");
			return 2;
		}
		var opts = ParseOptions(args);
		if (!opts.TryGetValue("title", out string title))
		{
			Console.Error.WriteLine("--title is required");
			return 2;
		}
		string description = opts.TryGetValue("description-file", out string descFile) ? File.ReadAllText(descFile) : "";
		var visibility = ParseVisibility(opts.TryGetValue("visibility", out string vis) ? vis : "private");

		string cloudZip = UploadToCloud(args[1]);
		string cloudPreview = UploadToCloud(args[2]);
		Console.WriteLine($"Publishing '{title}' ({visibility}) ...");
		var result = Await<RemoteStoragePublishFileResult_t>(SteamRemoteStorage.PublishWorkshopFile(
			cloudZip, cloudPreview, new AppId_t(AppId), title, description, visibility, new List<string>(),
			EWorkshopFileType.k_EWorkshopFileTypeCommunity));
		Console.WriteLine("result: " + result.m_eResult + (result.m_bUserNeedsToAcceptWorkshopLegalAgreement ? " (user must accept the Workshop legal agreement)" : ""));
		if (result.m_eResult != EResult.k_EResultOK)
			return 1;
		Console.WriteLine("new item id: " + result.m_nPublishedFileId);
		Console.WriteLine("https://steamcommunity.com/sharedfiles/filedetails/?id=" + result.m_nPublishedFileId);
		return Info(result.m_nPublishedFileId.m_PublishedFileId);
	}

	/// <summary>Steam Cloud diagnostics for the legacy path.</summary>
	private static int Cloud()
	{
		Console.WriteLine($"cloud enabled for account: {SteamRemoteStorage.IsCloudEnabledForAccount()}");
		Console.WriteLine($"cloud enabled for app:     {SteamRemoteStorage.IsCloudEnabledForApp()}");
		SteamRemoteStorage.GetQuota(out ulong total, out ulong available);
		Console.WriteLine($"quota: {available}/{total} bytes available");
		int count = SteamRemoteStorage.GetFileCount();
		Console.WriteLine($"files in cloud: {count}");
		for (int i = 0; i < count; i++)
		{
			string name = SteamRemoteStorage.GetFileNameAndSize(i, out int size);
			Console.WriteLine($"  {name} {size} bytes persisted={SteamRemoteStorage.FilePersisted(name)}");
		}
		return 0;
	}

	/// <summary>
	/// ISteamUGC update with a content folder. Used to test whether a folder holding a
	/// single zip is stored as a legacy-downloadable item.
	///   ugc-update &lt;id&gt; &lt;contentFolder&gt; [preview.png] [--changenote N]
	/// </summary>
	private static int UgcUpdate(string[] args)
	{
		if (args.Length < 3)
		{
			Console.Error.WriteLine("usage: ugc-update <id> <contentFolder> [preview] [--changenote N]");
			return 2;
		}
		ulong id = ulong.Parse(args[1]);
		string folder = Path.GetFullPath(args[2]);
		string previewPath = args.Length > 3 && !args[3].StartsWith("--") ? Path.GetFullPath(args[3]) : null;
		var opts = ParseOptions(args);
		Console.WriteLine($"content folder: {folder} ({string.Join(", ", Directory.GetFiles(folder).Select(Path.GetFileName))})");
		UGCUpdateHandle_t handle = SteamUGC.StartItemUpdate(new AppId_t(AppId), new PublishedFileId_t(id));
		Check(SteamUGC.SetItemContent(handle, folder), "SetItemContent");
		if (previewPath != null)
			Check(SteamUGC.SetItemPreview(handle, previewPath), "SetItemPreview");
		opts.TryGetValue("changenote", out string note);
		Console.WriteLine($"Submitting UGC update to item {id} ...");
		var result = Await<SubmitItemUpdateResult_t>(SteamUGC.SubmitItemUpdate(handle, note ?? ""), 600);
		Console.WriteLine("result: " + result.m_eResult + (result.m_bUserNeedsToAcceptWorkshopLegalAgreement ? " (user must accept the Workshop legal agreement)" : ""));
		if (result.m_eResult != EResult.k_EResultOK)
			return 1;
		return Info(id);
	}

	/// <summary>
	/// Downloads the item with this Steam client (owner can fetch private items) and reports
	/// what the game would see: the install path and whether it is a single zip file.
	/// </summary>
	private static int Download(ulong id)
	{
		var fileId = new PublishedFileId_t(id);
		EResult downloadResult = EResult.k_EResultPending;
		bool done = false;
		using var cb = Callback<DownloadItemResult_t>.Create(r =>
		{
			if (r.m_nPublishedFileId == fileId) { downloadResult = r.m_eResult; done = true; }
		});
		Console.WriteLine($"item state before: {(EItemState)SteamUGC.GetItemState(fileId)}");
		if (!SteamUGC.DownloadItem(fileId, true))
		{
			Console.Error.WriteLine("DownloadItem returned false (invalid item or not allowed)");
			return 1;
		}
		var deadline = DateTime.UtcNow.AddSeconds(180);
		while (!done && DateTime.UtcNow < deadline)
		{
			SteamAPI.RunCallbacks();
			Thread.Sleep(100);
		}
		Console.WriteLine($"download result: {(done ? downloadResult.ToString() : "timeout")}");
		Console.WriteLine($"item state after:  {(EItemState)SteamUGC.GetItemState(fileId)}");
		if (!SteamUGC.GetItemInstallInfo(fileId, out ulong size, out string path, 1024, out uint timestamp))
		{
			Console.WriteLine("GetItemInstallInfo: not installed");
			return 1;
		}
		Console.WriteLine($"install path: {path} ({size} bytes)");
		if (File.Exists(path))
		{
			Console.WriteLine("path is a FILE: the game will open it as a zip (legacy item, OK)");
			using var zip = System.IO.Compression.ZipFile.OpenRead(path);
			foreach (var e in zip.Entries) Console.WriteLine($"  {e.FullName} {e.Length}");
			return 0;
		}
		if (Directory.Exists(path))
		{
			Console.WriteLine("path is a DIRECTORY: File.Exists fails in the game -> 'Steam failed to download the mod'");
			foreach (var f in Directory.GetFiles(path, "*", SearchOption.AllDirectories)) Console.WriteLine($"  {f}");
			return 1;
		}
		Console.WriteLine("path does not exist");
		return 1;
	}

	/// <summary>
	/// Metadata-only ISteamUGC update (no content): description, title, visibility. Works on
	/// items created by steamcmd, which the legacy API refuses to touch. Run with the
	/// environment variable SteamAppId=457140 so the item's own app is used.
	///   ugc-describe &lt;id&gt; --description-file F [--title T] [--visibility public|friends|unlisted|private] [--changenote N]
	/// </summary>
	private static int UgcDescribe(string[] args)
	{
		if (args.Length < 2)
		{
			Console.Error.WriteLine("usage: ugc-describe <id> --description-file F [--title T] [--visibility V] [--changenote N]");
			return 2;
		}
		ulong id = ulong.Parse(args[1]);
		var opts = ParseOptions(args);
		UGCUpdateHandle_t handle = SteamUGC.StartItemUpdate(new AppId_t(AppId), new PublishedFileId_t(id));
		if (opts.TryGetValue("description-file", out string descFile))
			Check(SteamUGC.SetItemDescription(handle, File.ReadAllText(descFile)), "SetItemDescription");
		if (opts.TryGetValue("title", out string title))
			Check(SteamUGC.SetItemTitle(handle, title), "SetItemTitle");
		if (opts.TryGetValue("visibility", out string vis))
			Check(SteamUGC.SetItemVisibility(handle, ParseVisibility(vis)), "SetItemVisibility");
		opts.TryGetValue("changenote", out string note);
		Console.WriteLine($"Submitting metadata update to item {id} ...");
		var result = Await<SubmitItemUpdateResult_t>(SteamUGC.SubmitItemUpdate(handle, note ?? ""), 300);
		Console.WriteLine("result: " + result.m_eResult);
		return result.m_eResult == EResult.k_EResultOK ? 0 : 1;
	}

	// ---- helpers ----

	/// <summary>Writes a local file into the user's Steam Cloud for this app and returns its cloud name.</summary>
	private static string UploadToCloud(string localPath)
	{
		byte[] bytes = File.ReadAllBytes(localPath);
		// Same cloud names Klei's OniUploader64.exe uses (it is a native wxWidgets app driving
		// ISteamRemoteStorage v014: FileWrite, FileShare, PublishWorkshopFile, UpdatePublishedFile).
		string cloudName = localPath.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) ? "mod_publish_data_file.zip" : "mod_publish_preview.png";
		Console.WriteLine($"Uploading {cloudName} ({bytes.Length} bytes) to Steam Cloud ...");
		if (!SteamRemoteStorage.FileWrite(cloudName, bytes, bytes.Length))
			throw new Exception("FileWrite failed for " + cloudName + " (cloud quota or Steam Cloud disabled for this app?)");
		// Legacy publishing shares the cloud file (gives it a UGC handle).
		var share = Await<RemoteStorageFileShareResult_t>(SteamRemoteStorage.FileShare(cloudName));
		if (share.m_eResult != EResult.k_EResultOK)
			Console.Error.WriteLine($"  warning: FileShare failed for {cloudName}: {share.m_eResult}");
		else
			Console.WriteLine($"  shared as UGC handle {share.m_hFile}");
		return cloudName;
	}

	private static ERemoteStoragePublishedFileVisibility ParseVisibility(string vis)
	{
		return vis switch
		{
			"public" => ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityPublic,
			"friends" => ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityFriendsOnly,
			"unlisted" => ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityUnlisted,
			_ => ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityPrivate,
		};
	}

	private static void Check(bool ok, string what)
	{
		if (!ok)
			throw new Exception(what + " returned false");
	}

	private static Dictionary<string, string> ParseOptions(string[] args)
	{
		var opts = new Dictionary<string, string>();
		for (int i = 0; i < args.Length - 1; i++)
			if (args[i].StartsWith("--"))
				opts[args[i].Substring(2)] = args[++i];
		return opts;
	}

	/// <summary>Pumps Steam callbacks until the call result arrives.</summary>
	private static T Await<T>(SteamAPICall_t call, int timeoutSeconds = 120)
	{
		T result = default;
		bool done = false, failed = false;
		using var cr = CallResult<T>.Create((r, ioFailure) => { result = r; failed = ioFailure; done = true; });
		cr.Set(call);
		var deadline = DateTime.UtcNow.AddSeconds(timeoutSeconds);
		while (!done)
		{
			if (DateTime.UtcNow > deadline)
				throw new TimeoutException("Steam call timed out: " + typeof(T).Name);
			SteamAPI.RunCallbacks();
			Thread.Sleep(50);
		}
		if (failed)
			throw new Exception("Steam call I/O failure: " + typeof(T).Name);
		return result;
	}
}
