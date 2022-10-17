// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Posing;

using Anamnesis.Memory;

public struct BoneReference
{
	public BoneReference(HkaPoseMemory pose, int index)
	{
		this.Pose = pose;
		this.Index = index;
	}

	public HkaPoseMemory Pose { get; init; }
	public int Index { get; set; }

	public HkaSkeletonMemory? Skeleton => this.Pose.Skeleton;
	public TransformMemory? Transform => this.Pose.Transforms?[this.Index];

	public Vector Position => this.Transform?.Position ?? Vector.Zero;
	public Quaternion Rotation => this.Transform?.Rotation ?? Quaternion.Identity;
	public Vector Scale => this.Transform?.Scale ?? Vector.One;

	public void SetPosition(Vector newPosition)
	{
		if (this.Transform == null)
			return;

		this.Transform.Position = newPosition;
	}

	public void SetRotation(Quaternion newRotation)
	{
		if (this.Transform == null)
			return;

		this.Transform.Rotation = newRotation;
	}

	public void SetScale(Vector newScale)
	{
		if (this.Transform == null)
			return;

		this.Transform.Scale = newScale;
	}
}