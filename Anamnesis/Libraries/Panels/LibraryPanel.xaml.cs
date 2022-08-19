// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Libraries.Panels;

using Anamnesis.Libraries.Items;
using Anamnesis.Panels;
using PropertyChanged;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

public partial class LibraryPanel : PanelBase
{
	public LibraryPanel(IPanelHost host)
		: base(host)
	{
		this.InitializeComponent();

		this.ContentArea.DataContext = this;
	}

	public Pack? SelectedPack { get; set; }
	public ObservableCollection<Tag> FilterByTags { get; init; } = new();

	private void OnPackSelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		this.FilterByTags.Clear();

		if (this.SelectedPack == null)
			return;

		foreach(string tag in this.SelectedPack.AvailableTags)
		{
			this.FilterByTags.Add(new(this, tag));
		}
	}

	private void SetTagEnabled(Tag tag, bool enable)
	{
		// special case, if all tags are enabled, and we toggle one off,
		// actually toggle them all off except the one that was clicked.
		if (!enable)
		{
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
		}

		// Apply tag filters
		// TODO: are tags and or or? should we filter the available tags by the selected tags?
		// What if people want to see files that are in two seperate taged groups?
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
