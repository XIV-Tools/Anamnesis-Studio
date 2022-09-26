// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Libraries;

using Anamnesis.Libraries.Items;
using Anamnesis.Tags;

public class LibraryFilter : TagFilterBase<EntryBase>
{
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

	public bool HasSearchOrTags => this.Tags.Count > 1;

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
			if (entry.Parent != this.CurrentDirectory)
			{
				return false;
			}
		}

		return true;
	}
}
