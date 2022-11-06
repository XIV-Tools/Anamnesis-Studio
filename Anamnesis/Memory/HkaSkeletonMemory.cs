// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

using System;

public class HkaSkeletonMemory : MemoryBase
{
	[Bind(0x010, BindFlags.Pointer)] public Utf8String Name { get => this.GetValue<Utf8String>(); set => this.SetValue(value); }
	[Bind(0x018)] public ParentingArrayMemory? ParentIndices { get => this.GetValue<ParentingArrayMemory?>(); set => this.SetValue(value); }
	[Bind(0x028)] public BoneArrayMemory? Bones { get => this.GetValue<BoneArrayMemory?>(); set => this.SetValue(value); }

	public class ParentingArrayMemory : HkaArrayMemory<short>
	{
		public override int ElementSize => 2;
	}

	public class BoneArrayMemory : HkaArrayMemory<HkaBone>
	{
		public override int ElementSize => 16;
	}

	public abstract class HkaArrayMemory<T> : ArrayMemory<T, int>
	{
		public override int AddressOffset => 0x000;
		public override int CountOffset => 0x008;
	}

	public class HkaBone : MemoryBase
	{
		[Bind(0x000, BindFlags.Pointer)] public Utf8String Name { get; set; }

		public override void SetAddress(IntPtr address)
		{
			base.SetAddress(address);
		}
	}
}
