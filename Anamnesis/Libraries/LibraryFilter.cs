﻿// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Libraries;

using Anamnesis.Libraries.Items;
using PropertyChanged;
using System.Collections.Generic;
using XivToolsWpf;

[AddINotifyPropertyChangedInterface]
public class LibraryFilter : IComparer<EntryBase>, IComparer<Pack>
{
	public HashSet<string> Tags { get; init; } = new();
	public bool CancelRequested { get; private set; } = false;
	public bool RestartRequeted { get; private set; } = false;

	public void Restart()
	{
		this.CancelRequested = true;
		this.RestartRequeted = true;
	}

	public void Cancel()
	{
		this.CancelRequested = true;
	}

	public void ResetCancelation()
	{
		this.CancelRequested = false;
		this.RestartRequeted = false;
	}

	public bool Filter(EntryBase entry, DirectoryEntry? parent, string[]? searchQuerry)
	{
		bool anyTagMatch = false;

		if (entry is ItemEntry item)
		{
			// Check if any tags match the search
			if (searchQuerry != null)
			{
				foreach (string tag in item.Tags)
				{
					if (SearchUtility.Matches(tag, searchQuerry))
					{
						anyTagMatch = true;
						break;
					}
				}
			}

			if (this.Tags.Count > 0)
			{
				if (!item.HasAllTags(this.Tags))
				{
					return false;
				}
			}
		}

		if (SearchUtility.Matches(entry.Name, searchQuerry) ||
			anyTagMatch)
			return true;

		return false;
	}

	public bool Filter(Pack group, Pack? parent)
	{
		return true;
	}

	public int Compare(EntryBase? x, EntryBase? y)
	{
		// Directoreis alweays go to the top.
		if (x is DirectoryEntry && y is ItemEntry)
			return -1;

		if (x is ItemEntry && y is DirectoryEntry)
			return 1;

		// TODO: sort modes
		/*
		if (sortMode == Sort.None)
		{
			return 0;
		}
		else if (sortMode == Sort.AlphaNumeric)
		{
			if (itemA.Name == null || itemB.Name == null)
				return 0;

			return itemA.Name.CompareTo(itemB.Name);
		}
		else if (sortMode == Sort.Date)
		{
			if (itemA.DateModified == null || itemB.DateModified == null)
				return 0;

			DateTime dateA = (DateTime)itemA.DateModified;
			DateTime dateB = (DateTime)itemB.DateModified;
			return dateA.CompareTo(dateB);
		}*/

		return string.Compare(x?.Name, y?.Name);
	}

	public int Compare(Pack? x, Pack? y)
	{
		// TODO: sort modes?
		return string.Compare(x?.Name, y?.Name);
	}
}
