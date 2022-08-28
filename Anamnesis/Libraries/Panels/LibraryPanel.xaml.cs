﻿// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Libraries.Panels;

using Anamnesis.Libraries.Items;
using Anamnesis.Panels;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

	public bool NarrowMode { get; set; }
	public LibraryFilter Filter { get; init; } = new();
	public Pack? SelectedPack { get; set; }
	public ObservableCollection<Tag> FilterByTags { get; init; } = new();
	public FastObservableCollection<EntryBase> Entries { get; init; } = new();
	public DirectoryEntry? CurrentDirectory { get; set; }

	public bool ViewList { get; set; } = false;

	[AlsoNotifyFor(nameof(SelectedEntry))]
	public ItemEntry? SelectedItem => this.SelectedEntry as ItemEntry;

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

	protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
	{
		base.OnRenderSizeChanged(sizeInfo);
		this.NarrowMode = this.ActualWidth < 800;
	}

	private void OnPackSelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		this.Entries.Clear();
		this.CurrentDirectory = null;

		if (this.SelectedPack != null)
		{
			this.FilterByTags.Clear();

			foreach (string tag in this.SelectedPack.AvailableTags)
			{
				this.FilterByTags.Add(new Tag(this, tag));
			}
		}

		this.FilterItems();
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

		if (this.filterTask != null && !this.filterTask.IsCompleted)
		{
			this.Filter.Restart();
			return;
		}

		this.filterTask = Task.Run(async () =>
		{
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
			await this.FilterItemsInternalAsync();
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

		DirectoryEntry? current = this.SelectedPack;

		if (this.CurrentDirectory != null)
			current = this.CurrentDirectory;

		if (current == null)
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

		IEnumerable<EntryBase> filteredItems = await current.GetItems(this.Filter, this.searchQuery);

		if (this.Filter.CancelRequested)
			return;

		await Dispatch.MainThread();

		this.Entries.Replace(filteredItems);

		foreach (EntryBase entry in filteredItems)
		{
			if (entry is ItemEntry item)
			{
				foreach (string tag in item.Tags)
				{
					availableTags.Add(tag);
				}
			}
		}

		if (this.Filter.CancelRequested)
			return;

		foreach (Tag tag in this.FilterByTags)
		{
			tag.IsAvailable = availableTags.Contains(tag.Name);
		}
	}

	private void OnPackRefreshClicked(object sender, RoutedEventArgs e)
	{
		this.SelectedPack?.Refresh();
	}

	private void OnPackUpdateClicked(object sender, RoutedEventArgs e)
	{
		this.SelectedPack?.Update();
	}

	private void OnBackClicked(object sender, RoutedEventArgs e)
	{
		////this.SelectedEntry = null;
		this.CurrentDirectory = this.CurrentDirectory?.Parent;

		if (this.CurrentDirectory is Pack)
			this.CurrentDirectory = null;

		this.FilterItems(true);
	}

	private void OnItemDoubleClicked(object sender, MouseButtonEventArgs e)
	{
		if (this.SelectedEntry is DirectoryEntry directory)
		{
			this.CurrentDirectory = directory;
			this.FilterItems(true);
		}
		else if (this.SelectedEntry is ItemEntry item)
		{
		}
	}

	private void OnClearSearchClicked(object sender, RoutedEventArgs e)
	{
		this.Search = string.Empty;
		this.SearchBox.Focus();
	}

	private void OnSelectionChanged(EntryBase? entry)
	{
		foreach(Tag tag in this.FilterByTags)
		{
			tag.IsCurrent = entry?.HasTag(tag.Name) ?? false;
		}
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
