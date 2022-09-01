// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Libraries.Sources;

using Anamnesis.Files;
using Anamnesis.Libraries.Items;
using Anamnesis.Serialization;
using Anamnesis.Services;
using FontAwesome.Sharp;
using Lumina.Excel.GeneratedSheets;
using Octokit;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;
using XivToolsWpf;
using XivToolsWpf.Extensions;

internal class GitHubSource : FileSource
{
	private static readonly TimeSpan CheckFrequency = TimeSpan.FromHours(1);

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

	private string LocalDir => FileService.ParseToFilePath(FileService.StoreDirectory + "/GitHub/" + this.RepositoryName.Replace("/", "_"));
	private string PacksCachePath => this.LocalDir + "/packsCache.json";

	public override async Task Load()
	{
		RepositoryCache? cache = this.LoadCache();

		if (cache == null)
			cache = new RepositoryCache(this.RepositoryName);

		await cache.Load(this);
		this.SaveCache(cache);
	}

	private RepositoryCache? LoadCache()
	{
		if (!File.Exists(this.PacksCachePath))
			return null;

		string json = File.ReadAllText(this.PacksCachePath);
		return SerializerService.Deserialize<RepositoryCache>(json);
	}

	private void SaveCache(RepositoryCache cache)
	{
		if (!Directory.Exists(this.LocalDir))
			Directory.CreateDirectory(this.LocalDir);

		string json = SerializerService.Serialize(cache);
		File.WriteAllText(this.PacksCachePath, json);
	}

	/*
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
	}*/

	public class RepositoryCache
	{
		public RepositoryCache()
		{
		}

		public RepositoryCache(string repository)
		{
			string[] parts = repository.Split('/');

			if (parts.Length != 2)
				throw new Exception("gitHUB pack repository name must be in the format \"Owner/Name\", such as \"XIV-Tools/AnamnesisStandardPacks\"");

			this.RepositoryOwner = parts[0];
			this.RepositoryName = parts[1];
		}

		public string RepositoryOwner { get; set; } = string.Empty;
		public string RepositoryName { get; set; } = string.Empty;
		public DateTimeOffset LastChecked { get; set; }
		public Dictionary<string, PackCache> PacksCache { get; set; } = new();

		public async Task Load(GitHubSource source)
		{
			// Get the latest pack definitions if we need them
			TimeSpan timeSinceLastCheck = DateTimeOffset.UtcNow - this.LastChecked;
			if (this.PacksCache.Count <= 0 || timeSinceLastCheck > GitHubSource.CheckFrequency)
			{
				await this.CheckForUpdates();
			}

			foreach ((string url, PackCache cache) in this.PacksCache)
			{
				Pack pack = new(url, source);
				pack.Name = cache.Definition?.Name ?? cache.DefinitionFileName;
				pack.Author = cache.Definition?.Author ?? this.RepositoryOwner;
				pack.Description = cache.Definition?.Description;
				pack.Thumbnail = cache.Definition?.GetImage();
				pack.IsUpdateAvailable = cache.HasUpdate;

				await source.AddPack(pack);
			}

			// TODO: download the update? idk.
		}

		private async Task CheckForUpdates()
		{
			IReadOnlyList<RepositoryContent> content = await GitHubSource.GitHubClient.Repository.Content.GetAllContents(this.RepositoryOwner, this.RepositoryName);

			foreach (RepositoryContent repositoryContent in content)
			{
				if (repositoryContent.Name.EndsWith(".packdef"))
				{
					string definitionUrl = repositoryContent.DownloadUrl;

					PackCache cache;
					if (!this.PacksCache.ContainsKey(definitionUrl))
					{
						cache = new();
						cache.DefinitionFileName = repositoryContent.Name;
						this.PacksCache.Add(definitionUrl, cache);
					}
					else
					{
						cache = this.PacksCache[definitionUrl];
					}

					cache.HasUpdate = cache.CheckForUpdates(repositoryContent, content);
				}
			}
		}

		public class PackCache
		{
			public PackDefinitionFile? Definition { get; set; }
			public string? LastDefinitionSha { get; set; }
			public string? LastDirectorySha { get; set; }
			public string? DefinitionFileName { get; set; }

			public bool HasUpdate { get; set; }

			public bool CheckForUpdates(RepositoryContent definitionContent, IReadOnlyList<RepositoryContent> allContent)
			{
				if (this.Definition == null)
					return true;

				// Update the definition file if we don't have it, or if its changed
				if (this.LastDefinitionSha != definitionContent.Sha)
					return true;

				RepositoryContent directoryContent = this.GetDirectoryContent(this.Definition.Directory, allContent);
				if (this.LastDirectorySha != directoryContent.Sha)
					return true;

				return false;
			}

			/*string jsonContent = await HttpClient.GetStringAsync(definitionContent.DownloadUrl);
			this.Definition = SerializerService.Deserialize<PackDefinitionFile>(jsonContent);
			this.LastDefinitionSha = definitionContent.Sha;*/

			private RepositoryContent GetDirectoryContent(string? directory, IReadOnlyList<RepositoryContent> allContent)
			{
				string? dir = directory?.Replace('\\', '/').Trim('/');

				foreach (RepositoryContent content in allContent)
				{
					if (content.Path == dir)
					{
						return content;
					}
				}

				throw new Exception($"Could not find GitHub pack content directory: {directory}");
			}
		}
	}
}
