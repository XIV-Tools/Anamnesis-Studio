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

	public override bool CanApply => base.CanRevert && this.actor != null;
	public override bool CanRevert => base.CanRevert && this.actor != null;

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
			if (this.CanRevert)
				await this.Revert();

			this.actor = actor;
			this.RaisePropertyChanged(nameof(this.Actor));

			if (this.CanApply && this.LivePreview)
			{
				await this.Apply(true);
			}

			this.RaisePropertyChanged(nameof(this.CanApply));
			this.RaisePropertyChanged(nameof(this.CanRevert));
		}
		catch (Exception ex)
		{
			this.Log.Error(ex, "Failed to set actor");
		}
	}
}
