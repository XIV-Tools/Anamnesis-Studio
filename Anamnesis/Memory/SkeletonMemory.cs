// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

using Anamnesis.Actor.Posing;
using System.Collections.Generic;

public class SkeletonMemory : ArrayMemory<PartialSkeletonMemory, short>
{
	public override int AddressOffset => 0x068;
	public override int CountOffset => 0x050;
	public override int ElementSize => 448;

	public BoneViewModel? GetBone(string boneName)
	{
		List<BoneReference> bones = this.FindBones(boneName);

		if (bones.Count <= 0)
			return null;

		return new BoneViewModel(boneName, bones);
	}

	public List<BoneReference> FindBones(string boneName)
	{
		List<BoneReference> bones = new();
		for (int i = 0; i < this.Count; i++)
		{
			bones.AddRange(this[i].GetBone(boneName, this, i));
		}

		return bones;
	}
}
