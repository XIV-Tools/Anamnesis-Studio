// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Views;

using Anamnesis.GameData.Excel;
using PropertyChanged;
using System.ComponentModel;
using System.Windows.Controls;
using XivToolsWpf.DependencyProperties;

[AddINotifyPropertyChangedInterface]
public partial class CustomizeIconOption : UserControl, INotifyPropertyChanged
{
	public static IBind<byte> ValueDp = Binder.Register<byte, CustomizeIconOption>(nameof(Value), OnValueChanged);
	public static IBind<CharaMakeType.Menu?> MenuDp = Binder.Register<CharaMakeType.Menu?, CustomizeIconOption>(nameof(Menu), OnMenuChanged);
	public static IBind<bool> FlippedDp = Binder.Register<bool, CustomizeIconOption>(nameof(Flipped));

	public CustomizeIconOption()
	{
		this.InitializeComponent();
		this.ContentArea.DataContext = this;
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

	[AlsoNotifyFor(nameof(Menu), nameof(Value))]
	public CharaMakeType.Menu.Option? Option
	{
		get => this.Menu?.GetOption(this.Value);
		set => this.Value = value?.Value ?? this.Menu?.InitVal ?? 0;
	}

	public bool Flipped
	{
		get => FlippedDp.Get(this);
		set => FlippedDp.Set(this, value);
	}

	public static void OnValueChanged(CustomizeIconOption sender, byte newalue)
	{
		sender.PropertyChanged?.Invoke(sender, new(nameof(Option)));
	}

	public static void OnMenuChanged(CustomizeIconOption sender, CharaMakeType.Menu? newalue)
	{
		sender.PropertyChanged?.Invoke(sender, new(nameof(Option)));
	}
}
