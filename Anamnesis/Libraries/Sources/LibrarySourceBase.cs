// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Libraries.Sources;

using Anamnesis.Libraries.Items;
using Serilog;
using System.Collections.Generic;
using System.Threading.Tasks;

public abstract class LibrarySourceBase
{
	protected ILogger Log => Serilog.Log.ForContext(this.GetType());

	public abstract Task<List<Pack>> LoadPacks();
}
