// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Views;

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Anamnesis.GameData.Sheets;
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

	private void OnHairClicked(object sender, RoutedEventArgs e)
	{
		if (this.Actor?.Customize == null)
			return;

		CustomizeFeatureSelectorDrawer selector = new CustomizeFeatureSelectorDrawer(CustomizeSheet.Features.Hair, this.Actor.Customize.Gender, this.Actor.Customize.TribeId, this.Actor.Customize.Hair);
		selector.SelectionChanged += (v) =>
		{
			this.Actor.Customize.Hair = v;
		};

		throw new NotImplementedException();
	}

	private void OnFacePaintClicked(object sender, RoutedEventArgs e)
	{
		if (this.Actor?.Customize == null)
			return;

		CustomizeFeatureSelectorDrawer selector = new CustomizeFeatureSelectorDrawer(CustomizeSheet.Features.FacePaint, this.Actor.Customize.Gender, this.Actor.Customize.TribeId, this.Actor.Customize.FacePaint);
		selector.SelectionChanged += (v) =>
		{
			this.Actor.Customize.FacePaint = v;
		};

		throw new NotImplementedException();
	}
}
