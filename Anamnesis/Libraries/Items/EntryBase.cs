// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Libraries.Items;

using PropertyChanged;
using Serilog;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

[AddINotifyPropertyChangedInterface]
public abstract class EntryBase : INotifyPropertyChanged
{
	public event PropertyChangedEventHandler? PropertyChanged;

	public string? Name { get; set; }
	public abstract bool IsDirectory { get; }
	public abstract bool HasThumbnail { get; }
	public ObservableCollection<string> Tags { get; init; } = new();

	public ILogger Log => Serilog.Log.ForContext(this.GetType());

	public override string ToString()
	{
		return $"[{this.GetType()}] {this.Name}";
	}

	public bool HasTag(string tag)
	{
		// possibly hashset this.
		foreach (string availableTag in this.Tags)
		{
			if (availableTag == tag)
			{
				return true;
			}
		}

		return false;
	}

	public bool HasAllTags(IEnumerable<string> required)
	{
		bool allRequiredTagsFound = true;

		foreach (string requiredTag in required)
		{
			allRequiredTagsFound &= this.HasTag(requiredTag);
		}

		return allRequiredTagsFound;
	}

	protected void RaisePropertyChanged(string propertyName)
	{
		this.PropertyChanged?.Invoke(this, new(propertyName));
	}
}
