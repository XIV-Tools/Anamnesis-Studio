// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Views;

using Anamnesis.Actor.Panels;
using Anamnesis.Memory;
using PropertyChanged;
using System.Windows.Controls;

[AddINotifyPropertyChangedInterface]
public partial class PoseGuiView : UserControl
{
	public PoseGuiView()
	{
		this.InitializeComponent();
		this.ContentArea.DataContext = this;
	}

	// TODO: this might be better as a setting, or a value per actor
	public bool FlipSides { get; set; }

	public ActorMemory? Actor => (this.DataContext as ActorPanelBase)?.Actor;
}
