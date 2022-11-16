// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Files;

using Anamnesis.Services;
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

	public FileBase? BaseFile
	{
		get => this.baseFile;
		set => this.SetFile(value).Run();
	}

	public virtual Task Initialize(FileBase file)
	{
		this.BaseFile = file;

		// Small hack: raise a property changed for a property 'File' that we don't have yet,
		// as its alm,ost asuredly used by our derrived type for the specific file type.
		this.PropertyChanged?.Invoke(this, new("File"));

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

	protected async Task SetFile(FileBase? file)
	{
		try
		{
			await this.Revert();
			this.baseFile = file;
			this.RaisePropertyChanged(nameof(this.BaseFile));
			await this.Apply(true);
		}
		catch (Exception ex)
		{
			this.Log.Error("Failed to set file", ex);
		}
	}

	protected void RaisePropertyChanged(string propertyName)
	{
		this.PropertyChanged?.Invoke(this, new(propertyName));
	}
}