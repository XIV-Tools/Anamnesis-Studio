// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Files;

using PropertyChanged;
using Serilog;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Controls;
using XivToolsWpf.Extensions;

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

	protected void OnConfigurationChanged()
	{
		this.HandleConfigurationChange().Run();
	}

	protected void SetFile(FileBase? file)
	{
		this.baseFile = file;
		this.RaisePropertyChanged(nameof(this.BaseFile));
		this.RaisePropertyChanged(nameof(this.CanApply));
		this.RaisePropertyChanged(nameof(this.CanRevert));

		// Small hack: raise a property changed for a property 'File' that we don't have yet,
		// as its almost asuredly used by our derrived type for the specific file type.
		this.RaisePropertyChanged("File");
	}

	protected void RaisePropertyChanged(string propertyName)
	{
		this.PropertyChanged?.Invoke(this, new(propertyName));
	}

	private async Task HandleConfigurationChange()
	{
		if (this.CanRevert)
			await this.Revert();

		if (this.CanApply && this.LivePreview)
		{
			await this.Apply(true);
		}
	}
}