// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Libraries.Panels;

using Anamnesis.Files;
using Anamnesis.Libraries.Items;
using Anamnesis.Panels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using XivToolsWpf;
using XivToolsWpf.Extensions;

public partial class LibraryPanel : PanelBase
{
	private const int FilterSearchInputDelay = 250;

	private Task? filterTask = null;
	private string[]? searchQuery;
	private string searchText = string.Empty;
	private int filterDelay = 500;
	private EntryBase? selectedEntry;

	public LibraryFilter Filter { get; init; } = new();
	public FastObservableCollection<EntryBase> Entries { get; init; } = new();
	public DirectoryEntry? CurrentDirectory { get; set; }
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

	public string Search
	{
		get => this.searchText;
		set
		{
			this.searchText = value;

			if (string.IsNullOrEmpty(value))
			{
				this.searchQuery = null;
			}
			else
			{
				this.searchQuery = value.ToLower().Split(' ');
			}

			this.FilterItems(true);
		}
	}

	private void FilterItems(bool delay = false)
	{
		this.filterDelay = delay ? FilterSearchInputDelay : 0;

		if (this.filterTask != null && !this.filterTask.IsCompleted && this.Filter.IsFiltering)
		{
			this.Filter.Restart();
			return;
		}

		this.filterTask = Task.Run(async () =>
		{
			await this.Dispatcher.MainThread();
			this.Filter.IsFiltering = true;

			while (this.filterDelay > 0)
			{
				await Task.Delay(10);
				this.filterDelay -= 10;
			}

			this.Filter.ResetCancelation();
			await this.FilterItemsAsync();
		});
	}

	private async Task FilterItemsAsync()
	{
		try
		{
			this.Filter.IsFiltering = true;
			await this.FilterItemsInternalAsync();
			this.Filter.IsFiltering = false;
		}
		catch (Exception ex)
		{
			this.Log.Error(ex, "Error filtering library");
		}

		if (this.Filter.RestartRequeted)
		{
			this.Filter.ResetCancelation();
			this.FilterItems();
		}

		this.Filter.ResetCancelation();
	}

	private async Task FilterItemsInternalAsync()
	{
		if (this.Filter.CancelRequested)
			return;

		HashSet<string> availableTags = new();

		await Dispatch.NonUiThread();
		if (this.Filter.CancelRequested)
			return;

		IEnumerable<EntryBase> filteredItems;
		if (this.CurrentDirectory == null)
		{
			filteredItems = await this.Filter.GetItems(LibraryService.Instance.Packs, this.searchQuery);
		}
		else
		{
			filteredItems = await this.Filter.GetItems(this.CurrentDirectory, this.searchQuery);
		}

		if (this.Filter.CancelRequested)
			return;

		await this.Dispatcher.MainThread();
		this.Entries.Replace(filteredItems);
	}

	private void OnBackClicked(object sender, RoutedEventArgs e)
	{
		this.SelectedEntry = null;
		this.CurrentDirectory = this.CurrentDirectory?.Parent;
		this.FilterItems();
	}

	private async void OnItemDoubleClicked(object sender, MouseButtonEventArgs e)
	{
		if (this.SelectedEntry?.CanOpen != true)
			return;

		if (this.SelectedEntry is DirectoryEntry directory)
		{
			this.CurrentDirectory = directory;
			this.FilterItems();
		}
		else if (this.SelectedEntry is ItemEntry item)
		{
			await item.Open();
		}
	}

	private void OnClearSearchClicked(object sender, RoutedEventArgs e)
	{
		this.Search = string.Empty;
		this.SearchBox.Focus();
	}

	private void OnSelectionChanged(EntryBase? entry)
	{
	}

	private void OnTabChanged(object sender, RoutedEventArgs e)
	{
		this.CurrentDirectory = null;
		this.Filter.Favorites = false;
		this.Filter.Flatten = false;
		this.FilterItems();
	}

	private void OnFavoritesChecked(object sender, RoutedEventArgs e)
	{
		this.CurrentDirectory = null;
		this.Filter.Type = LibraryFilter.Types.All;
		this.Filter.Flatten = true;
		this.FilterItems();
	}

	private void OnFlattenChanged(object sender, RoutedEventArgs e)
	{
		this.FilterItems();
	}
}
