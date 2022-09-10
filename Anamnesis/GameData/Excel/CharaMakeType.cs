// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Excel;

using Anamnesis.Actor.Utilities;
using Anamnesis.GameData.Sheets;
using Anamnesis.Libraries.Items;
using Anamnesis.Memory;
using Anamnesis.Services;
using Lumina.Data;
using Lumina.Excel;
using System;
using System.Collections.Generic;
using System.Windows.Documents;
using ExcelRow = Anamnesis.GameData.Sheets.ExcelRow;
using LuminaData = Lumina.GameData;

// Much of this has been taken from  Ottermandias / Glamourer.
// https://github.com/Ottermandias/Glamourer/blob/main/Glamourer.GameData/Customization/CharaMakeParams.cs
[Sheet("CharaMakeType", columnHash: 0x80d7db6d)]
public class CharaMakeType : ExcelRow
{
	public const int NumOptions = 28;
	public const int NumVoices = 12;
	public const int NumGraphics = 10;
	public const int MaxNumValues = 100;
	public const int NumFaces = 8;
	public const int NumFeatures = 7;
	public const int NumEquip = 3;

	public string Name => this.RowId.ToString();

	public ActorCustomizeMemory.Genders Gender { get; private set; }
	public ActorCustomizeMemory.Races Race { get; private set; }
	public ActorCustomizeMemory.Tribes Tribe { get; private set; }

	public byte[] Voices { get; set; } = new byte[NumVoices];
	public FacialFeatureOptions[] FacialFeatureByFace { get; set; } = new FacialFeatureOptions[NumFaces];

	public Menu? Races { get; set; }
	public Menu? Genders { get; set; }
	public Menu? Ages { get; set; }
	public Menu? Heights { get; set; }
	public Menu? Tribes { get; set; }
	public Menu? Faces { get; set; }
	public Menu? Hairs { get; set; }
	public Menu? HighlightTypes { get; set; }
	public Menu? SkinTones { get; set; }
	public Menu? EyeColors { get; set; }
	public Menu? HairTones { get; set; }
	public Menu? Highlights { get; set; }
	public Menu? FacialFeatures { get; set; }
	public Menu? FacialFeatureColors { get; set; }
	public Menu? Eyebrows { get; set; }
	public Menu? Heterochromia { get; set; }
	public Menu? EyeShapes { get; set; }
	public Menu? Noses { get; set; }
	public Menu? Jaws { get; set; }
	public Menu? Mouths { get; set; }
	public Menu? LipsToneFurPatterns { get; set; }
	public Menu? EarMuscleTailSizes { get; set; }
	public Menu? TailEarsTypes { get; set; }
	public Menu? Busts { get; set; }
	public Menu? FacePaints { get; set; }
	public Menu? FacePaintColors { get; set; }

