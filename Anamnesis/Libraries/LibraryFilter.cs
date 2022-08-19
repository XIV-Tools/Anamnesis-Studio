// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Libraries;

using Anamnesis.Libraries.Items;
using PropertyChanged;
using System.Collections.Generic;

[AddINotifyPropertyChangedInterface]
public class LibraryFilter : IComparer<ItemBase>, IComparer<Group>
{
	public bool IncludeEmptyGroups { get; set; } = true;

	public bool Filter(ItemBase item, Group? parent)
	{
		return true;
	}

	public bool Filter(Group group, Group? parent)
	{
		if (!this.IncludeEmptyGroups && group.Items.Count <= 0 && group.Groups.Count <= 0)
			return false;

		return true;
	}

	public int Compare(ItemBase? x, ItemBase? y)
	{
		// TODO: sort modes?
		return string.Compare(x?.Name, y?.Name);
	}

	public int Compare(Group? x, Group? y)
	{
		// TODO: sort modes?
		return string.Compare(x?.Name, y?.Name);
	}
}
