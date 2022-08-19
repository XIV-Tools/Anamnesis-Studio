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
using System.Net.Http;
using System.Threading.Tasks;

internal class GitHubSource : LibrarySourceBase
{
	// TODO allow github username here from settings:
	private static readonly GitHubClient GitHubClient = new(new ProductHeaderValue("XIV-Tools"));
	private static readonly HttpClient HttpClient = new();

	public GitHubSource(string repositoryName)
	{
		this.RepositoryName = repositoryName;
	}

	public override IconChar Icon => IconChar.Github;
	public override string Name => LocalizationService.GetStringFormatted("Library_GitHUBSource", this.RepositoryName);

	public string RepositoryName { get; set; }
	public GitHubCache? Cache { get; private set; }

	public string LocalDir => FileService.StoreDirectory + "/GitHUB/" + this.RepositoryName.Replace("/", "_");

	protected override async Task Load(bool force)
	{
		string[] parts = this.RepositoryName.Split('/');

		if (parts.Length != 2)
			throw new Exception("gitHUB pack repository name must be in the format \"Owner/Name\", such as \"XIV-Tools/AnamnesisStandardPacks\"");

		this.Cache = this.GetCache();
		if (this.Cache == null)
		{
			force = true;
			this.Cache = new GitHubCache();
		}

		TimeSpan timeSinceLastCheck = DateTimeOffset.UtcNow - this.Cache.LastChecked;

		// Only check the repo once every 6 hours
		if (!force && timeSinceLastCheck < TimeSpan.FromHours(6))
		{
			// Load from the cache
			foreach (PackDefinitionFile definition in this.Cache.Packs)
			{
				this.AddPack(new Pack(definition, this));
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
					await this.AddPack(contentItem);
					packCount++;
				}
			}

			if (packCount <= 0)
			{
				this.Log.Error($"No packs found in repository: \"{this.RepositoryName}\". Ensure there is a *.packdef file at the root of the repository.");
			}

			this.SaveCache(this.Cache);
		}
	}

	private async Task AddPack(RepositoryContent content)
	{
		try
		{
			string jsonContent = await HttpClient.GetStringAsync(content.DownloadUrl);
			PackDefinitionFile definition = SerializerService.Deserialize<PackDefinitionFile>(jsonContent);
			this.AddPack(new Pack(definition, this));

			this.Cache?.Packs.Add(definition);
		}
		catch (Exception ex)
		{
			this.Log.Error(ex, "Failed to load pack definition");
		}
	}

	private GitHubCache? GetCache()
	{
		string cacheFile = this.LocalDir + "/packs.json";

		if (!File.Exists(cacheFile))
			return null;

		string json = File.ReadAllText(cacheFile);
		return SerializerService.Deserialize<GitHubCache>(json);
	}

	private void SaveCache(GitHubCache cache)
	{
		string cacheFile = this.LocalDir + "/packs.json";

		if (!Directory.Exists(this.LocalDir))
			Directory.CreateDirectory(this.LocalDir);

		string json = SerializerService.Serialize(cache);
		File.WriteAllText(cacheFile, json);
	}

	public class GitHubCache
	{
		public DateTimeOffset LastChecked { get; set; }
		public List<PackDefinitionFile> Packs { get; set; } = new();
	}
}
