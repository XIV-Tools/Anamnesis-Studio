// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

public class ValueMemory<T> : MemoryBase
{
#pragma warning disable CS8618
	[Bind(0x000)] public T Value { get => this.GetValue<T>(); set => this.SetValue(value); }
#pragma warning restore CS8618
}
