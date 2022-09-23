// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData;

using Anamnesis.GameData.Sheets;
using Anamnesis.Tags;
using XivToolsWpf;
using XivToolsWpf.Selectors;

public interface IItem : ITagged
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

	bool FitsInSlot(ItemSlots slot);

	bool ISearchable.Search(string[] query)
	{
		bool matches = false;

		matches |= SearchUtility.Matches(this.Name, query);
		matches |= SearchUtility.Matches(this.Description, query);
		matches |= SearchUtility.Matches(this.ModelSet.ToString(), query);
		matches |= SearchUtility.Matches(this.ModelBase.ToString(), query);
		matches |= SearchUtility.Matches(this.ModelVariant.ToString(), query);

		if (this.HasSubModel)
		{
			matches |= SearchUtility.Matches(this.SubModelSet.ToString(), query);
			matches |= SearchUtility.Matches(this.SubModelBase.ToString(), query);
			matches |= SearchUtility.Matches(this.SubModelVariant.ToString(), query);
		}

		matches |= SearchUtility.Matches(this.RowId.ToString(), query);

		return matches;
	}
}
