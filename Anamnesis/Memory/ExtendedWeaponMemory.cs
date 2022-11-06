// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

public class ExtendedWeaponMemory : WeaponSubModelMemory
{
	[Bind(0x028, BindFlags.Pointer)] public WeaponSubModelMemory? SubModel { get => this.GetValue<WeaponSubModelMemory?>(); set => this.SetValue(value); }
}
