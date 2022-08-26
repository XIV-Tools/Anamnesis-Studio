// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Libraries.Items;

using PropertyChanged;
using Serilog;
using System.ComponentModel;

[AddINotifyPropertyChangedInterface]
public abstract class EntryBase : INotifyPropertyChanged
{
	public event PropertyChangedEventHandler? PropertyChanged;

	public string? Name { get; set; }
	public abstract bool IsDirectory { get; }
	public abstract bool HasThumbnail { get; }

	public ILogger Log => Serilog.Log.ForContext(this.GetType());

	public override string ToString()
	{
		return $"[{this.GetType()}] {this.Name}";
	}

	protected void RaisePropertyChanged(string propertyName)
	{
		this.PropertyChanged?.Invoke(this, new(propertyName));
	}
}