	public override void PopulateData(RowParser parser, LuminaData gameData, Language language)
	{
		base.PopulateData(parser, gameData, language);

		this.Race = (ActorCustomizeMemory.Races)parser.ReadColumn<int>(0);
		this.Tribe = (ActorCustomizeMemory.Tribes)parser.ReadColumn<int>(1);
		this.Gender = (ActorCustomizeMemory.Genders)parser.ReadColumn<sbyte>(2);

		for (int i = 0; i < NumOptions; i++)
		{
			uint index = parser.ReadColumn<uint>(3 + (6 * NumOptions) + i);

			Menu menu = new();
			menu.Id = parser.ReadColumn<uint>(3 + (0 * NumOptions) + i);
			menu.InitVal = parser.ReadColumn<byte>(3 + (1 * NumOptions) + i);
			menu.Type = (Menu.Types)parser.ReadColumn<byte>(3 + (2 * NumOptions) + i);
			menu.NumOptions = parser.ReadColumn<byte>(3 + (3 * NumOptions) + i);
			menu.LookAt = parser.ReadColumn<byte>(3 + (4 * NumOptions) + i);
			menu.Mask = parser.ReadColumn<uint>(3 + (5 * NumOptions) + i);
			menu.CustomizationIndex = parser.ReadColumn<uint>(3 + (6 * NumOptions) + i);
			menu.Min = parser.ReadColumn<byte>(2999 + i);
			menu.Max = (byte)(parser.ReadColumn<byte>(87 + i) - 1 + menu.Min);

			// Hairstyles are MakeCustomize
			if (index == 6 || index == 24)
			{
				CustomizeSheet.Features featureType = index == 6 ? CustomizeSheet.Features.Hair : CustomizeSheet.Features.FacePaint;
				List<CharaMakeCustomize> customize = GameDataService.Instance.CharacterMakeCustomize.GetFeatureOptions(featureType, this.Tribe, this.Gender);

				menu.NumOptions = (byte)customize.Count;
				menu.Options = new Menu.Option[menu.NumOptions];
				for (byte j = 0; j < menu.NumOptions; ++j)
				{
					menu.Options[j] = new Menu.Option();
					menu.Options[j].Value = customize[j].FeatureId;
					menu.Options[j].Icon = customize[j].Icon;
					menu.Options[j].Customize = customize[j];
				}
			}
			else
			{
				// Colors are Human.cmp
				ColorData.Entry[]? colors = index switch
				{
					8 => ColorData.GetSkin(this.Tribe, this.Gender),
					9 => ColorData.GetEyeColors(),
					10 => ColorData.GetHair(this.Tribe, this.Gender),
					11 => ColorData.GetHairHighlights(),
					13 => ColorData.GetLimbalColors(), // what about for non au-ra? hmmm
					20 => ColorData.GetLipColors(),
					25 => ColorData.GetFacePaintColor(),
					_ => null,
				};

				if (colors != null)
					menu.NumOptions = (byte)colors.Length;

				menu.Options = new Menu.Option[menu.NumOptions];
				for (byte j = 0; j < menu.NumOptions; ++j)
				{
					menu.Options[j] = new Menu.Option();
					menu.Options[j].Value = (byte)(menu.Min + j);

					if (menu.Type == Menu.Types.ColorPicker || menu.Type == Menu.Types.DoubleColorPicker)
					{
						////if (colors == null || colors.Length != menu.NumOptions)
						////	throw new Exception("No color or colors where wrong count for menu type");

						if (colors != null && j < colors.Length)
						{
							menu.Options[j].Color = colors[j];
						}
					}
					else
					{
						menu.Options[j].Icon = new ImageReference(parser.ReadColumn<uint>(3 + ((7 + j) * NumOptions) + i));
					}
				}
			}

			/*option.Graphic = new byte[NumGraphics];
			for (var j = 0; j < NumGraphics; ++j)
			{
				option.Graphic[j] = parser.ReadColumn<byte>(3 + ((MaxNumValues + 7 + j) * NumOptions) + i);
			}*/

			switch (index)
			{
				case 0: this.Races = menu; break;
				case 1: this.Genders = menu; break;
				case 2: this.Ages = menu; break;
				case 3: this.Heights = menu; break;
				case 4: this.Tribes = menu; break;
				case 5: this.Faces = menu; break;
				case 6: this.Hairs = menu; break;
				case 7: this.HighlightTypes = menu; break;
				case 8: this.SkinTones = menu; break;
				case 9: this.EyeColors = menu; break;
				case 10: this.HairTones = menu; break;
				case 11: this.Highlights = menu; break;
				case 12: this.FacialFeatures = menu; break;
				case 13: this.FacialFeatureColors = menu; break;
				case 14: this.Eyebrows = menu; break;
				case 15: this.Heterochromia = menu; break;
				case 16: this.EyeShapes = menu; break;
				case 17: this.Noses = menu; break;
				case 18: this.Jaws = menu; break;
				case 19: this.Mouths = menu; break;
				case 20: this.LipsToneFurPatterns = menu; break;
				case 21: this.EarMuscleTailSizes = menu; break;
				case 22: this.TailEarsTypes = menu; break;
				case 23: this.Busts = menu; break;
				case 24: this.FacePaints = menu; break;
				case 25: this.FacePaintColors = menu; break;
			}
		}

		for (var i = 0; i < NumVoices; ++i)
		{
			this.Voices[i] = parser.ReadColumn<byte>(3 + ((MaxNumValues + 7 + NumGraphics) * NumOptions) + i);
		}

		/*for (var i = 0; i < NumFaces; ++i)
		{
			this.FacialFeatureByFace[i].Icons = new uint[NumFeatures];
			for (var j = 0; j < NumFeatures; ++j)
			{
				this.FacialFeatureByFace[i].Icons![j] = (uint)parser.ReadColumn<int>(3 + ((MaxNumValues + 7 + NumGraphics) * NumOptions) + NumVoices + (j * NumFaces) + i);
			}
		}

		for (var i = 0; i < NumEquip; ++i)
		{
			Equip[i] = new CharaMakeType.CharaMakeTypeUnkData3347Obj()
			{
				Helmet = parser.ReadColumn<ulong>(3 + (MaxNumValues + 7 + NumGraphics) * NumMenus + NumVoices + NumFaces * NumFeatures + i * 7 + 0),
				Top = parser.ReadColumn<ulong>(3 + (MaxNumValues + 7 + NumGraphics) * NumMenus + NumVoices + NumFaces * NumFeatures + i * 7 + 1),
				Gloves = parser.ReadColumn<ulong>(3 + (MaxNumValues + 7 + NumGraphics) * NumMenus + NumVoices + NumFaces * NumFeatures + i * 7 + 2),
				Legs = parser.ReadColumn<ulong>(3 + (MaxNumValues + 7 + NumGraphics) * NumMenus + NumVoices + NumFaces * NumFeatures + i * 7 + 3),
				Shoes = parser.ReadColumn<ulong>(3 + (MaxNumValues + 7 + NumGraphics) * NumMenus + NumVoices + NumFaces * NumFeatures + i * 7 + 4),
				Weapon = parser.ReadColumn<ulong>(3 + (MaxNumValues + 7 + NumGraphics) * NumMenus + NumVoices + NumFaces * NumFeatures + i * 7 + 5),
				SubWeapon = parser.ReadColumn<ulong>(3 + (MaxNumValues + 7 + NumGraphics) * NumMenus + NumVoices + NumFaces * NumFeatures + i * 7 + 6),
			};
		}*/
	}

