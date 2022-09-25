// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Libraries.Items;

using FontAwesome.Sharp;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

[AddINotifyPropertyChangedInterface]
public class DirectoryEntry : EntryBase
{
	public DirectoryEntry? Parent { get; set; }
	public List<EntryBase> Entries { get; init; } = new();
	public override bool IsDirectory => true;
	public override IconChar Icon => IconChar.None;
	public override IconChar IconBack => IconChar.Folder;

	public override bool CanOpen => true;

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

	public override Task Open() => throw new NotSupportedException();
}