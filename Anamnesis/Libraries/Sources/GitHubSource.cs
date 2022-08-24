// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Libraries.Sources;

using Anamnesis.Files;
using Anamnesis.Libraries.Items;
using Anamnesis.Serialization;
using Anamnesis.Services;
using FontAwesome.Sharp;
using Octokit;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;
using XivToolsWpf;
using XivToolsWpf.Extensions;
using static Anamnesis.Libraries.Sources.GitHubSource.GitHubCache;
using static Anamnesis.Panels.ImportPosePanel;

internal class GitHubSource : FileSource
{
	// TODO allow github username here from settings:
	private static readonly GitHubClient GitHubClient = new(new ProductHeaderValue("XIV-Tools"));
	private static readonly HttpClient HttpClient = new();

	public GitHubSource(string repositoryName)
	{
		this.RepositoryName = repositoryName;
	}

	public override IconChar Icon => IconChar.Github;
	public override string Name => this.RepositoryName; //// LocalizationService.GetStringFormatted("Library_GitHubSource", this.RepositoryName);

	public string RepositoryName { get; set; }
	public GitHubCache? Cache { get; private set; }

	public string LocalDir => FileService.ParseToFilePath(FileService.StoreDirectory + "/GitHub/" + this.RepositoryName.Replace("/", "_"));

	protected override async Task Load()
	{
		string[] parts = this.RepositoryName.Split('/');

		if (parts.Length != 2)
			throw new Exception("gitHUB pack repository name must be in the format \"Owner/Name\", such as \"XIV-Tools/AnamnesisStandardPacks\"");

		this.LoadCache();

		if (this.Cache == null)
			throw new InvalidOperationException();

		TimeSpan timeSinceLastCheck = DateTimeOffset.UtcNow - this.Cache.LastChecked;

		// Only check the repo once every 6 hours
		if (timeSinceLastCheck < TimeSpan.FromHours(6))
		{
			// Load from the cache
			foreach ((string defUrl, GitHubCache.PackCache packCache) in this.Cache.PacksCache)
			{
				if (packCache.Definition == null)
					throw new Exception("No pack definition in cache");

				Pack pack = new(defUrl, packCache.Definition, this);
				pack.IsUpdateAvailable = packCache.HasUpdate;
				await this.AddPack(pack);

				DirectoryInfo? packDir = this.GetPackDirectory(packCache.Definition, new(this.LocalDir));
				if (packDir == null || !packDir.Exists)
				{
					pack.IsUpdateAvailable = true;
				}
				else
				{
					await this.GetFiles(pack, packDir);
				}
			}
		}
		else
		{
			// Check live repository
			this.Cache.LastChecked = DateTimeOffset.UtcNow;

			int packCount = 0;
			IReadOnlyList<RepositoryContent> content = await GitHubClient.Repository.Content.GetAllContents(parts[0], parts[1]);
			foreach (RepositoryContent contentItem in content)
			{
				if (contentItem.Name.EndsWith(".packdef"))
				{
					string jsonContent = await HttpClient.GetStringAsync(contentItem.DownloadUrl);
					PackDefinitionFile definition = SerializerService.Deserialize<PackDefinitionFile>(jsonContent);
					Pack pack = new Pack(contentItem.Url, definition, this);
					await this.AddPack(pack);

					await this.PopulatePack(pack, definition, contentItem, content);

					DirectoryInfo? packDir = this.GetPackDirectory(definition, new(this.LocalDir));
					await this.GetFiles(pack, packDir);
					packCount++;
				}
			}

			if (packCount <= 0)
			{
				this.Log.Error($"No packs found in repository: \"{this.RepositoryName}\". Ensure there is a *.packdef file at the root of the repository.");
			}

			this.SaveCache();
		}
	}

