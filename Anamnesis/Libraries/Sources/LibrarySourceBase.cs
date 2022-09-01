// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Libraries.Sources;

using Anamnesis.Libraries.Items;
using FontAwesome.Sharp;
using Serilog;
using System.Threading.Tasks;

public abstract class LibrarySourceBase
{
	public abstract IconChar Icon { get; }
	public abstract string Name { get; }

	protected ILogger Log => Serilog.Log.ForContext(this.GetType());

	public abstract Task Load();

	protected async Task AddPack(Pack pack)
	{
		await LibraryService.Instance.AddPack(pack);
	}
}
