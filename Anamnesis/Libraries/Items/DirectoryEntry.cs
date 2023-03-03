// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Libraries.Items;

using Anamnesis.Files;
using FontAwesome.Sharp.Pro;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

[AddINotifyPropertyChangedInterface]
public class DirectoryEntry : EntryBase
{
	public List<EntryBase> Entries { get; init; } = new();
	public override bool IsDirectory => true;
	public override ProIcons Icon => ProIcons.None;
	public override ProIcons IconBack => ProIcons.Folder;

	public override bool CanOpen => true;

	public virtual void AddEntry(EntryBase entry)
	{
		this.Entries.Add(entry);
		entry.Parent = this;
	}

	public virtual void ClearItems()
	{
		this.Entries.Clear();
	}

	public override bool IsType(LibraryFilter.Types type)
	{
		return false;
	}

	public override Task Open(FileImporterBase? preview = null) => throw new NotSupportedException();
}