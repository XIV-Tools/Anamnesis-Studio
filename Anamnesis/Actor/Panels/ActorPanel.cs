// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Panels;

using Anamnesis.Memory;
using Anamnesis.Panels;
using System.ComponentModel;
using System.Threading.Tasks;
using XivToolsWpf.Extensions;

public abstract class ActorPanelBase : PanelBase
{
	protected ActorPanelBase(IPanelHost host)
		: base(host)
	{
		this.Services.Target.ActorSelected += this.OnActorSelected;
		this.OnActorSelected(this.Services.Target.CurrentSelection?.GetMemory());
	}

	////public override string Id => base.Id + "_" + this.Actor?.Names.DisplayName;
	public ActorMemory? Actor { get; private set; }

	protected virtual Task OnActorChanged()
	{
		return Task.CompletedTask;
	}

	private void OnActorSelected(ActorMemory? actor)
	{
		if (actor == null)
			return;

		if (this.Actor != null)
		{
			this.Actor.PropertyChanged -= this.OnActorPropertyChanged;
			this.Actor.Names.PropertyChanged -= this.OnActorPropertyChanged;
		}

		this.Actor = actor;

		if (this.Actor == null)
			return;

		this.Actor.PropertyChanged += this.OnActorPropertyChanged;
		this.Actor.Names.PropertyChanged += this.OnActorPropertyChanged;

		this.UpdateTitle();

		this.OnActorChanged().Run();
	}

	private void OnActorPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName == nameof(Names.Text) || e.PropertyName == nameof(ActorMemory.Color))
		{
			this.UpdateTitle();
		}
	}

	private void UpdateTitle()
	{
		if (this.Actor == null)
			return;

		this.Title = " - " + this.Actor.Names.Text;
		this.TitleColor = this.Actor.Color;
	}
}
