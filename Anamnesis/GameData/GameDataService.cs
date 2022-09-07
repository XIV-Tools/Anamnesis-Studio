// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Services;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Anamnesis.Actor;
using Anamnesis.Files;
using Anamnesis.GameData;
using Anamnesis.GameData.Excel;
using Anamnesis.GameData.Sheets;
using Anamnesis.Memory;
using Lumina.Data;
using Microsoft.Win32;
using LuminaData = global::Lumina.GameData;

public class GameDataService : ServiceBase<GameDataService>
{
	internal static LuminaData? LuminaData;

	private static readonly Dictionary<Type, Lumina.Excel.ExcelSheetImpl> Sheets = new Dictionary<Type, Lumina.Excel.ExcelSheetImpl>();

	private Dictionary<string, string>? npcNames;
	private Dictionary<uint, ItemCategories>? itemCategories;

	public enum ClientRegion
	{
		Global,
		Korean,
		Chinese,
	}

	public static ClientRegion Region { get; private set; }

	public static string GamePath
	{
		get
		{
			if (MemoryService.Process?.MainModule != null)
				return Path.GetDirectoryName(MemoryService.Process.MainModule.FileName) + "\\..\\";

			if (SettingsService.Current.GamePath == null)
			{
				bool valid = false;

				do
				{
					OpenFileDialog dlg = new OpenFileDialog();
					dlg.Filter = "FFXIV | ffxiv_dx11.exe";
					dlg.Title = "Select FFXIV Installation";
					bool? result = dlg.ShowDialog();

					if (result == true)
					{
						string path = Path.GetDirectoryName(dlg.FileName) + "\\..\\";

						if (!Directory.Exists(path + "/boot") || !Directory.Exists(path + "/game"))
						{
							Log.Error("Invalid game path.");
							valid = false;
						}
						else
						{
							SettingsService.Current.GamePath = path;
							valid = true;
						}
					}
					else
					{
						throw new Exception("Must select a game path");
					}
				}
				while (!valid);
			}

			if (SettingsService.Current.GamePath == null)
				throw new Exception("No game path");

			return SettingsService.Current.GamePath;
		}
	}

	public ExcelSheet<Race> Races { get; private set; } = null!;
	public ExcelSheet<Tribe> Tribes { get; private set; } = null!;
	public ExcelSheet<Item> Items { get; private set; } = null!;
	public ExcelSheet<Perform> Perform { get; private set; } = null!;
	public ExcelSheet<Stain> Dyes { get; private set; } = null!;
	public ExcelSheet<EventNpc> EventNPCs { get; private set; } = null!;
	public ExcelSheet<BattleNpc> BattleNPCs { get; private set; } = null!;
	public ExcelSheet<Mount> Mounts { get; private set; } = null!;
	public ExcelSheet<MountCustomize> MountCustomize { get; private set; } = null!;
	public ExcelSheet<Companion> Companions { get; private set; } = null!;
	public ExcelSheet<Territory> Territories { get; private set; } = null!;
	public ExcelSheet<Weather> Weathers { get; private set; } = null!;
	public ExcelSheet<CharaMakeCustomize> CharacterMakeCustomize { get; private set; } = null!;
	public ExcelSheet<CharaMakeType> CharacterMakeTypes { get; private set; } = null!;
	public ExcelSheet<ResidentNpc> ResidentNPCs { get; private set; } = null!;
	public ExcelSheet<WeatherRate> WeatherRates { get; private set; } = null!;
	public ExcelSheet<EquipRaceCategory> EquipRaceCategories { get; private set; } = null!;
	public ExcelSheet<BattleNpcName> BattleNpcNames { get; private set; } = null!;
	public ExcelSheet<GameData.Excel.Action> Actions { get; private set; } = null!;
	public ExcelSheet<ActionTimeline> ActionTimelines { get; private set; } = null!;
	public ExcelSheet<Emote> Emotes { get; private set; } = null!;
	public ExcelSheet<Ornament> Ornaments { get; private set; } = null!;
	public ExcelSheet<BuddyEquip> BuddyEquips { get; private set; } = null!;
	public ExcelSheet<Lobby> Lobby { get; private set; } = null!;

	public EquipmentSheet Equipment { get; private set; } = null!;

	public static ExcelSheet<T> GetSheet<T>()
		where T : Lumina.Excel.ExcelRow
	{
		lock (Sheets)
		{
			Type type = typeof(T);

			Lumina.Excel.ExcelSheetImpl? sheet;
			if (Sheets.TryGetValue(type, out sheet) && sheet is ExcelSheet<T> sheetT)
				return sheetT;

			if (LuminaData == null)
				throw new Exception("Game Data Service has not been initialized");

			sheet = ExcelSheet<T>.GetSheet(LuminaData);
			Sheets.Add(type, sheet);
			return (ExcelSheet<T>)sheet;
		}
	}

