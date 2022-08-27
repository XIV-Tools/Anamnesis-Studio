// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Libraries.Items;

using PropertyChanged;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using XivToolsWpf;
using System;
using Serilog;

[AddINotifyPropertyChangedInterface]
public class DirectoryEntry : EntryBase
{
	private readonly List<EntryBase> allEntries = new();

	public DirectoryEntry? Parent { get; set; }
	public override bool IsDirectory => true;
	public override bool HasThumbnail => false;

	public int ItemCount => this.allEntries.Count;

	public async Task<IEnumerable<EntryBase>> GetItems(LibraryFilter filter, string[]? searchQuery)
	{
		ConcurrentQueue<EntryBase> entries;
		lock (this.allEntries)
		{
			entries = new ConcurrentQueue<EntryBase>(this.allEntries);
		}

		await Dispatch.NonUiThread();

		if (filter.CancelRequested)
			return entries;

		ConcurrentBag<EntryBase> filteredEntries = new();

		int threads = 4;
		List<Task> tasks = new List<Task>();
		for (int i = 0; i < threads; i++)
		{
			Task t = Task.Run(() =>
			{
				while (!entries.IsEmpty)
				{
					EntryBase? entry;
					if (!entries.TryDequeue(out entry))
						continue;

					try
					{
						if (!filter.Filter(entry, this, searchQuery))
						{
							continue;
						}
					}
					catch (Exception ex)
					{
						this.Log.Error(ex, $"Failed to filter pack Item: {entry}");
					}

					filteredEntries.Add(entry);

					if (filter.CancelRequested)
					{
						entries.Clear();
					}
				}
			});

			tasks.Add(t);
		}

		await Task.WhenAll(tasks.ToArray());

		if (filter.CancelRequested)
			return entries;

		List<EntryBase> items = new(filteredEntries);
		items.Sort(filter);
		return items;
	}

	public virtual void AddEntry(EntryBase entry)
	{
		this.allEntries.Add(entry);
	}

	public virtual void ClearItems()
	{
		this.allEntries.Clear();
	}
}