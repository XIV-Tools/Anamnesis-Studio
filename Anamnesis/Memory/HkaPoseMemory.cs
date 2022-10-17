// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

using Anamnesis.Actor;
using Anamnesis.Actor.Posing;
using Anamnesis.Posing;

public class HkaPoseMemory : MemoryBase
{
	[Bind(0x000, BindFlags.Pointer)] public HkaSkeletonMemory? Skeleton { get; set; }
	[Bind(0x010)] public TransformArrayMemory? Transforms { get; set; }

	public BoneReference? GetBone(string name)
	{
		if (this.Transforms == null || this.Skeleton == null || this.Skeleton.Bones == null)
			return null;

		for (int i = 0; i < this.Skeleton.Bones.Count; i++)
		{
			string boneName = this.Skeleton.Bones[i].Name.ToString();

			if (boneName == name)
			{
				return new BoneReference(this, i);
			}
		}

		return null;
	}

	protected override void HandlePropertyChanged(PropertyChange change)
	{
		// Big hack to keep bone change history names short.
		if (change.Origin == PropertyChange.Origins.User && change.TopPropertyName == nameof(this.Transforms))
		{
			change.Name = "[History_ChangeBone]";
		}

		base.HandlePropertyChanged(change);
	}

	public class TransformArrayMemory : ArrayMemory<TransformMemory, int>
	{
		public override int CountOffset => 0x000;
		public override int AddressOffset => 0x008;
		public override int ElementSize => 0x030;
	}
}