	public static byte[] GetFileData(string path)
	{
		if (LuminaData == null)
			throw new Exception("Game Data Service has not been initialized");

		FileResource? file = LuminaData.GetFile(path);
		if (file == null)
			throw new Exception($"Failed to read file from game data: \"{path}\"");

		return file.Data;
	}

	public static string? GetNpcName(INpcBase npc)
	{
		if (Instance.npcNames == null)
			return null;

		string stringKey = npc.ToStringKey();
		string? name;

		if (!Instance.npcNames.TryGetValue(stringKey, out name))
			return null;

		// Is this a BattleNpcName entry?
		if (name.Contains("N:"))
		{
			if (Instance.BattleNpcNames == null)
				return name;

			uint bNpcNameKey = uint.Parse(name.Remove(0, 2));

			BattleNpcName? row = Instance.BattleNpcNames.GetRow(bNpcNameKey);
			if (row == null || string.IsNullOrEmpty(row.Name))
				return name;

			return row.Name;
		}

		return name;
	}

	public static ItemCategories GetCategory(Item item)
	{
		ItemCategories category = ItemCategories.None;
		if (Instance.itemCategories != null && !Instance.itemCategories.TryGetValue(item.RowId, out category))
			category = ItemCategories.None;

		if (FavoritesService.IsFavorite(item))
			category = category.SetFlag(ItemCategories.Favorites, true);

		if (FavoritesService.IsOwned(item))
			category = category.SetFlag(ItemCategories.Owned, true);

		return category;
	}

	public override Task Initialize()
	{
		Language defaultLuminaLaunguage = Language.English;
		Region = ClientRegion.Global;

		if (File.Exists(Path.Combine(GameDataService.GamePath, "FFXIVBoot.exe")) || File.Exists(Path.Combine(GameDataService.GamePath, "rail_files", "rail_game_identify.json")))
		{
			Region = ClientRegion.Chinese;
			defaultLuminaLaunguage = Language.ChineseSimplified;
		}
		else if (File.Exists(Path.Combine(GameDataService.GamePath, "boot", "FFXIV_Boot.exe")))
		{
			Region = ClientRegion.Korean;
			defaultLuminaLaunguage = Language.Korean;
		}

		Log.Information($"Found game client region: {Region}");

		// these are json files that we write by hand
		try
		{
			this.Equipment = new EquipmentSheet("Data/Equipment.json");
			this.itemCategories = EmbeddedFileUtility.Load<Dictionary<uint, ItemCategories>>("Data/ItemCategories.json");
			this.npcNames = EmbeddedFileUtility.Load<Dictionary<string, string>>("Data/NpcNames.json");
		}
		catch (Exception ex)
		{
			throw new Exception("Failed to read data sheets", ex);
		}

		try
		{
			Lumina.LuminaOptions options = new Lumina.LuminaOptions();
			options.DefaultExcelLanguage = defaultLuminaLaunguage;

			LuminaData = new LuminaData(GameDataService.GamePath + "\\game\\sqpack\\", options);

			this.Races = GetSheet<Race>();
			this.Tribes = GetSheet<Tribe>();
			this.Items = GetSheet<Item>();
			this.Dyes = GetSheet<Stain>();
			this.EventNPCs = GetSheet<EventNpc>();
			this.BattleNPCs = GetSheet<BattleNpc>();
			this.Mounts = GetSheet<Mount>();
			this.MountCustomize = GetSheet<MountCustomize>();
			this.Companions = GetSheet<Companion>();
			this.Territories = GetSheet<Territory>();
			this.Weathers = GetSheet<Weather>();
			this.CharacterMakeCustomize = GetSheet<CharaMakeCustomize>();
			this.CharacterMakeTypes = GetSheet<CharaMakeType>();
			this.ResidentNPCs = GetSheet<ResidentNpc>();
			this.Perform = GetSheet<Perform>();
			this.WeatherRates = GetSheet<WeatherRate>();
			this.EquipRaceCategories = GetSheet<EquipRaceCategory>();
			this.BattleNpcNames = GetSheet<BattleNpcName>();
			this.Actions = GetSheet<GameData.Excel.Action>();
			this.ActionTimelines = GetSheet<ActionTimeline>();
			this.Emotes = GetSheet<Emote>();
			this.Ornaments = GetSheet<Ornament>();
			this.BuddyEquips = GetSheet<BuddyEquip>();
			this.Lobby = GetSheet<Lobby>();
		}
		catch (Exception ex)
		{
			throw new Exception("Failed to initialize Lumina (Are your game files up to date?)", ex);
		}

		return base.Initialize();
	}
}
