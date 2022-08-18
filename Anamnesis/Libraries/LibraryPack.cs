// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Libraries;

using Anamnesis.Libraries.Items;
using PropertyChanged;
using System.Collections.ObjectModel;

[AddINotifyPropertyChangedInterface]
public class LibraryPack : ILibraryItemCollection
{
	public string Name { get; set; } = string.Empty;
	public string Desription { get; set; } = string.Empty;
	public string Author { get; set; } = string.Empty;
	public ObservableCollection<ItemBase> Items { get; init; } = new();
}

public interface ILibraryItemCollection
{
	public ObservableCollection<ItemBase> Items { get; }
}