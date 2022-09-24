// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Panels.Character;

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Anamnesis.Actor.Panels.Character.Equipment;
using Anamnesis.Actor.Utilities;
using Anamnesis.GameData;
using Anamnesis.GameData.Excel;
using Anamnesis.Memory;
using Anamnesis.Services;
using PropertyChanged;

[AddINotifyPropertyChangedInterface]
public partial class CharacterEquipment : UserControl
{
	private ItemView? editingSlotView;

	public CharacterEquipment()
	{
		this.InitializeComponent();
	}

	private ActorMemory? Actor
	{
		get
		{
			if (this.DataContext is CharacterPanel characterPanel)
				return characterPanel.Actor;

			return null;
		}
	}

	public void EditSlot(ItemSlots slot)
	{
		List<ItemView> itemViews = this.FindChildren<ItemView>();
		foreach (ItemView view in itemViews)
		{
			view.IsPopupOpen = view.Slot == slot;
		}
	}

	public void EditSlot(ItemView? view)
	{
		if (this.editingSlotView == view)
			return;

		if (this.editingSlotView != null)
		{
			this.editingSlotView.IsPopupOpen = false;
		}

		this.editingSlotView = view;
		this.SlotEditorPopup.IsOpen = view != null;

		if (this.editingSlotView != null)
		{
			this.EquipmentSelector.EquipmentEditor = this;
			this.EquipmentSelector.Filter.Actor = this.Actor;
			this.EquipmentSelector.Filter.Slot = this.editingSlotView.Slot;

			this.editingSlotView.IsPopupOpen = true;
		}
	}

	private void OnSlotEditorPopupClosed(object sender, EventArgs e)
	{
		this.EditSlot(null);
	}

	private void OnClearClicked(object? sender = null, RoutedEventArgs? e = null)
	{
		if (this.Actor == null)
			return;

		ItemUtility.Clear(this.Actor, ItemSlots.MainHand);
		ItemUtility.Clear(this.Actor, ItemSlots.Head);
		ItemUtility.Clear(this.Actor, ItemSlots.Body);
		ItemUtility.Clear(this.Actor, ItemSlots.Hands);
		ItemUtility.Clear(this.Actor, ItemSlots.Legs);
		ItemUtility.Clear(this.Actor, ItemSlots.Feet);
		ItemUtility.Clear(this.Actor, ItemSlots.OffHand);
		ItemUtility.Clear(this.Actor, ItemSlots.Neck);
		ItemUtility.Clear(this.Actor, ItemSlots.Wrists);
		ItemUtility.Clear(this.Actor, ItemSlots.RightRing);
		ItemUtility.Clear(this.Actor, ItemSlots.LeftRing);

		this.Actor?.ModelObject?.Weapons?.Hide();
		this.Actor?.ModelObject?.Weapons?.SubModel?.Hide();
	}

	private void OnNpcSmallclothesClicked(object sender, RoutedEventArgs e)
	{
		if (this.Actor == null)
			return;

		if (!this.Actor.IsHuman)
		{
			this.OnClearClicked(sender, e);
			return;
		}

		this.Actor.Equipment?.Hands?.Equip(ItemUtility.NpcBodyItem);
		this.Actor.Equipment?.Body?.Equip(ItemUtility.NpcBodyItem);
		this.Actor.Equipment?.Legs?.Equip(ItemUtility.NpcBodyItem);
		this.Actor.Equipment?.Feet?.Equip(ItemUtility.NpcBodyItem);

		ItemUtility.Clear(this.Actor, ItemSlots.Ears);
		ItemUtility.Clear(this.Actor, ItemSlots.Head);
		ItemUtility.Clear(this.Actor, ItemSlots.LeftRing);
		ItemUtility.Clear(this.Actor, ItemSlots.RightRing);
		ItemUtility.Clear(this.Actor, ItemSlots.Neck);
		ItemUtility.Clear(this.Actor, ItemSlots.Wrists);
	}

	private void OnRaceGearClicked(object sender, RoutedEventArgs e)
	{
		if (this.Actor == null)
			return;

		ItemUtility.EquipRacialGear(this.Actor, ItemSlots.Body);
		ItemUtility.EquipRacialGear(this.Actor, ItemSlots.Hands);
		ItemUtility.EquipRacialGear(this.Actor, ItemSlots.Legs);
		ItemUtility.EquipRacialGear(this.Actor, ItemSlots.Feet);

		ItemUtility.Clear(this.Actor, ItemSlots.Ears);
		ItemUtility.Clear(this.Actor, ItemSlots.Head);
		ItemUtility.Clear(this.Actor, ItemSlots.LeftRing);
		ItemUtility.Clear(this.Actor, ItemSlots.RightRing);
		ItemUtility.Clear(this.Actor, ItemSlots.Neck);
		ItemUtility.Clear(this.Actor, ItemSlots.Wrists);
	}
}
