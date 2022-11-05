// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Panels.Character;

using Anamnesis.Memory;
using Anamnesis.Services;
using System.Windows.Controls;
using XivToolsWpf.DependencyProperties;

public partial class CharacterInfo : UserControl
{
	public static IBind<ActorMemory?> ActorDp = Binder.Register<ActorMemory?, CharacterInfo>(nameof(Actor));

	public CharacterInfo()
	{
		this.InitializeComponent();
		this.ContentArea.DataContext = this;
	}

	public ServiceManager Services => App.Services;

	public ActorMemory? Actor
	{
		get => ActorDp.Get(this);
		set => ActorDp.Set(this, value);
	}

	public int WetnessMode
	{
		get
		{
			if (this.Actor?.ModelObject == null)
				return -1;

			if (this.Actor.ModelObject.LockWetness)
				return 1;

			if (this.Actor.ModelObject.ForceDrenched)
				return 2;

			return 0;
		}

		set
		{
			if (this.Actor?.ModelObject == null)
				return;

			switch (value)
			{
				case 0:
				{
					this.Actor.ModelObject.Wetness = 0;
					this.Actor.ModelObject.LockWetness = false;

					this.Actor.ModelObject.Drenched = 0;
					this.Actor.ModelObject.ForceDrenched = false;
					break;
				}

				case 1:
				{
					this.Actor.ModelObject.Wetness = 1;
					this.Actor.ModelObject.LockWetness = true;

					this.Actor.ModelObject.Drenched = 0;
					this.Actor.ModelObject.ForceDrenched = false;
					break;
				}

				case 2:
				{
					this.Actor.ModelObject.Wetness = 0;
					this.Actor.ModelObject.LockWetness = false;

					this.Actor.ModelObject.Drenched = 1;
					this.Actor.ModelObject.ForceDrenched = true;
					break;
				}
			}
		}
	}
}
