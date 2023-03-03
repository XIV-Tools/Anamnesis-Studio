// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Tags;

using Anamnesis.Utils;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using XivToolsWpf;
using XivToolsWpf.DependencyProperties;
using XivToolsWpf.Extensions;

[AddINotifyPropertyChangedInterface]
public partial class TagFilter : UserControl, IComparer<Tag>
{
	public static readonly IBind<TagFilterBase?> FilterDp = Binder.Register<TagFilterBase?, TagFilter>(nameof(Filter), BindMode.OneWay);
	public static readonly IBind<bool> IsPopupOpenDp = Binder.Register<bool, TagFilter>(nameof(IsPopupOpen));

	private readonly AddTag addTagItem = new();
	private readonly FuncQueue tagSearchQueue;

	public TagFilter()
	{
		this.InitializeComponent();
		this.ContentArea.DataContext = this;
		this.tagSearchQueue = new(this.SearchAsync, 250);
		this.PropertyChanged += this.OnSelfPropertyChanged;
	}

	public FastObservableCollection<Tag> AvailableTags { get; init; } = new();
	public FastObservableCollection<Tag> FilterByTags { get; init; } = new();

	public SearchTag SearchTag { get; init; } = new(string.Empty);
	public Tag? SuggestTag { get; set; }
	public string? TagSearchText { get; set; }

	public TagFilterBase? Filter
	{
		get => FilterDp.Get(this);
		set => FilterDp.Set(this, value);
	}

	public bool IsPopupOpen
	{
		get => IsPopupOpenDp.Get(this);
		set => IsPopupOpenDp.Set(this, value);
	}

	public int Compare(Tag? x, Tag? y) => string.Compare(x?.Name, y?.Name);

	private void OnLoaded(object sender, RoutedEventArgs e)
	{
		if (!this.FilterByTags.Contains(this.addTagItem))
		{
			this.FilterByTags.Add(this.addTagItem);
		}

		if (this.Filter?.AvailableTags != null)
		{
			this.AvailableTags.SortAndReplace(this.Filter.AvailableTags, this);
		}
	}

	private void RemoveTag(Tag tag)
	{
		this.FilterByTags.Remove(tag);
		this.AvailableTags.Add(tag);
		this.AvailableTags.Sort(this);

		this.Filter?.Tags.Replace(this.FilterByTags);
		this.Filter?.OnTagsChanged();

		this.addTagItem.ShowHint = this.FilterByTags.Count <= 1;
	}

	private void AddTag(Tag tag)
	{
		this.addTagItem.ShowHint = false;

		this.FilterByTags.Insert(this.FilterByTags.Count - 1, tag);
		this.AvailableTags.Remove(tag);
		this.AvailableTags.Sort(this);

		this.Filter?.Tags.Replace(this.FilterByTags);
		this.Filter?.OnTagsChanged();
	}

	private void OnTagClicked(object sender, RoutedEventArgs e)
	{
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
		////this.tagSearchQueue.InvokeImmediate();
	}

	private void OnTagSearchLostKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
	{
		this.AddTagPopup.StaysOpen = false;
	}

	private async void OnTagSearchPreviewKeyDown(object sender, KeyEventArgs e)
	{
		if (e.Key == Key.Return)
		{
			if (this.SearchTag.Name == null)
				return;

			this.AddTag(new SearchTag(this.SearchTag.Name));
			this.TagSearchText = null;
		}
		else if (e.Key == Key.Tab)
		{
			this.tagSearchQueue.InvokeImmediate();
			await this.tagSearchQueue.WaitForPendingExecute();

			if (this.AvailableTags.Count > 0)
			{
				this.AddTag(this.AvailableTags[0]);
			}

			// clear the serach and run it again so the next time we open we have blank results.
			this.TagSearchText = null;
			this.tagSearchQueue.InvokeImmediate();

			// IF we are not holing shift, close the popup
			if (!Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
			{
				this.IsPopupOpen = false;
				Keyboard.ClearFocus();
			}
		}
		else
		{
			this.tagSearchQueue.Invoke();
		}
	}

	private void OnPopupOpened(object sender, EventArgs e)
	{
		if (this.Filter?.AvailableTags != null)
		{
			this.AvailableTags.SortAndReplace(this.Filter.AvailableTags, this);
			this.SuggestTag = this.AvailableTags.FirstOrDefault();
		}
	}

	private void OnPopupClosed(object sender, EventArgs e)
	{
		this.TagSearchText = null;
	}

	private async Task SearchAsync()
	{
		await this.Dispatcher.MainThread();
		string? str = this.TagSearchText;
		HashSet<Tag>? allTags = this.Filter?.AvailableTags != null ? new(this.Filter.AvailableTags) : null;
		HashSet<Tag> filterByTags = new(this.FilterByTags);
		await Dispatch.NonUiThread();

		string[]? querry = null;
		if (str != null)
		{
			str = str.ToLower();
			querry = str.Split(' ');
		}

		List<Tag> tags = new List<Tag>();
		if (allTags != null)
		{
			foreach (Tag tag in allTags)
			{
				if (filterByTags.Contains(tag))
					continue;

				if (!tag.Search(querry))
					continue;

				tags.Add(tag);
			}
		}

		await this.Dispatcher.MainThread();

		this.AvailableTags.SortAndReplace(tags, this);
		this.SuggestTag = this.AvailableTags.FirstOrDefault();
	}

	private void OnSelfPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName == nameof(this.TagSearchText))
		{
			if (this.TagSearchText != null)
			{
				this.SearchTag.SetName(this.TagSearchText);
			}
		}
	}
}

public class AddTag : Tag
{
	public AddTag()
		: base("New Tag")
	{
	}

	public bool ShowHint { get; set; } = true;

	public override bool CanCompare => false;
	public override bool Search(string[]? querry) => throw new NotSupportedException();
}