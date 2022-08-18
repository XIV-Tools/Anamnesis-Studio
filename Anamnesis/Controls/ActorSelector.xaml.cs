// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Controls;

using Anamnesis.Services;
using System.Threading.Tasks;
using System.Windows.Controls;
using XivToolsWpf.DependencyProperties;

public partial class ActorSelector : UserControl
{
	public static readonly IBind<PinnedActor?> SelectionDp = Binder.Register<PinnedActor?, ActorSelector>(nameof(Selection));

	public ActorSelector()
	{
		this.InitializeComponent();
		this.ContentArea.DataContext = this;
	}

	public event SelectionChangedEventHandler? SelectionChanged;

	public ServiceManager Services => App.Services;

	public PinnedActor? Selection
	{
		get => SelectionDp.Get(this);
		set => SelectionDp.Set(this, value);
	}

	private async void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
	{
		await Task.Delay(100);

		if (this.Selection != null)
			return;

		if (this.Services.Target.IsPlayerTargetPinnable)
		{
			this.Selection = TargetService.GetPinned(this.Services.Target.PlayerTarget);
		}
		else if (this.Services.Target.PinnedActorCount > 0)
		{
			this.Selection = this.Services.Target.PinnedActors[0];
		}
	}

	private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		this.SelectionChanged?.Invoke(this, e);
	}
}
