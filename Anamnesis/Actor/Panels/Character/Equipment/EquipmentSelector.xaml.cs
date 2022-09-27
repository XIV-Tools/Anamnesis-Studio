// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Panels.Character.Equipment;

using Anamnesis.Actor.Utilities;
using Anamnesis.GameData;
using System.Threading.Tasks;
using System.Windows;
using System;
using System.Windows.Controls;
using Anamnesis.GameData.Excel;
using Anamnesis.Memory;
using Anamnesis.Tags;
using Anamnesis.Actor.Items;
using XivToolsWpf.DependencyProperties;
using System.Collections.Generic;

public partial class EquipmentSelector : UserControl
{
	public static readonly IBind<IItem?> ValueDp = Binder.Register<IItem?, EquipmentSelector>(nameof(Value), BindMode.TwoWay);

	public EquipmentSelector()
	{
		this.InitializeComponent();
		this.ContentArea.DataContext = this;
	}

	public enum SortModes
	{
		Name,
		NameInv,
		Row,
		RowInv,
		Level,
		LevelInv,
	}

	public CharacterEquipment? EquipmentEditor { get; set; }
	public EquipmentFilter Filter { get; init; } = new();

	public List<ItemSlots> Slots { get; set; } = new()
	{
		ItemSlots.MainHand,
		ItemSlots.OffHand,

		ItemSlots.Head,
		ItemSlots.Body,
		ItemSlots.Hands,
		ItemSlots.Legs,
		ItemSlots.Feet,

		ItemSlots.Ears,
		ItemSlots.Neck,
		ItemSlots.Wrists,
		ItemSlots.RightRing,
		ItemSlots.LeftRing,
	};

	public IItem? Value
	{
		get => ValueDp.Get(this);
		set => ValueDp.Set(this, value);
	}

	protected Task LoadItems()
	{
		// Chocobo Items
		this.Selector.AddItem(ItemUtility.NoneItem);
		this.Selector.AddItem(ItemUtility.YellowChocoboSkin);
		this.Selector.AddItem(ItemUtility.BlackChocoboSkin);

		foreach (BuddyEquip buddyEquip in App.Services.GameData.BuddyEquips)
		{
			if (buddyEquip.Head != null)
				this.Selector.AddItem(buddyEquip.Head);

			if (buddyEquip.Body != null)
				this.Selector.AddItem(buddyEquip.Body);

			if (buddyEquip.Feet != null)
				this.Selector.AddItem(buddyEquip.Feet);
		}

		// Player / NPC Items
		this.Selector.AddItem(ItemUtility.NoneItem);
		this.Selector.AddItem(ItemUtility.NpcBodyItem);
		this.Selector.AddItem(ItemUtility.InvisibileBodyItem);
		this.Selector.AddItem(ItemUtility.InvisibileHeadItem);
		this.Selector.AddItems(App.Services.GameData.Equipment);
		this.Selector.AddItems(App.Services.GameData.Items);
		this.Selector.AddItems(App.Services.GameData.Perform);

		return Task.CompletedTask;
	}

	private void OnSlotChanged(object sender, SelectionChangedEventArgs e)
	{
		// Bit of a hack, but we need to have the selection in the
		// equiopment panel reflect the new slot selection, so just force it!
		this.EquipmentEditor?.EditSlot(this.Filter.Slot);
	}

	private void OnRaceGearClicked(object sender, RoutedEventArgs e)
	{
		if (this.Filter.Actor == null)
			return;

		ItemUtility.EquipRacialGear(this.Filter.Actor, this.Filter.Slot);
	}

	private void OnNpcSmallclothesClicked(object sender, RoutedEventArgs e)
	{
		if (this.Filter.Actor == null)
			return;

		ItemUtility.EquipNpcSmallclothes(this.Filter.Actor, this.Filter.Slot);
	}

