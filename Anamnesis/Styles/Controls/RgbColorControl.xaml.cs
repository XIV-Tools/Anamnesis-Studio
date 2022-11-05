// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Styles.Controls;

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using PropertyChanged;
using XivToolsWpf.DependencyProperties;

using Color = Anamnesis.Memory.Color;
using WpfColor = System.Windows.Media.Color;

[AddINotifyPropertyChangedInterface]
public partial class RgbColorControl : UserControl, INotifyPropertyChanged
{
	public static readonly IBind<Color?> ValueDp = Binder.Register<Color?, RgbColorControl>(nameof(Value), OnValueChanged);

	private static readonly ColorConverter ColorConverter = new();

	public RgbColorControl()
	{
		this.InitializeComponent();
		this.ContentArea.DataContext = this;
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	public Color? Value
	{
		get => ValueDp.Get(this);
		set => ValueDp.Set(this, value);
	}

	public WpfColor? WpfColor
	{
		get
		{
			WpfColor c = default;

			if (this.Value != null)
			{
				Color color = (Color)this.Value;
				c.R = (byte)(Clamp(color.R) * 255);
				c.G = (byte)(Clamp(color.G) * 255);
				c.B = (byte)(Clamp(color.B) * 255);
				c.A = 255;
			}
			else
			{
				c.A = 0;
			}

			return c;
		}
	}

	public float R
	{
		get => this.Value?.R ?? 0;
		set
		{
			if (this.Value == null)
				return;

			Color c = (Color)this.Value;
			c.R = value;
			this.Value = c;
		}
	}

	public float G
	{
		get => this.Value?.G ?? 0;
		set
		{
			if (this.Value == null)
				return;

			Color c = (Color)this.Value;
			c.G = value;
			this.Value = c;
		}
	}

	public float B
	{
		get => this.Value?.B ?? 0;
		set
		{
			if (this.Value == null)
				return;

			Color c = (Color)this.Value;
			c.B = value;
			this.Value = c;
		}
	}

	private static void OnValueChanged(RgbColorControl sender, Color? value)
	{
		sender.PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(nameof(WpfColor)));
		sender.PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(nameof(R)));
		sender.PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(nameof(G)));
		sender.PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(nameof(B)));
	}

	private static double Clamp(double v)
	{
		v = Math.Min(v, 1);
		v = Math.Max(v, 0);
		return v;
	}

	private void OnClick(object sender, RoutedEventArgs e)
	{
	}
}
