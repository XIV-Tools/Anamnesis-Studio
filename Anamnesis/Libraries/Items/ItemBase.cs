// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Libraries.Items;

using PropertyChanged;
using System.Collections.Generic;
using System.Collections.ObjectModel;

[AddINotifyPropertyChangedInterface]
public abstract class ItemBase
{
	public string? Name { get; set; }
	public string? Desription { get; set; } = null;
	public string? Author { get; set; } = null;
	public string? Version { get; set; } = null;
	public ObservableCollection<string> Tags { get; init; } = new();

	public abstract bool CanLoad { get; }

	public override string ToString()
	{
		return $"[{this.GetType()}] {this.Name}";
	}

	public bool HasAllTags(IEnumerable<string> required)
	{
		bool allRequiredTagsFound = true;

		foreach (string requiredTag in required)
		{
			bool tagFound = false;
			foreach (string availableTag in this.Tags)
			{
				if (availableTag == requiredTag)
				{
					tagFound = true;
					break;
				}
			}

			allRequiredTagsFound &= tagFound;
		}

		return allRequiredTagsFound;
	}
}
