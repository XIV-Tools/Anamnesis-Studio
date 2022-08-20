// © Anamnesis.
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
		await this.Load();
		this.IsLoading = false;
	}

	public async Task LoadPack(Pack pack)
	{
		this.IsLoading = true;
		await this.Load(pack);
		this.IsLoading = false;
	}

	public async Task UpdatePack(Pack pack)
	{
		pack.IsUpdating = true;
		this.IsLoading = true;
		await this.Update(pack);
		this.IsLoading = false;
		pack.IsUpdating = false;
	}

	protected abstract Task Load();
	protected abstract Task Load(Pack pack);
	protected abstract Task Update(Pack pack);

	protected async Task AddPack(Pack pack)
	{
		await LibraryService.Instance.AddPack(pack);
	}
}
