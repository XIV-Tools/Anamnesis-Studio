// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData;

using Anamnesis.GameData.Sheets;
using Anamnesis.Tags;

public interface IItem
{
	uint RowId { get; }

	string Name { get; }
	string? Description { get; }
	byte EquipLevel { get; }

	ImageReference? Icon { get; }

	ushort ModelSet { get; }
	ushort ModelBase { get; }
	ushort ModelVariant { get; }

	bool HasSubModel { get; }
	ushort SubModelSet { get; }
	ushort SubModelBase { get; }
	ushort SubModelVariant { get; }

	bool IsFavorite { get; set; }

	TagCollection Tags { get; }

	bool FitsInSlot(ItemSlots slot);
}
