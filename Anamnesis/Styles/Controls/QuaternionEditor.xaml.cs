// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Styles.Controls;

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Anamnesis.Keyboard;
using Anamnesis.Memory;
using Anamnesis.Services;
using PropertyChanged;
using XivToolsWpf.DependencyProperties;
using XivToolsWpf.Meida3D;
using XivToolsWpf.Meida3D.Lines;

using CmQuaternion = Anamnesis.Memory.Quaternion;
using Color = System.Windows.Media.Color;
using Quaternion = System.Windows.Media.Media3D.Quaternion;
using Vector = Anamnesis.Memory.Vector;

/// <summary>
/// Interaction logic for QuaternionEditor.xaml.
/// </summary>
[AddINotifyPropertyChangedInterface]
public partial class QuaternionEditor : UserControl
{
	public static readonly IBind<CmQuaternion> LocalValueDp = Binder.Register<CmQuaternion, QuaternionEditor>(nameof(LocalValue), OnLocalValueChanged);
	public static readonly IBind<CmQuaternion> WorldValueDp = Binder.Register<CmQuaternion, QuaternionEditor>(nameof(WorldValue), OnWorldValueChanged);
	public static readonly IBind<double> TickDp = Binder.Register<double, QuaternionEditor>(nameof(TickFrequency));
	public static readonly IBind<Vector> LocalValueEulerDp = Binder.Register<Vector, QuaternionEditor>(nameof(LocalValueEuler), OnLocalValueEulerChanged);

	private readonly RotationGizmo rotationGizmo;
	private readonly QuaternionRotation3D rotationTransform = new();
	private bool lockdp = false;

	public QuaternionEditor()
	{
		this.InitializeComponent();
		this.ContentArea.DataContext = this;

		this.TickFrequency = 0.5;

		this.rotationGizmo = new RotationGizmo(this);
		this.Viewport.Children.Add(this.rotationGizmo);

		this.Viewport.Camera = new PerspectiveCamera(new Point3D(0, 0, -2.0), new Vector3D(0, 0, 1), new Vector3D(0, 1, 0), 45);
		this.Viewport.Camera.Transform = new RotateTransform3D(this.rotationTransform);
	}

	public double TickFrequency
	{
		get => TickDp.Get(this);
		set => TickDp.Set(this, value);
	}

	public CmQuaternion LocalValue
	{
		get => LocalValueDp.Get(this);
		set => LocalValueDp.Set(this, value);
	}

	public CmQuaternion WorldValue
	{
		get => WorldValueDp.Get(this);
		set => WorldValueDp.Set(this, value);
	}

	public Vector LocalValueEuler
	{
		get => LocalValueEulerDp.Get(this);
		set => LocalValueEulerDp.Set(this, value);
	}

	public Settings Settings => SettingsService.Current;

	public void SetWorldRotation(Quaternion worldValue)
	{
		Quaternion newrot = worldValue;

		if (this.lockdp)
			return;

		this.lockdp = true;
		this.WorldValue = new CmQuaternion((float)newrot.X, (float)newrot.Y, (float)newrot.Z, (float)newrot.W);
		OnLocalValueChanged(this, this.LocalValue);
		this.lockdp = false;
	}

	private static void OnLocalValueChanged(QuaternionEditor sender, CmQuaternion value)
	{
		sender.rotationGizmo.SetRotation(sender.WorldValue);

		if (sender.lockdp)
			return;

		sender.lockdp = true;
		sender.LocalValueEuler = sender.LocalValue.ToEuler();
		sender.lockdp = false;
	}

	private static void OnWorldValueChanged(QuaternionEditor sender, CmQuaternion value)
	{
		sender.rotationGizmo.SetRotation(sender.WorldValue);
	}

	private static void OnLocalValueEulerChanged(QuaternionEditor sender, Vector val)
	{
		sender.rotationGizmo.SetRotation(sender.WorldValue);

		if (sender.lockdp)
			return;

		sender.lockdp = true;
		sender.LocalValue = CmQuaternion.FromEuler(sender.LocalValueEuler);
		sender.lockdp = false;
	}

