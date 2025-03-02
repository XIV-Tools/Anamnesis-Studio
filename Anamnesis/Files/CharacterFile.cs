﻿// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Files;

using System;
using System.Threading.Tasks;
using Anamnesis.Actor.Utilities;
using Anamnesis.GameData.Excel;
using Anamnesis.Memory;
using Anamnesis.Services;
using Anamnesis.Tags;
using Serilog;

[Serializable]
public class CharacterFile : JsonFileBase
{
	[Flags]
	public enum SaveModes
	{
		None = 0,

		EquipmentGear = 1,
		EquipmentAccessories = 2,
		EquipmentWeapons = 4,
		AppearanceHair = 8,
		AppearanceFace = 16,
		AppearanceBody = 32,
		AppearanceExtended = 64,

		Equipment = EquipmentGear | EquipmentAccessories | EquipmentWeapons,
		Appearance = AppearanceHair | AppearanceFace | AppearanceBody | AppearanceExtended,

		All = EquipmentGear | EquipmentAccessories | EquipmentWeapons | AppearanceHair | AppearanceFace | AppearanceBody | AppearanceExtended,
	}

	public override string FileExtension => ".chara";
	public override string TypeName => "Anamnesis Character File";
	public override Type? ImporterType => typeof(CharacterFileImporter);

	public SaveModes SaveMode { get; set; } = SaveModes.All;

	public string? Nickname { get; set; } = null;
	public uint ModelType { get; set; } = 0;
	public ActorTypes ObjectKind { get; set; } = ActorTypes.None;

	// appearance
	public ActorCustomizeMemory.Races? Race { get; set; }
	public ActorCustomizeMemory.Genders? Gender { get; set; }
	public ActorCustomizeMemory.Ages? Age { get; set; }
	public byte? Height { get; set; }
	public ActorCustomizeMemory.Tribes? Tribe { get; set; }
	public byte? Head { get; set; }
	public byte? Hair { get; set; }
	public bool? EnableHighlights { get; set; }
	public byte? Skintone { get; set; }
	public byte? REyeColor { get; set; }
	public byte? HairTone { get; set; }
	public byte? Highlights { get; set; }
	public ActorCustomizeMemory.FacialFeature? FacialFeatures { get; set; }
	public byte? LimbalEyes { get; set; } // facial feature color
	public byte? Eyebrows { get; set; }
	public byte? LEyeColor { get; set; }
	public byte? Eyes { get; set; }
	public byte? Nose { get; set; }
	public byte? Jaw { get; set; }
	public byte? Mouth { get; set; }
	public byte? LipsToneFurPattern { get; set; }
	public byte? EarMuscleTailSize { get; set; }
	public byte? TailEarsType { get; set; }
	public byte? Bust { get; set; }
	public byte? FacePaint { get; set; }
	public byte? FacePaintColor { get; set; }

	// weapons
	public WeaponSave? MainHand { get; set; }
	public WeaponSave? OffHand { get; set; }

	// equipment
	public ItemSave? HeadGear { get; set; }
	public ItemSave? Body { get; set; }
	public ItemSave? Hands { get; set; }
	public ItemSave? Legs { get; set; }
	public ItemSave? Feet { get; set; }
	public ItemSave? Ears { get; set; }
	public ItemSave? Neck { get; set; }
	public ItemSave? Wrists { get; set; }
	public ItemSave? LeftRing { get; set; }
	public ItemSave? RightRing { get; set; }

	// extended appearance
	// NOTE: extended weapon values are stored in the WeaponSave
	public Color? SkinColor { get; set; }
	public Color? SkinGloss { get; set; }
	public Color? LeftEyeColor { get; set; }
	public Color? RightEyeColor { get; set; }
	public Color? LimbalRingColor { get; set; }
	public Color? HairColor { get; set; }
	public Color? HairGloss { get; set; }
	public Color? HairHighlight { get; set; }
	public Color4? MouthColor { get; set; }
	public Vector? BustScale { get; set; }
	public float? Transparency { get; set; }
	public float? MuscleTone { get; set; }
	public float? HeightMultiplier { get; set; }

	public ExportData Export { get; set; } = new();

	public override void GenerateTags(TagCollection tags)
	{
		base.GenerateTags(tags);

		// Race
		if (this.Race != null)
		{
			Race? race = GameDataService.Instance.Races.Find((byte)this.Race);

			if (race != null)
			{
				tags.Add(race.DisplayName);
			}
		}

		// Tribe
		if (this.Tribe != null)
		{
			Tribe? tribe = GameDataService.Instance.Tribes.Find((byte)this.Tribe);

			if (tribe != null)
			{
				tags.Add(tribe.DisplayName);
			}
		}

		// Gender
		if (this.Gender != null)
		{
			tags.Add(((ActorCustomizeMemory.Genders)this.Gender).ToString());
		}
	}

