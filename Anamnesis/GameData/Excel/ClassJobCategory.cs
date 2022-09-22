// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Excel;

using Anamnesis.GameData.Sheets;
using Anamnesis.Tags;
using Lumina.Data;
using Lumina.Excel;

using ExcelRow = Anamnesis.GameData.Sheets.ExcelRow;

[Sheet("ClassJobCategory", 2091841742u)]
public class ClassJobCategory : ExcelRow
{
	public string? Name { get; set; }

	public bool Gladiator { get; set; }
	public bool Pugilist { get; set; }
	public bool Marauder { get; set; }
	public bool Lancer { get; set; }
	public bool Archer { get; set; }
	public bool Conjurer { get; set; }
	public bool Thaumaturge { get; set; }
	public bool Carpenter { get; set; }
	public bool Blacksmith { get; set; }
	public bool Armorer { get; set; }
	public bool Goldsmith { get; set; }
	public bool Leatherworker { get; set; }
	public bool Weaver { get; set; }
	public bool Alchemist { get; set; }
	public bool Culinarian { get; set; }
	public bool Miner { get; set; }
	public bool Botanist { get; set; }
	public bool Fisher { get; set; }
	public bool Paladin { get; set; }
	public bool Monk { get; set; }
	public bool Warrior { get; set; }
	public bool Dragoon { get; set; }
	public bool Bard { get; set; }
	public bool WhiteMage { get; set; }
	public bool BlackMage { get; set; }
	public bool Arcanist { get; set; }
	public bool Summoner { get; set; }
	public bool Scholar { get; set; }
	public bool Rogue { get; set; }
	public bool Ninja { get; set; }
	public bool Machinist { get; set; }
	public bool DarkKnight { get; set; }
	public bool Astrologian { get; set; }
	public bool Samurai { get; set; }
	public bool RedMage { get; set; }
	public bool BlueMage { get; set; }
	public bool Gunbreaker { get; set; }
	public bool Dancer { get; set; }
	public bool Reaper { get; set; }
	public bool Sage { get; set; }

	public override void PopulateData(RowParser parser, Lumina.GameData gameData, Language language)
	{
		base.PopulateData(parser, gameData, language);
		this.Name = parser.ReadString(0);

		////ADV = ((parser.ReadColumn<bool>(1) ? ((byte)1) : ((byte)0)) != 0);

		this.Gladiator = parser.ReadColumn<bool>(2);
		this.Pugilist = parser.ReadColumn<bool>(3);
		this.Marauder = parser.ReadColumn<bool>(4);
		this.Lancer = parser.ReadColumn<bool>(5);
		this.Archer = parser.ReadColumn<bool>(6);
		this.Conjurer = parser.ReadColumn<bool>(7);
		this.Thaumaturge = parser.ReadColumn<bool>(8);
		this.Carpenter = parser.ReadColumn<bool>(9);
		this.Blacksmith = parser.ReadColumn<bool>(10);
		this.Armorer = parser.ReadColumn<bool>(11);
		this.Goldsmith = parser.ReadColumn<bool>(12);
		this.Leatherworker = parser.ReadColumn<bool>(13);
		this.Weaver = parser.ReadColumn<bool>(14);
		this.Alchemist = parser.ReadColumn<bool>(15);
		this.Culinarian = parser.ReadColumn<bool>(16);
		this.Miner = parser.ReadColumn<bool>(17);
		this.Botanist = parser.ReadColumn<bool>(18);
		this.Fisher = parser.ReadColumn<bool>(19);
		this.Paladin = parser.ReadColumn<bool>(20);
		this.Monk = parser.ReadColumn<bool>(21);
		this.Warrior = parser.ReadColumn<bool>(22);
		this.Dragoon = parser.ReadColumn<bool>(23);
		this.Bard = parser.ReadColumn<bool>(24);
		this.WhiteMage = parser.ReadColumn<bool>(25);
		this.BlackMage = parser.ReadColumn<bool>(26);
		this.Arcanist = parser.ReadColumn<bool>(27);
		this.Summoner = parser.ReadColumn<bool>(28);
		this.Scholar = parser.ReadColumn<bool>(29);
		this.Rogue = parser.ReadColumn<bool>(30);
		this.Ninja = parser.ReadColumn<bool>(31);
		this.Machinist = parser.ReadColumn<bool>(32);
		this.DarkKnight = parser.ReadColumn<bool>(33);
		this.Astrologian = parser.ReadColumn<bool>(34);
		this.Samurai = parser.ReadColumn<bool>(35);
		this.RedMage = parser.ReadColumn<bool>(36);
		this.BlueMage = parser.ReadColumn<bool>(37);
		this.Gunbreaker = parser.ReadColumn<bool>(38);
		this.Dancer = parser.ReadColumn<bool>(39);

		// might be backwards:
		this.Reaper = parser.ReadColumn<bool>(40);
		this.Sage = parser.ReadColumn<bool>(41);
	}