	private void OnClearClicked(object? sender = null, RoutedEventArgs? e = null)
	{
		if (this.Filter.Actor == null)
			return;

		ItemUtility.Clear(this.Filter.Actor, this.Filter.Slot);
	}

	private void OnSelectionChanged(bool close)
	{
		if (this.Filter.Actor == null)
			return;

		IItem? item = this.Selector.Value as IItem;
		this.Equip(item);
	}

	private void Equip(IItem? item)
	{
		IEquipmentItemMemory? memory;

		if (this.Filter.Actor == null)
			return;

		if (this.Filter.Slot == ItemSlots.MainHand)
		{
			memory = this.Filter.Actor.MainHand;
		}
		else if (this.Filter.Slot == ItemSlots.OffHand)
		{
			memory = this.Filter.Actor.OffHand;
		}
		else
		{
			memory = this.Filter.Actor.Equipment?.GetSlot(this.Filter.Slot);
		}

		if (memory != null)
		{
			if (item == null)
			{
				ItemUtility.Clear(this.Filter.Actor, this.Filter.Slot);
			}
			else
			{
				memory.Equip(item);
			}
		}
	}

	public class EquipmentFilter : TagFilterBase<IItem>
	{
		public bool IncludeUnequipableItems { get; set; } = true;
		public SortModes SortMode { get; set; } = SortModes.Row;
		public ItemSlots Slot { get; set; }
		public ActorMemory? Actor { get; set; }

		public override int CompareItems(IItem itemA, IItem itemB)
		{
			if (itemA.IsFavorite && !itemB.IsFavorite)
			{
				return -1;
			}
			else if (!itemA.IsFavorite && itemB.IsFavorite)
			{
				return 1;
			}

			// Push the Emperor's New Fists to the top of the list for weapons.
			if (this.Slot == ItemSlots.MainHand || this.Slot == ItemSlots.OffHand)
			{
				if (itemA == ItemUtility.EmperorsNewFists && itemB != ItemUtility.EmperorsNewFists)
				{
					return -1;
				}
				else if (itemA != ItemUtility.EmperorsNewFists && itemB == ItemUtility.EmperorsNewFists)
				{
					return 1;
				}
			}

			switch (this.SortMode)
			{
				case SortModes.Name: return itemA.Name.CompareTo(itemB.Name);
				case SortModes.NameInv: return -itemA.Name.CompareTo(itemB.Name);
				case SortModes.Row: return itemA.RowId.CompareTo(itemB.RowId);
				case SortModes.RowInv: return -itemA.RowId.CompareTo(itemB.RowId);
				case SortModes.Level: return itemA.EquipLevel.CompareTo(itemB.EquipLevel);
				case SortModes.LevelInv: return -itemA.EquipLevel.CompareTo(itemB.EquipLevel);
			}

			throw new NotImplementedException($"Sort mode {this.SortMode} not implemented");
		}

		public override bool FilterItem(IItem item)
		{
			bool actorIsChocobo = this.Actor != null && this.Actor.IsChocobo;

			bool itemIsForChocobo = item is ChocoboSkinItem
				|| item is BuddyEquip.BuddyItem
				|| item is BuddyEquip
				|| item is DummyNoneItem
				|| (item is DummyItem di && di.IsChocoboItem);

			if (actorIsChocobo != itemIsForChocobo)
				return false;

			// skip items without names
			////if (string.IsNullOrEmpty(item.Name))
			////	return false;

			if (!item.FitsInSlot(this.Slot))
				return false;

			if (!this.IncludeUnequipableItems && item is Item ivm && !this.CanEquip(ivm))
				return false;

			return true;
		}

		private bool CanEquip(Item item)
		{
			if (item.EquipRestriction == null || this.Actor == null || this.Actor.Customize == null)
				return true;

			return item.EquipRestriction!.CanEquip(this.Actor.Customize.RaceId, this.Actor.Customize.Gender);
		}
	}
}
