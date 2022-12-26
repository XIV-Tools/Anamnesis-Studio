// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Libraries.Items;

using Anamnesis.Files;
using Anamnesis.Tags;
using FontAwesome.Sharp;
using PropertyChanged;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using XivToolsWpf;
using XivToolsWpf.Commands;
using XivToolsWpf.Extensions;

[AddINotifyPropertyChangedInterface]
public abstract class EntryBase : ITagged, INotifyPropertyChanged
{
	public event PropertyChangedEventHandler? PropertyChanged;

	public string? Name { get; set; }
	public string? Description { get; set; } = null;
	public TagCollection Tags { get; init; } = new();
	public List<EntryAction> Actions { get; init; } = new();
	public virtual ImageSource? Thumbnail { get; set; }
	public DirectoryEntry? Parent { get; set; }

	public abstract bool IsDirectory { get; }
	public abstract IconChar Icon { get; }
	public abstract IconChar IconBack { get; }
	public virtual bool CanOpen => true;
	public virtual Type? ImporterType => null;

	public bool HasThumbnail => this.Thumbnail != null;

	public ILogger Log => Serilog.Log.ForContext(this.GetType());

	public override string ToString()
	{
		return $"[{this.GetType()}] {this.Name}";
	}

	public abstract bool IsType(LibraryFilter.Types type);
	public abstract Task Open(FileImporterBase? importer = null);

	public virtual bool Search(string[] query)
	{
		if (SearchUtility.Matches(this.Name, query))
			return true;

		if (SearchUtility.Matches(this.Description, query))
			return true;

		return false;
	}

	protected void RaisePropertyChanged(string propertyName)
	{
		this.PropertyChanged?.Invoke(this, new(propertyName));
	}

	[AddINotifyPropertyChangedInterface]
	public class EntryAction
	{
		public EntryAction(string label, IconChar icon, Func<Task> callback, bool canExecute = true)
		{
			this.Label = label;
			this.Icon = icon;
			this.Execute = callback;
			this.CanExecute = canExecute;
			this.Command = new SimpleCommand(() => this.Execute().Run(), () => this.CanExecute);
		}

		public ICommand Command { get; init; }
		public Func<Task> Execute { get; init; }
		public string Label { get; init; }
		public IconChar Icon { get; init; }
		public bool CanExecute { get; set; }
	}
}
