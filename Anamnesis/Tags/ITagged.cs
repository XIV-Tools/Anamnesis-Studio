// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Tags;

using XivToolsWpf.Selectors;

public interface ITagged : ISearchable
{
	TagCollection Tags { get; }
}
