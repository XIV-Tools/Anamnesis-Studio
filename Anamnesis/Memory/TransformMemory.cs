﻿// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

public class TransformMemory : MemoryBase
{
	[Bind(0x000)] public Vector Position { get; set; }
	[Bind(0x010)] public Quaternion Rotation { get; set; }
	[Bind(0x020)] public Vector Scale { get; set; }
}
