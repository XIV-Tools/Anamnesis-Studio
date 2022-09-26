// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Libraries.Panels;

using Anamnesis.Libraries.Items;
using Anamnesis.Panels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using XivToolsWpf;
using XivToolsWpf.Extensions;
using XivToolsWpf.Selectors;

public partial class LibraryPanel : PanelBase, IFilterable
{
	private EntryBase? selectedEntry;

	public LibraryPanel()
		: base()
	{
		this.Filter.Filterable = this;
	}

	public LibraryFilter Filter { get; init; } = new();
	public FastObservableCollection<EntryBase> Entries { get; init; } = new();
	public bool ViewList { get; set; } = false;

	public EntryBase? SelectedEntry
	{
		get => this.selectedEntry;
		set
		{
			this.selectedEntry = value;
			this.OnSelectionChanged(value);
		}
	}

	public async Task SetFilteredItems(IEnumerable items)
	{
		await this.Dispatcher.MainThread();
		this.Entries.Replace(items);
	}

	public Task<IEnumerable<object>> GetAllItems()
	{
		List<EntryBase> entries = new();

		foreach (Pack pack in this.Services.Library.Packs)
		{
			entries.Add(pack);
			this.GetEntries(pack, ref entries);
		}

		return Task.FromResult<IEnumerable<object>>(entries);
	}

	private void GetEntries(DirectoryEntry directory, ref List<EntryBase> results)
	{
		results.AddRange(directory.Entries);

		foreach (EntryBase entry in directory.Entries)
		{
			if (entry is DirectoryEntry subDirectory)
			{
				this.GetEntries(subDirectory, ref results);
			}
		}
	}

	private void OnBackClicked(object sender, RoutedEventArgs e)
	{
		this.SelectedEntry = null;
		this.Filter.CurrentDirectory = this.Filter.CurrentDirectory?.Parent;
	}

	private async void OnItemDoubleClicked(object sender, MouseButtonEventArgs e)
	{
		if (this.SelectedEntry?.CanOpen != true)
			return;

		if (this.SelectedEntry is DirectoryEntry directory)
		{
			this.Filter.CurrentDirectory = directory;
		}
		else if (this.SelectedEntry is ItemEntry item)
		{
			await item.Open();
		}
	}

	private void OnSelectionChanged(EntryBase? entry)
	{
	}

	private void OnTabChanged(object sender, RoutedEventArgs e)
	{
		this.Filter.CurrentDirectory = null;
		this.Filter.Favorites = false;
		this.Filter.Flatten = false;
	}

	private void OnFavoritesChecked(object sender, RoutedEventArgs e)
	{
		this.Filter.CurrentDirectory = null;
		this.Filter.Type = LibraryFilter.Types.All;
		this.Filter.Flatten = true;
	}
}