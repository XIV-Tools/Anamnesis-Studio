// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Libraries.Items;

using FontAwesome.Sharp;
using System.Windows.Media;

public abstract class ItemEntry : EntryBase
{
	public string? Description { get; set; } = null;
	public string? Author { get; set; } = null;
	public string? Version { get; set; } = null;
	public virtual ImageSource? Thumbnail => null;
	public abstract IconChar Icon { get; }
	public override bool IsDirectory => false;

	public abstract bool CanLoad { get; }
}