	public void WriteToFile(ActorMemory actor, SaveModes mode)
	{
		this.Nickname = actor.Names.Nickname;
		this.ModelType = (uint)actor.ModelType;
		this.ObjectKind = actor.ObjectKind;

		if (actor.Customize == null)
			return;

		this.SaveMode = mode;

		if (this.IncludeSection(SaveModes.EquipmentWeapons, mode))
		{
			if (actor.MainHand != null)
				this.MainHand = new WeaponSave(actor.MainHand);
			////this.MainHand.Color = actor.GetValue(Offsets.Main.MainHandColor);
			////this.MainHand.Scale = actor.GetValue(Offsets.Main.MainHandScale);

			if (actor.OffHand != null)
				this.OffHand = new WeaponSave(actor.OffHand);
			////this.OffHand.Color = actor.GetValue(Offsets.Main.OffhandColor);
			////this.OffHand.Scale = actor.GetValue(Offsets.Main.OffhandScale);
		}

		if (this.IncludeSection(SaveModes.EquipmentGear, mode))
		{
			if (actor.Equipment?.Head != null)
				this.HeadGear = new ItemSave(actor.Equipment.Head);

			if (actor.Equipment?.Body != null)
				this.Body = new ItemSave(actor.Equipment.Body);

			if (actor.Equipment?.Hands != null)
				this.Hands = new ItemSave(actor.Equipment.Hands);

			if (actor.Equipment?.Legs != null)
				this.Legs = new ItemSave(actor.Equipment.Legs);

			if (actor.Equipment?.Feet != null)
				this.Feet = new ItemSave(actor.Equipment.Feet);
		}

		if (this.IncludeSection(SaveModes.EquipmentAccessories, mode))
		{
			if (actor.Equipment?.Ears != null)
				this.Ears = new ItemSave(actor.Equipment.Ears);

			if (actor.Equipment?.Neck != null)
				this.Neck = new ItemSave(actor.Equipment.Neck);

			if (actor.Equipment?.Wrists != null)
				this.Wrists = new ItemSave(actor.Equipment.Wrists);

			if (actor.Equipment?.LeftRing != null)
				this.LeftRing = new ItemSave(actor.Equipment.LeftRing);

			if (actor.Equipment?.RightRing != null)
				this.RightRing = new ItemSave(actor.Equipment.RightRing);
		}

		if (this.IncludeSection(SaveModes.AppearanceHair, mode))
		{
			this.Hair = actor.Customize?.Hair;
			this.EnableHighlights = actor.Customize?.EnableHighlights;
			this.HairTone = actor.Customize?.HairTone;
			this.Highlights = actor.Customize?.Highlights;
			this.HairColor = actor.ModelObject?.ShaderParameters?.HairColor;
			this.HairGloss = actor.ModelObject?.ShaderParameters?.HairGloss;
			this.HairHighlight = actor.ModelObject?.ShaderParameters?.HairHighlight;
		}

		if (this.IncludeSection(SaveModes.AppearanceFace, mode) || this.IncludeSection(SaveModes.AppearanceBody, mode))
		{
			this.Race = actor.Customize?.RaceId;
			this.Gender = actor.Customize?.Gender;
			this.Tribe = actor.Customize?.TribeId;
			this.Age = actor.Customize?.Age;
		}

		if (this.IncludeSection(SaveModes.AppearanceFace, mode))
		{
			this.Head = actor.Customize?.Face;
			this.REyeColor = actor.Customize?.RightEyeColor;
			this.LimbalEyes = actor.Customize?.FacialFeatureColor;
			this.FacialFeatures = actor.Customize?.FacialFeatures;
			this.Eyebrows = actor.Customize?.Eyebrows;
			this.LEyeColor = actor.Customize?.LeftEyeColor;
			this.Eyes = actor.Customize?.Eyes;
			this.Nose = actor.Customize?.Nose;
			this.Jaw = actor.Customize?.Jaw;
			this.Mouth = actor.Customize?.MouthId;
			this.LipsToneFurPattern = actor.Customize?.LipsToneFurPattern;
			this.FacePaint = actor.Customize?.FacePaint;
			this.FacePaintColor = actor.Customize?.FacePaintColor;
			this.LeftEyeColor = actor.ModelObject?.ShaderParameters?.LeftEyeColor;
			this.RightEyeColor = actor.ModelObject?.ShaderParameters?.RightEyeColor;
			this.LimbalRingColor = actor.ModelObject?.ShaderParameters?.LimbalRingColor;
			this.MouthColor = actor.ModelObject?.ShaderParameters?.MouthColor;
		}

		if (this.IncludeSection(SaveModes.AppearanceBody, mode))
		{
			this.Height = actor.Customize?.Height;
			this.Skintone = actor.Customize?.SkinTone;
			this.EarMuscleTailSize = actor.Customize?.EarMuscleTailSize;
			this.TailEarsType = actor.Customize?.TailEarsType;
			this.Bust = actor.Customize?.Bust;

			this.HeightMultiplier = actor.ModelObject?.Height;
			this.SkinColor = actor.ModelObject?.ShaderParameters?.SkinColor;
			this.SkinGloss = actor.ModelObject?.ShaderParameters?.SkinGloss;
			this.MuscleTone = actor.ModelObject?.ShaderParameters?.MuscleTone;
			this.BustScale = actor.ModelObject?.Bust?.Scale;
			this.Transparency = actor.Transparency;
		}

		try
		{
			this.Export.Write(actor);
		}
		catch (Exception ex)
		{
			Log.Error(ex, "Error writing colors");
		}
	}

