// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

using System.Windows.Media.Media3D;
using PropertyChanged;
using XivToolsWpf.Meida3D;

public class CameraMemory : MemoryBase
{
	[Bind(0x114)] public float Zoom { get => this.GetValue<ushort>(); set => this.SetValue(value); }
	[Bind(0x118)] public float MinZoom { get => this.GetValue<ushort>(); set => this.SetValue(value); }
	[Bind(0x11C)] public float MaxZoom { get => this.GetValue<ushort>(); set => this.SetValue(value); }
	[Bind(0x12C)] public float FieldOfView { get => this.GetValue<ushort>(); set => this.SetValue(value); }
	[Bind(0x130)] public Vector2D Angle { get => this.GetValue<Vector2D>(); set => this.SetValue(value); }
	[Bind(0x14C)] public float YMin { get => this.GetValue<ushort>(); set => this.SetValue(value); }
	[Bind(0x148)] public float YMax { get => this.GetValue<ushort>(); set => this.SetValue(value); }
	[Bind(0x150)] public Vector2D Pan { get => this.GetValue<Vector2D>(); set => this.SetValue(value); }
	[Bind(0x160)] public float Rotation { get => this.GetValue<ushort>(); set => this.SetValue(value); }

	[AlsoNotifyFor(nameof(CameraMemory.Angle), nameof(CameraMemory.Rotation))]
	public System.Windows.Media.Media3D.Quaternion Rotation3d
	{
		get
		{
			Vector3D camEuler = default;
			camEuler.Y = (float)MathUtils.RadiansToDegrees((double)this.Angle.X);
			camEuler.Z = (float)-MathUtils.RadiansToDegrees((double)this.Angle.Y);
			camEuler.X = (float)MathUtils.RadiansToDegrees((double)this.Rotation);
			return camEuler.ToQuaternion();
		}
	}

	[AlsoNotifyFor(nameof(CameraMemory.Angle), nameof(CameraMemory.Rotation))]
	public Vector Euler
	{
		get
		{
			Vector camEuler = default;
			camEuler.Y = (float)MathUtils.RadiansToDegrees((double)this.Angle.X);
			camEuler.Z = (float)MathUtils.RadiansToDegrees((double)this.Angle.Y);
			camEuler.X = (float)MathUtils.RadiansToDegrees((double)this.Rotation);
			return camEuler;
		}

		set
		{
			this.Rotation = (float)MathUtils.DegreesToRadians(value.X);
			var angleX = (float)MathUtils.DegreesToRadians(value.Y);
			var angleY = (float)MathUtils.DegreesToRadians(value.Z);
			this.Angle = new Vector2D(angleX, angleY);
		}
	}

	public bool FreezeAngle
	{
		get => this.IsFrozen(nameof(CameraMemory.Angle));
		set => this.SetFrozen(nameof(CameraMemory.Angle), value);
	}

	/*protected override void OnViewToModel(string fieldName, object? value)
	{
		if (!GposeService.Instance.IsGpose)
			return;

		base.OnViewToModel(fieldName, value);
	}*/
}
