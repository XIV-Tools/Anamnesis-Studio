// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Posing;

using Anamnesis.Actor.Extensions;
using Anamnesis.Core;
using Anamnesis.Memory;
using System.Collections.Generic;

public struct BoneReference
{
	private readonly HkaPoseMemory pose;
	private readonly int index;

	public BoneReference(HkaPoseMemory pose, int index)
	{
		this.pose = pose;
		this.index = index;
	}

	public TransformMemory? Transform => this.pose.Transforms?[this.index];

	public Vector Position => this.Transform?.Position ?? Vector.Zero;
	public Quaternion Rotation => this.Transform?.Rotation ?? Quaternion.Identity;
	public Vector Scale => this.Transform?.Scale ?? Vector.One;

	public Matrix Matrix
	{
		get
		{
			Matrix m = new();
			m.Translate(this.Position);
			////m.Rotate(this.Rotation);
			////m.Scale(this.Scale);
			return m;
		}

		set
		{
			if (this.Transform == null)
				return;

			var newPos = value.TransformPoint(Vector.Zero);
			this.Transform.Position = newPos;
		}
	}

	public void SetPosition(Vector newPosition)
	{
		Vector delta = newPosition - this.Position;
		this.ChangePosition(delta);
	}

	public void ChangePosition(Vector delta)
	{
		Matrix m = this.Matrix;
		m.Translate(delta);
		this.Matrix = m;

		/*if (this.Transform == null)
			return;

		this.Transform.Position += delta;

		List<BoneReference>? children = this.GetChildren();
		if (children == null)
			return;

		foreach (BoneReference child in children)
		{
			child.ChangePosition(delta);
		}*/
	}

	public void SetRotation(Quaternion newRotation)
	{
	}

	public void ChangeRotation(Quaternion delta)
	{
		if (this.Transform == null)
			return;

		this.Transform.Rotation *= delta;
	}

	public void SetScale(Vector newScale)
	{
		Vector delta = newScale - this.Scale;
		this.ChangeScale(delta);
	}

	public void ChangeScale(Vector delta)
	{
		if (this.Transform == null)
			return;

		this.Transform.Scale += delta;

		List<BoneReference>? children = this.GetChildren();
		if (children == null)
			return;

		foreach (BoneReference child in children)
		{
			child.ChangeScale(delta);
		}
	}

	public BoneReference? GetParent()
	{
		int? parentIndex = this.pose.Skeleton?.ParentIndices?[this.index];
		if (parentIndex == -1 || parentIndex == null)
			return null;

		return new BoneReference(this.pose, (int)parentIndex);
	}

	public List<BoneReference>? GetChildren()
	{
		if (this.pose.Skeleton?.ParentIndices == null)
			return null;

		List<BoneReference> results = new();

		for (int childIndex = 0; childIndex < this.pose.Skeleton.ParentIndices.Count; childIndex++)
		{
			if (this.pose.Skeleton.ParentIndices[childIndex] == this.index)
			{
				results.Add(new BoneReference(this.pose, childIndex));
			}
		}

		return results;
	}
}