	private static string? GetAxisName(Vector3D? axis)
	{
		if (axis == null)
			return null;

		Vector3D v = (Vector3D)axis;

		if (v.X > v.Y && v.X > v.Z)
			return "X";

		if (v.Y > v.X && v.Y > v.Z)
			return "Y";

		if (v.Z > v.X && v.Z > v.Y)
			return "Z";

		return null;
	}

	private void OnViewportMouseDown(object sender, MouseButtonEventArgs e)
	{
		Mouse.Capture(this.Viewport);
	}

	private void OnViewportMouseUp(object sender, MouseButtonEventArgs e)
	{
		Mouse.Capture(null);

		if (e.ChangedButton == MouseButton.Right)
		{
			this.LockedIndicator.IsChecked = this.rotationGizmo.LockHoveredGizmo();
			this.LockedIndicator.IsEnabled = (bool)this.LockedIndicator.IsChecked;
			this.LockedAxisDisplay.Text = GetAxisName(this.rotationGizmo.Locked?.Axis);
		}

		this.rotationGizmo.Hover(null);
	}

	private void OnViewportMouseMove(object sender, MouseEventArgs e)
	{
		Point mousePosition = e.GetPosition(this.Viewport);

		if (e.LeftButton != MouseButtonState.Pressed)
		{
			HitTestResult result = VisualTreeHelper.HitTest(this.Viewport, mousePosition);
			this.rotationGizmo.Hover(result?.VisualHit);
		}
		else
		{
			Point3D mousePos3D = new Point3D(mousePosition.X, mousePosition.Y, 0);
			this.rotationGizmo.Drag(mousePos3D);
		}
	}

	private void OnViewportMouseLeave(object sender, MouseEventArgs e)
	{
		if (e.LeftButton == MouseButtonState.Pressed)
			return;

		this.rotationGizmo.Hover(null);
	}

	private void OnViewportMouseWheel(object sender, MouseWheelEventArgs e)
	{
		double delta = e.Delta > 0 ? this.TickFrequency : -this.TickFrequency;

		if (Keyboard.IsKeyDown(Key.LeftShift))
			delta *= 10;

		this.rotationGizmo.Scroll(delta);
	}

	private void LockedIndicator_Unchecked(object sender, RoutedEventArgs e)
	{
		this.rotationGizmo.UnlockGizmo();
		this.LockedIndicator.IsEnabled = false;
		this.LockedAxisDisplay.Text = GetAxisName(this.rotationGizmo.Locked?.Axis);
	}

	private bool Rotate(KeyboardKeyStates state, double x, double y, double z)
	{
		// only roate on Press or Down events
		if (state == KeyboardKeyStates.Released)
			return false;

		if (!this.IsVisible)
			return false;

		Vector euler = this.LocalValueEuler;
		euler.X = Float.Wrap(euler.X + (float)x, 0, 360);
		euler.Y = Float.Wrap(euler.Y + (float)y, 0, 360);
		euler.Z = Float.Wrap(euler.Z + (float)z, 0, 360);
		this.LocalValueEuler = euler;

		return true;
	}

	private void WatchCamera()
	{
		bool vis = true;
		while (vis && Application.Current != null)
		{
			try
			{
				this.Dispatcher.Invoke(() =>
				{
					vis = this.IsVisible; ////&& this.IsEnabled;

					if (App.Services.Camera.Camera != null)
					{
						Vector3D camEuler = default;
						camEuler.Y = (float)MathUtils.RadiansToDegrees((double)App.Services.Camera.Camera.Angle.X) - 180;
						camEuler.Z = (float)-MathUtils.RadiansToDegrees((double)App.Services.Camera.Camera.Angle.Y);
						camEuler.X = (float)MathUtils.RadiansToDegrees((double)App.Services.Camera.Camera.Rotation);
						this.rotationTransform.Quaternion = camEuler.ToQuaternion();
					}
				});
			}
			catch (Exception)
			{
			}

			Thread.Sleep(16);
		}
	}

