// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Posing;

using Anamnesis.Memory;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

[AddINotifyPropertyChangedInterface]
public class BoneViewModel : ITransform, INotifyPropertyChanged
{
	private readonly List<BoneReference> boneReferences;

	public BoneViewModel(SkeletonMemory skeleton, string name, List<BoneReference> bones)
	{
		this.Skeleton = skeleton;
		this.Name = name;
		this.boneReferences = bones;

		Task.Run(this.Test);
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	public ActorMemory? Actor => this.Model?.Parent as ActorMemory;
	public ActorModelMemory? Model => this.Skeleton.Parent as ActorModelMemory;
	public SkeletonMemory Skeleton { get; init; }
	public string Name { get; init; }
	public string LocalizedBoneName => $"[Pose_{this.Name}]";

	public bool CanTranslate => true;
	public bool CanRotate => true;
	public bool CanScale => true;
	public bool CanLinkScale => true;
	public bool ScaleLinked { get; set; } = true;

	public bool CanEdit => this.CanTranslate || this.CanRotate || this.CanScale;

	public BoneReference PrimaryBone
	{
		get
		{
			if (this.boneReferences.Count <= 0)
				throw new Exception("No bone references in bone view model");

			return this.boneReferences[0];
		}
	}

	public Vector Position
	{
		get => this.PrimaryBone.Position;
		set
		{
			HashSet<TransformMemory> writtenMemories = new();
			foreach (BoneReference bone in this.boneReferences)
			{
				bone.SetPosition(value, ref writtenMemories);
			}
		}
	}

	public Quaternion WorldRotation
	{
		// ModelRotation Euler Z seems to be inverted for our needs.
		// I don't know why.
		get => this.Rotation * (Quaternion.Identity * this.ModelRotation.Invert());
		set
		{
			this.Rotation = value * (this.ModelRotation * Quaternion.Identity.Invert());
		}
	}

	public Quaternion Rotation
	{
		get => this.PrimaryBone.Rotation;
		set
		{
			HashSet<TransformMemory> writtenMemories = new();
			foreach (BoneReference bone in this.boneReferences)
			{
				bone.SetRotation(value, ref writtenMemories);
			}
		}
	}

	public Vector Scale
	{
		get => this.PrimaryBone.Scale;
		set
		{
			HashSet<TransformMemory> writtenMemories = new();
			foreach (BoneReference bone in this.boneReferences)
			{
				bone.SetScale(value, ref writtenMemories);
			}
		}
	}

	public Quaternion ModelRotation
	{
		get => (this.Skeleton.Parent as ActorModelMemory)?.Transform?.Rotation ?? Quaternion.Identity;
	}

	public List<BoneViewModel>? GetChildren()
	{
		HashSet<string> childrenNames = new();
		foreach (BoneReference bone in this.boneReferences)
		{
			List<BoneReference>? childrenReferences = bone.GetChildren();
			if (childrenReferences == null)
				continue;

			foreach (BoneReference childReference in childrenReferences)
			{
				if (childReference.Name == null)
					continue;

				childrenNames.Add(childReference.Name);
			}
		}

		if (childrenNames.Count <= 0)
			return null;

		List<BoneViewModel> children = new();
		foreach (string boneName in childrenNames)
		{
			BoneViewModel? bone = this.Skeleton.GetBone(boneName);

			if (bone == null)
				continue;

			children.Add(bone);
		}

		return children;
	}

	public BoneViewModel? GetParent()
	{
		List<BoneReference> parents = new();
		string name = string.Empty;
		foreach (BoneReference bone in this.boneReferences)
		{
			BoneReference? parent = bone.GetParent();

			if (parent == null)
				continue;

			parents.Add(parent.Value);

			if (parent.Value.Name != null)
			{
				name = parent.Value.Name;
			}
		}

		if (parents.Count <= 0)
			return null;

		return new BoneViewModel(this.Skeleton, name, parents);
	}

	private async Task Test()
	{
		while (true)
		{
			await Task.Delay(33);
			this.PropertyChanged?.Invoke(this, new(nameof(this.WorldRotation)));
		}
	}
}
