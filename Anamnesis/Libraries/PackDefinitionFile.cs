// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Libraries;

using Anamnesis.Files;

public class PackDefinitionFile : JsonFileBase
{
	public override string FileExtension => ".packdef";

	public string? Name { get; set; }
	public string? Directory { get; set; }
}
