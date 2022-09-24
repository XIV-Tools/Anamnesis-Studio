// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Excel;

using System;
using Anamnesis.Memory;
using Anamnesis.Services;
using Lumina.Data;
using Lumina.Excel;
using Lumina.Text;
using System.Collections.Generic;

using ExcelRow = Anamnesis.GameData.Sheets.ExcelRow;
using CustomizeRaces = Anamnesis.Memory.ActorCustomizeMemory.Races;
using CustomizeGenders = Anamnesis.Memory.ActorCustomizeMemory.Genders;

[Sheet("Race", 0x3403807a)]
public class Race : ExcelRow
{
	public CustomizeRaces RaceId => (CustomizeRaces)this.RowId;
	public string Name => this.RaceId.ToString();
	public string DisplayName => this.Masculine;

	public string Feminine { get; private set; } = string.Empty;
	public string Masculine { get; private set; } = string.Empty;

	public IItem RacialGearMasculineBody { get; private set; } = null!;
	public IItem RacialGearMasculineHands { get; private set; } = null!;
	public IItem RacialGearMasculineLegs { get; private set; } = null!;
	public IItem RacialGearMasculineFeet { get; private set; } = null!;
	public IItem RacialGearFeminineBody { get; private set; } = null!;
	public IItem RacialGearFeminineHands { get; private set; } = null!;
	public IItem RacialGearFeminineLegs { get; private set; } = null!;
	public IItem RacialGearFeminineFeet { get; private set; } = null!;

	// Customize options
	public List<Tribe> Tribes { get; private set; } = new();
	public List<CustomizeGenders> Genders { get; private set; } = new();

	public override void PopulateData(RowParser parser, Lumina.GameData gameData, Language language)
	{
		base.PopulateData(parser, gameData, language);

		this.Masculine = parser.ReadColumn<SeString>(0) ?? string.Empty;
		this.Feminine = parser.ReadColumn<SeString>(1) ?? string.Empty;

		this.RacialGearMasculineBody = GameData.Items.Get(parser.ReadColumn<int>(2));
		this.RacialGearMasculineHands = GameData.Items.Get(parser.ReadColumn<int>(3));
		this.RacialGearMasculineLegs = GameData.Items.Get(parser.ReadColumn<int>(4));
		this.RacialGearMasculineFeet = GameData.Items.Get(parser.ReadColumn<int>(5));
		this.RacialGearFeminineBody = GameData.Items.Get(parser.ReadColumn<int>(6));
		this.RacialGearFeminineHands = GameData.Items.Get(parser.ReadColumn<int>(7));
		this.RacialGearFeminineLegs = GameData.Items.Get(parser.ReadColumn<int>(8));
		this.RacialGearFeminineFeet = GameData.Items.Get(parser.ReadColumn<int>(9));

		if (!Enum.IsDefined<ActorCustomizeMemory.Races>(this.RaceId))
			return;

		if (GameData.Tribes == null)
			throw new Exception("No Tribes list in game data service");

		this.Tribes = this.RaceId switch
		{
			ActorCustomizeMemory.Races.Hyur => new()
			{
				GameData.Tribes.Get((byte)ActorCustomizeMemory.Tribes.Midlander),
				GameData.Tribes.Get((byte)ActorCustomizeMemory.Tribes.Highlander),
			},

			ActorCustomizeMemory.Races.Elezen => new()
			{
				GameData.Tribes.Get((byte)ActorCustomizeMemory.Tribes.Wildwood),
				GameData.Tribes.Get((byte)ActorCustomizeMemory.Tribes.Duskwight),
			},

			ActorCustomizeMemory.Races.Lalafel => new()
			{
				GameData.Tribes.Get((byte)ActorCustomizeMemory.Tribes.Plainsfolk),
				GameData.Tribes.Get((byte)ActorCustomizeMemory.Tribes.Dunesfolk),
			},

			ActorCustomizeMemory.Races.Miqote => new()
			{
				GameData.Tribes.Get((byte)ActorCustomizeMemory.Tribes.SeekerOfTheSun),
				GameData.Tribes.Get((byte)ActorCustomizeMemory.Tribes.KeeperOfTheMoon),
			},

			ActorCustomizeMemory.Races.Roegadyn => new()
			{
				GameData.Tribes.Get((byte)ActorCustomizeMemory.Tribes.SeaWolf),
				GameData.Tribes.Get((byte)ActorCustomizeMemory.Tribes.Hellsguard),
			},

			ActorCustomizeMemory.Races.AuRa => new()
			{
				GameData.Tribes.Get((byte)ActorCustomizeMemory.Tribes.Raen),
				GameData.Tribes.Get((byte)ActorCustomizeMemory.Tribes.Xaela),
			},

			ActorCustomizeMemory.Races.Hrothgar => new()
			{
				GameData.Tribes.Get((byte)ActorCustomizeMemory.Tribes.Helions),
				GameData.Tribes.Get((byte)ActorCustomizeMemory.Tribes.TheLost),
			},

			ActorCustomizeMemory.Races.Viera => new()
			{
				GameData.Tribes.Get((byte)ActorCustomizeMemory.Tribes.Rava),
				GameData.Tribes.Get((byte)ActorCustomizeMemory.Tribes.Veena),
			},

			_ => throw new Exception($"Unrecognized race Id: {this.RaceId}"),
		};

		this.Genders.Clear();
		if (this.RaceId == CustomizeRaces.Hrothgar)
		{
			this.Genders.Add(CustomizeGenders.Masculine);
		}
		else
		{
			this.Genders.Add(CustomizeGenders.Masculine);
			this.Genders.Add(CustomizeGenders.Feminine);
		}
	}

	public IItem? GetRacialGear(CustomizeGenders gender, ItemSlots slot)
	{
		switch (slot)
		{
			case ItemSlots.Body: return gender == CustomizeGenders.Masculine ? this.RacialGearMasculineBody : this.RacialGearFeminineBody;
			case ItemSlots.Hands: return gender == CustomizeGenders.Masculine ? this.RacialGearMasculineHands : this.RacialGearFeminineHands;
			case ItemSlots.Legs: return gender == CustomizeGenders.Masculine ? this.RacialGearMasculineLegs : this.RacialGearFeminineLegs;
			case ItemSlots.Feet: return gender == CustomizeGenders.Masculine ? this.RacialGearMasculineFeet : this.RacialGearFeminineFeet;
		}

		return null;
	}
}
