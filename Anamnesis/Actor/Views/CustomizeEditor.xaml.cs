// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Views;

using System.Collections.Generic;
using System.Windows.Controls;
using Anamnesis.Memory;
using Anamnesis.Services;
using PropertyChanged;
using XivToolsWpf.DependencyProperties;

[AddINotifyPropertyChangedInterface]
public partial class CustomizeEditor : UserControl
{
	public static IBind<ActorMemory?> ActorDp = Binder.Register<ActorMemory?, CustomizeEditor>(nameof(Actor));

	public CustomizeEditor()
	{
		this.InitializeComponent();
		this.ContentArea.DataContext = this;

		this.AgeComboBox.ItemsSource = new List<ActorCustomizeMemory.Ages>()
		{
			ActorCustomizeMemory.Ages.Young,
			ActorCustomizeMemory.Ages.Normal,
			ActorCustomizeMemory.Ages.Old,
		};
	}

	public ServiceManager Services => App.Services;

	public ActorMemory? Actor
	{
		get => ActorDp.Get(this);
		set => ActorDp.Set(this, value);
	}
}
