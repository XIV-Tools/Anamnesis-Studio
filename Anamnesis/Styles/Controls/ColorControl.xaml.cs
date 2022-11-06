// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Styles.Controls;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Anamnesis.Services;
using PropertyChanged;
using XivToolsWpf.DependencyProperties;

using Binder = XivToolsWpf.DependencyProperties.Binder;
using Color3 = Anamnesis.Memory.Color;
using Color4 = Anamnesis.Memory.Color4;

using WpfColor = System.Windows.Media.Color;

[AddINotifyPropertyChangedInterface]
public partial class ColorControl : UserControl, INotifyPropertyChanged
{
	public static readonly IBind<object?> ValueDp = Binder.Register<object?, ColorControl>(nameof(Value), OnValueChanged);

	private static readonly ColorConverter ColorConverter = new();

	public ColorControl()
	{
		this.InitializeComponent();
		this.ContentArea.DataContext = this;
		this.PopupContentArea.DataContext = this;

		this.PopulateColors();
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	public object? Value
	{
		get => ValueDp.Get(this);
		set => ValueDp.Set(this, value);
	}

	public Color4 Color4
	{
		get
		{
			if (this.Value is Color3 rgb)
			{
				return new(rgb);
			}
			else if (this.Value is Color4 rgba)
			{
				return rgba;
			}
			else
			{
				return new();
			}
		}
		set
		{
			if (this.Value is Color3 rgb)
			{
				rgb.R = value.R;
				rgb.G = value.G;
				rgb.B = value.B;
				this.Value = rgb;
			}
			else if (this.Value is Color4 rgba)
			{
				rgba.R = value.R;
				rgba.G = value.G;
				rgba.B = value.B;
				rgba.A = value.A;
				this.Value = rgba;
			}
			else
			{
				this.Value = value;
			}
		}
	}

	public WpfColor? WpfColor
	{
		get
		{
			WpfColor c = default;

			Color4 color = (Color4)this.Color4;
			c.R = (byte)(Clamp(color.R) * 255);
			c.G = (byte)(Clamp(color.G) * 255);
			c.B = (byte)(Clamp(color.B) * 255);
			c.A = (byte)(Clamp(color.A) * 255);

			return c;
		}
	}

	[AlsoNotifyFor(nameof(Value))]
	public float R
	{
		get => this.Color4.R;
		set
		{
			Color4 c = (Color4)this.Color4;
			c.R = value;
			this.Color4 = c;
		}
	}

	[AlsoNotifyFor(nameof(Value))]
	public float G
	{
		get => this.Color4.G;
		set
		{
			Color4 c = (Color4)this.Color4;
			c.G = value;
			this.Color4 = c;
		}
	}

	[AlsoNotifyFor(nameof(Value))]
	public float B
	{
		get => this.Color4.B;
		set
		{
			Color4 c = (Color4)this.Color4;
			c.B = value;
			this.Color4 = c;
		}
	}

	[AlsoNotifyFor(nameof(Value))]
	public float A
	{
		get => this.Color4.A;
		set
		{
			Color4 c = (Color4)this.Color4;
			c.A = value;
			this.Color4 = c;
		}
	}

	[AlsoNotifyFor(nameof(R))]
	public int RByte
	{
		get => (int)(this.R * 255);
		set => this.R = value / 255.0f;
	}

	[AlsoNotifyFor(nameof(G))]
	public int GByte
	{
		get => (int)(this.G * 255);
		set => this.G = value / 255.0f;
	}

	[AlsoNotifyFor(nameof(B))]
	public int BByte
	{
		get => (int)(this.B * 255);
		set => this.B = value / 255.0f;
	}

	[AlsoNotifyFor(nameof(A))]
	public int AByte
	{
		get => (int)(this.A * 255);
		set => this.A = value / 255.0f;
	}

	public bool EnableAlpha => this.Value is Color4;

	private static void OnValueChanged(ColorControl sender, object? value)
	{
		sender.PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(nameof(Color4)));
		OnColor4Changed(sender, null);
	}

	private static void OnColor4Changed(ColorControl sender, Color4? value)
	{
		sender.PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(nameof(WpfColor)));
		sender.PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(nameof(R)));
		sender.PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(nameof(G)));
		sender.PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(nameof(B)));
		sender.PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(nameof(A)));
		sender.PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(nameof(RByte)));
		sender.PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(nameof(GByte)));
		sender.PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(nameof(BByte)));
		sender.PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(nameof(AByte)));
	}

	private static double Clamp(double v)
	{
		v = Math.Min(v, 1);
		v = Math.Max(v, 0);
		return v;
	}

	private void PopulateColors()
	{
		List<ColorOption> colors = new List<ColorOption>();
		PropertyInfo[] properties = typeof(Colors).GetProperties(BindingFlags.Static | BindingFlags.Public);
		foreach (PropertyInfo property in properties)
		{
			if (property == null)
				continue;

			object? obj = property.GetValue(null);

			if (obj == null)
				continue;

			if (property.Name == "Transparent")
				continue;

			colors.Add(new ColorOption((WpfColor)obj, property.Name));
		}

		ColorConverter colorConverter = new ColorConverter();
		colors.Sort((a, b) =>
		{
			string? aHex = colorConverter.ConvertToString(a.Color);
			string? bHex = colorConverter.ConvertToString(b.Color);

			if (aHex == null || bHex == null)
				throw new Exception("Failed to convert colors to hex");

			return aHex.CompareTo(bHex);
		});

		foreach (ColorOption c in colors)
		{
			this.List.Items.Add(c);
		}

		if (FavoritesService.Colors != null)
		{
			foreach (Color4 color in FavoritesService.Colors)
			{
				ColorOption op = new ColorOption(color, string.Empty);
				this.RecentList.Items.Add(op);
			}
		}
	}

	private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (sender is ListBox list)
		{
			ColorOption? op = list.SelectedItem as ColorOption;

			if (op == null)
				return;

			this.Value = op.AsColor();

			if (list == this.List)
			{
				this.RecentList.SelectedItem = null;
			}
			else if (list == this.RecentList)
			{
				this.List.SelectedItem = null;
			}
		}
	}

	private class ColorOption
	{
		public ColorOption(WpfColor c, string name)
		{
			this.Name = name;
			this.Color = c;
		}

		public ColorOption(Color4 c, string name)
		{
			this.Name = name;

			WpfColor color = default(WpfColor);
			color.ScR = c.R;
			color.ScG = c.G;
			color.ScB = c.B;
			color.ScA = c.A;
			this.Color = color;
		}

		public WpfColor Color { get; set; }
		public string Name { get; set; }

		public Color4 AsColor()
		{
			return new Color4(this.Color.ScR, this.Color.ScG, this.Color.ScB, this.Color.ScA);
		}
	}
}
