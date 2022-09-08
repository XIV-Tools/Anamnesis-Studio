// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Views;

using Anamnesis.GameData.Excel;
using PropertyChanged;
using System.ComponentModel;
using System.Windows.Controls;
using XivToolsWpf.DependencyProperties;

[AddINotifyPropertyChangedInterface]
public partial class CustomizeNumberOption : UserControl, INotifyPropertyChanged
{
	public static IBind<byte> ValueDp = Binder.Register<byte, CustomizeNumberOption>(nameof(Value), OnValueChanged);
	public static IBind<CharaMakeType.Menu?> MenuDp = Binder.Register<CharaMakeType.Menu?, CustomizeNumberOption>(nameof(Menu), OnMenuChanged);

	public CustomizeNumberOption()
	{
		this.InitializeComponent();
		this.ContentArea.DataContext = this;
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	public bool ManualEntry { get; set; }

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

	public static void OnValueChanged(CustomizeNumberOption sender, byte newalue)
	{
		sender.PropertyChanged?.Invoke(sender, new(nameof(Option)));
	}

	public static void OnMenuChanged(CustomizeNumberOption sender, CharaMakeType.Menu? newalue)
	{
		sender.PropertyChanged?.Invoke(sender, new(nameof(Option)));
	}
}
