// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Files;

using Anamnesis.Memory;
using System;
using System.Threading.Tasks;
using XivToolsWpf.Extensions;

public abstract class ActorFileImporterBase : FileImporterBase
{
	private ActorMemory? actor;

	public ActorMemory Actor
	{
		get => this.actor!;
		set => this.SetActor(value).Run();
	}

	public override async Task Apply(bool isPreview)
	{
		await base.Apply(isPreview);

		if (this.Actor == null)
		{
			throw new Exception("Must select a target actor");
		}
	}

	public override async Task Revert()
	{
		await base.Revert();

		if (this.Actor == null)
		{
			throw new Exception("Must select a target actor");
		}
	}

	protected async Task SetActor(ActorMemory? actor)
	{
		try
		{
			await this.Revert();
			this.actor = actor;
			this.RaisePropertyChanged(nameof(this.Actor));
			await this.Apply(true);
		}
		catch (Exception ex)
		{
			this.Log.Error("Failed to set actor", ex);
		}
	}
}
