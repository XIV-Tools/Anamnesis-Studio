// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

using PropertyChanged;

public class ShaderParametersMemory : MemoryBase
{
	[Bind(0x00)] public Color SkinColor { get; set; }
	[Bind(0x0C)] public float MuscleTone { get; set; }
	[Bind(0x10)] public Color SkinGloss { get; set; }
	[Bind(0x20)] public Color4 MouthColor { get; set; }
	[Bind(0x30)] public Color HairColor { get; set; }
	[Bind(0x40)] public Color HairGloss { get; set; }
	[Bind(0x50)] public Color HairHighlight { get; set; }
	[Bind(0x60)] public Color LeftEyeColor { get; set; }
	[Bind(0x70)] public Color RightEyeColor { get; set; }
	[Bind(0x80)] public Color LimbalRingColor { get; set; }

	public bool FreezeSkinColor
	{
		get => this.IsFrozen(nameof(this.SkinColor));
		set => this.SetFrozen(nameof(this.SkinColor), value);
	}

	public bool FreezeMuscleTone
	{
		get => this.IsFrozen(nameof(this.MuscleTone));
		set => this.SetFrozen(nameof(this.MuscleTone), value);
	}

	public bool FreezeSkinGloss
	{
		get => this.IsFrozen(nameof(this.SkinGloss));
		set => this.SetFrozen(nameof(this.SkinGloss), value);
	}

	public bool FreezeMouthColor
	{
		get => this.IsFrozen(nameof(this.MouthColor));
		set => this.SetFrozen(nameof(this.MouthColor), value);
	}

	public bool FreezeHairColor
	{
		get => this.IsFrozen(nameof(this.HairColor));
		set => this.SetFrozen(nameof(this.HairColor), value);
	}

	public bool FreezeHairGloss
	{
		get => this.IsFrozen(nameof(this.HairGloss));
		set => this.SetFrozen(nameof(this.HairGloss), value);
	}

	public bool FreezeHairHighlight
	{
		get => this.IsFrozen(nameof(this.HairHighlight));
		set => this.SetFrozen(nameof(this.HairHighlight), value);
	}

	public bool FreezeLeftEyeColor
	{
		get => this.IsFrozen(nameof(this.LeftEyeColor));
		set => this.SetFrozen(nameof(this.LeftEyeColor), value);
	}

	public bool FreezeRightEyeColor
	{
		get => this.IsFrozen(nameof(this.RightEyeColor));
		set => this.SetFrozen(nameof(this.RightEyeColor), value);
	}

	public bool FreezeLimbalRingColor
	{
		get => this.IsFrozen(nameof(this.LimbalRingColor));
		set => this.SetFrozen(nameof(this.LimbalRingColor), value);
	}

	[AlsoNotifyFor(
		nameof(this.SkinColor),
		nameof(this.MuscleTone),
		nameof(this.SkinGloss),
		nameof(this.MouthColor),
		nameof(this.HairColor),
		nameof(this.HairGloss),
		nameof(this.HairHighlight),
		nameof(this.LeftEyeColor),
		nameof(this.RightEyeColor),
		nameof(this.LimbalRingColor))]
	public bool FreezeAll
	{
		get
		{
			bool allFrozen = true;
			allFrozen &= this.IsFrozen(nameof(this.SkinColor));
			allFrozen &= this.IsFrozen(nameof(this.MuscleTone));
			allFrozen &= this.IsFrozen(nameof(this.SkinGloss));
			allFrozen &= this.IsFrozen(nameof(this.MouthColor));
			allFrozen &= this.IsFrozen(nameof(this.HairColor));
			allFrozen &= this.IsFrozen(nameof(this.HairGloss));
			allFrozen &= this.IsFrozen(nameof(this.HairHighlight));
			allFrozen &= this.IsFrozen(nameof(this.LeftEyeColor));
			allFrozen &= this.IsFrozen(nameof(this.RightEyeColor));
			allFrozen &= this.IsFrozen(nameof(this.LimbalRingColor));
			return allFrozen;
		}
		set
		{
			this.SetFrozen(nameof(this.SkinColor), value);
			this.SetFrozen(nameof(this.MuscleTone), value);
			this.SetFrozen(nameof(this.SkinGloss), value);
			this.SetFrozen(nameof(this.MouthColor), value);
			this.SetFrozen(nameof(this.HairColor), value);
			this.SetFrozen(nameof(this.HairGloss), value);
			this.SetFrozen(nameof(this.HairHighlight), value);
			this.SetFrozen(nameof(this.LeftEyeColor), value);
			this.SetFrozen(nameof(this.RightEyeColor), value);
			this.SetFrozen(nameof(this.LimbalRingColor), value);
		}
	}
}
