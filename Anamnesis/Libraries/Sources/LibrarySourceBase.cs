// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Libraries.Sources;

using Anamnesis.Libraries.Items;
using Serilog;
using System.Threading.Tasks;

public abstract class LibrarySourceBase
{
	public bool IsLoading { get; private set; }

	protected ILogger Log => Serilog.Log.ForContext(this.GetType());

	public async Task LoadPacks()
	{
		this.IsLoading = true;
		await this.Load();
		this.IsLoading = false;
	}

	protected abstract Task Load();

	protected void AddPack(Pack pack)
	{
		LibraryService.Instance.AddPack(pack);
	}
}
