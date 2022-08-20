﻿// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Libraries.Sources;

using Anamnesis.Libraries.Items;
using FontAwesome.Sharp;
using Serilog;
using System.Threading.Tasks;

public abstract class LibrarySourceBase
{
	public bool IsLoading { get; private set; }
	public abstract IconChar Icon { get; }
	public abstract string Name { get; }

	protected ILogger Log => Serilog.Log.ForContext(this.GetType());

	public async Task LoadPacks()
	{
		this.IsLoading = true;
		await this.Load(false);
		this.IsLoading = false;
	}

	protected abstract Task Load(bool force);

	protected async Task AddPack(Pack pack)
	{
		await LibraryService.Instance.AddPack(pack);
	}
}
