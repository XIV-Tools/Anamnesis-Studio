// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

using Anamnesis.Actor.Posing;
using System;
using System.Collections.Generic;

public class PartialSkeletonMemory : MemoryBase
{
	[Bind(0x12C)] public short ConnectedBoneIndex { get => this.GetValue<short>(); set => this.SetValue(value); }
	[Bind(0x12E)] public short ConnectedParentBoneIndex { get => this.GetValue<short>(); set => this.SetValue(value); }

	[Bind(0x140, BindFlags.Pointer)] public HkaPoseMemory? Pose0 { get => this.GetValue<HkaPoseMemory?>(); set => this.SetValue(value); }
	[Bind(0x148, BindFlags.Pointer)] public HkaPoseMemory? Pose1 { get => this.GetValue<HkaPoseMemory?>(); set => this.SetValue(value); }
	[Bind(0x150, BindFlags.Pointer)] public HkaPoseMemory? Pose2 { get => this.GetValue<HkaPoseMemory?>(); set => this.SetValue(value); }
	[Bind(0x158, BindFlags.Pointer)] public HkaPoseMemory? Pose3 { get => this.GetValue<HkaPoseMemory?>(); set => this.SetValue(value); }

	public int Count => 4;

	public HkaPoseMemory? this[int index]
	{
		get
		{
			switch (index)
			{
				case 0: return this.Pose0;
				case 1: return this.Pose1;
				case 2: return this.Pose2;
				case 3: return this.Pose3;
			}

			throw new NotSupportedException();
		}
	}

	public List<BoneReference> GetBone(string name, SkeletonMemory skeleton, int partialSkeletonIndex)
	{
		List<BoneReference> bones = new();

		for (int i = 0; i < this.Count; i++)
		{
			BoneReference? poseBone = this[i]?.FindBone(name, skeleton, partialSkeletonIndex, i);
			if (poseBone != null)
			{
				bones.Add((BoneReference)poseBone);
			}
		}

		return bones;
	}
}
