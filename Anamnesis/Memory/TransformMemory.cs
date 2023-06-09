// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

using System;

public class TransformMemory : MemoryBase, ITransform
{
	private PropertyBindInfo? positionBind;
	private PropertyBindInfo? rotationBind;
	private PropertyBindInfo? scaleBind;

	[Bind(0x000)] public Vector Position { get => this.GetValue<Vector>(); set => this.SetValue(value); }
	[Bind(0x010)] public Quaternion Rotation { get => this.GetValue<Quaternion>(); set => this.SetValue(value); }
	[Bind(0x020)] public Vector Scale { get => this.GetValue<Vector>(); set => this.SetValue(value); }

	public bool CanTranslate => true;
	public bool CanRotate => true;
	public bool CanScale => true;
	public bool CanLinkScale => true;
	public bool ScaleLinked { get; set; } = true;

	// Fast version accessors that bypass most safety checks and delayed writes.
	public Vector FastPosition
	{
		get => this.FastGetValue<Vector>(this.positionBind);
		set => this.FastSetValue(this.positionBind, value);
	}

	public Quaternion FastRotation
	{
		get => this.FastGetValue<Quaternion>(this.rotationBind);
		set => this.FastSetValue(this.rotationBind, value);
	}

	public Vector FastScale
	{
		get => this.FastGetValue<Vector>(this.scaleBind);
		set => this.FastSetValue(this.scaleBind, value);
	}

	public Quaternion WorldRotation
	{
		get => this.Rotation;
		set => this.Rotation = value;
	}

	public bool IsValid()
	{
		return this.Position.IsValid() && this.Rotation.IsValid() && this.Scale.IsValid();
	}

	public override void SetAddress(IntPtr address)
	{
		base.SetAddress(address);

		this.positionBind = this.GetBind(nameof(TransformMemory.Position));
		this.rotationBind = this.GetBind(nameof(TransformMemory.Rotation));
		this.scaleBind = this.GetBind(nameof(TransformMemory.Scale));
	}
}
