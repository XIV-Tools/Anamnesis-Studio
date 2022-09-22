// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Panels.Character.Equipment;

using Anamnesis.Actor.Utilities;
using Anamnesis.GameData;
using System.Threading.Tasks;
using System.Windows;
using System;
using System.Windows.Controls;
using XivToolsWpf;
using Anamnesis.GameData.Excel;
using Anamnesis.Memory;
using XivToolsWpf.Selectors;

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
		Row,
		Level,
	}

	public EquipmentFilter Filter { get; init; } = new();

	protected Task LoadItems()
	{
		/*if (this.actor?.IsChocobo == true)
		{
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
		}
		else
		{*/
		this.Selector.AddItem(ItemUtility.NoneItem);
		this.Selector.AddItem(ItemUtility.NpcBodyItem);
		this.Selector.AddItem(ItemUtility.InvisibileBodyItem);
		this.Selector.AddItem(ItemUtility.InvisibileHeadItem);
		this.Selector.AddItems(App.Services.GameData.Equipment);
		this.Selector.AddItems(App.Services.GameData.Items);
		this.Selector.AddItems(App.Services.GameData.Perform);
		////}

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

	public class EquipmentFilter : Selector.FilterBase<IItem>
	{
		public Classes Class { get; set; } = Classes.All;
		public ItemCategories Category { get; set; } = ItemCategories.All;
		public bool ShowLocked { get; set; } = true;
		public bool AutoOffhand { get; set; } = true;
		public bool ForceMainModel { get; set; } = false;
		public bool ForceOffModel { get; set; } = false;
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
				case SortModes.Row: return itemA.RowId.CompareTo(itemB.RowId);
				case SortModes.Level: return itemA.EquipLevel.CompareTo(itemB.EquipLevel);
			}

			throw new NotImplementedException($"Sort mode {this.SortMode} not implemented");
		}

		public override bool FilterItem(IItem item, string[]? search)
		{
			// skip items without names
			if (string.IsNullOrEmpty(item.Name))
				return false;

			if (this.Slot == ItemSlots.MainHand || this.Slot == ItemSlots.OffHand)
			{
				////if (this.Slot == ItemSlots.OffHand && !forceMainModel && !item.HasSubModel)
				////	return false;

				if (!item.IsWeapon)
					return false;
			}
			else
			{
				if (!item.FitsInSlot(this.Slot))
					return false;
			}

			if (!this.HasClass(this.Class, item.EquipableClasses))
				return false;

			if (!this.ValidCategory(item))
				return false;

			if (!this.ShowLocked && item is Item ivm && !this.CanEquip(ivm))
				return false;

			return this.MatchesSearch(item, search);
		}

		private bool MatchesSearch(IItem item, string[]? search = null)
		{
			bool matches = false;

			matches |= SearchUtility.Matches(item.Name, search);
			matches |= SearchUtility.Matches(item.Description, search);
			matches |= SearchUtility.Matches(item.ModelSet.ToString(), search);
			matches |= SearchUtility.Matches(item.ModelBase.ToString(), search);
			matches |= SearchUtility.Matches(item.ModelVariant.ToString(), search);

			if (item.HasSubModel)
			{
				matches |= SearchUtility.Matches(item.SubModelSet.ToString(), search);
				matches |= SearchUtility.Matches(item.SubModelBase.ToString(), search);
				matches |= SearchUtility.Matches(item.SubModelVariant.ToString(), search);
			}

			matches |= SearchUtility.Matches(item.RowId.ToString(), search);

			if (item.Mod != null && item.Mod.ModPack != null)
			{
				matches |= SearchUtility.Matches(item.Mod.ModPack.Name, search);
			}

			return matches;
		}

		private bool ValidCategory(IItem item)
		{
			ItemCategories itemCategory = item.Category;

			// Include none category
			bool categoryFiltered = this.Category.HasFlag(ItemCategories.Standard) && itemCategory == ItemCategories.None;

			categoryFiltered |= this.Category.HasFlag(ItemCategories.Standard) && itemCategory.HasFlag(ItemCategories.Standard);
			categoryFiltered |= this.Category.HasFlag(ItemCategories.Premium) && itemCategory.HasFlag(ItemCategories.Premium);
			categoryFiltered |= this.Category.HasFlag(ItemCategories.Limited) && itemCategory.HasFlag(ItemCategories.Limited);
			categoryFiltered |= this.Category.HasFlag(ItemCategories.Deprecated) && itemCategory.HasFlag(ItemCategories.Deprecated);
			categoryFiltered |= this.Category.HasFlag(ItemCategories.CustomEquipment) && itemCategory.HasFlag(ItemCategories.CustomEquipment);
			categoryFiltered |= this.Category.HasFlag(ItemCategories.Performance) && itemCategory.HasFlag(ItemCategories.Performance);
			categoryFiltered |= this.Category.HasFlag(ItemCategories.Modded) && item.Mod != null;
			categoryFiltered |= this.Category.HasFlag(ItemCategories.Favorites) && item.IsFavorite;
			categoryFiltered |= this.Category.HasFlag(ItemCategories.Owned) && item.IsOwned;
			return categoryFiltered;
		}

		private bool CanEquip(Item item)
		{
			if (item.EquipRestriction == null || this.Actor == null || this.Actor.Customize == null)
				return true;

			return item.EquipRestriction!.CanEquip(this.Actor.Customize.RaceId, this.Actor.Customize.Gender);
		}

		private bool HasClass(Classes a, Classes b)
		{
			foreach (Classes? job in Enum.GetValues(typeof(Classes)))
			{
				if (job == null || job == Classes.None)
					continue;

				if (a.HasFlag(job) && b.HasFlag(job))
				{
					return true;
				}
			}

			return false;
		}
	}
}