	public async Task Apply(ActorMemory actor, SaveModes mode, bool allowRefresh = true)
	{
		if (this.Tribe == 0)
			this.Tribe = ActorCustomizeMemory.Tribes.Midlander;

		if (this.Race == 0)
			this.Race = ActorCustomizeMemory.Races.Hyur;

		if (this.Tribe != null && !Enum.IsDefined((ActorCustomizeMemory.Tribes)this.Tribe))
			throw new Exception($"Invalid tribe: {this.Tribe} in appearance file");

		if (this.Race != null && !Enum.IsDefined((ActorCustomizeMemory.Races)this.Race))
			throw new Exception($"Invalid race: {this.Race} in appearance file");

		if (actor.Customize == null)
			return;

		Log.Information("Reading appearance from file");

		actor.AutomaticRefreshEnabled = false;

		////if (actor.CanRefresh)
		{
			actor.EnableReading = false;

			if (!string.IsNullOrEmpty(this.Nickname))
				actor.Names.Nickname = this.Nickname;

			actor.ModelType = (int)this.ModelType;
			////actor.ObjectKind = this.ObjectKind;

			if (this.IncludeSection(SaveModes.EquipmentWeapons, mode))
			{
				this.MainHand?.Write(actor.MainHand, true);
				this.OffHand?.Write(actor.OffHand, false);
			}

			if (this.IncludeSection(SaveModes.EquipmentGear, mode))
			{
				this.HeadGear?.Write(actor.Equipment?.Head);
				this.Body?.Write(actor.Equipment?.Body);
				this.Hands?.Write(actor.Equipment?.Hands);
				this.Legs?.Write(actor.Equipment?.Legs);
				this.Feet?.Write(actor.Equipment?.Feet);
			}

			if (this.IncludeSection(SaveModes.EquipmentAccessories, mode))
			{
				this.Ears?.Write(actor.Equipment?.Ears);
				this.Neck?.Write(actor.Equipment?.Neck);
				this.Wrists?.Write(actor.Equipment?.Wrists);
				this.RightRing?.Write(actor.Equipment?.RightRing);
				this.LeftRing?.Write(actor.Equipment?.LeftRing);
			}

			if (this.IncludeSection(SaveModes.AppearanceHair, mode))
			{
				if (this.Hair != null)
					actor.Customize.Hair = (byte)this.Hair;

				if (this.EnableHighlights != null)
					actor.Customize.EnableHighlights = (bool)this.EnableHighlights;

				if (this.HairTone != null)
					actor.Customize.HairTone = (byte)this.HairTone;

				if (this.Highlights != null)
					actor.Customize.Highlights = (byte)this.Highlights;
			}

			if (this.IncludeSection(SaveModes.AppearanceFace, mode) || this.IncludeSection(SaveModes.AppearanceBody, mode))
			{
				if (this.Race != null)
					actor.Customize.RaceId = (ActorCustomizeMemory.Races)this.Race;

				if (this.Gender != null)
					actor.Customize.Gender = (ActorCustomizeMemory.Genders)this.Gender;

				if (this.Tribe != null)
					actor.Customize.TribeId = (ActorCustomizeMemory.Tribes)this.Tribe;

				if (this.Age != null)
					actor.Customize.Age = (ActorCustomizeMemory.Ages)this.Age;
			}

			if (this.IncludeSection(SaveModes.AppearanceFace, mode))
			{
				if (this.Head != null)
					actor.Customize.Face = (byte)this.Head;

				if (this.REyeColor != null)
					actor.Customize.RightEyeColor = (byte)this.REyeColor;

				if (this.FacialFeatures != null)
					actor.Customize.FacialFeatures = (ActorCustomizeMemory.FacialFeature)this.FacialFeatures;

				if (this.LimbalEyes != null)
					actor.Customize.FacialFeatureColor = (byte)this.LimbalEyes;

				if (this.Eyebrows != null)
					actor.Customize.Eyebrows = (byte)this.Eyebrows;

				if (this.LEyeColor != null)
					actor.Customize.LeftEyeColor = (byte)this.LEyeColor;

				if (this.Eyes != null)
					actor.Customize.Eyes = (byte)this.Eyes;

				if (this.Nose != null)
					actor.Customize.Nose = (byte)this.Nose;

				if (this.Jaw != null)
					actor.Customize.Jaw = (byte)this.Jaw;

				if (this.Mouth != null)
					actor.Customize.MouthId = (byte)this.Mouth;

				if (this.LipsToneFurPattern != null)
					actor.Customize.LipsToneFurPattern = (byte)this.LipsToneFurPattern;

				if (this.FacePaint != null)
					actor.Customize.FacePaint = (byte)this.FacePaint;

				if (this.FacePaintColor != null)
					actor.Customize.FacePaintColor = (byte)this.FacePaintColor;
			}

			if (this.IncludeSection(SaveModes.AppearanceBody, mode))
			{
				if (this.Height != null)
					actor.Customize.Height = (byte)this.Height;

				if (this.Skintone != null)
					actor.Customize.SkinTone = (byte)this.Skintone;

				if (this.EarMuscleTailSize != null)
					actor.Customize.EarMuscleTailSize = (byte)this.EarMuscleTailSize;

				if (this.TailEarsType != null)
					actor.Customize.TailEarsType = (byte)this.TailEarsType;

				if (this.Bust != null)
					actor.Customize.Bust = (byte)this.Bust;
			}

			if (allowRefresh)
			{
				await actor.RefreshAsync();
			}

			// Setting customize values will reset the extended appearance, which me must read.
			actor.EnableReading = true;
			actor.Tick();
		}

		Log.Verbose("Begin reading Extended Appearance from file");

		if (actor.ModelObject?.ShaderParameters != null)
		{
			if (this.IncludeSection(SaveModes.AppearanceHair, mode))
			{
				actor.ModelObject.ShaderParameters.HairColor = this.HairColor ?? actor.ModelObject.ShaderParameters.HairColor;
				actor.ModelObject.ShaderParameters.HairGloss = this.HairGloss ?? actor.ModelObject.ShaderParameters.HairGloss;
				actor.ModelObject.ShaderParameters.HairHighlight = this.HairHighlight ?? actor.ModelObject.ShaderParameters.HairHighlight;
			}

			if (this.IncludeSection(SaveModes.AppearanceFace, mode))
			{
				actor.ModelObject.ShaderParameters.LeftEyeColor = this.LeftEyeColor ?? actor.ModelObject.ShaderParameters.LeftEyeColor;
				actor.ModelObject.ShaderParameters.RightEyeColor = this.RightEyeColor ?? actor.ModelObject.ShaderParameters.RightEyeColor;
				actor.ModelObject.ShaderParameters.LimbalRingColor = this.LimbalRingColor ?? actor.ModelObject.ShaderParameters.LimbalRingColor;
				actor.ModelObject.ShaderParameters.MouthColor = this.MouthColor ?? actor.ModelObject.ShaderParameters.MouthColor;
			}

			if (this.IncludeSection(SaveModes.AppearanceBody, mode))
			{
				actor.ModelObject.ShaderParameters.SkinColor = this.SkinColor ?? actor.ModelObject.ShaderParameters.SkinColor;
				actor.ModelObject.ShaderParameters.SkinGloss = this.SkinGloss ?? actor.ModelObject.ShaderParameters.SkinGloss;
				actor.ModelObject.ShaderParameters.MuscleTone = this.MuscleTone ?? actor.ModelObject.ShaderParameters.MuscleTone;
				actor.Transparency = this.Transparency ?? actor.Transparency;

				if (Float.IsValid(this.HeightMultiplier))
					actor.ModelObject.Height = this.HeightMultiplier ?? actor.ModelObject.Height;

				if (actor.ModelObject.Bust?.Scale != null && Vector.IsValid(this.BustScale))
				{
					actor.ModelObject.Bust.Scale = this.BustScale ?? actor.ModelObject.Bust.Scale;
				}
			}
		}

		actor.AutomaticRefreshEnabled = true;
		actor.EnableReading = true;

		Log.Information("Finished reading appearance from file");
	}

