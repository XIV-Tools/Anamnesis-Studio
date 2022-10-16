// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Posing;

using Anamnesis.Memory;
using PropertyChanged;
using System.Collections.Generic;

[AddINotifyPropertyChangedInterface]
public class BoneViewModel
{
	private readonly List<TransformMemory> transforms;

	public BoneViewModel(string name, List<TransformMemory> transforms)
	{
		this.Name = name;
		this.transforms = transforms;
	}

	public string Name { get; init; }

	public bool IsSelected
	{
		get;
		set;
	}
}