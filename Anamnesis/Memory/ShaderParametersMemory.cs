// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

using PropertyChanged;

public class ShaderParametersMemory : MemoryBase
{
	[Bind(0x00)] public Color SkinColor { get => this.GetValue<Color>(); set => this.SetValue(value); }
	[Bind(0x0C)] public float MuscleTone { get => this.GetValue<short>(); set => this.SetValue(value); }
	[Bind(0x10)] public Color SkinGloss { get => this.GetValue<Color>(); set => this.SetValue(value); }
	[Bind(0x20)] public Color4 MouthColor { get => this.GetValue<Color4>(); set => this.SetValue(value); }
	[Bind(0x30)] public Color HairColor { get => this.GetValue<Color>(); set => this.SetValue(value); }
	[Bind(0x40)] public Color HairGloss { get => this.GetValue<Color>(); set => this.SetValue(value); }
	[Bind(0x50)] public Color HairHighlight { get => this.GetValue<Color>(); set => this.SetValue(value); }
	[Bind(0x60)] public Color LeftEyeColor { get => this.GetValue<Color>(); set => this.SetValue(value); }
	[Bind(0x70)] public Color RightEyeColor { get => this.GetValue<Color>(); set => this.SetValue(value); }
	[Bind(0x80)] public Color LimbalRingColor { get => this.GetValue<Color>(); set => this.SetValue(value); }

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
		nameof(this.FreezeSkinColor),
		nameof(this.FreezeMuscleTone),
		nameof(this.FreezeSkinGloss),
		nameof(this.FreezeMouthColor),
		nameof(this.FreezeHairColor),
		nameof(this.FreezeHairGloss),
		nameof(this.FreezeHairHighlight),
		nameof(this.FreezeLeftEyeColor),
		nameof(this.FreezeRightEyeColor),
		nameof(this.FreezeLimbalRingColor))]
	public bool FreezeAll
	{
		get
		{
			bool allFrozen = true;
			allFrozen &= this.FreezeSkinColor;
			allFrozen &= this.FreezeMuscleTone;
			allFrozen &= this.FreezeSkinGloss;
			allFrozen &= this.FreezeMouthColor;
			allFrozen &= this.FreezeHairColor;
			allFrozen &= this.FreezeHairGloss;
			allFrozen &= this.FreezeHairHighlight;
			allFrozen &= this.FreezeLeftEyeColor;
			allFrozen &= this.FreezeRightEyeColor;
			allFrozen &= this.FreezeLimbalRingColor;
			return allFrozen;
		}
		set
		{
			this.FreezeSkinColor = value;
			this.FreezeMuscleTone = value;
			this.FreezeSkinGloss = value;
			this.FreezeMouthColor = value;
			this.FreezeHairColor = value;
			this.FreezeHairGloss = value;
			this.FreezeHairHighlight = value;
			this.FreezeLeftEyeColor = value;
			this.FreezeRightEyeColor = value;
			this.FreezeLimbalRingColor = value;
		}
	}
}
