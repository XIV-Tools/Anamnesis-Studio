// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Libraries.Panels;

using Anamnesis.Files;
using Anamnesis.Libraries.Items;
using Anamnesis.Panels;
using PropertyChanged;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using XivToolsWpf;
using XivToolsWpf.Extensions;
using XivToolsWpf.Selectors;

using static Anamnesis.Libraries.Sources.FileSource;

public partial class LibraryPanel : PanelBase, IFilterable
{
	private readonly DirectoryEntry rootDir = new();
	private EntryBase? selectedEntry;
	private bool selectionChanging = false;
	private bool livePreview;

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
	public FileImporterBase? Importer { get; private set; }

	public bool LivePreview
	{
		get => this.livePreview;
		set
		{
			this.livePreview = value;
			this.OnSelectionChanged(this.selectedEntry, true).Run();
		}
	}

	public EntryBase? SelectedEntry
	{
		get => this.selectedEntry;
		set
		{
			if (this.selectionChanging)
				return;

			this.OnSelectionChanged(value).Run();
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

	public override void Close()
	{
		if (this.LivePreview && this.Importer != null && this.Importer.CanRevert)
			this.Importer.Revert().Run();

		base.Close();
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

		this.OnSelectionChanged(null).Run();
	}

	private async void OnItemDoubleClicked(object sender, MouseButtonEventArgs e)
	{
		if (this.SelectedEntry?.CanOpen != true)
			return;

		if (this.SelectedEntry is DirectoryEntry directory)
		{
			this.Filter.CurrentDirectory = directory;
			this.OnSelectionChanged(null).Run();
		}
		else if (this.SelectedEntry is ItemEntry item)
		{
			await item.Open();
		}

		this.RaisePropertyChanged(nameof(LibraryPanel.CurrentPath));
	}

	private async Task OnSelectionChanged(EntryBase? entry, bool forceRevertPreview = false)
	{
		this.selectionChanging = true;

		this.selectedEntry = entry;

		try
		{
			await this.UpdateImporter(entry, forceRevertPreview);
		}
		catch (Exception ex)
		{
			this.Log.Error(ex, "Failed to update importer for library panel");
		}

		this.selectionChanging = false;
	}

	private async Task UpdateImporter(EntryBase? entry, bool forceRevert)
	{
		// If we have an active importer, and we are about to change to a new type, do a revert.
		// otherwise, don't revert.
		if (this.Importer != null && this.Importer.CanRevert && (this.Importer.GetType() != entry?.ImporterType || forceRevert))
			await this.Importer.Revert();

		if (entry == null || entry.ImporterType == null)
			return;

		if (this.Importer?.GetType() != entry.ImporterType)
		{
			this.Importer = null;

			if (entry.ImporterType == null)
				return;

			this.Importer = Activator.CreateInstance(entry.ImporterType) as FileImporterBase;

			if (this.Importer == null)
			{
				throw new Exception($"Failed to create instance of file importer: {entry.ImporterType} for library panel");
			}
		}

		if (this.Importer != null)
		{
			this.Importer.LivePreview = this.LivePreview;
			await entry.Open(this.Importer);
		}
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
		if (sender is Button btn && btn.DataContext is DirectoryEntry directory)
		{
			this.Filter.CurrentDirectory = directory;
			this.RaisePropertyChanged(nameof(LibraryPanel.CurrentPath));
		}
	}

	private void OnRevertClicked(object sender, RoutedEventArgs e)
	{
		this.LivePreview = false;
		this.Importer?.Revert().Run();
	}

	private void OnApplyClicked(object sender, RoutedEventArgs e)
	{
		this.LivePreview = false;
		this.Importer?.Apply(false).Run();
	}
}