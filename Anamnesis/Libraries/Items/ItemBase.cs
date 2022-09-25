// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Libraries.Items;

using System.Threading.Tasks;

public abstract class ItemEntry : EntryBase
{
	public string? Author { get; set; } = null;
	public string? Version { get; set; } = null;
	public override bool IsDirectory => false;
}