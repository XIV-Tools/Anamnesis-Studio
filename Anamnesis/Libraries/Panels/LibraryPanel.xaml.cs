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
using XivToolsWpf;
using XivToolsWpf.Extensions;

public partial class LibraryPanel : PanelBase
{
	private bool supressTagEvents = false;

	public LibraryPanel(IPanelHost host)
		: base(host)
	{
		this.InitializeComponent();

		this.ContentArea.DataContext = this;
	}

	public LibraryFilter Filter { get; init; } = new();
	public Pack? SelectedPack { get; set; }
	public ObservableCollection<Tag> FilterByTags { get; init; } = new();
	public ObservableCollection<ItemBase> Items { get; init; } = new();

	private void OnPackSelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		this.Items.Clear();

		if (this.SelectedPack != null)
		{
			this.FilterByTags.Clear();

			foreach (string tag in this.SelectedPack.AvailableTags)
			{
				this.FilterByTags.Add(new Tag(this, tag));
			}
		}

		this.FilterItems().Run();
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

		this.FilterItems().Run();
	}

	private async Task FilterItems()
	{
		if (this.SelectedPack == null)
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

		List<ItemBase> filteredItems = this.SelectedPack.GetItems(this.Filter);

		await Dispatch.MainThread();
		this.Items.Clear();
		foreach (ItemBase item in filteredItems)
		{
			this.Items.Add(item);

			foreach (string tag in item.Tags)
			{
				availableTags.Add(tag);
			}
		}

		foreach (Tag tag in this.FilterByTags)
		{
			tag.IsAvailable = availableTags.Contains(tag.Name);
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
