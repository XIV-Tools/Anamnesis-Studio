// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Libraries;

using Anamnesis.Files;
using Anamnesis.Libraries.Items;
using Anamnesis.Libraries.Sources;
using Anamnesis.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using XivToolsWpf;
using XivToolsWpf.Extensions;

public class LibraryService : ServiceBase<LibraryService>
{
	private readonly List<LibrarySourceBase> sources = new();
	private readonly List<Pack> packs = new();

	public ObservableCollection<Pack> Packs { get; init; } = new();
	public bool IsLoading { get; private set; }
	public LibraryFilter Filter { get; init; } = new();

	public override Task Start()
	{
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

			await Dispatch.MainThread();

			this.FilterItems();
		}
		catch (Exception ex)
		{
			Log.Error(ex, $"Error loading library");
		}

		this.IsLoading = false;
	}

	public void FilterItems()
	{
		IOrderedEnumerable<Pack> sortedPacks = this.packs.OrderBy(item => item, this.Filter);

		this.Packs.Clear();

		foreach (Pack pack in sortedPacks)
		{
			pack.Filter(this.Filter);
			this.Packs.Add(pack);
		}
	}

	public void AddSource(LibrarySourceBase source)
	{
		this.sources.Add(source);
	}

	public void AddPack(Pack pack)
	{
		if (pack.Name == null)
			return;

		this.packs.Add(pack);
	}
}
