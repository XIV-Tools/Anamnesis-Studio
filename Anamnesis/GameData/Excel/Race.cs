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
	public int RSEMBody { get; private set; } = 0;
	public int RSEMHands { get; private set; } = 0;
	public int RSEMLegs { get; private set; } = 0;
	public int RSEMFeet { get; private set; } = 0;
	public int RSEFBody { get; private set; } = 0;
	public int RSEFHands { get; private set; } = 0;
	public int RSEFLegs { get; private set; } = 0;
	public int RSEFFeet { get; private set; } = 0;

	// Customize options
	public List<Tribe> Tribes { get; private set; } = new();
	public List<CustomizeGenders> Genders { get; private set; } = new();

	public override void PopulateData(RowParser parser, Lumina.GameData gameData, Language language)
	{
		base.PopulateData(parser, gameData, language);

		this.Masculine = parser.ReadColumn<SeString>(0) ?? string.Empty;
		this.Feminine = parser.ReadColumn<SeString>(1) ?? string.Empty;

		this.RSEMBody = parser.ReadColumn<int>(2);
		this.RSEMHands = parser.ReadColumn<int>(3);
		this.RSEMLegs = parser.ReadColumn<int>(4);
		this.RSEMFeet = parser.ReadColumn<int>(5);
		this.RSEFBody = parser.ReadColumn<int>(6);
		this.RSEFHands = parser.ReadColumn<int>(7);
		this.RSEFLegs = parser.ReadColumn<int>(8);
		this.RSEFFeet = parser.ReadColumn<int>(9);

		if (!Enum.IsDefined<ActorCustomizeMemory.Races>(this.RaceId))
			return;

		if (GameDataService.Instance.Tribes == null)
			throw new Exception("No Tribes list in game data service");

		this.Tribes = this.RaceId switch
		{
			ActorCustomizeMemory.Races.Hyur => new()
			{
				GameDataService.Instance.Tribes.Get((byte)ActorCustomizeMemory.Tribes.Midlander),
				GameDataService.Instance.Tribes.Get((byte)ActorCustomizeMemory.Tribes.Highlander),
			},

			ActorCustomizeMemory.Races.Elezen => new()
			{
				GameDataService.Instance.Tribes.Get((byte)ActorCustomizeMemory.Tribes.Wildwood),
				GameDataService.Instance.Tribes.Get((byte)ActorCustomizeMemory.Tribes.Duskwight),
			},

			ActorCustomizeMemory.Races.Lalafel => new()
			{
				GameDataService.Instance.Tribes.Get((byte)ActorCustomizeMemory.Tribes.Plainsfolk),
				GameDataService.Instance.Tribes.Get((byte)ActorCustomizeMemory.Tribes.Dunesfolk),
			},

			ActorCustomizeMemory.Races.Miqote => new()
			{
				GameDataService.Instance.Tribes.Get((byte)ActorCustomizeMemory.Tribes.SeekerOfTheSun),
				GameDataService.Instance.Tribes.Get((byte)ActorCustomizeMemory.Tribes.KeeperOfTheMoon),
			},

			ActorCustomizeMemory.Races.Roegadyn => new()
			{
				GameDataService.Instance.Tribes.Get((byte)ActorCustomizeMemory.Tribes.SeaWolf),
				GameDataService.Instance.Tribes.Get((byte)ActorCustomizeMemory.Tribes.Hellsguard),
			},

			ActorCustomizeMemory.Races.AuRa => new()
			{
				GameDataService.Instance.Tribes.Get((byte)ActorCustomizeMemory.Tribes.Raen),
				GameDataService.Instance.Tribes.Get((byte)ActorCustomizeMemory.Tribes.Xaela),
			},

			ActorCustomizeMemory.Races.Hrothgar => new()
			{
				GameDataService.Instance.Tribes.Get((byte)ActorCustomizeMemory.Tribes.Helions),
				GameDataService.Instance.Tribes.Get((byte)ActorCustomizeMemory.Tribes.TheLost),
			},

			ActorCustomizeMemory.Races.Viera => new()
			{
				GameDataService.Instance.Tribes.Get((byte)ActorCustomizeMemory.Tribes.Rava),
				GameDataService.Instance.Tribes.Get((byte)ActorCustomizeMemory.Tribes.Veena),
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
}
