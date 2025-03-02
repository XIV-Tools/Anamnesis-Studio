﻿// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

public class WeaponModelMemory : MemoryBase
{
	[Bind(0x050)] public TransformMemory? Transform { get => this.GetValue<TransformMemory?>(); set => this.SetValue(value); }
	[Bind(0x0A0, BindFlags.Pointer | BindFlags.OnlyInGPose)] public SkeletonMemory? Skeleton { get => this.GetValue<SkeletonMemory?>(); set => this.SetValue(value); }
}
