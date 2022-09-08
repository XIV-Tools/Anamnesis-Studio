// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

using System;
using System.Security.AccessControl;
using Anamnesis.GameData.Excel;
using Anamnesis.Services;
using PropertyChanged;

public class ActorCustomizeMemory : MemoryBase
{
	public enum Genders : byte
	{
		Masculine = 0,
		Feminine = 1,
	}

	public enum Races : byte
	{
		Hyur = 1,
		Elezen = 2,
		Lalafel = 3,
		Miqote = 4,
		Roegadyn = 5,
		AuRa = 6,
		Hrothgar = 7,
		Viera = 8,
	}

	public enum Tribes : byte
	{
		Midlander = 1,
		Highlander = 2,
		Wildwood = 3,
		Duskwight = 4,
		Plainsfolk = 5,
		Dunesfolk = 6,
		SeekerOfTheSun = 7,
		KeeperOfTheMoon = 8,
		SeaWolf = 9,
		Hellsguard = 10,
		Raen = 11,
		Xaela = 12,
		Helions = 13,
		TheLost = 14,
		Rava = 15,
		Veena = 16,
	}

	public enum Ages : byte
	{
		Normal = 1,
		Old = 3,
		Young = 4,
	}

	[Flags]
	public enum FacialFeature : byte
	{
		None = 0,
		First = 1,
		Second = 2,
		Third = 4,
		Fourth = 8,
		Fifth = 16,
		Sixth = 32,
		Seventh = 64,
		LegacyTattoo = 128,
	}

	/*public static Customize Default()
	{
		Customize ap = default;

		ap.Race = Races.Hyur;
		ap.Gender = Genders.Feminine;

		ap.Age = Ages.Normal;
		ap.Tribe = Tribes.Midlander;

		return ap;
	}*/

	[Bind(0x000, BindFlags.ActorRefresh)] public Races RaceId { get; set; }
	[Bind(0x001, BindFlags.ActorRefresh)] public Genders Gender { get; set; }
	[Bind(0x002, BindFlags.ActorRefresh)] public Ages Age { get; set; }
	[Bind(0x003, BindFlags.ActorRefresh)] public byte Height { get; set; }
	[Bind(0x004, BindFlags.ActorRefresh)] public Tribes TribeId { get; set; }
	[Bind(0x005, BindFlags.ActorRefresh)] public byte Face { get; set; }
	[Bind(0x006, BindFlags.ActorRefresh)] public byte Hair { get; set; }
	[Bind(0x007, BindFlags.ActorRefresh)] public byte HighlightType { get; set; }
	[Bind(0x008, BindFlags.ActorRefresh)] public byte Skintone { get; set; }
	[Bind(0x009, BindFlags.ActorRefresh)] public byte REyeColor { get; set; }
	[Bind(0x00a, BindFlags.ActorRefresh)] public byte HairTone { get; set; }
	[Bind(0x00b, BindFlags.ActorRefresh)] public byte Highlights { get; set; }
	[Bind(0x00c, BindFlags.ActorRefresh)] public FacialFeature FacialFeatures { get; set; }
	[Bind(0x00d, BindFlags.ActorRefresh)] public byte FacialFeatureColor { get; set; }
	[Bind(0x00e, BindFlags.ActorRefresh)] public byte Eyebrows { get; set; }
	[Bind(0x00f, BindFlags.ActorRefresh)] public byte LEyeColor { get; set; }
	[Bind(0x010, BindFlags.ActorRefresh)] public byte Eyes { get; set; }
	[Bind(0x011, BindFlags.ActorRefresh)] public byte Nose { get; set; }
	[Bind(0x012, BindFlags.ActorRefresh)] public byte Jaw { get; set; }
	[Bind(0x013, BindFlags.ActorRefresh)] public byte Mouth { get; set; }
	[Bind(0x014, BindFlags.ActorRefresh)] public byte LipsToneFurPattern { get; set; }
	[Bind(0x015, BindFlags.ActorRefresh)] public byte EarMuscleTailSize { get; set; }
	[Bind(0x016, BindFlags.ActorRefresh)] public byte TailEarsType { get; set; }
	[Bind(0x017, BindFlags.ActorRefresh)] public byte Bust { get; set; }
	[Bind(0x018, BindFlags.ActorRefresh)] public byte FacePaint { get; set; }
	[Bind(0x019, BindFlags.ActorRefresh)] public byte FacePaintColor { get; set; }

	public bool EnableHighlights
	{
		get => this.HighlightType != 0;
		set => this.HighlightType = value ? (byte)128 : (byte)0;
	}

	public byte Lips
	{
		get => (byte)(this.EnableLipColor ? this.Mouth - 128 : this.Mouth);
		set => this.Mouth = (byte)(this.EnableLipColor ? value - 128 : value);
	}

	public bool EnableLipColor
	{
		get => this.Mouth >= 128;
		set => this.Mouth = (byte)(this.Lips + (value ? 128 : 0));
	}

	[AlsoNotifyFor(nameof(LEyeColor), nameof(REyeColor))]
	public bool LinkEyeColors { get; set; }

	[AlsoNotifyFor(nameof(LEyeColor))]
	public byte MainEyeColor
	{
		get => this.LEyeColor;
		set
		{
			if (this.LinkEyeColors)
				this.REyeColor = value;

			this.LEyeColor = value;
		}
	}

	public bool SmallIris
	{
		get => this.Eyes > 128;
		set => this.Eyes = (byte)(this.EyeShape + (value ? 128 : 0));
	}

	[AlsoNotifyFor(nameof(Eyes))]
	public byte EyeShape
	{
		get => (byte)(this.Eyes - (this.SmallIris ? 128 : 0));
		set => this.Eyes = (byte)(value + (this.SmallIris ? 128 : 0));
	}

	[AlsoNotifyFor(nameof(RaceId))]
	public Race? Race
	{
		get => GameDataService.Instance.Races.Get((byte)this.RaceId);
		set
		{
			this.RaceId = (Races)(value?.RowId ?? 0);

			if (value != null)
			{
				// Validate tribe
				if (this.Tribe == null || !value.Tribes.Contains(this.Tribe))
					this.Tribe = value.Tribes[0];

				// Validate gender
				if (!value.Genders.Contains(this.Gender))
					this.Gender = value.Genders[0];

				// Validate Age
				if (!this.CanAge)
					this.Age = Ages.Normal;
			}
		}
	}

	[AlsoNotifyFor(nameof(RaceId))]
	public Tribe? Tribe
	{
		get => GameDataService.Instance.Tribes.Get((byte)this.TribeId);
		set => this.TribeId = (Tribes)(value?.RowId ?? 0);
	}

	public bool CanAge
	{
		get
		{
			bool canAge = this.TribeId == Tribes.Midlander;
			canAge |= this.RaceId == Races.Miqote && this.Gender == Genders.Feminine;
			canAge |= this.RaceId == Races.Elezen;
			canAge |= this.RaceId == Races.AuRa;
			return canAge;
		}
	}

	[AlsoNotifyFor(nameof(RaceId), nameof(TribeId), nameof(Gender))]
	public CharaMakeType? MakeType
	{
		get
		{
			foreach (CharaMakeType set in GameDataService.Instance.CharacterMakeTypes)
			{
				if (set.Tribe != this.TribeId || set.Gender != this.Gender)
					continue;

				return set;
			}

			return null;
		}
	}
}
