// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Libraries;

using Anamnesis.Libraries.Items;
using Anamnesis.Libraries.Sources;
using Anamnesis.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using XivToolsWpf;
using XivToolsWpf.Extensions;

public class LibraryService : ServiceBase<LibraryService>
{
	private readonly List<LibrarySourceBase> sources = new();

	public ObservableCollection<Pack> Packs { get; init; } = new();
	public bool IsLoading { get; private set; }

	public override Task Start()
	{
		this.AddSource(new GameDataSource());
		this.AddSource(new FileSource(SettingsService.Current.LocalPacksDirectory));
		this.AddSource(new GitHubSource("XIV-Tools/AnamnesisStandardPacks"));

		this.LoadSources().Run();

		return base.Start();
	}

	public async Task LoadSources()
	{
		this.IsLoading = true;

		try
		{
			List<Task> sourceTasks = new();
			foreach (LibrarySourceBase source in this.sources)
			{
				sourceTasks.Add(source.LoadPacks());
			}

			await Task.WhenAll(sourceTasks);
		}
		catch (Exception ex)
		{
			Log.Error(ex, $"Error loading library");
		}

		this.IsLoading = false;
	}

	public void AddSource(LibrarySourceBase source)
	{
		this.sources.Add(source);
	}

	public async Task AddPack(Pack pack)
	{
		if (pack.Name == null)
			return;

		await Dispatch.MainThread();
		this.Packs.Add(pack);
	}
}
