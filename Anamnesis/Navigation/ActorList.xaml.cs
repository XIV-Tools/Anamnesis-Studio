// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Navigation;

using Anamnesis.Memory;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using XivToolsWpf;
using XivToolsWpf.Extensions;
using XivToolsWpf.Selectors;

public partial class ActorList : UserControl, INotifyPropertyChanged
{
	private static readonly ActorFilter FilterInstance = new();

	public ActorList()
	{
		this.InitializeComponent();
		this.PropertyChanged += this.OnSelfPropertyChanged;
		this.ActorSelectorContentArea.DataContext = this;
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	public ActorFilter Filter => FilterInstance;

	protected Task LoadItems()
	{
		if (!ActorService.Exists)
			return Task.CompletedTask;

		this.Selector.AddItems(ActorService.Instance.GetAllActors());
		return Task.CompletedTask;
	}

	private void OnSelfPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		this.Selector.FilterItems();
	}

	private void OnAddPlayerTargetActorClicked(object sender, RoutedEventArgs e)
	{
		this.Selector.Value = TargetService.GetTargetedActor();
		this.Selector.RaiseSelectionChanged();
	}

	private void OnAddActorClicked(object sender, RoutedEventArgs e)
	{
		this.ActorSelectorPopup.IsOpen = true;

		this.Selector.ClearItems();
		this.Selector.AddItems(ActorService.Instance.GetAllActors());
		this.Selector.FilterItems();
	}

	private void OnActorSelected(bool close)
	{
		this.ActorSelectorPopup.IsOpen = false;

		ActorBasicMemory? actor = this.Selector.Value as ActorBasicMemory;

		if (actor == null)
			return;

		TargetService.PinActor(actor).Run();
	}

	public class ActorFilter : Selector.FilterBase<ActorBasicMemory>
	{
		public bool IncludePlayers { get; set; } = true;
		public bool IncludeCompanions { get; set; } = true;
		public bool IncludeNPCs { get; set; } = true;
		public bool IncludeMounts { get; set; } = true;
		public bool IncludeOrnaments { get; set; } = true;
		public bool IncludeOther { get; set; } = false;
		public bool IncludeHidden { get; set; } = false;

		public override int CompareItems(ActorBasicMemory actorA, ActorBasicMemory actorB)
		{
			if (actorA.IsGPoseActor && !actorB.IsGPoseActor)
				return -1;

			if (!actorA.IsGPoseActor && actorB.IsGPoseActor)
				return 1;

			return actorA.DistanceFromPlayer.CompareTo(actorB.DistanceFromPlayer);
		}

		public override bool FilterItem(ActorBasicMemory actor, string[]? search)
		{
			////if (GposeService.Instance.IsGpose != actor.IsGPoseActor)
			////	return false;

			if (!SearchUtility.Matches(actor.Names.FullName, search) && !SearchUtility.Matches(actor.Names.Nickname, search))
				return false;

			if (TargetService.IsPinned(actor))
				return false;

			if (!this.IncludeHidden && actor.IsHidden)
				return false;

			if (!this.IncludePlayers && actor.ObjectKind == Memory.ActorTypes.Player)
				return false;

			if (!this.IncludeCompanions && actor.ObjectKind == Memory.ActorTypes.Companion)
				return false;

			if (!this.IncludeMounts && actor.ObjectKind == Memory.ActorTypes.Mount)
				return false;

			if (!this.IncludeOrnaments && actor.ObjectKind == Memory.ActorTypes.Ornament)
				return false;

			if (!this.IncludeNPCs && (actor.ObjectKind == Memory.ActorTypes.BattleNpc || actor.ObjectKind == Memory.ActorTypes.EventNpc))
				return false;

			if (!this.IncludeOther
				&& actor.ObjectKind != Memory.ActorTypes.Player
				&& actor.ObjectKind != Memory.ActorTypes.Companion
				&& actor.ObjectKind != Memory.ActorTypes.BattleNpc
				&& actor.ObjectKind != Memory.ActorTypes.EventNpc
				&& actor.ObjectKind != Memory.ActorTypes.Mount
				&& actor.ObjectKind != Memory.ActorTypes.Ornament)
			{
				return false;
			}

			return true;
		}
	}
}