	private bool IncludeSection(SaveModes section, SaveModes mode)
	{
		return this.SaveMode.HasFlag(section) && mode.HasFlag(section);
	}

	[Serializable]
	public class WeaponSave
	{
		public WeaponSave()
		{
		}

		public WeaponSave(WeaponMemory from)
		{
			this.ModelSet = from.Set;
			this.ModelBase = from.Base;
			this.ModelVariant = from.Variant;
			this.DyeId = from.Dye;
		}

		public Color Color { get; set; }
		public Vector Scale { get; set; }
		public ushort ModelSet { get; set; }
		public ushort ModelBase { get; set; }
		public ushort ModelVariant { get; set; }
		public byte DyeId { get; set; }

		public void Write(WeaponMemory? vm, bool isMainHand)
		{
			if (vm == null)
				return;

			vm.Set = this.ModelSet;

			// sanity check values
			if (vm.Set != 0)
			{
				vm.Base = this.ModelBase;
				vm.Variant = this.ModelVariant;
				vm.Dye = this.DyeId;
			}
			else
			{
				if (isMainHand)
				{
					vm.Set = ItemUtility.EmperorsNewFists.ModelSet;
					vm.Base = ItemUtility.EmperorsNewFists.ModelBase;
					vm.Variant = ItemUtility.EmperorsNewFists.ModelVariant;
				}
				else
				{
					vm.Set = 0;
					vm.Base = 0;
					vm.Variant = 0;
				}

				vm.Dye = ItemUtility.NoneDye.Id;
			}
		}
	}

