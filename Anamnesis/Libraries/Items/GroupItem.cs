// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Libraries.Items;

using System.Collections.ObjectModel;
using System.Linq;

public class GroupItem : ItemBase
{
	public GroupItem(string name)
	{
		this.Name = name;
	}

	public ObservableCollection<ItemBase> Items { get; init; } = new();
	public ObservableCollection<ItemBase> FilteredItems { get; init; } = new();
	public override bool CanLoad => false;

	public void Filter(LibraryFilter filter)
	{
		IOrderedEnumerable<ItemBase> sortedItems = this.Items.OrderBy(item => item, filter);

		this.FilteredItems.Clear();

		foreach (ItemBase obj in sortedItems)
		{
			if (obj is GroupItem groupItem)
				groupItem.Filter(filter);

			if (!filter.Filter(obj, this))
				continue;

			this.FilteredItems.Add(obj);
		}
	}
}
