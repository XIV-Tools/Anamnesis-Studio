﻿// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Tags;

using Anamnesis.Utils;
using Octokit;
using PropertyChanged;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using XivToolsWpf;
using XivToolsWpf.DependencyProperties;
using XivToolsWpf.Extensions;
using static Anamnesis.Libraries.Panels.LibraryPanel;
using static Anamnesis.Updater.UpdateService.Release;

[AddINotifyPropertyChangedInterface]
public partial class TagFilter : UserControl, IComparer<Tag>
{
	public static readonly IBind<TagCollection> AllTagsDp = Binder.Register<TagCollection, TagFilter>(nameof(AllTags), BindMode.OneWay);

	private readonly AddTag addTagItem = new();
	private readonly FuncQueue tagSearchQueue;

	public TagFilter()
	{
		this.InitializeComponent();
		this.ContentArea.DataContext = this;
		this.tagSearchQueue = new(this.SearchAsync, 250);
	}

	public FastObservableCollection<Tag> AvailableTags { get; init; } = new();
	public FastObservableCollection<Tag> FilterByTags { get; init; } = new();
	public bool IsPopupOpen { get; set; }

	public string? TagSearchText { get; set; }

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
		this.tagSearchQueue.InvokeImmediate();
	}

	private void OnTagSearchLostKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
	{
		this.AddTagPopup.StaysOpen = false;
	}

	private void OnTagSearchPreviewKeyDown(object sender, KeyEventArgs e)
	{
		if (e.Key == Key.Return)
		{
			if (this.AvailableTags.Count > 0)
			{
				this.AddTag(this.AvailableTags[0]);
			}
			else if (this.AvailableTags.Count <= 0 && this.TagSearchText != null)
			{
				this.AddTag(new SearchTag(this.TagSearchText));
			}

			this.TagSearchText = null;
			this.tagSearchQueue.InvokeImmediate();
		}
		else if (e.Key == Key.Escape)
		{
			this.IsPopupOpen = false;
			this.TagSearchText = null;
			Keyboard.ClearFocus();
		}
		else
		{
			this.tagSearchQueue.Invoke();
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
		HashSet<Tag> allTags = new(this.AllTags);
		HashSet<Tag> filterByTags = new(this.FilterByTags);
		await Dispatch.NonUiThread();

		string[]? querry = null;
		if (str != null)
		{
			str = str.ToLower();
			querry = str.Split(' ');
		}

		List<Tag> tags = new List<Tag>();
		foreach (Tag tag in allTags)
		{
			if (filterByTags.Contains(tag))
				continue;

			if (!tag.Search(querry))
				continue;

			tags.Add(tag);
		}

		await this.Dispatcher.MainThread();

		this.AvailableTags.SortAndReplace(tags, this);
	}
}

public class SearchTag : Tag
{
	public SearchTag(string search)
		: base(search)
	{
	}
}

public class AddTag : Tag
{
	public AddTag()
		: base("New Tag")
	{
	}
}