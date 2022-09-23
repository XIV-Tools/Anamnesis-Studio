// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Tags;

using PropertyChanged;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using XivToolsWpf.DependencyProperties;
using XivToolsWpf.Extensions;

[AddINotifyPropertyChangedInterface]
public partial class TagFilter : UserControl, IComparer<Tag>
{
	public static readonly IBind<TagCollection> AllTagsDp = Binder.Register<TagCollection, TagFilter>(nameof(AllTags), BindMode.OneWay);

	private readonly AddTag addTagItem = new();

	public TagFilter()
	{
		this.InitializeComponent();
		this.ContentArea.DataContext = this;
	}

	public FastObservableCollection<Tag> AvailableTags { get; init; } = new();
	public FastObservableCollection<Tag> FilterByTags { get; init; } = new();
	public bool IsPopupOpen { get; set; }

	public TagCollection AllTags
	{
		get => AllTagsDp.Get(this);
		set => AllTagsDp.Set(this, value);
	}

	public int Compare(Tag? x, Tag? y) => string.Compare(x?.Name, y?.Name);

	private void OnLoaded(object sender, RoutedEventArgs e)
	{
		if (!this.FilterByTags.Contains(this.addTagItem))
		{
			this.FilterByTags.Add(this.addTagItem);
		}

		this.AvailableTags.SortAndReplace(this.AllTags, this);
	}

	private void RemoveTag(Tag tag)
	{
		this.FilterByTags.Remove(tag);
		this.AvailableTags.Add(tag);
		this.AvailableTags.Sort(this);
	}

	private void AddTag(Tag tag)
	{
		this.FilterByTags.Insert(this.FilterByTags.Count - 1, tag);
		this.AvailableTags.Remove(tag);
		this.AvailableTags.Sort(this);
	}

	private void OnRemoveTagButtonClicked(object sender, RoutedEventArgs e)
	{
		if (sender is Button btn && btn.DataContext is Tag tag)
		{
			this.RemoveTag(tag);
		}
	}

	private void OnAddTagButtonClicked(object sender, RoutedEventArgs e)
	{
		if (sender is Button btn && btn.DataContext is Tag tag)
		{
			this.AddTag(tag);
		}
	}

	private void OnTagSearchGotKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
	{
		this.IsPopupOpen = true;
		this.AddTagPopup.StaysOpen = true;
	}

	private void OnTagSearchLostKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
	{
		this.AddTagPopup.StaysOpen = false;
	}
}

public class AddTag : Tag
{
	public AddTag()
		: base("New Tag")
	{
	}
}