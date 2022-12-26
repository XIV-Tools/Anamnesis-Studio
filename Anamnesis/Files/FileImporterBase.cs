// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Files;

using PropertyChanged;
using Serilog;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Controls;

[AddINotifyPropertyChangedInterface]
public abstract class FileImporterBase : UserControl, INotifyPropertyChanged
{
	private FileBase? baseFile;

	public FileImporterBase()
	{
		this.GetType().GetMethod("InitializeComponent")?.Invoke(this, null);
		this.DataContext = this;
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	public ILogger Log => Serilog.Log.ForContext(this.GetType());

	public bool LivePreview { get; set; }
	public FileBase? BaseFile => this.baseFile;

	public virtual bool CanApply => this.baseFile != null;
	public virtual bool CanRevert => this.baseFile != null;

	public virtual Task Initialize(FileBase file)
	{
		this.SetFile(file);
		return Task.CompletedTask;
	}

	public virtual Task Apply(bool isPreview)
	{
		if (this.BaseFile == null)
			throw new Exception("No characte file in character file importer");

		return Task.CompletedTask;
	}

	public virtual Task Revert()
	{
		if (this.BaseFile == null)
			throw new Exception("No characte file in character file importer");

		return Task.CompletedTask;
	}

	protected void SetFile(FileBase? file)
	{
		this.baseFile = file;
		this.RaisePropertyChanged(nameof(this.BaseFile));

		// Small hack: raise a property changed for a property 'File' that we don't have yet,
		// as its almost asuredly used by our derrived type for the specific file type.
		this.PropertyChanged?.Invoke(this, new("File"));
	}

	protected void RaisePropertyChanged(string propertyName)
	{
		this.PropertyChanged?.Invoke(this, new(propertyName));
	}
}