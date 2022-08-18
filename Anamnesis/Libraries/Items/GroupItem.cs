// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Libraries.Items;

using System.Collections.ObjectModel;

public class GroupItem : ItemBase, ILibraryItemCollection
{
	public GroupItem(string name)
	{
		this.Name = name;
	}

	public ObservableCollection<ItemBase> Items { get; init; } = new();
	public override bool CanLoad => false;
}
