// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Panels;

using Anamnesis.Memory;
using Anamnesis.Panels;
using System;
using System.Threading.Tasks;
using XivToolsWpf.Extensions;

public abstract class ActorPanelBase : PanelBase
{
	public ActorMemory? Actor { get; set; }

	protected virtual Task OnActorChanged()
	{
		// TODO!
		return Task.CompletedTask;
	}
}