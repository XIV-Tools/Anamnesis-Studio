// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

public class TransformMemory : MemoryBase, ITransform
{
	[Bind(0x000)] public Vector Position { get => this.GetValue<Vector>(); set => this.SetValue(value); }
	[Bind(0x010)] public Quaternion Rotation { get => this.GetValue<Quaternion>(); set => this.SetValue(value); }
	[Bind(0x020)] public Vector Scale { get => this.GetValue<Vector>(); set => this.SetValue(value); }

	public bool CanTranslate => true;
	public bool CanRotate => true;
	public bool CanScale => true;
	public bool CanLinkScale => true;
	public bool ScaleLinked { get; set; } = true;

	public bool IsValid()
	{
		return this.Position.IsValid() && this.Rotation.IsValid() && this.Scale.IsValid();
	}
}
