// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Libraries;

using Anamnesis.Libraries.Items;
using PropertyChanged;
using System.Collections.Generic;

[AddINotifyPropertyChangedInterface]
public class LibraryFilter : IComparer<ItemBase>
{
	public bool IncludeItems { get; set; } = false;
	public bool IncludeEmptyGroups { get; set; } = true;

	public bool Filter(ItemBase item, ItemBase? parent)
	{
		if (!this.IncludeItems && item is not GroupItem)
			return false;

		if (!this.IncludeEmptyGroups && item is GroupItem gi && gi.FilteredItems.Count <= 0)
			return false;

		return true;
	}

	public int Compare(ItemBase? x, ItemBase? y)
	{
		// Groups always go to the top.
		if (x is GroupItem && y?.GetType() == typeof(ItemBase))
			return -1;

		if (x?.GetType() == typeof(ItemBase) && y is GroupItem)
			return 1;

		// TODO: sort modes?
		return string.Compare(x?.Name, y?.Name);
	}
}
