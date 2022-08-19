// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Libraries;

using Anamnesis.Libraries.Items;
using PropertyChanged;
using System.Collections.Generic;

[AddINotifyPropertyChangedInterface]
public class LibraryFilter : IComparer<ItemBase>, IComparer<Pack>
{
	public bool Filter(ItemBase item, Pack? parent)
	{
		return true;
	}

	public bool Filter(Pack group, Pack? parent)
	{
		return true;
	}

	public int Compare(ItemBase? x, ItemBase? y)
	{
		// TODO: sort modes?
		return string.Compare(x?.Name, y?.Name);
	}

	public int Compare(Pack? x, Pack? y)
	{
		// TODO: sort modes?
		return string.Compare(x?.Name, y?.Name);
	}
}