	private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		if (this.IsVisible)
		{
			// Watch camera thread
			new Thread(new ThreadStart(this.WatchCamera)).Start();
		}
	}

	private void OnSizeChanged(object sender, SizeChangedEventArgs e)
	{
		if (e.NewSize.Width > 300)
		{
			Grid.SetRow(this.VectorEditor, 0);
			Grid.SetColumn(this.VectorEditor, 1);
		}
		else
		{
			Grid.SetRow(this.VectorEditor, 1);
			Grid.SetColumn(this.VectorEditor, 0);
		}
	}

	private class RotationGizmo : ModelVisual3D
	{
		private readonly QuaternionEditor target;
		private readonly QuaternionRotation3D rotationTransform = new();

		public RotationGizmo(QuaternionEditor target)
		{
			this.target = target;

			Sphere sphere = new Sphere();
			sphere.Radius = 0.48;
			Color c = Colors.Black;
			c.A = 128;
			sphere.Material = new DiffuseMaterial(new SolidColorBrush(c));
			this.Children.Add(sphere);

			this.Children.Add(new AxisGizmo(Colors.Blue, new Vector3D(1, 0, 0)));
			this.Children.Add(new AxisGizmo(Colors.Green, new Vector3D(0, 1, 0)));
			this.Children.Add(new AxisGizmo(Colors.Red, new Vector3D(0, 0, 1)));

			this.Transform = new RotateTransform3D(this.rotationTransform);
		}

		public AxisGizmo? Locked
		{
			get;
			private set;
		}

		public AxisGizmo? Hovered
		{
			get;
			private set;
		}

		public AxisGizmo? Active
		{
			get
			{
				if (this.Locked != null)
					return this.Locked;

				return this.Hovered;
			}
		}

		public void SetRotation(CmQuaternion rotation)
		{
			Quaternion q = this.rotationTransform.Quaternion;
			q.X = rotation.X;
			q.Y = rotation.Y;
			q.Z = rotation.Z;
			q.W = rotation.W;
			this.rotationTransform.Quaternion = q;
		}

		public bool LockHoveredGizmo()
		{
			if (this.Locked != null)
				this.Locked.Locked = false;

			this.Locked = this.Hovered;

			if (this.Locked != null)
				this.Locked.Locked = true;

			return this.Locked != null;
		}

		public void UnlockGizmo()
		{
			if (this.Locked != null)
				this.Locked.Locked = false;

			this.Locked = null;
		}

		public bool Hover(DependencyObject? visual)
		{
			if (this.Locked != null)
			{
				this.Hovered = null;
				return true;
			}

			AxisGizmo? gizmo = null;
			if (visual is Circle r)
			{
				gizmo = (AxisGizmo)VisualTreeHelper.GetParent(r);
			}
			else if (visual is Cylinder c)
			{
				gizmo = (AxisGizmo)VisualTreeHelper.GetParent(c);
			}

			if (this.Hovered != null)
				this.Hovered.Hovered = false;

			this.Hovered = gizmo;

			if (this.Hovered != null)
			{
				this.Hovered.Hovered = true;
				return true;
			}

			return false;
		}

		public void Drag(Point3D mousePosition)
		{
			if (this.Active == null)
				return;

			Vector3D angleDelta = this.Active.Drag(mousePosition);
			this.ApplyDelta(angleDelta);
		}

		public void Scroll(double delta)
		{
			if (this.Active == null)
				return;

			Vector3D angleDelta = this.Active.Axis * delta;
			this.ApplyDelta(angleDelta);
		}

		private void ApplyDelta(Vector3D angleEuler)
		{
			Quaternion angle = angleEuler.ToQuaternion();

			Quaternion valueQuat = new Quaternion(this.target.WorldValue.X, this.target.WorldValue.Y, this.target.WorldValue.Z, this.target.WorldValue.W);
			valueQuat *= angle;
			this.target.SetWorldRotation(valueQuat);
		}
	}

	private class AxisGizmo : ModelVisual3D
	{
		public readonly Vector3D Axis;
		private readonly Circle circle;
		private readonly Cylinder cylinder;
		private Color color;

		private Point3D? lastPoint;

		public AxisGizmo(Color color, Vector3D axis)
		{
			this.Axis = axis;
			this.color = color;

			Vector3D rotationAxis = new Vector3D(axis.Z, 0, axis.X);

			this.circle = new Circle();
			this.circle.Thickness = 1;
			this.circle.Color = color;
			this.circle.Radius = 0.5;
			this.circle.Transform = new RotateTransform3D(new AxisAngleRotation3D(axis, 90));
			this.Children.Add(this.circle);

			this.cylinder = new Cylinder();
			this.cylinder.Radius = 0.49;
			this.cylinder.Length = 0.20;
			this.cylinder.Transform = new RotateTransform3D(new AxisAngleRotation3D(axis, 90));
			this.cylinder.Material = new DiffuseMaterial(new SolidColorBrush(Colors.Transparent));
			this.Children.Add(this.cylinder);
		}

		public bool Hovered
		{
			set
			{
				if (!value)
				{
					this.circle.Color = this.color;
					this.circle.Thickness = 1;
					this.lastPoint = null;
				}
				else
				{
					this.circle.Color = Colors.Yellow;
					this.circle.Thickness = 3;
				}
			}
		}

		public bool Locked
		{
			set
			{
				if (!value)
				{
					this.circle.Color = this.color;
					this.circle.Thickness = 1;
					this.lastPoint = null;
				}
				else
				{
					this.circle.Color = Colors.White;
					this.circle.Thickness = 3;
				}
			}
		}

		public void StartDrag()
		{
			this.lastPoint = null;
		}

		public Vector3D Drag(Point3D mousePosition)
		{
			bool useCircularDrag = true;

			if (useCircularDrag)
			{
				Point3D? point = this.circle.NearestPoint2D(mousePosition);

				if (point == null)
					return default;

				point = this.circle.TransformToAncestor(this).Transform((Point3D)point);

				if (this.lastPoint == null)
				{
					this.lastPoint = point;
					return default;
				}
				else
				{
					Vector3D axis = new Vector3D(0, 1, 0);

					Vector3D from = (Vector3D)this.lastPoint;
					Vector3D to = (Vector3D)point;

					this.lastPoint = null;

					double angle = Vector3D.AngleBetween(from, to);

					Vector3D cross = Vector3D.CrossProduct(from, to);
					if (Vector3D.DotProduct(axis, cross) < 0)
						angle = -angle;

					// X rotation gizmo is always backwards...
					if (this.Axis.X >= 1)
						angle = -angle;

					float speed = 2;

					if (Keyboard.IsKeyDown(Key.LeftShift))
						speed = 4;

					if (Keyboard.IsKeyDown(Key.LeftCtrl))
						speed = 0.5f;

					return this.Axis * (angle * speed);
				}
			}
			else
			{
				if (this.lastPoint == null)
				{
					this.lastPoint = mousePosition;
					return default;
				}
				else
				{
					Vector3D delta = (Point3D)this.lastPoint - mousePosition;
					this.lastPoint = mousePosition;

					float speed = 0.5f;

					if (Keyboard.IsKeyDown(Key.LeftShift))
						speed = 2;

					if (Keyboard.IsKeyDown(Key.LeftCtrl))
						speed = 0.25f;

					double distPos = Math.Max(delta.X, delta.Y);
					double distNeg = Math.Min(delta.X, delta.Y);

					double dist = distNeg;
					if (Math.Abs(distPos) > Math.Abs(distNeg))
						dist = distPos;

					return this.Axis * (-dist * speed);
				}
			}
		}
	}
}