	public TagCollection ToTags()
	{
		TagCollection tags = new();

		if (this.Gladiator)
		{
			tags.Add("Gladiator");
			tags.Add("Tank");
			tags.Add("Class");
		}

		if (this.Pugilist)
		{
			tags.Add("Pugilist");
			tags.Add("Damage");
			tags.Add("Class");
			tags.Add("Melee");
		}

		if (this.Marauder)
		{
			tags.Add("Marauder");
			tags.Add("Tank");
			tags.Add("Class");
		}

		if (this.Lancer)
		{
			tags.Add("Lancer");
			tags.Add("Damage");
			tags.Add("Class");
			tags.Add("Melee");
		}

		if (this.Archer)
		{
			tags.Add("Archer");
			tags.Add("Damage");
			tags.Add("Class");
			tags.Add("Physical Ranged");
		}

		if (this.Conjurer)
		{
			tags.Add("Conjurer");
			tags.Add("Healer");
			tags.Add("Class");
		}

		if (this.Thaumaturge)
		{
			tags.Add("Thaumaturge");
			tags.Add("Damage");
			tags.Add("Class");
			tags.Add("Magical Ranged");
		}

		if (this.Carpenter)
		{
			tags.Add("Carpenter");
			tags.Add("Crafter");
		}

		if (this.Blacksmith)
		{
			tags.Add("Blacksmith");
			tags.Add("Crafter");
		}

		if (this.Armorer)
		{
			tags.Add("Armorer");
			tags.Add("Crafter");
		}

		if (this.Goldsmith)
		{
			tags.Add("Goldsmith");
			tags.Add("Crafter");
		}

		if (this.Leatherworker)
		{
			tags.Add("Leatherworker");
			tags.Add("Crafter");
		}

		if (this.Weaver)
		{
			tags.Add("Weaver");
			tags.Add("Crafter");
		}

		if (this.Alchemist)
		{
			tags.Add("Alchemist");
			tags.Add("Crafter");
		}

		if (this.Culinarian)
		{
			tags.Add("Culinarian");
			tags.Add("Crafter");
		}

		if (this.Miner)
		{
			tags.Add("Miner");
			tags.Add("Gatherer");
		}

		if (this.Botanist)
		{
			tags.Add("Botanist");
			tags.Add("Gatherer");
		}

		if (this.Fisher)
		{
			tags.Add("Fisher");
			tags.Add("Gatherer");
		}

		if (this.Paladin)
		{
			tags.Add("Paladin");
			tags.Add("Tank");
			tags.Add("Job");
		}

		if (this.Monk)
		{
			tags.Add("Monk");
			tags.Add("Damage");
			tags.Add("Job");
			tags.Add("Melee");
		}

		if (this.Warrior)
		{
			tags.Add("Warrior");
			tags.Add("Tank");
			tags.Add("Job");
		}

		if (this.Dragoon)
		{
			tags.Add("Dragoon");
			tags.Add("Damage");
			tags.Add("Job");
			tags.Add("Melee");
		}

		if (this.Bard)
		{
			tags.Add("Bard");
			tags.Add("Damage");
			tags.Add("Job");
			tags.Add("Physical Ranged");
		}

		if (this.WhiteMage)
		{
			tags.Add("White Mage");
			tags.Add("Healer");
			tags.Add("Job");
		}

		if (this.BlackMage)
		{
			tags.Add("Black Mage");
			tags.Add("Damage");
			tags.Add("Job");
			tags.Add("Magical Ranged");
		}

		if (this.Arcanist)
		{
			tags.Add("Arcanist");
			tags.Add("Damage");
			tags.Add("Class");
			tags.Add("Magical Ranged");
		}

		if (this.Summoner)
		{
			tags.Add("Summoner");
			tags.Add("Damage");
			tags.Add("Job");
			tags.Add("Magical Ranged");
		}

		if (this.Scholar)
		{
			tags.Add("Scholar");
			tags.Add("Healer");
			tags.Add("Job");
		}

		if (this.Rogue)
		{
			tags.Add("Rogue");
			tags.Add("Damage");
			tags.Add("Class");
			tags.Add("Melee");
		}

		if (this.Ninja)
		{
			tags.Add("Ninja");
			tags.Add("Damage");
			tags.Add("Job");
			tags.Add("Melee");
		}

		if (this.Machinist)
		{
			tags.Add("Machinist");
			tags.Add("Damage");
			tags.Add("Job");
			tags.Add("Physical Ranged");
		}

		if (this.DarkKnight)
		{
			tags.Add("DarkKnight");
			tags.Add("Tank");
			tags.Add("Job");
		}

		if (this.Astrologian)
		{
			tags.Add("Astrologian");
			tags.Add("Healer");
			tags.Add("Job");
		}

		if (this.Samurai)
		{
			tags.Add("Samurai");
			tags.Add("Damage");
			tags.Add("Job");
			tags.Add("Melee");
		}

		if (this.RedMage)
		{
			tags.Add("Red Mage");
			tags.Add("Damage");
			tags.Add("Job");
			tags.Add("Magical Ranged");
		}

		if (this.BlueMage)
		{
			tags.Add("Blue Mage");
			tags.Add("Damage");
			tags.Add("Limited");
			tags.Add("Job");
		}

		if (this.Gunbreaker)
		{
			tags.Add("Gunbreaker");
			tags.Add("Tank");
			tags.Add("Job");
		}

		if (this.Dancer)
		{
			tags.Add("Dancer");
			tags.Add("Damage");
			tags.Add("Job");
			tags.Add("Physical Ranged");
		}

		if (this.Reaper)
		{
			tags.Add("Reaper");
			tags.Add("Damage");
			tags.Add("Job");
			tags.Add("Melee");
		}

		if (this.Sage)
		{
			tags.Add("Sage");
			tags.Add("Healer");
			tags.Add("Job");
		}

		return tags;
	}
}
