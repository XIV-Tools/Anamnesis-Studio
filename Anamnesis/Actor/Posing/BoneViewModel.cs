// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Posing;

using Anamnesis.Memory;
using PropertyChanged;
using System.Collections.Generic;

[AddINotifyPropertyChangedInterface]
public class BoneViewModel : ITransform
{
	private readonly List<TransformMemory> transforms;

	public BoneViewModel(string name, List<TransformMemory> transforms)
	{
		this.Name = name;
		this.transforms = transforms;
	}

	public string Name { get; init; }

	public bool IsSelected { get; set; }

	public bool CanTranslate => true;
	public bool CanRotate => true;
	public bool CanScale => true;
	public bool CanLinkScale => true;
	public bool ScaleLinked { get; set; } = true;

	public bool CanEdit => this.CanTranslate || this.CanRotate || this.CanScale;

	public Vector Position
	{
		get
		{
			if (this.transforms.Count <= 0)
				return Vector.Zero;

			return this.transforms[0].Position;
		}

		set
		{
			foreach (TransformMemory transform in this.transforms)
			{
				transform.Position = value;
			}
		}
	}

	public Quaternion Rotation
	{
		get
		{
			if (this.transforms.Count <= 0)
				return Quaternion.Identity;

			return this.transforms[0].Rotation;
		}

		set
		{
			foreach (TransformMemory transform in this.transforms)
			{
				transform.Rotation = value;
			}
		}
	}

	public Vector Scale
	{
		get
		{
			if (this.transforms.Count <= 0)
				return Vector.One;

			return this.transforms[0].Scale;
		}

		set
		{
			foreach (TransformMemory transform in this.transforms)
			{
				transform.Scale = value;
			}
		}
	}
}