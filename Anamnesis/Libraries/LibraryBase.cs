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

	public string NameKey { get; set; } = string.Empty;
	public ObservableCollection<GroupItem> AllPacks { get; init; } = new();
	public ObservableCollection<GroupItem> FilteredPacks { get; init; } = new();
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
		IOrderedEnumerable<GroupItem> sortedPacks = this.AllPacks.OrderBy(item => item, this.Filter);

		this.FilteredPacks.Clear();

		foreach (GroupItem obj in sortedPacks)
		{
			obj.Filter(this.Filter);
			this.FilteredPacks.Add(obj);
		}
	}

	protected void AddSource(LibrarySourceBase source)
	{
		this.sources.Add(source);
	}

	private async Task LoadSource(LibrarySourceBase source)
	{
		await Dispatch.NonUiThread();
		List<PackItem> packs = await source.LoadPacks();

		await Dispatch.MainThread();
		foreach (PackItem pack in packs)
		{
			this.AllPacks.Add(pack);
		}
	}
}