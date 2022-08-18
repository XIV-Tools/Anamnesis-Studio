// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Libraries;

using Anamnesis.Libraries.Sources;
using PropertyChanged;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using XivToolsWpf;

[AddINotifyPropertyChangedInterface]
public abstract class LibraryBase
{
	private readonly List<LibrarySourceBase> sources = new();

	public string Name { get; set; } = string.Empty;
	public ObservableCollection<LibraryPack> Packs { get; init; } = new();
	public bool IsLoading { get; private set; }

	protected ILogger Log => Serilog.Log.ForContext(this.GetType());

	public abstract void Initialize();

	public async Task LoadLibrary()
	{
		this.IsLoading = true;

		this.Initialize();

		try
		{
			List<Task> sourceTasks = new();
			foreach(LibrarySourceBase source in this.sources)
			{
				sourceTasks.Add(this.LoadSource(source));
			}

			await Task.WhenAll(sourceTasks);
		}
		catch (Exception ex)
		{
			this.Log.Error(ex, $"Error loading library: {this.Name}");
		}

		this.IsLoading = false;
	}

	protected void AddSource(LibrarySourceBase source)
	{
		this.sources.Add(source);
	}

	private async Task LoadSource(LibrarySourceBase source)
	{
		await Dispatch.NonUiThread();
		List<LibraryPack> packs = await source.Load();

		await Dispatch.MainThread();
		foreach (LibraryPack pack in packs)
		{
			this.Packs.Add(pack);
		}
	}
}