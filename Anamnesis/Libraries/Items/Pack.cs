// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Libraries.Items;

using Anamnesis.Libraries.Sources;
using FontAwesome.Sharp;
using PropertyChanged;

[AddINotifyPropertyChangedInterface]
public class Pack : DirectoryEntry
{
	public Pack(string id, PackDefinitionFile packDefinition, LibrarySourceBase source)
		: this(id, source)
	{
		this.Name = packDefinition.Name;
		this.Author = packDefinition.Author;
		this.Description = packDefinition.Description;
		this.Version = packDefinition.Version;
	}

	public Pack(string id, LibrarySourceBase source)
	{
		this.Id = id;
		this.Source = source;
	}

	public override IconChar Icon => this.Source?.Icon ?? IconChar.None;
	public override IconChar IconBack => IconChar.Folder;
	public string? Author { get; set; }
	public string? Version { get; set; }
	public string Id { get; set; }

	public bool IsUpdateAvailable { get; set; } = false;
	public bool IsUpdating { get; set; } = false;

	public LibrarySourceBase? Source { get; set; }

	public override bool CanOpen => !this.IsUpdating && this.Entries.Count > 0;

	public override void ClearItems()
	{
		base.ClearItems();
	}
}