	protected override async Task Load(Pack? pack)
	{
		if (pack == null)
			return;

		string[] parts = this.RepositoryName.Split('/');

		if (parts.Length != 2)
			throw new Exception("gitHUB pack repository name must be in the format \"Owner/Name\", such as \"XIV-Tools/AnamnesisStandardPacks\"");

		this.LoadCache();

		if (this.Cache == null)
			throw new InvalidOperationException();

		this.Cache.LastChecked = DateTimeOffset.UtcNow;

		IReadOnlyList<RepositoryContent> content = await GitHubClient.Repository.Content.GetAllContents(parts[0], parts[1]);
		foreach (RepositoryContent contentItem in content)
		{
			if (contentItem.Name.EndsWith(".packdef"))
			{
				string jsonContent = await HttpClient.GetStringAsync(contentItem.DownloadUrl);
				PackDefinitionFile definition = SerializerService.Deserialize<PackDefinitionFile>(jsonContent);

				await this.PopulatePack(pack, definition, contentItem, content);
			}
		}

		this.SaveCache();
	}

	protected override async Task Update(Pack pack)
	{
		if (this.Cache == null)
			this.LoadCache();

		if (this.Cache == null)
			throw new InvalidOperationException();

		if (!this.Cache.PacksCache.ContainsKey(pack.Id))
			throw new Exception("Pack is not part of this github source");

		GitHubCache.PackCache cache = this.Cache.PacksCache[pack.Id];
		await this.DownloadContents(pack, cache);
	}

	private async Task PopulatePack(Pack pack, PackDefinitionFile definition, RepositoryContent defFileContent, IReadOnlyList<RepositoryContent> allContent)
	{
		try
		{
			if (this.Cache == null)
				return;

			await Dispatch.NonUiThread();

			GitHubCache.PackCache? packCache;
			if (!this.Cache.PacksCache.TryGetValue(defFileContent.Url, out packCache))
			{
				packCache = new(definition);
				this.Cache.PacksCache.Add(defFileContent.Url, packCache);
			}

			if (packCache.LastDefinitionSha != defFileContent.Sha)
			{
				// This pack def has updated!
				packCache.Definition = definition;
				packCache.LastDefinitionSha = defFileContent.Sha;
				packCache.HasUpdate = true;
				pack.IsUpdateAvailable = true;
			}

			string directory = defFileContent.Path;
			if (definition.Directory == null)
			{
				// TODO: the root repository sha?
				throw new NotImplementedException();
			}
			else
			{
				string dir = definition.Directory.Replace('\\', '/').Trim('/');

				RepositoryContent? contentDirectory = null;
				foreach (RepositoryContent content in allContent)
				{
					if (content.Path == dir)
					{
						contentDirectory = content;
						break;
					}
				}

				if (contentDirectory == null)
					throw new Exception($"Could not find content directory: {dir} for pack: {definition.Name} in repo: {this.RepositoryName}");

				if (packCache.LastDirectorySha != contentDirectory.Sha || packCache.DownloadState != GitHubCache.PackCache.DownloadStates.Downloaded)
				{
					// The contents of the pack have changed!
					packCache.LastDirectorySha = contentDirectory.Sha;
					packCache.DownloadUrl = contentDirectory.DownloadUrl;
					packCache.DownloadState = GitHubCache.PackCache.DownloadStates.NotDownloaded;
					packCache.HasUpdate = true;
					pack.IsUpdateAvailable = true;
				}
			}
		}
		catch (Exception ex)
		{
			this.Log.Error(ex, "Failed to load pack definition");
		}
	}

