// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Libraries.Items;

using FontAwesome.Sharp;
using PropertyChanged;
using Serilog;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Media;

[AddINotifyPropertyChangedInterface]
public abstract class EntryBase : INotifyPropertyChanged
{
	public event PropertyChangedEventHandler? PropertyChanged;

	public string? Name { get; set; }
	public string? Description { get; set; } = null;
	public ObservableCollection<string> Tags { get; init; } = new();
	public virtual ImageSource? Thumbnail { get; set; }
	public bool IsUpdateAvailable { get; set; } = false;
	public bool IsUpdating { get; set; } = false;

	public abstract bool IsDirectory { get; }
	public abstract IconChar Icon { get; }
	public abstract IconChar IconBack { get; }
	public virtual bool CanOpen => !this.IsUpdating;

	public bool HasThumbnail => this.Thumbnail != null;

	public ILogger Log => Serilog.Log.ForContext(this.GetType());

	public override string ToString()
	{
		return $"[{this.GetType()}] {this.Name}";
	}

	public abstract bool IsType(LibraryFilter.Types type);

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
