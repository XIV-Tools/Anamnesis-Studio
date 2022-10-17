// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Posing;

using Anamnesis.Actor.Extensions;
using Anamnesis.Core;
using Anamnesis.Memory;
using Serilog;
using System.Collections.Generic;

public struct BoneReference
{
	private readonly SkeletonMemory skeleton;
	private readonly int partialSkeletonIndex;
	private readonly int poseIndex;
	private readonly int boneIndex;

	public BoneReference(SkeletonMemory skeleton, int partialSkeletonIndex, int poseIndex, int boneIndex)
	{
		this.skeleton = skeleton;
		this.partialSkeletonIndex = partialSkeletonIndex;
		this.poseIndex = poseIndex;
		this.boneIndex = boneIndex;
	}

	public SkeletonMemory Skeleton => this.skeleton;
	public PartialSkeletonMemory? PartialSkeleton => this.skeleton[this.partialSkeletonIndex];
	public HkaPoseMemory? HkaPose => this.PartialSkeleton?[this.poseIndex];
	public TransformMemory? Transform => this.HkaPose?.Transforms?[this.boneIndex];

	public Vector Position => this.Transform?.Position ?? Vector.Zero;
	public Quaternion Rotation => this.Transform?.Rotation ?? Quaternion.Identity;
	public Vector Scale => this.Transform?.Scale ?? Vector.One;

	public string? Name => this.HkaPose?.Skeleton?.Bones?[this.boneIndex]?.Name.ToString();

	public void SetPosition(Vector newPosition, ref HashSet<TransformMemory> writtenMemories)
	{
		Vector delta = newPosition - this.Position;
		this.Change(delta, Vector.Zero, Quaternion.Identity, this.Position, ref writtenMemories);
	}

	public void SetRotation(Quaternion newRotation, ref HashSet<TransformMemory> writtenMemories)
	{
		// ??
		Quaternion delta = Quaternion.Identity;
		this.Change(Vector.Zero, Vector.Zero, delta, this.Position, ref writtenMemories);
	}

	public void SetScale(Vector newScale, ref HashSet<TransformMemory> writtenMemories)
	{
		Vector delta = newScale - this.Scale;
		this.Change(Vector.Zero, delta, Quaternion.Identity, this.Position, ref writtenMemories);
	}

	public void Change(Vector deltaPosition, Vector deltaScale, Quaternion deltaRotation, Vector root, ref HashSet<TransformMemory> writtenMemories)
	{
		if (this.Transform == null)
			return;

		// Have we already written to this bone? then abort.
		if (writtenMemories.Contains(this.Transform))
			return;

		writtenMemories.Add(this.Transform);

		// This matrix stuff is _hard_ girls.
		// It /might/ end up easier for me to just do all the math by hand.... But I'm not sure...
		// Get current matrix
		/*Matrix m = new();
		m.Scale(this.Scale);
		m.Rotate(this.Rotation);
		m.Translate(this.Position);

		// Add the new deltas
		m.ScaleAt(deltaScale, root);
		m.RotateAt(deltaRotation, root);
		m.Translate(deltaPosition);*/

		this.Transform.Position += deltaPosition; ////m.TransformPoint(Vector.Zero);
		this.Transform.Scale += deltaScale;
		this.Transform.Rotation *= deltaRotation;

		List<BoneReference>? children = this.GetChildren();
		if (children == null)
			return;

		foreach (BoneReference child in children)
		{
			child.Change(deltaPosition, deltaScale, deltaRotation, root, ref writtenMemories);
		}
	}

	public BoneReference? GetParent()
	{
		int? parentIndex = this.HkaPose?.Skeleton?.ParentIndices?[this.boneIndex];
		if (parentIndex == -1 || parentIndex == null)
			return null;

		return new BoneReference(this.skeleton, this.partialSkeletonIndex, this.poseIndex, (int)parentIndex);
	}

	public List<BoneReference>? GetChildren()
	{
		// We really might want to cache this for some duration, this is _expensive_
		// However, due to the nature of our memory lookups, theres no guarantee the
		// Skeleton will stay in one shape, especailly not with new actor redraw methods...
		if (this.HkaPose?.Skeleton?.ParentIndices == null || this.HkaPose?.Skeleton?.Bones == null)
			return null;

		List<BoneReference> results = new();

		for (int childIndex = 0; childIndex < this.HkaPose.Skeleton.ParentIndices.Count; childIndex++)
		{
			if (this.HkaPose.Skeleton.ParentIndices[childIndex] == this.boneIndex)
			{
				string childBoneName = this.HkaPose.Skeleton.Bones[childIndex].Name.ToString();

				// This _might_ be a tad wonky, but the alternative is manual and pain.
				// We need to support 'children' bones that are actually in other partial skeletons.
				// the only way we currently understand th elinkages between partial skeletons is that
				// the bones inside a partial skeleton will have the same name where they overlap.
				// i.e. j_kao is the face root bone that is in the body and face partial skeletons.
				results.AddRange(this.skeleton.FindBones(childBoneName));

				// Adding only the children within this partial skeleton will result in changes not propogating
				// into other partial skeletons, for example, head rotations really need to move the face bones.
				////results.Add(new BoneReference(this.skeleton, this.partialSkeletonIndex, this.poseIndex, childIndex));
			}
		}

		return results;
	}
}