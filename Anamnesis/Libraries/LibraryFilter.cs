// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Libraries;

using Anamnesis.Libraries.Items;
using Anamnesis.Tags;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

public class LibraryFilter : TagFilterBase<EntryBase>
{
	public readonly ConcurrentDictionary<DirectoryEntry, bool> UsedDirectories = new();

	public enum Types
	{
		All,
		Poses,
		Characters,
		Scenes,
	}

	// Actual filter params
	public Types Type { get; set; } = Types.All;
	public bool Flatten { get; set; } = true;
	public bool Favorites { get; set; } = true;
	public DirectoryEntry? CurrentDirectory { get; set; }

	public bool HasSearchOrTags => this.Tags.Count > 1 || !string.IsNullOrEmpty(this.Search);

	public override int CompareItems(EntryBase a, EntryBase b)
	{
		// Directoreis alweays go to the top.
		if (a is DirectoryEntry && b is ItemEntry)
			return -1;

		if (a is ItemEntry && b is DirectoryEntry)
			return 1;

		return string.Compare(a?.Name, b?.Name);
	}

	public override bool FilterItem(EntryBase entry)
	{
		if (!entry.IsDirectory && !entry.IsType(this.Type))
			return false;

		if (this.Flatten || this.HasSearchOrTags)
		{
			if (entry.IsDirectory)
			{
				return false;
			}
		}
		else
		{
			if (!entry.IsDirectory)
			{
				DirectoryEntry? parent = entry.Parent;
				while (parent != this.CurrentDirectory && parent != null)
				{
					this.UsedDirectories.TryAdd(parent, true);
					parent = parent.Parent;
				}

				if (parent != this.CurrentDirectory)
				{
					return false;
				}
			}
		}

		return true;
	}

	protected override async Task<IEnumerable<object>?> Filter()
	{
		this.UsedDirectories.Clear();
		IEnumerable<object>? results = await base.Filter();

		if (results == null)
			return results;

		if (this.Flatten || this.HasSearchOrTags)
			return results;

		List<object> finalFilteredResults = new();
		foreach (EntryBase entry in results)
		{
			// TODO: this should be handled through a "all packs" list, or an update flyout or something.
			// but for now, just show any packs that need an update in all categories.
			if (entry is Pack pack && pack.IsUpdateAvailable)
			{
				finalFilteredResults.Add(entry);
				continue;
			}

			// skip unused directories.
			if (entry is DirectoryEntry dir && !this.UsedDirectories.ContainsKey(dir))
				continue;

			// Is this item _in_ the current directory
			if (entry.Parent == this.CurrentDirectory)
			{
				finalFilteredResults.Add(entry);
			}
		}

		return finalFilteredResults;
	}
}
