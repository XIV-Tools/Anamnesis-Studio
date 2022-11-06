// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

using System;
using Anamnesis.GameData;
using PropertyChanged;

public class WeaponMemory : MemoryBase, IEquipmentItemMemory
{
	[Flags]
	public enum WeaponFlagDefs : byte
	{
		WeaponHidden = 1 << 1,
	}

	[Bind(0x000, BindFlags.ActorRefresh)] public ushort Set { get => this.GetValue<ushort>(); set => this.SetValue(value); }
	[Bind(0x002, BindFlags.ActorRefresh)] public ushort Base { get => this.GetValue<ushort>(); set => this.SetValue(value); }
	[Bind(0x004, BindFlags.ActorRefresh)] public ushort Variant { get => this.GetValue<ushort>(); set => this.SetValue(value); }
	[Bind(0x006, BindFlags.ActorRefresh)] public byte Dye { get => this.GetValue<byte>(); set => this.SetValue(value); }
	[Bind(0x008, BindFlags.Pointer)] public WeaponModelMemory? Model { get => this.GetValue<WeaponModelMemory?>(); set => this.SetValue(value); }
	[Bind(0x040)] public bool IsSheathed { get => this.GetValue<bool>(); set => this.SetValue(value); }
	[Bind(0x05C)] public WeaponFlagDefs WeaponFlags { get => this.GetValue<WeaponFlagDefs>(); set => this.SetValue(value); }

	[DependsOn(nameof(WeaponFlags), nameof(IsSheathed))]
	public bool WeaponHidden
	{
		get => (this.IsSheathed && this.WeaponFlags.HasFlag(WeaponFlagDefs.WeaponHidden)) || (!this.IsSheathed && this.Model?.Transform?.Scale == Vector.Zero);
		set
		{
			if (value)
			{
				this.WeaponFlags |= WeaponFlagDefs.WeaponHidden;
			}
			else
			{
				this.WeaponFlags &= ~WeaponFlagDefs.WeaponHidden;
			}

			if (this.Model?.Transform == null)
				return;

			// If the weapon is unsheathed (in hands) the visibility flag won't work,
			// so fall back to setting the weapons scale to 0.
			if (!this.IsSheathed)
			{
				this.Model.Transform.Scale = value ? Vector.Zero : Vector.One;
			}

			// Special handling for a weapon with 0 scale that has been sheathed attempting to un-hide
			else if (!value && this.Model.Transform.Scale == Vector.Zero)
			{
				this.Model.Transform.Scale = Vector.One;
			}
		}
	}

	public void Equip(IItem item)
	{
		this.Set = item.ModelSet;
		this.Base = item.ModelBase;
		this.Variant = item.ModelVariant;
	}

	public bool Is(IItem? item)
	{
		if (item == null)
			return this.Set == 0 && this.Base == 0 && this.Variant == 0;

		return this.Set == item.ModelSet
			&& this.Base == item.ModelBase
			&& this.Variant == item.ModelVariant;
	}
}
