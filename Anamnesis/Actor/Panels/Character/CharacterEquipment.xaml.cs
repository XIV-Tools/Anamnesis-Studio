// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Panels.Character;

using System;
using System.Windows;
using System.Windows.Controls;
using Anamnesis.Actor.Panels.Character.Equipment;
using Anamnesis.Actor.Utilities;
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

	public void EditSlot(ItemView? view)
	{
		if (this.editingSlotView == view)
			return;

		if (this.editingSlotView != null)
		{
			this.editingSlotView.IsPopupOpen = false;
			this.SlotEditorPopup.IsOpen = false;
		}

		this.editingSlotView = view;

		if (this.editingSlotView != null)
		{
			this.editingSlotView.IsPopupOpen = true;
			this.SlotEditorPopup.IsOpen = true;
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

		this.Actor.MainHand?.Clear(this.Actor.IsHuman);
		this.Actor.OffHand?.Clear(this.Actor.IsHuman);
		this.Actor.Equipment?.Arms?.Clear(this.Actor.IsHuman);
		this.Actor.Equipment?.Chest?.Clear(this.Actor.IsHuman);
		this.Actor.Equipment?.Ear?.Clear(this.Actor.IsHuman);
		this.Actor.Equipment?.Feet?.Clear(this.Actor.IsHuman);
		this.Actor.Equipment?.Head?.Clear(this.Actor.IsHuman);
		this.Actor.Equipment?.Legs?.Clear(this.Actor.IsHuman);
		this.Actor.Equipment?.LFinger?.Clear(this.Actor.IsHuman);
		this.Actor.Equipment?.Neck?.Clear(this.Actor.IsHuman);
		this.Actor.Equipment?.RFinger?.Clear(this.Actor.IsHuman);
		this.Actor.Equipment?.Wrist?.Clear(this.Actor.IsHuman);

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

		this.Actor.Equipment?.Ear?.Clear(this.Actor.IsHuman);
		this.Actor.Equipment?.Head?.Clear(this.Actor.IsHuman);
		this.Actor.Equipment?.LFinger?.Clear(this.Actor.IsHuman);
		this.Actor.Equipment?.Neck?.Clear(this.Actor.IsHuman);
		this.Actor.Equipment?.RFinger?.Clear(this.Actor.IsHuman);
		this.Actor.Equipment?.Wrist?.Clear(this.Actor.IsHuman);
		this.Actor.Equipment?.Arms?.Equip(ItemUtility.NpcBodyItem);
		this.Actor.Equipment?.Chest?.Equip(ItemUtility.NpcBodyItem);
		this.Actor.Equipment?.Legs?.Equip(ItemUtility.NpcBodyItem);
		this.Actor.Equipment?.Feet?.Equip(ItemUtility.NpcBodyItem);
	}

	private void OnRaceGearClicked(object sender, RoutedEventArgs e)
	{
		if (this.Actor == null)
			return;

		if (this.Actor.Customize?.Race == null)
			return;

		Race? race = this.Actor.Customize.Race;

		if (race == null)
			return;

		if (this.Actor.Customize.Gender == ActorCustomizeMemory.Genders.Masculine)
		{
			var body = App.Services.GameData.Items.Get((uint)race.RSEMBody);
			var hands = App.Services.GameData.Items.Get((uint)race.RSEMHands);
			var legs = App.Services.GameData.Items.Get((uint)race.RSEMLegs);
			var feet = App.Services.GameData.Items.Get((uint)race.RSEMFeet);

			this.Actor.Equipment?.Chest?.Equip(body);
			this.Actor.Equipment?.Arms?.Equip(hands);
			this.Actor.Equipment?.Legs?.Equip(legs);
			this.Actor.Equipment?.Feet?.Equip(feet);
		}
		else
		{
			var body = App.Services.GameData.Items.Get((uint)race.RSEFBody);
			var hands = App.Services.GameData.Items.Get((uint)race.RSEFHands);
			var legs = App.Services.GameData.Items.Get((uint)race.RSEFLegs);
			var feet = App.Services.GameData.Items.Get((uint)race.RSEFFeet);

			this.Actor.Equipment?.Chest?.Equip(body);
			this.Actor.Equipment?.Arms?.Equip(hands);
			this.Actor.Equipment?.Legs?.Equip(legs);
			this.Actor.Equipment?.Feet?.Equip(feet);
		}

		this.Actor.Equipment?.Ear?.Clear(this.Actor.IsHuman);
		this.Actor.Equipment?.Head?.Clear(this.Actor.IsHuman);
		this.Actor.Equipment?.LFinger?.Clear(this.Actor.IsHuman);
		this.Actor.Equipment?.Neck?.Clear(this.Actor.IsHuman);
		this.Actor.Equipment?.RFinger?.Clear(this.Actor.IsHuman);
		this.Actor.Equipment?.Wrist?.Clear(this.Actor.IsHuman);
	}
}
