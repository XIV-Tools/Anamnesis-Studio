// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Libraries.Items;

using PropertyChanged;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

[AddINotifyPropertyChangedInterface]
public class Group
{
	private readonly List<Group> allGroups = new();
	private readonly List<ItemBase> allItems = new();

	public Group(string name)
	{
		this.Name = name;
	}

	public ObservableCollection<Group> Groups { get; init; } = new();
	public ObservableCollection<ItemBase> Items { get; init; } = new();

	public int TotalCount => this.allItems.Count + this.allGroups.Count;

	public string? Name { get; set; }

	public void AddItem(ItemBase item)
	{
		this.allItems.Add(item);
	}

	public void AddGroup(Group group)
	{
		this.allGroups.Add(group);
	}

	public void Filter(LibraryFilter filter)
	{
		IOrderedEnumerable<ItemBase> sortedItems = this.allItems.OrderBy(item => item, filter);
		this.Items.Clear();
		foreach (ItemBase obj in sortedItems)
		{
			if (!filter.Filter(obj, this))
				continue;

			this.Items.Add(obj);
		}

		IOrderedEnumerable<Group> sortedGroups = this.allGroups.OrderBy(group => group, filter);
		this.Groups.Clear();
		foreach (Group groupItem in sortedGroups)
		{
			groupItem.Filter(filter);

			if (!filter.Filter(groupItem, this))
				continue;

			this.Groups.Add(groupItem);
		}
	}
}
