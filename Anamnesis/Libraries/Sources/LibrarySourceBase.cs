// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Libraries.Sources;

using Anamnesis.Libraries.Items;
using FontAwesome.Sharp;
using PropertyChanged;
using Serilog;
using System.Threading.Tasks;

[AddINotifyPropertyChangedInterface]
public abstract class LibrarySourceBase
{
	public bool IsLoading { get; set; }
	public abstract IconChar Icon { get; }
	public abstract string Name { get; }

	protected ILogger Log => Serilog.Log.ForContext(this.GetType());

	public async Task Load()
	{
		this.IsLoading = true;
		await this.Load(false);
		this.IsLoading = false;
	}

	public async Task Refresh()
	{
		this.IsLoading = true;
		await this.Load(true);
		this.IsLoading = false;
	}

	protected abstract Task Load(bool refresh);

	protected async Task AddPack(Pack pack)
	{
		await LibraryService.Instance.AddPack(pack);
	}
}
