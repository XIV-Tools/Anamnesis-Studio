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

public partial class EquipmentSelector : UserControl
{
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

	public EquipmentFilter Filter { get; init; } = new();
	public TagCollection AllTags { get; init; } = new();

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

		this.AllTags.Clear();
		foreach (IItem item in this.Selector.Entries)
		{
			this.AllTags.AddRange(item.Tags);
		}

		return Task.CompletedTask;
	}

	private void ClearSlot()
	{
		this.OnClearClicked();
	}

	private void OnClearClicked(object? sender = null, RoutedEventArgs? e = null)
	{
		/*if (this.IsMainHandSlot)
		{
			this.Value = ItemUtility.EmperorsNewFists;
		}
		else
		{
			this.Value = ItemUtility.NoneItem;
		}

		this.RaiseSelectionChanged();*/
	}

	private void OnNpcSmallclothesClicked(object sender, RoutedEventArgs e)
	{
		/*if (this.IsSmallclothesSlot)
		{
			this.Value = ItemUtility.NpcBodyItem;
		}
		else
		{
			this.Value = ItemUtility.NoneItem;
		}

		this.RaiseSelectionChanged();*/
	}

	public class EquipmentFilter : TagFilterBase<IItem>
	{
		public bool IncludeUnequipableItems { get; set; } = true;
		public SortModes SortMode { get; set; } = SortModes.Row;
		public ItemSlots? Slot { get; set; }
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
			if (string.IsNullOrEmpty(item.Name))
				return false;

			if (this.Slot != null && !item.FitsInSlot((ItemSlots)this.Slot))
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
