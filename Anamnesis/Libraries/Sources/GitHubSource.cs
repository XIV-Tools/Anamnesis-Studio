// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Libraries.Sources;

using Anamnesis.Files;
using Anamnesis.GameData.Excel;
using Anamnesis.Libraries.Items;
using Anamnesis.Serialization;
using Anamnesis.Services;
using FontAwesome.Sharp;
using Lumina.Excel.GeneratedSheets;
using Octokit;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;
using XivToolsWpf;
using XivToolsWpf.Extensions;
using static Anamnesis.Libraries.Sources.GitHubSource.RepositoryCache;

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

	protected DirectoryInfo GetDirectory(string? definitionDirectory)
	{
		if (definitionDirectory == null)
			definitionDirectory = "pack";

		return new(this.LocalDir + "/" + definitionDirectory.Replace('\\', '/').Trim('/') + "/");
	}

	protected override async Task Load(bool refresh)
	{
		RepositoryCache? cache = this.LoadCache();

		if (cache == null)
			cache = new RepositoryCache(this.RepositoryName);

		await cache.Load(this, refresh);
		this.SaveCache(cache);
	}

	protected async Task Update(GitHubPack pack)
	{
		RepositoryCache? cache = this.LoadCache();

		if (cache == null)
			return;

		if (!cache.PacksCache.TryGetValue(pack.Id, out var packCache))
			return;

		try
		{
			pack.IsUpdating = true;

			IReadOnlyList<RepositoryContent> definitionContents = await GitHubSource.GitHubClient.Repository.Content.GetAllContents(cache.RepositoryOwner, cache.RepositoryName, packCache.DefinitionFilePath);

			if (definitionContents.Count != 1)
				throw new Exception("Unable to get definition content from gitHub");

			RepositoryContent definitionContent = definitionContents[0];

			string jsonContent = await HttpClient.GetStringAsync(definitionContent.DownloadUrl);
			packCache.Definition = SerializerService.Deserialize<PackDefinitionFile>(jsonContent);

			if (packCache.DownloadState == PackCache.DownloadStates.Downloading)
				throw new Exception("GitHub pack is already downloading");

			string? directoryName = packCache.Definition?.Directory;
			DirectoryInfo targetDirectory = this.GetDirectory(directoryName);
			if (targetDirectory.Exists)
				targetDirectory.Delete(true);

			packCache.DownloadState = PackCache.DownloadStates.Downloading;

			// Since theres no way to actualy download a directory, we'll download the entire archive
			// and just extract the parts we need.
			// This isn't great, but the alternative is to get all the file contents then download them individually which
			// will just get us rate-limited real fast.
			byte[] archiveBytes = await GitHubClient.Repository.Content.GetArchive(cache.RepositoryOwner, cache.RepositoryName, ArchiveFormat.Zipball);
			////await File.WriteAllBytesAsync(this.LocalDir + "/archive.zip", archiveBytes);
			MemoryStream stream = new MemoryStream(archiveBytes);
			ZipArchive archive = new ZipArchive(stream, ZipArchiveMode.Read);
			foreach (ZipArchiveEntry entry in archive.Entries)
			{
				string entryPath = entry.FullName;

				// GitHub always has the first folder in the zip the name of the arhive plus its sha, so jsut ignore that.
				entryPath = entryPath.Substring(entryPath.IndexOf('/') + 1);

				if (entryPath.StartsWith(directoryName ?? string.Empty) && Path.HasExtension(entryPath))
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

			packCache.DownloadState = PackCache.DownloadStates.Downloaded;

			if (packCache.Definition == null)
				throw new Exception("Pack cache has no definition");

			pack.IsUpdating = false;
			pack.IsUpdateAvailable = false;
			packCache.HasUpdate = false;

			this.SaveCache(cache);

			await this.Refresh();
		}
		catch (Exception ex)
		{
			this.Log.Error(ex, "Error downloading pack contents");
			packCache.DownloadState = PackCache.DownloadStates.ErrorDuringDownload;
		}

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

	public class GitHubPack : Pack
	{
		public EntryAction RefreshAction;
		public EntryAction UpdateAction;

		public GitHubPack(string url, GitHubSource source)
			: base(url, source)
		{
			this.RefreshAction = new("Refresh", IconChar.Refresh, this.OnRefresh);
			this.UpdateAction = new("Update", IconChar.Download, this.OnUpdate);

			this.Actions.Add(this.RefreshAction);
			this.Actions.Add(this.UpdateAction);

			this.UpdateAction.CanExecute = this.IsUpdateAvailable;
		}

		public new GitHubSource? Source => base.Source as GitHubSource;

		private async Task OnRefresh()
		{
			if (this.Source == null)
				return;

			this.RefreshAction.CanExecute = false;
			await this.Source.Refresh();
			this.RefreshAction.CanExecute = true;
		}

		private async Task OnUpdate()
		{
			if (this.Source == null)
				return;

			this.UpdateAction.CanExecute = false;
			await this.Source.Update(this);
			this.UpdateAction.CanExecute = this.IsUpdateAvailable;
		}
	}

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

		public async Task Load(GitHubSource source, bool refresh)
		{
			// Get the latest pack definitions if we need them
			TimeSpan timeSinceLastCheck = DateTimeOffset.UtcNow - this.LastChecked;
			if (refresh || this.PacksCache.Count <= 0 || timeSinceLastCheck > GitHubSource.CheckFrequency)
			{
				await this.CheckForUpdates();
				this.LastChecked = DateTimeOffset.UtcNow;
			}

			foreach ((string url, PackCache cache) in this.PacksCache)
			{
				GitHubPack pack = new(url, source);
				pack.Name = cache.Definition?.Name ?? cache.DefinitionFileName;
				pack.Author = cache.Definition?.Author ?? this.RepositoryOwner;
				pack.Description = cache.Definition?.Description;
				pack.Thumbnail = cache.Definition?.GetImage();
				pack.IsUpdateAvailable = cache.HasUpdate || cache.HasDefinitionUpdate;

				List<string> tags = new();

				DirectoryInfo dir = source.GetDirectory(cache.Definition?.Directory);
				if (dir.Exists)
					source.Populate(pack, dir, tags);

				await source.AddPack(pack);

				if (cache.HasDefinitionUpdate || cache.HasUpdate)
				{
					// TODO: notify!
				}
			}
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
						this.PacksCache.Add(definitionUrl, cache);
					}
					else
					{
						cache = this.PacksCache[definitionUrl];
					}

					await cache.CheckForUpdates(repositoryContent, content);
				}
			}
		}

		public class PackCache
		{
			public enum DownloadStates
			{
				None,
				Downloading,
				Downloaded,
				ErrorDuringDownload,
			}

			public string? DefinitionFilePath { get; set; }
			public PackDefinitionFile? Definition { get; set; }
			public string? LastDefinitionSha { get; set; }
			public string? LastDirectorySha { get; set; }
			public string? DefinitionFileName { get; set; }

			public bool HasDefinitionUpdate { get; set; }
			public bool HasUpdate { get; set; }
			public DownloadStates DownloadState { get; set; }

			protected ILogger Log => Serilog.Log.ForContext<PackCache>();

			public Task CheckForUpdates(RepositoryContent definitionContent, IReadOnlyList<RepositoryContent> allContent)
			{
				this.DefinitionFilePath = definitionContent.Path;
				this.DefinitionFileName = definitionContent.Name;

				this.HasDefinitionUpdate = false;
				this.HasUpdate = false;

				// Update the definition file if we don't have it, or if its changed
				if (this.Definition == null || this.LastDefinitionSha != definitionContent.Sha)
				{
					this.LastDefinitionSha = definitionContent.Sha;
					this.HasDefinitionUpdate = true;
				}

				if (this.Definition == null)
				{
					this.HasUpdate = true;
					return Task.CompletedTask;
				}

				RepositoryContent directoryContent = this.GetDirectoryContent(this.Definition.Directory, allContent);
				if (this.LastDirectorySha != directoryContent.Sha)
				{
					this.LastDirectorySha = directoryContent.Sha;
					this.HasUpdate = true;
				}

				return Task.CompletedTask;
			}

			public RepositoryContent GetDirectoryContent(string? directory, IReadOnlyList<RepositoryContent> allContent)
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
