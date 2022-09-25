// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Libraries.Items;

using Anamnesis.Tags;
using FontAwesome.Sharp;
using PropertyChanged;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using XivToolsWpf.Commands;
using XivToolsWpf.Extensions;

[AddINotifyPropertyChangedInterface]
public abstract class EntryBase : INotifyPropertyChanged
{
	public event PropertyChangedEventHandler? PropertyChanged;

	public string? Name { get; set; }
	public string? Description { get; set; } = null;
	public TagCollection Tags { get; init; } = new();
	public List<EntryAction> Actions { get; init; } = new();
	public virtual ImageSource? Thumbnail { get; set; }

	public abstract bool IsDirectory { get; }
	public abstract IconChar Icon { get; }
	public abstract IconChar IconBack { get; }
	public virtual bool CanOpen => true;

	public bool HasThumbnail => this.Thumbnail != null;

	public ILogger Log => Serilog.Log.ForContext(this.GetType());

	public override string ToString()
	{
		return $"[{this.GetType()}] {this.Name}";
	}

	public abstract bool IsType(LibraryFilter.Types type);
	public abstract Task Open();

	protected void RaisePropertyChanged(string propertyName)
	{
		this.PropertyChanged?.Invoke(this, new(propertyName));
	}

	[AddINotifyPropertyChangedInterface]
	public class EntryAction
	{
		public EntryAction(string key, IconChar icon, Func<Task> callback, bool canExecute = true)
		{
			this.DisplayKey = key;
			this.DisplayIcon = icon;
			this.Execute = callback;
			this.CanExecute = canExecute;
			this.Command = new SimpleCommand(() => this.Execute().Run(), () => this.CanExecute);
		}

		public ICommand Command { get; init; }
		public Func<Task> Execute { get; init; }
		public string DisplayKey { get; init; }
		public IconChar DisplayIcon { get; init; }
		public bool CanExecute { get; set; }
	}
}
