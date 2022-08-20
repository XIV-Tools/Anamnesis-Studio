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

	public HashSet<string> AvailableTags { get; init; } = new();
	public LibrarySourceBase? Source { get; set; }

	public List<ItemBase> GetItems(LibraryFilter filter)
	{
		List<ItemBase> items = new();
		IOrderedEnumerable<ItemBase> sortedItems = this.allItems.OrderBy(item => item, filter);
		foreach (ItemBase obj in sortedItems)
		{
			if (!filter.Filter(obj, this))
				continue;

			items.Add(obj);
		}

		return items;
	}

	public void AddItem(ItemBase item)
	{
		this.allItems.Add(item);
	}
}