	[Serializable]
	public class ItemSave
	{
		public ItemSave()
		{
		}

		public ItemSave(ItemMemory from)
		{
			this.ModelBase = from.Base;
			this.ModelVariant = from.Variant;
			this.DyeId = from.Dye;
		}

		public ushort ModelBase { get; set; }
		public byte ModelVariant { get; set; }
		public byte DyeId { get; set; }

		public void Write(ItemMemory? vm)
		{
			if (vm == null)
				return;

			vm.Base = this.ModelBase;
			vm.Variant = this.ModelVariant;
			vm.Dye = this.DyeId;
		}
	}

	public class ExportData
	{
		// write-only data - these entries are written for use in other applications
		public Color? SkinToneColor { get; set; }
		public Color? HairToneColor { get; set; }
		public Color? HighlightColor { get; set; }
		public Color? LEyeColor { get; set; }
		public Color? REyeColor { get; set; }
		public Color? FacialFeatureColor { get; set; }
		public Color? FacePaintColor { get; set; }
		public Color? LipColor { get; set; }

		public void Write(ActorMemory actor)
		{
			if (actor.Customize == null)
				return;

			this.SkinToneColor = ColorData.GetSkin(actor.Customize.TribeId, actor.Customize.Gender)[actor.Customize.SkinTone].CmColor;
			this.HairToneColor = ColorData.GetHair(actor.Customize.TribeId, actor.Customize.Gender)[actor.Customize.HairTone].CmColor;
			this.HighlightColor = ColorData.GetHairHighlights()[actor.Customize.Highlights].CmColor;
			this.LEyeColor = ColorData.GetEyeColors()[actor.Customize.LeftEyeColor].CmColor;
			this.REyeColor = ColorData.GetEyeColors()[actor.Customize.RightEyeColor].CmColor;
			this.FacePaintColor = ColorData.GetFacePaintColor()[actor.Customize.FacePaintColor].CmColor;
			this.FacialFeatureColor = ColorData.GetLimbalColors()[actor.Customize.FacialFeatureColor].CmColor;

			// Everyone except hrothgar get lip color:
			if (actor.Customize.TribeId != ActorCustomizeMemory.Tribes.Helions && actor.Customize.TribeId != ActorCustomizeMemory.Tribes.TheLost)
			{
				this.LipColor = ColorData.GetLipColors()[actor.Customize.LipsToneFurPattern].CmColor;
			}
		}
	}
}
