// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Posing;

using Anamnesis.Memory;
using PropertyChanged;
using System;
using System.Collections.Generic;

[AddINotifyPropertyChangedInterface]
public class BoneViewModel : ITransform
{
	private readonly List<BoneReference> boneReferences;

	public BoneViewModel(string name, List<BoneReference> bones)
	{
		this.Name = name;
		this.boneReferences = bones;
	}

	public string Name { get; init; }

	public bool IsSelected { get; set; }

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
			foreach (BoneReference bone in this.boneReferences)
			{
				bone.SetPosition(value);
			}
		}
	}

	public Quaternion Rotation
	{
		get => this.PrimaryBone.Rotation;
		set
		{
			foreach (BoneReference bone in this.boneReferences)
			{
				bone.SetRotation(value);
			}
		}
	}

	public Vector Scale
	{
		get => this.PrimaryBone.Scale;
		set
		{
			foreach (BoneReference bone in this.boneReferences)
			{
				bone.SetScale(value);
			}
		}
	}
}
