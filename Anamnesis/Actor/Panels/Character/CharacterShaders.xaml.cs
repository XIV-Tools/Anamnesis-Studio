// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Panels.Character;

using Anamnesis.Memory;
using Anamnesis.Services;
using System.Windows.Controls;
using XivToolsWpf.DependencyProperties;

public partial class CharacterShaders : UserControl
{
	public static IBind<ActorMemory?> ActorDp = Binder.Register<ActorMemory?, CharacterShaders>(nameof(Actor));

	public CharacterShaders()
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
}
