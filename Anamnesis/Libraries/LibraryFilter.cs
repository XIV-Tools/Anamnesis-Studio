// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Libraries;

using Anamnesis.Libraries.Items;
using Anamnesis.Libraries.Sources;
using PropertyChanged;
using System.Collections.Concurrent;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using XivToolsWpf;
using Serilog;

[AddINotifyPropertyChangedInterface]
public class LibraryFilter : IComparer<EntryBase>, IComparer<Pack>, INotifyPropertyChanged
{
	public event PropertyChangedEventHandler? PropertyChanged;

	public enum Types
	{
		Packs,
		Poses,
		Characters,
		Scenes,
	}

	public HashSet<string> Tags { get; init; } = new();
	public bool CancelRequested { get; private set; } = false;
	public bool RestartRequeted { get; private set; } = false;
	public Types Type { get; set; } = Types.Packs;
	public bool Flatten { get; set; } = true;

	protected ILogger Log => Serilog.Log.ForContext<LibraryFilter>();

	public void Restart()
	{
		this.CancelRequested = true;
		this.RestartRequeted = true;
	}

	public void Cancel()
	{
		this.CancelRequested = true;
	}

	public void ResetCancelation()
	{
		this.CancelRequested = false;
		this.RestartRequeted = false;
	}

	public async Task<IEnumerable<EntryBase>> GetItems(IEnumerable<DirectoryEntry> directories, string[]? searchQuery)
	{
		List<Task<List<EntryBase>>> tasks = new();
		foreach (Pack pack in directories)
		{
			tasks.Add(this.GetItems(pack, this.Flatten, searchQuery));
		}

		await Task.WhenAll(tasks);

		List<EntryBase> results = new();
		foreach (Task<List<EntryBase>> task in tasks)
		{
			results.AddRange(task.Result);
		}

		return results;
	}

	public async Task<List<EntryBase>> GetItems(DirectoryEntry directory, bool flatten, string[]? searchQuery)
	{
		List<EntryBase> result = new();

		ConcurrentQueue<EntryBase> entries;
		lock (directory.Entries)
		{
			entries = new ConcurrentQueue<EntryBase>(directory.Entries);
		}

		await Dispatch.NonUiThread();

		if (this.CancelRequested)
			return result;

		ConcurrentBag<EntryBase> filteredEntries = new();

		int threads = 4;
		List<Task> tasks = new List<Task>();
		for (int i = 0; i < threads; i++)
		{
			Task t = Task.Run(async () =>
			{
				while (!entries.IsEmpty)
				{
					EntryBase? entry;
					if (!entries.TryDequeue(out entry))
						continue;

					if (entry is DirectoryEntry dir)
					{
						// If we are flattening, and this is a directory, add its content to be filtered.
						if (flatten)
						{
							lock (dir.Entries)
							{
								foreach (EntryBase newEntry in dir.Entries)
								{
									if (this.CancelRequested)
									{
										entries.Clear();
										return;
									}

									entries.Enqueue(newEntry);
								}
							}

							continue;
						}
						else
						{
							// If we are not flattening, check if the directory has any children that
							// pass the filter.
							List<EntryBase> children = await this.GetItems(dir, true, searchQuery);
							if (children.Count <= 0)
							{
								continue;
							}
						}
					}

					try
					{
						if (!this.Filter(entry, directory, searchQuery))
						{
							continue;
						}
					}
					catch (Exception ex)
					{
						this.Log.Error(ex, $"Failed to filter pack Item: {entry}");
					}

					filteredEntries.Add(entry);

					if (this.CancelRequested)
					{
						entries.Clear();
					}
				}
			});

			tasks.Add(t);
		}

		await Task.WhenAll(tasks.ToArray());

		if (this.CancelRequested)
			return result;

		result = new(filteredEntries);
		result.Sort(this);
		return result;
	}

	public bool Filter(EntryBase entry, DirectoryEntry? parent, string[]? searchQuerry)
	{
		bool anyTagMatch = false;

		if (!entry.IsDirectory && !entry.IsType(this.Type))
			return false;

		if (entry is ItemEntry item)
		{
			// Check if any tags match the search
			if (searchQuerry != null)
			{
				foreach (string tag in item.Tags)
				{
					if (SearchUtility.Matches(tag, searchQuerry))
					{
						anyTagMatch = true;
						break;
					}
				}
			}

			if (this.Tags.Count > 0)
			{
				if (!item.HasAllTags(this.Tags))
				{
					return false;
				}
			}
		}

		if (SearchUtility.Matches(entry.Name, searchQuerry) ||
			anyTagMatch)
			return true;

		return false;
	}

	public bool Filter(Pack group, Pack? parent)
	{
		return true;
	}

	public int Compare(EntryBase? x, EntryBase? y)
	{
		// Directoreis alweays go to the top.
		if (x is DirectoryEntry && y is ItemEntry)
			return -1;

		if (x is ItemEntry && y is DirectoryEntry)
			return 1;

		// TODO: sort modes
		/*
		if (sortMode == Sort.None)
		{
			return 0;
		}
		else if (sortMode == Sort.AlphaNumeric)
		{
			if (itemA.Name == null || itemB.Name == null)
				return 0;

			return itemA.Name.CompareTo(itemB.Name);
		}
		else if (sortMode == Sort.Date)
		{
			if (itemA.DateModified == null || itemB.DateModified == null)
				return 0;

			DateTime dateA = (DateTime)itemA.DateModified;
			DateTime dateB = (DateTime)itemB.DateModified;
			return dateA.CompareTo(dateB);
		}*/

		return string.Compare(x?.Name, y?.Name);
	}

	public int Compare(Pack? x, Pack? y)
	{
		// TODO: sort modes?
		return string.Compare(x?.Name, y?.Name);
	}
}
