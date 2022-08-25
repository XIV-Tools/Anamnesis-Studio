// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Libraries.Items;

using PropertyChanged;

[AddINotifyPropertyChangedInterface]
public abstract class EntryBase
{
	public string? Name { get; set; }
	public abstract bool IsDirectory { get; }

	public override string ToString()
	{
		return $"[{this.GetType()}] {this.Name}";
	}
}
