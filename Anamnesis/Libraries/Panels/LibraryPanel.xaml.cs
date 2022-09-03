// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Libraries.Panels;

using Anamnesis.Libraries.Items;
using Anamnesis.Panels;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using XivToolsWpf;
using XivToolsWpf.Extensions;

public partial class LibraryPanel : PanelBase
{
	private const int FilterSearchInputDelay = 250;

	private Task? filterTask = null;
	private bool supressTagEvents = false;
	private string[]? searchQuery;
	private string searchText = string.Empty;
	private int filterDelay = 500;
	private EntryBase? selectedEntry;

	public LibraryPanel(IPanelHost host)
		: base(host)
	{
		this.InitializeComponent();

		this.ContentArea.DataContext = this;
	}

	public LibraryFilter Filter { get; init; } = new();
	public FastObservableCollection<Tag> FilterByTags { get; init; } = new();
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

	private void SetTagEnabled(Tag tag, bool enable)
	{
		if (this.supressTagEvents)
			return;

		// special case, if all tags are enabled, and we toggle one off,
		// actually toggle them all off except the one that was clicked.
		if (!enable)
		{
			this.supressTagEvents = true;

			bool allEnabled = true;
			bool allDisabled = true;
			foreach (Tag otherTag in this.FilterByTags)
			{
				if (otherTag == tag)
					continue;

				allEnabled &= otherTag.IsEnabled;
				allDisabled &= !otherTag.IsEnabled;
			}

			if (allEnabled)
			{
				foreach (Tag otherTag in this.FilterByTags)
				{
					otherTag.IsEnabled = otherTag == tag;
				}
			}

			// if all tags are disabled, enable them all.
			if (allDisabled)
			{
				foreach (Tag otherTag in this.FilterByTags)
				{
					otherTag.IsEnabled = true;
				}
			}

			this.supressTagEvents = false;
		}

		this.FilterItems();
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

		this.Filter.Tags.Clear();
		foreach (Tag tag in this.FilterByTags)
		{
			if (tag.IsEnabled)
			{
				this.Filter.Tags.Add(tag.Name);
			}
		}

		if (this.Filter.CancelRequested)
			return;

		// IF all tags are enabled just don't filter by them.
		if (this.Filter.Tags.Count == this.FilterByTags.Count)
			this.Filter.Tags.Clear();

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

		await Dispatch.MainThread();

		this.Entries.Replace(filteredItems);
	}

	private async Task UpdateTags(bool getAllTags)
	{
		await Dispatch.NonUiThread();

		LibraryFilter filter = new();
		filter.Favorites = false;
		filter.Flatten = true;
		filter.Type = this.Filter.Type;
		IEnumerable<EntryBase>? entries = null;

		if (getAllTags)
		{
			entries = await filter.GetItems(LibraryService.Instance.Packs, null);

			if (filter.CancelRequested)
				return;

			Dictionary<string, Tag> tags = new();

			foreach (EntryBase entry in entries)
			{
				foreach (string tag in entry.Tags)
				{
					if (tags.ContainsKey(tag))
						continue;

					tags.Add(tag, new(this, tag));
				}
			}

			if (filter.CancelRequested)
				return;

			await Dispatch.MainThread();
			this.FilterByTags.Replace(tags.Values);
		}

		await Dispatch.NonUiThread();

		if (this.CurrentDirectory != null)
			entries = await filter.GetItems(this.CurrentDirectory, null);

		if (filter.CancelRequested)
			return;

		if (entries != null)
		{
			HashSet<string> availableTags = new();
			foreach (EntryBase entry in entries)
			{
				if (entry is ItemEntry item)
				{
					foreach (string tag in item.Tags)
					{
						availableTags.Add(tag);
					}
				}
			}

			if (filter.CancelRequested)
				return;

			await Dispatch.MainThread();
			foreach (Tag tag in this.FilterByTags)
			{
				tag.IsAvailable = availableTags.Contains(tag.Name);
			}
		}
		else
		{
			await Dispatch.MainThread();
			foreach (Tag tag in this.FilterByTags)
			{
				tag.IsAvailable = true;
			}
		}
	}

	private void OnBackClicked(object sender, RoutedEventArgs e)
	{
		this.SelectedEntry = null;
		this.CurrentDirectory = this.CurrentDirectory?.Parent;
		this.FilterItems();
		this.UpdateTags(false).Run();
	}

	private void OnItemDoubleClicked(object sender, MouseButtonEventArgs e)
	{
		if (this.SelectedEntry?.CanOpen != true)
			return;

		if (this.SelectedEntry is DirectoryEntry directory)
		{
			this.CurrentDirectory = directory;
			this.FilterItems();
			this.UpdateTags(false).Run();
		}
		else if (this.SelectedEntry is ItemEntry item)
		{
			// TODO...
		}
	}

	private void OnClearSearchClicked(object sender, RoutedEventArgs e)
	{
		this.Search = string.Empty;
		this.SearchBox.Focus();
	}

	private void OnSelectionChanged(EntryBase? entry)
	{
		foreach (Tag tag in this.FilterByTags)
		{
			tag.IsCurrent = entry?.HasTag(tag.Name) ?? false;
		}
	}

	private void OnTabChanged(object sender, RoutedEventArgs e)
	{
		this.CurrentDirectory = null;
		this.Filter.Favorites = false;
		this.Filter.Flatten = false;

		this.UpdateTags(true).Run();
		this.FilterItems();
	}

	private void OnFavoritesChecked(object sender, RoutedEventArgs e)
	{
		this.CurrentDirectory = null;
		this.Filter.Type = LibraryFilter.Types.All;
		this.Filter.Flatten = true;

		this.UpdateTags(true).Run();
		this.FilterItems();
	}

	private void OnFlattenChanged(object sender, RoutedEventArgs e)
	{
		this.FilterItems();
	}

	[AddINotifyPropertyChangedInterface]
	public new class Tag
	{
		private readonly LibraryPanel owner;
		private bool isEnabled = true;

		public Tag(LibraryPanel owner, string name)
		{
			this.Name = name;
			this.owner = owner;
		}

		public string Name { get; set; }
		public bool IsAvailable { get; set; } = true;
		public bool IsCurrent { get; set; } = false;

		public bool IsEnabled
		{
			get => this.isEnabled;
			set
			{
				this.isEnabled = value;
				this.owner.SetTagEnabled(this, value);
			}
		}
	}
}
