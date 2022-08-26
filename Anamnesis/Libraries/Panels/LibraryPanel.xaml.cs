// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Libraries.Panels;

using Anamnesis.Libraries.Items;
using Anamnesis.Panels;
using PropertyChanged;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using XivToolsWpf;
using XivToolsWpf.Extensions;

public partial class LibraryPanel : PanelBase
{
	private bool filterQueued = false;
	private Task? filterTask = null;
	private bool supressTagEvents = false;
	private string[]? searchQuery;
	private string searchText = string.Empty;

	public LibraryPanel(IPanelHost host)
		: base(host)
	{
		this.InitializeComponent();

		this.ContentArea.DataContext = this;
	}

	public EntryBase? SelectedEntry { get; set; }
	public LibraryFilter Filter { get; init; } = new();
	public Pack? SelectedPack { get; set; }
	public ObservableCollection<Tag> FilterByTags { get; init; } = new();
	public FastObservableCollection<EntryBase> Entries { get; init; } = new();
	public DirectoryEntry? CurrentDirectory { get; set; }

	public bool ViewList { get; set; } = false;

	[AlsoNotifyFor(nameof(SelectedEntry))]
	public ItemEntry? SelectedItem => this.SelectedEntry as ItemEntry;

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

			this.FilterItems();
		}
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

	private void FilterItems()
	{
		if (this.filterTask != null && !this.filterTask.IsCompleted)
		{
			this.filterQueued = true;
			return;
		}

		this.filterTask = Task.Run(async () => await this.FilterItemsAsync());
	}

	private async Task FilterItemsAsync()
	{
		DirectoryEntry? current = this.SelectedPack;

		if (this.CurrentDirectory != null)
			current = this.CurrentDirectory;

		if (current == null)
			return;

		HashSet<string> availableTags = new();

		await Dispatch.NonUiThread();

		this.Filter.Tags.Clear();
		foreach (Tag tag in this.FilterByTags)
		{
			if (tag.IsEnabled)
			{
				this.Filter.Tags.Add(tag.Name);
			}
		}

		// IF all tags are enabled just don't filter by them.
		if (this.Filter.Tags.Count == this.FilterByTags.Count)
			this.Filter.Tags.Clear();

		IEnumerable<EntryBase> filteredItems = await current.GetItems(this.Filter, this.searchQuery);

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

		foreach (Tag tag in this.FilterByTags)
		{
			tag.IsAvailable = availableTags.Contains(tag.Name);
		}

		if (this.filterQueued)
		{
			this.FilterItems();
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

		this.FilterItems();
	}

	private void OnItemDoubleClicked(object sender, MouseButtonEventArgs e)
	{
		if (this.SelectedEntry is DirectoryEntry directory)
		{
			this.CurrentDirectory = directory;
			this.FilterItems();
		}
		else if (this.SelectedEntry is ItemEntry item)
		{
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
