// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Libraries.Items;

using Anamnesis.Libraries.Sources;
using FontAwesome.Sharp;
using PropertyChanged;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using XivToolsWpf.Extensions;

[AddINotifyPropertyChangedInterface]
public class Pack
{
	private readonly List<ItemBase> allItems = new();

	public Pack(string id, PackDefinitionFile packDefinition, LibrarySourceBase source)
	{
		this.Id = id;
		this.Name = packDefinition.Name;
		this.Author = packDefinition.Author;
		this.Description = packDefinition.Description;
		this.Version = packDefinition.Version;
		this.Source = source;
	}

	public string? Name { get; set; }
	public string? Author { get; set; }
	public string? Description { get; set; }
	public string? Version { get; set; }
	public bool IsUpdateAvailable { get; set; }
	public bool IsUpdating { get; set; }
	public string Id { get; set; }

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

	public void Refresh()
	{
		this.Source?.LoadPack(this).Run();
	}

	public void Update()
	{
		this.Source?.UpdatePack(this).Run();
	}
}
