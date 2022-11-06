// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Posing;

using Anamnesis.Memory;
using Serilog;
using System;
using System.Collections.Generic;

public struct BoneReference : IEquatable<BoneReference>
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

	public static bool operator ==(BoneReference left, BoneReference right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(BoneReference left, BoneReference right)
	{
		return !(left == right);
	}

	public void SetPosition(Vector newPosition, ref HashSet<TransformMemory> writtenMemories)
	{
		Vector delta = newPosition - this.Position;
		Change(this, null, this, delta, Vector.Zero, Quaternion.Identity, ref writtenMemories);
	}

	public void SetRotation(Quaternion newRotation, ref HashSet<TransformMemory> writtenMemories)
	{
		Quaternion delta = this.Rotation.Invert() * newRotation;
		Change(this, null, this, Vector.Zero, Vector.Zero, delta, ref writtenMemories);
	}

	public void SetScale(Vector newScale, ref HashSet<TransformMemory> writtenMemories)
	{
		Vector delta = newScale - this.Scale;
		Change(this, null, this, Vector.Zero, delta, Quaternion.Identity, ref writtenMemories);
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

	public override bool Equals(object? obj)
	{
		return obj is BoneReference reference && this.Equals(reference);
	}

	public bool Equals(BoneReference other)
	{
		return EqualityComparer<SkeletonMemory>.Default.Equals(this.skeleton, other.skeleton) &&
			   this.partialSkeletonIndex == other.partialSkeletonIndex &&
			   this.poseIndex == other.poseIndex &&
			   this.boneIndex == other.boneIndex;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(this.skeleton, this.partialSkeletonIndex, this.poseIndex, this.boneIndex);
	}

	private static void Change(BoneReference bone, BoneReference? parent, BoneReference source, Vector deltaPosition, Vector deltaScale, Quaternion deltaRotation, ref HashSet<TransformMemory> writtenMemories, int depth = 0)
	{
		if (bone.Transform == null)
			return;

		if (!bone.Transform.IsValid())
		{
			Log.Verbose($"Bone: {bone.Name} transform is not valid for bone change");
			return;
		}

		// Have we already written to this bone? then abort.
		if (writtenMemories.Contains(bone.Transform))
			return;

		writtenMemories.Add(bone.Transform);

		if (App.Services.Pose.FreezePositions)
			bone.Transform.Position += deltaPosition;

		if (App.Services.Pose.FreezeScale)
			bone.Transform.Scale += deltaScale;

		if (App.Services.Pose.FreezeRotation)
			bone.Transform.Rotation *= deltaRotation;

		if (depth > 1000)
		{
			Log.Error("Bone stack depth exceded! (do we have circular bone references?)");
			return;
		}

		if (App.Services.Pose.FreezePositions && bone != source)
		{
			Vector offset = bone.Transform.Position - source.Position;
			float originalLength = offset.Length;

			// Hack, but sometimes bones have weird values.
			if (originalLength < 100)
			{
				offset = (Quaternion.Identity * deltaRotation) * offset;

				if (!Float.IsApproximately(offset.Length, originalLength))
				{
					Log.Warning($"Bone {bone.Name} rotation resulted in a different offset length: got {offset.Length} expected: {originalLength} delta: {Math.Abs(originalLength - offset.Length)}");
				}
				else
				{
					bone.Transform.Position = source.Position + offset;
				}
			}
		}

		if (App.Services.Pose.EnableParenting)
		{
			List<BoneReference>? children = bone.GetChildren();
			if (children == null)
				return;

			depth++;

			foreach (BoneReference child in children)
			{
				Change(child, bone, source, deltaPosition, deltaScale, deltaRotation, ref writtenMemories, depth);
			}
		}
	}
}