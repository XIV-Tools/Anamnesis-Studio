// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Libraries.Items;

using FontAwesome.Sharp;
using PropertyChanged;
using System.Collections.Generic;

[AddINotifyPropertyChangedInterface]
public class DirectoryEntry : EntryBase
{
	public DirectoryEntry? Parent { get; set; }
	public List<EntryBase> Entries { get; init; } = new();
	public override bool IsDirectory => true;
	public override IconChar Icon => IconChar.None;

	public int ItemCount => this.Entries.Count;

	public virtual void AddEntry(EntryBase entry)
	{
		this.Entries.Add(entry);
	}

	public virtual void ClearItems()
	{
		this.Entries.Clear();
	}

	public override bool IsType(LibraryFilter.Types type)
	{
		return false;
	}
}