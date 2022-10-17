// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

using Anamnesis.Actor.Posing;
using System.Collections.Generic;
using System.Windows.Documents;

public class SkeletonMemory : ArrayMemory<PartialSkeletonMemory, short>
{
	public override int AddressOffset => 0x068;
	public override int CountOffset => 0x050;
	public override int ElementSize => 448;

	public BoneViewModel? GetBone(string boneName)
	{
		List<BoneReference> transforms = new();
		foreach (PartialSkeletonMemory partialSkeleton in this)
		{
			transforms.AddRange(partialSkeleton.GetBone(boneName));
		}

		if (transforms.Count <= 0)
			return null;

		return new BoneViewModel(boneName, transforms);
	}
}
