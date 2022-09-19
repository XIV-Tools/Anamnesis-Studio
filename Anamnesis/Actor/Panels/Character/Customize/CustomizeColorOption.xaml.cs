// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Views;

using Anamnesis.GameData.Excel;
using PropertyChanged;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using XivToolsWpf.DependencyProperties;

[AddINotifyPropertyChangedInterface]
public partial class CustomizeColorOption : UserControl, INotifyPropertyChanged
{
	public static IBind<byte> ValueDp = Binder.Register<byte, CustomizeColorOption>(nameof(Value), OnValueChanged);
	public static IBind<CharaMakeType.Menu?> MenuDp = Binder.Register<CharaMakeType.Menu?, CustomizeColorOption>(nameof(Menu), OnMenuChanged);
	public static IBind<CornerRadius> CornerRadiusDp = Binder.Register<CornerRadius, CustomizeColorOption>(nameof(CornerRadius));

	public CustomizeColorOption()
	{
		this.InitializeComponent();
		this.ContentArea.DataContext = this;
		this.CornerRadius = new(6, 6, 6, 6);
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	public byte Value
	{
		get => ValueDp.Get(this);
		set => ValueDp.Set(this, value);
	}

	public CharaMakeType.Menu? Menu
	{
		get => MenuDp.Get(this);
		set => MenuDp.Set(this, value);
	}

	public CornerRadius CornerRadius
	{
		get => CornerRadiusDp.Get(this);
		set => CornerRadiusDp.Set(this, value);
	}

	[AlsoNotifyFor(nameof(CornerRadius))]
	public CornerRadius LeftElementCornerRadius => new(this.CornerRadius.TopLeft, 0, 0, this.CornerRadius.BottomLeft);

	[AlsoNotifyFor(nameof(CornerRadius))]
	public CornerRadius RightElementCornerRadius => new(0, this.CornerRadius.TopRight, this.CornerRadius.BottomRight, 0);

	[AlsoNotifyFor(nameof(Menu), nameof(Value))]
	public CharaMakeType.Menu.Option? Option
	{
		get => this.Menu?.GetOption(this.Value);
		set => this.Value = value?.Value ?? this.Menu?.InitVal ?? 0;
	}

	public static void OnValueChanged(CustomizeColorOption sender, byte newalue)
	{
		sender.PropertyChanged?.Invoke(sender, new(nameof(Option)));
	}

	public static void OnMenuChanged(CustomizeColorOption sender, CharaMakeType.Menu? newalue)
	{
		sender.PropertyChanged?.Invoke(sender, new(nameof(Option)));
	}
}
