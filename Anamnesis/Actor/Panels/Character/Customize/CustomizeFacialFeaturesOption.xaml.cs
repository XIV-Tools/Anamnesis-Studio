// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Views;

using Anamnesis.GameData.Excel;
using Anamnesis.GameData.Sheets;
using Anamnesis.Memory;
using PropertyChanged;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Controls;
using XivToolsWpf.DependencyProperties;

public partial class CustomizeFacialFeaturesOption : UserControl
{
	public static IBind<ActorCustomizeMemory.FacialFeature> ValueDp = Binder.Register<ActorCustomizeMemory.FacialFeature, CustomizeFacialFeaturesOption>(nameof(Value), OnValueChanged);
	public static IBind<CharaMakeType.FacialFeatureOptions?> FacialFeaturesDp = Binder.Register<CharaMakeType.FacialFeatureOptions?, CustomizeFacialFeaturesOption>(nameof(FacialFeatures), OnFeaturesChanged, BindMode.OneWay);

	public CustomizeFacialFeaturesOption()
	{
		this.InitializeComponent();
		this.ContentArea.DataContext = this;
	}

	public ObservableCollection<Option> Options { get; init; } = new();

	public ActorCustomizeMemory.FacialFeature Value
	{
		get => ValueDp.Get(this);
		set => ValueDp.Set(this, value);
	}

	public CharaMakeType.FacialFeatureOptions? FacialFeatures
	{
		get => FacialFeaturesDp.Get(this);
		set => FacialFeaturesDp.Set(this, value);
	}

	public static void OnValueChanged(CustomizeFacialFeaturesOption sender, ActorCustomizeMemory.FacialFeature newValue)
	{
		foreach (Option op in sender.Options)
		{
			op.Notify();
		}
	}

	public static void OnFeaturesChanged(CustomizeFacialFeaturesOption sender, CharaMakeType.FacialFeatureOptions? newValue)
	{
		sender.Options.Clear();

		if (newValue == null)
			return;

		foreach (CharaMakeType.FacialFeatureOptions.Option option in newValue.Options)
		{
			Option op = new Option(sender);
			op.Value = option.Value;
			op.Icon = option.Icon;
			sender.Options.Add(op);
		}
	}

	[AddINotifyPropertyChangedInterface]
	public class Option : INotifyPropertyChanged
	{
		public Option(CustomizeFacialFeaturesOption owner)
		{
			this.Owner = owner;
		}

		public event PropertyChangedEventHandler? PropertyChanged;

		public CustomizeFacialFeaturesOption Owner { get; init; }
		public ActorCustomizeMemory.FacialFeature Value { get; set; }
		public ImageReference? Icon { get; set; }

		public bool IsEnabled
		{
			get => this.Owner.Value.HasFlag(this.Value);
			set
			{
				if (value)
				{
					this.Owner.Value |= this.Value;
				}
				else
				{
					this.Owner.Value &= ~this.Value;
				}
			}
		}

		public void Notify()
		{
			this.PropertyChanged?.Invoke(this, new(nameof(this.IsEnabled)));
			this.PropertyChanged?.Invoke(this, new(nameof(this.Icon)));
		}
	}
}
