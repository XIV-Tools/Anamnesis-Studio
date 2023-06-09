// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Views;

using Anamnesis.Actor.Panels;
using Anamnesis.Memory;
using PropertyChanged;
using System.Threading.Tasks;
using System.Windows;
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

	public ActorMemory? Actor { get; private set; }

	[DependsOn(nameof(Actor))]
	public bool HasTail => this.Actor?.Customize?.RaceId == ActorCustomizeMemory.Races.Miqote
	|| this.Actor?.Customize?.RaceId == ActorCustomizeMemory.Races.AuRa
	|| this.Actor?.Customize?.RaceId == ActorCustomizeMemory.Races.Hrothgar
	|| this.IsIVCS;

	[DependsOn(nameof(Actor))]
	public bool IsCustomFace => this.Actor == null ? false : this.IsMiqote || this.IsHrothgar;

	[DependsOn(nameof(Actor))]
	public bool IsMiqote => this.Actor?.Customize?.RaceId == ActorCustomizeMemory.Races.Miqote;

	[DependsOn(nameof(Actor))]
	public bool IsViera => this.Actor?.Customize?.RaceId == ActorCustomizeMemory.Races.Viera;

	[DependsOn(nameof(Actor))]
	public bool IsElezen => this.Actor?.Customize?.RaceId == ActorCustomizeMemory.Races.Elezen;

	[DependsOn(nameof(Actor))]
	public bool IsHrothgar => this.Actor?.Customize?.RaceId == ActorCustomizeMemory.Races.Hrothgar;

	[DependsOn(nameof(Actor))]
	public bool HasTailOrEars => this.IsViera || this.HasTail;

	[DependsOn(nameof(Actor))]
	public bool IsEars01 => this.IsViera && this.Actor?.Customize?.TailEarsType <= 1;

	[DependsOn(nameof(Actor))]
	public bool IsEars02 => this.IsViera && this.Actor?.Customize?.TailEarsType == 2;

	[DependsOn(nameof(Actor))]
	public bool IsEars03 => this.IsViera && this.Actor?.Customize?.TailEarsType == 3;

	[DependsOn(nameof(Actor))]
	public bool IsEars04 => this.IsViera && this.Actor?.Customize?.TailEarsType == 4;

	[DependsOn(nameof(Actor))]
	public bool IsIVCS => this.Actor?.ModelObject?.Skeleton?.GetBone("iv_ko_c_l") != null;

	[DependsOn(nameof(Actor))]
	public bool IsVieraEarsFlop
	{
		get
		{
			if (this.IsViera && this.Actor?.Customize?.Gender == ActorCustomizeMemory.Genders.Feminine && this.Actor?.Customize?.TailEarsType == 3)
				return true;

			if (this.IsViera && this.Actor?.Customize?.Gender == ActorCustomizeMemory.Genders.Masculine && this.Actor?.Customize?.TailEarsType == 2)
				return true;

			return false;
		}
	}

	public Task OnActorChanged()
	{
		this.Actor = (this.DataContext as ActorPanelBase)?.Actor;
		return Task.CompletedTask;
	}
}
