// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Libraries.Panels;

using Anamnesis.Libraries.Items;
using Anamnesis.Panels;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using XivToolsWpf;
using XivToolsWpf.Extensions;
using XivToolsWpf.Selectors;

public partial class LibraryPanel : PanelBase, IFilterable
{
	private readonly DirectoryEntry rootDir = new();
	private EntryBase? selectedEntry;

	public LibraryPanel()
		: base()
	{
		this.rootDir.Name = "[Library_Root]";
		this.Filter.Filterable = this;
		this.Filter.CurrentDirectory = this.rootDir;
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

	public List<DirectoryEntry> CurrentPath
	{
		get
		{
			List<DirectoryEntry> path = new();
			DirectoryEntry? parent = this.Filter.CurrentDirectory;
			while (parent != null)
			{
				path.Add(parent);
				parent = parent.Parent;
			}

			path.Reverse();

			return path;
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

		// Update the root dir contents just in case.
		this.rootDir.ClearItems();
		foreach (Pack pack in this.Services.Library.Packs)
		{
			this.rootDir.AddEntry(pack);
		}

		this.GetEntries(this.rootDir, ref entries);

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

		this.RaisePropertyChanged(nameof(LibraryPanel.CurrentPath));
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

		this.RaisePropertyChanged(nameof(LibraryPanel.CurrentPath));
	}

	private void OnSelectionChanged(EntryBase? entry)
	{
	}

	private void OnTabChanged(object sender, RoutedEventArgs e)
	{
		this.Filter.CurrentDirectory = this.rootDir;
		this.Filter.Favorites = false;
		this.Filter.Flatten = false;

		this.RaisePropertyChanged(nameof(LibraryPanel.CurrentPath));
	}

	private void OnFavoritesChecked(object sender, RoutedEventArgs e)
	{
		this.Filter.CurrentDirectory = this.rootDir;
		this.Filter.Type = LibraryFilter.Types.All;
		this.Filter.Flatten = true;

		this.RaisePropertyChanged(nameof(LibraryPanel.CurrentPath));
	}

	private void OnDirectorySelected(object sender, RoutedEventArgs e)
	{
		if(sender is Button btn && btn.DataContext is DirectoryEntry directory)
		{
			this.Filter.CurrentDirectory = directory;
			this.RaisePropertyChanged(nameof(LibraryPanel.CurrentPath));
		}
	}
}