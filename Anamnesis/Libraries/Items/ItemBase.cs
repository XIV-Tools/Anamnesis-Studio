// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Libraries.Items;

public abstract class ItemEntry : EntryBase
{
	public string? Author { get; set; } = null;
	public string? Version { get; set; } = null;
	public override bool IsDirectory => false;

	public abstract bool CanLoad { get; }
}