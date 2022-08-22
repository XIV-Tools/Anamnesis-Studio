// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Libraries.Items;

using Anamnesis.Libraries.Sources;
using FontAwesome.Sharp;
using PropertyChanged;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using XivToolsWpf;
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

	public Pack(string id, LibrarySourceBase source)
	{
		this.Id = id;
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

	public int ItemCount => this.allItems.Count;

	public async Task<IEnumerable<ItemBase>> GetItems(LibraryFilter filter, string[]? searchQuery, CancellationToken cancellationToken = default)
	{
		ConcurrentQueue<ItemBase> entries;
		lock (this.allItems)
		{
			entries = new ConcurrentQueue<ItemBase>(this.allItems);
		}

		await Dispatch.NonUiThread();

		ConcurrentBag<ItemBase> filteredEntries = new ConcurrentBag<ItemBase>();

		int threads = 4;
		List<Task> tasks = new List<Task>();
		for (int i = 0; i < threads; i++)
		{
			Task t = Task.Run(() =>
			{
				while (!entries.IsEmpty)
				{
					ItemBase? entry;
					if (!entries.TryDequeue(out entry))
						continue;

					try
					{
						if (filter != null && !filter.Filter(entry, this, searchQuery))
						{
							continue;
						}
					}
					catch (Exception ex)
					{
						Log.Error(ex, $"Failed to filter pack Item: {entry}");
					}

					filteredEntries.Add(entry);

					if (cancellationToken.IsCancellationRequested)
					{
						entries.Clear();
					}
				}
			});

			tasks.Add(t);
		}

		await Task.WhenAll(tasks.ToArray());

		List<ItemBase> items = new(filteredEntries);
		items.Sort(filter);
		return items;
	}

	public void AddItem(ItemBase item)
	{
		foreach (string tag in item.Tags)
		{
			this.AvailableTags.Add(tag);
		}

		this.allItems.Add(item);
	}

	public void ClearItems()
	{
		this.AvailableTags.Clear();
		this.allItems.Clear();
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