	public class Menu
	{
		public enum Types
		{
			ListSelector = 0,
			IconSelector = 1,
			ColorPicker = 2,
			DoubleColorPicker = 3,
			MultiIconSelector = 4,
			Percentage = 5,
		}

		public string? Name => GameDataService.GetSheet<Lobby>().Get(this.Id).Text;

		public int OptionIndex { get; set; }
		public uint Id { get; set; }
		public byte InitVal { get; set; }
		public Types Type { get; set; }
		public byte NumOptions { get; set; }
		public byte LookAt { get; set; }
		public uint Mask { get; set; }
		public uint CustomizationIndex { get; set; }
		public byte Min { get; set; }
		public byte Max { get; set; }
		public Option[]? Options { get; set; }

		public byte[]? Values
		{
			get
			{
				if (this.Options == null)
					return null;

				List<byte> values = new();
				foreach (Option op in this.Options)
				{
					values.Add(op.Value);
				}

				return values.ToArray();
			}
		}

		public Option? GetOption(byte value)
		{
			if (this.Options == null)
				return null;

			foreach (Option op in this.Options)
			{
				if (op.Value == value)
				{
					return op;
				}
			}

			return null;
		}

		public class Option
		{
			public string? Name { get; set; }
			public byte Value { get; set; }
			public ImageReference? Icon { get; set; }
			public ColorData.Entry Color { get; set; }
			public CharaMakeCustomize? Customize { get; set; }
		}
	}

	public class FacialFeatureOptions
	{
		public uint[]? Icons { get; set; }
	}
}