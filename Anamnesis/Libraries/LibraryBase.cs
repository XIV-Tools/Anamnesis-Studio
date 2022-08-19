// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Libraries;

using Anamnesis.Libraries.Items;
using Anamnesis.Libraries.Sources;
using PropertyChanged;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using XivToolsWpf;

[AddINotifyPropertyChangedInterface]
public abstract class LibraryBase
{
	private readonly List<LibrarySourceBase> sources = new();
	private readonly List<Pack> packs = new();

	public string NameKey { get; set; } = string.Empty;
	public ObservableCollection<Pack> Packs { get; init; } = new();
	public bool IsLoading { get; private set; }
	public LibraryFilter Filter { get; init; } = new();

	protected ILogger Log => Serilog.Log.ForContext(this.GetType());

	public abstract void Initialize();

	public async Task LoadLibrary()
	{
		this.IsLoading = true;

		this.Initialize();

		try
		{
			List<Task> sourceTasks = new();
			foreach(LibrarySourceBase source in this.sources)
			{
				sourceTasks.Add(this.LoadSource(source));
			}

			await Task.WhenAll(sourceTasks);

			await Dispatch.MainThread();

			this.DoFilter();
		}
		catch (Exception ex)
		{
			this.Log.Error(ex, $"Error loading library: {this.NameKey}");
		}

		this.IsLoading = false;
	}

	public void DoFilter()
	{
		IOrderedEnumerable<Group> sortedPacks = this.packs.OrderBy(item => item, this.Filter);

		this.Packs.Clear();

		foreach (Pack pack in sortedPacks)
		{
			pack.Filter(this.Filter);
			this.Packs.Add(pack);
		}
	}

	protected void AddSource(LibrarySourceBase source)
	{
		this.sources.Add(source);
	}

	private async Task LoadSource(LibrarySourceBase source)
	{
		await Dispatch.NonUiThread();
		List<Pack> packs = await source.LoadPacks();

		await Dispatch.MainThread();
		foreach (Pack pack in packs)
		{
			this.packs.Add(pack);
		}
	}
}