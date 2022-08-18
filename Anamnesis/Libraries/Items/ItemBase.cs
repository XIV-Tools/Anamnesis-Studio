// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Libraries.Items;

using PropertyChanged;

[AddINotifyPropertyChangedInterface]
public abstract class ItemBase
{
	public string? Name { get; set; }
	public string? Desription { get; set; } = null;
	public string? Author { get; set; } = null;
	public string? Version { get; set; } = null;

	public abstract bool CanLoad { get; }

	public override string ToString()
	{
		return $"[{this.GetType()}] {this.Name}";
	}
}
