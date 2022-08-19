// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Libraries.Items;

using Anamnesis.Libraries.Sources;
using FontAwesome.Sharp;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

public class Pack
{
	private readonly List<ItemBase> allItems = new();

	public Pack(PackDefinitionFile packDefinition, LibrarySourceBase source)
	{
		this.Name = packDefinition.Name;
		this.Author = packDefinition.Author;
		this.Description = packDefinition.Description;
		this.Source = source;
	}

	public string? Name { get; set; }
	public string? Author { get; set; }
	public string? Description { get; set; }

	public ObservableCollection<ItemBase> Items { get; init; } = new();
	public ObservableCollection<string> AvailableTags { get; init; } = new();
	public LibrarySourceBase? Source { get; set; }

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
	}

	public void AddItem(ItemBase item)
	{
		this.allItems.Add(item);
	}
}
