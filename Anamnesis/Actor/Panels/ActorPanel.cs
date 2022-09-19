// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Panels;

using Anamnesis.Memory;
using Anamnesis.Panels;
using System.Threading.Tasks;
using XivToolsWpf.DependencyProperties;
using XivToolsWpf.Extensions;

public abstract class ActorPanelBase : PanelBase
{
	public static readonly IBind<ActorMemory?> ActorDp = Binder.Register<ActorMemory?, ActorPanelBase>(nameof(Actor), OnActorChanged, BindMode.TwoWay);

	public ActorMemory? Actor
	{
		get => ActorDp.Get(this);
		set => ActorDp.Set(this, value);
	}

	protected virtual Task OnActorChanged()
	{
		return Task.CompletedTask;
	}

	private static void OnActorChanged(ActorPanelBase sender, ActorMemory? newValue)
	{
		// TODO: ensure this task is not already running.
		sender.OnActorChanged().Run();
	}
}