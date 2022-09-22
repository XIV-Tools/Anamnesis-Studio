﻿// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Items;
using Anamnesis.GameData;
using Anamnesis.GameData.Sheets;
using Anamnesis.Services;
using Anamnesis.Tags;

public class DummyItem : IItem
{
	public DummyItem(ushort modelSet, ushort modelBase, ushort modelVariant)
	{
		this.ModelSet = modelSet;
		this.ModelBase = modelBase;
		this.ModelVariant = modelVariant;
	}

	public uint RowId => 0;
	public bool HasSubModel => false;
	public string Name => LocalizationService.GetString("Item_Unknown");
	public string Description => string.Empty;
	public ImageReference? Icon => null;
	public byte EquipLevel => 0;

	public ushort ModelBase { get; private set; }
	public ushort ModelVariant { get; private set; }
	public ushort ModelSet { get; private set; }

	public ushort SubModelBase { get; }
	public ushort SubModelVariant { get; }
	public ushort SubModelSet { get; }

	public bool IsFavorite
	{
		get => FavoritesService.IsFavorite(this);
		set => FavoritesService.SetFavorite(this, value);
	}

	public TagCollection Tags { get; init; } = new()
	{
		"Unknwown",
	};

	public virtual bool FitsInSlot(ItemSlots slot)
	{
		return true;
	}
}