	private async Task DownloadContents(Pack pack, GitHubCache.PackCache cache)
	{
		try
		{
			pack.IsUpdating = true;

			if (cache.DownloadState == GitHubCache.PackCache.DownloadStates.Downloading)
				throw new Exception("GitHub pack is already downloading");

			string? directoryName = cache.Definition?.Directory?.Replace('\\', '/').Trim('/') + "/";
			if (directoryName == null)
				throw new Exception("pack cache has no directory definition");

			string targetDirectory = this.LocalDir + "/" + directoryName;
			if (Directory.Exists(targetDirectory))
				Directory.Delete(targetDirectory, true);

			this.Log.Information($"Downloading pack: {cache.DownloadUrl}");
			cache.DownloadState = GitHubCache.PackCache.DownloadStates.Downloading;

			// Since theres no way to actualy download a directory, we'll download the entire archive
			// and just extract the parts we need.
			// This isn't great, but the alternative is to get all the file contents then download them individually which
			// will just get us rate-limited real fast.
			string[] parts = this.RepositoryName.Split('/');
			byte[] archiveBytes = await GitHubClient.Repository.Content.GetArchive(parts[0], parts[1], ArchiveFormat.Zipball);
			////await File.WriteAllBytesAsync(this.LocalDir + "/archive.zip", archiveBytes);
			MemoryStream stream = new MemoryStream(archiveBytes);
			ZipArchive archive = new ZipArchive(stream, ZipArchiveMode.Read);
			foreach (ZipArchiveEntry entry in archive.Entries)
			{
				string entryPath = entry.FullName;

				// GitHub always has the first folder in the zip the name of the arhive plus its sha, so jsut ignore that.
				entryPath = entryPath.Substring(entryPath.IndexOf('/') + 1);

				if (entryPath.StartsWith(directoryName) && Path.HasExtension(entryPath))
				{
					// Only extract files we can actually use.
					string extension = Path.GetExtension(entryPath);
					if (!SupportedFileExtensions.Contains(extension))
						continue;

					string destination = entryPath;
					destination = this.LocalDir + "/" + destination;

					string? destinationDirectory = Path.GetDirectoryName(destination);

					if (destinationDirectory == null)
						throw new Exception($"Failed to get directory from path: {destination}");

					if (!Directory.Exists(destinationDirectory))
						Directory.CreateDirectory(destinationDirectory);

					entry.ExtractToFile(destination);
				}
			}

			cache.DownloadState = GitHubCache.PackCache.DownloadStates.Downloaded;
			this.SaveCache();

			if (cache.Definition == null)
				throw new Exception("Pack cache has no definition");

			DirectoryInfo packDir = this.GetPackDirectory(cache.Definition, new(this.LocalDir));
			if (!packDir.Exists)
				throw new Exception($"Failed to get pack directory: {packDir.FullName}");

			await this.GetFiles(pack, packDir);

			pack.IsUpdating = false;
			pack.IsUpdateAvailable = false;
			cache.HasUpdate = false;

			this.SaveCache();
		}
		catch (Exception ex)
		{
			this.Log.Error(ex, "Error downloading pack contents");
			cache.DownloadState = GitHubCache.PackCache.DownloadStates.ErrorDuringDownload;
		}
	}

	private void LoadCache()
	{
		string cacheFile = this.LocalDir + "/packs.json";

		if (!File.Exists(cacheFile))
		{
			this.Cache = new();
			this.Cache.LastChecked = DateTimeOffset.MinValue;
			this.SaveCache();
			return;
		}

		string json = File.ReadAllText(cacheFile);
		this.Cache = SerializerService.Deserialize<GitHubCache>(json);
	}

	private void SaveCache()
	{
		if (this.Cache == null)
			return;

		string cacheFile = this.LocalDir + "/packs.json";

		if (!Directory.Exists(this.LocalDir))
			Directory.CreateDirectory(this.LocalDir);

		string json = SerializerService.Serialize(this.Cache);
		File.WriteAllText(cacheFile, json);
	}

	public class GitHubCache
	{
		public DateTimeOffset LastChecked { get; set; }
		public Dictionary<string, PackCache> PacksCache { get; set; } = new();

		public class PackCache
		{
			public PackCache()
			{
			}

			public PackCache(PackDefinitionFile? definition)
			{
				this.Definition = definition;
			}

			public enum DownloadStates
			{
				NotDownloaded,
				Downloading,
				Downloaded,
				ErrorDuringDownload,
			}

			public PackDefinitionFile? Definition { get; set; }
			public string? LastDefinitionSha { get; set; }
			public string? LastDirectorySha { get; set; }
			public string? DownloadUrl { get; set; }
			public DownloadStates DownloadState { get; set; } = DownloadStates.NotDownloaded;
			public bool HasUpdate { get; set; } = false;
		}
	}
}
