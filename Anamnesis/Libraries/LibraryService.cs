// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Libraries;

using System.Threading.Tasks;
using XivToolsWpf.Extensions;

public class LibraryService : ServiceBase<LibraryService>
{
	public PoseLibrary Poses { get; init; } = new();

	public override Task Initialize()
	{
		this.Poses.LoadLibrary().Run();

		return base.Initialize();
	}
}
