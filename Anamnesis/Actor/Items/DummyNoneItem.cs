﻿// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Items;

using Anamnesis.GameData;
using Anamnesis.GameData.Sheets;
using Anamnesis.Services;
using Anamnesis.Tags;

public class DummyNoneItem : IItem
{
	public string Name => LocalizationService.GetString("Item_None");
	public string Description => LocalizationService.GetString("Item_NoneDesc");
	public ImageReference? Icon => null;
	public ushort ModelBase => 0;
	public ushort ModelVariant => 0;
	public ushort ModelSet => 0;
	public uint RowId => 0;
	public bool HasSubModel => false;
	public ushort SubModelBase => 0;
	public ushort SubModelVariant => 0;
	public ushort SubModelSet => 0;
	public byte EquipLevel => 0;

	public bool IsFavorite
	{
		get => FavoritesService.IsFavorite(this);
		set => FavoritesService.SetFavorite(this, value);
	}

	public TagCollection Tags { get; init; } = new()
	{
		"None",
	};

	public bool FitsInSlot(ItemSlots slot)
	{
		return true;
	}
}
