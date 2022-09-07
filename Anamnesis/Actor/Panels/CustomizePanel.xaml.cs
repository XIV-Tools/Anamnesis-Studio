// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Panels;

using Anamnesis.Actor.Utilities;
using Anamnesis.GameData.Excel;
using Anamnesis.Memory;
using Anamnesis.Panels;
using Anamnesis.Services;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Data;

public partial class CustomizePanel : ActorPanelBase
{
	public CustomizePanel(IPanelHost host)
		: base(host)
	{
		this.InitializeComponent();
		this.ContentArea.DataContext = this;
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

		if (this.Actor.Customize?.RaceId == null)
			return;

		var race = GameDataService.Instance.Races.GetRow((uint)this.Actor.Customize.RaceId);

		if (race == null)
			return;

		if (this.Actor.Customize.Gender == ActorCustomizeMemory.Genders.Masculine)
		{
			var body = GameDataService.Instance.Items.Get((uint)race.RSEMBody);
			var hands = GameDataService.Instance.Items.Get((uint)race.RSEMHands);
			var legs = GameDataService.Instance.Items.Get((uint)race.RSEMLegs);
			var feet = GameDataService.Instance.Items.Get((uint)race.RSEMFeet);

			this.Actor.Equipment?.Chest?.Equip(body);
			this.Actor.Equipment?.Arms?.Equip(hands);
			this.Actor.Equipment?.Legs?.Equip(legs);
			this.Actor.Equipment?.Feet?.Equip(feet);
		}
		else
		{
			var body = GameDataService.Instance.Items.Get((uint)race.RSEFBody);
			var hands = GameDataService.Instance.Items.Get((uint)race.RSEFHands);
			var legs = GameDataService.Instance.Items.Get((uint)race.RSEFLegs);
			var feet = GameDataService.Instance.Items.Get((uint)race.RSEFFeet);

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

	/*private async void OnResetClicked(object sender, RoutedEventArgs e)
{
	if (this.Actor?.OriginalCharacterBackup == null)
		return;

	if (await GenericDialog.ShowLocalizedAsync("Character_Reset_Confirm", "Character_Reset", MessageBoxButton.YesNo) != true)
		return;

	await this.Actor.RestoreCharacterBackup(PinnedActor.BackupModes.Original);
}*/

	private ListCollectionView GenerateVoiceList()
	{
		List<VoiceEntry> entries = new();
		foreach (var makeType in GameDataService.Instance.CharacterMakeTypes)
		{
			if (makeType == null)
				continue;

			if (makeType.Tribe == 0)
				continue;

			Tribe? tribe = GameDataService.Instance.Tribes.GetRow((uint)makeType.Tribe);

			if (tribe == null)
				continue;

			int voiceCount = makeType.Voices!.Length;
			for (int i = 0; i < voiceCount; i++)
			{
				byte voiceId = makeType.Voices[i]!;
				VoiceEntry entry = new();
				entry.VoiceName = $"Voice #{i + 1} ({voiceId})";
				entry.VoiceCategory = $"{makeType.Race}, {tribe.Masculine} ({makeType.Gender})";
				entry.VoiceId = voiceId;
				entries.Add(entry);
			}
		}

		ListCollectionView voices = new ListCollectionView(entries);
		voices.GroupDescriptions.Add(new PropertyGroupDescription("VoiceCategory"));
		return voices;
	}

	public class VoiceEntry
	{
		public byte VoiceId { get; set; }
		public string VoiceName { get; set; } = string.Empty;
		public string VoiceCategory { get; set; } = string.Empty;
	}
}
