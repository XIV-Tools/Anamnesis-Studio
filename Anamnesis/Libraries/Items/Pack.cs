// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Libraries.Items;

using Anamnesis.Libraries.Sources;
using FontAwesome.Sharp;
using PropertyChanged;
using System.Collections.Generic;
using XivToolsWpf.Extensions;

[AddINotifyPropertyChangedInterface]
public class Pack : DirectoryEntry
{
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

	public override IconChar Icon => this.Source?.Icon ?? IconChar.None;
	public string? Author { get; set; }
	public string? Version { get; set; }
	public bool IsUpdateAvailable { get; set; }
	public bool IsUpdating { get; set; }
	public string Id { get; set; }

	public LibrarySourceBase? Source { get; set; }

	public override void ClearItems()
	{
		base.ClearItems();
	}
}
