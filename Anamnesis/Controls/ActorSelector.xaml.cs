// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Controls;

using Anamnesis.Memory;
using Anamnesis.Services;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using XivToolsWpf;
using XivToolsWpf.DependencyProperties;
using XivToolsWpf.Selectors;

public partial class ActorSelector : UserControl
{
	public static readonly IBind<ActorMemory?> SelectionDp = Binder.Register<ActorMemory?, ActorSelector>(nameof(Selection));
	private static readonly ActorFilter FilterInstance = new();

	public ActorSelector()
	{
		this.InitializeComponent();
		this.ContentArea.DataContext = this;
		this.ActorSelectorContentArea.DataContext = this;

		TargetService.TargetChanged += this.OnTargetChanged;
	}

	public bool SyncTarget { get; set; }
	public ServiceManager Services => App.Services;
	public ActorFilter Filter => FilterInstance;

	public ActorMemory? Selection
	{
		get => SelectionDp.Get(this);
		set => SelectionDp.Set(this, value);
	}

	private async void OnLoaded(object sender, RoutedEventArgs e)
	{
		await Task.Delay(100);

		if (this.Selection != null)
			return;

		if (this.Services.Target.TargetedActor != null)
		{
			this.Select(this.Services.Target.TargetedActor);
		}
		else
		{
			this.Select(this.Services.Actor.GetDefaultActor());
		}
	}

	private void OnTargetChanged(ActorBasicMemory? actor)
	{
		if (this.SyncTarget)
		{
			this.Dispatcher.Invoke(() =>
			{
				this.Select(actor);
			});
		}
	}

	private void OnAddPlayerTargetActorClicked(object sender, RoutedEventArgs e)
	{
		this.Selector.Value = this.Services.Target.TargetedActor;
		this.Selector.RaiseSelectionChanged();
	}

	private void OnClicked(object sender, RoutedEventArgs e)
	{
		this.Selector.ClearItems();
		this.Selector.AddItems(ActorService.Instance.GetAllActors());
		this.Selector.FilterItems();
	}

	private void Select(ActorBasicMemory? basic)
	{
		if (basic == null)
		{
			this.Selection = null;
		}
		else
		{
			if (this.Selection == null)
			{
				ActorMemory newActor = new();
				newActor.SetAddress(basic.Address);
				this.Selection = newActor;
			}
			else
			{
				this.Selection?.SetAddress(basic.Address);
			}
		}
	}

	private Task LoadItems()
	{
		if (!ActorService.Exists)
			return Task.CompletedTask;

		this.Selector.AddItems(ActorService.Instance.GetAllActors());
		return Task.CompletedTask;
	}

	private void OnActorSelected(bool close)
	{
		if (this.Selector.Value == null)
			return;

		this.ActorSelectorPopup.IsOpen = false;
		this.Select(this.Selector.Value as ActorBasicMemory);
	}

	public class ActorFilter : FilterBase<ActorBasicMemory>
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

		public override bool FilterItem(ActorBasicMemory actor)
		{
			////if (GposeService.Instance.IsGpose != actor.IsGPoseActor)
			////	return false;

			if (!SearchUtility.Matches(actor.Names.FullName, this.SearchQuery) && !SearchUtility.Matches(actor.Names.Nickname, this.SearchQuery))
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
