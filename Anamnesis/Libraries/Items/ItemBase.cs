// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Libraries.Items;

using PropertyChanged;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Media;

[AddINotifyPropertyChangedInterface]
public abstract class ItemEntry : EntryBase
{
	public string? Description { get; set; } = null;
	public string? Author { get; set; } = null;
	public string? Version { get; set; } = null;
	public ObservableCollection<string> Tags { get; init; } = new();
	public virtual ImageSource? Icon => null;
	public override bool IsDirectory => false;

	public abstract bool CanLoad { get; }

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