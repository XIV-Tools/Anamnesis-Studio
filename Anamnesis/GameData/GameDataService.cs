// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Services;

using System;
using System.Collections.Generic;
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

	public ExcelSheet<Race> Races => GetSheet<Race>();
	public ExcelSheet<Tribe> Tribes => GetSheet<Tribe>();
	public ExcelSheet<Item> Items => GetSheet<Item>();
	public ExcelSheet<Perform> Perform => GetSheet<Perform>();
	public ExcelSheet<Stain> Dyes => GetSheet<Stain>();
	public ExcelSheet<EventNpc> EventNPCs => GetSheet<EventNpc>();
	public ExcelSheet<BattleNpc> BattleNPCs => GetSheet<BattleNpc>();
	public ExcelSheet<Mount> Mounts => GetSheet<Mount>();
	public ExcelSheet<MountCustomize> MountCustomize => GetSheet<MountCustomize>();
	public ExcelSheet<Companion> Companions => GetSheet<Companion>();
	public ExcelSheet<Territory> Territories => GetSheet<Territory>();
	public ExcelSheet<Weather> Weathers => GetSheet<Weather>();
	public ExcelSheet<CharaMakeCustomize> CharacterMakeCustomize => GetSheet<CharaMakeCustomize>();
	public ExcelSheet<CharaMakeType> CharacterMakeTypes => GetSheet<CharaMakeType>();
	public ExcelSheet<ResidentNpc> ResidentNPCs => GetSheet<ResidentNpc>();
	public ExcelSheet<WeatherRate> WeatherRates => GetSheet<WeatherRate>();
	public ExcelSheet<EquipRaceCategory> EquipRaceCategories => GetSheet<EquipRaceCategory>();
	public ExcelSheet<BattleNpcName> BattleNpcNames => GetSheet<BattleNpcName>();
	public ExcelSheet<GameData.Excel.Action> Actions => GetSheet<GameData.Excel.Action>();
	public ExcelSheet<ActionTimeline> ActionTimelines => GetSheet<ActionTimeline>();
	public ExcelSheet<Emote> Emotes => GetSheet<Emote>();
	public ExcelSheet<Ornament> Ornaments => GetSheet<Ornament>();
	public ExcelSheet<BuddyEquip> BuddyEquips => GetSheet<BuddyEquip>();
	public ExcelSheet<Lobby> Lobby => GetSheet<Lobby>();

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
		}
		catch (Exception ex)
		{
			throw new Exception("Failed to initialize Lumina (Are your game files up to date?)", ex);
		}

		return base.Initialize();
	}
}
