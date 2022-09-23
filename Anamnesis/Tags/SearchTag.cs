// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Tags;

using System;

public class SearchTag : Tag
{
	public SearchTag(string search)
		: base(search)
	{
	}

	public override bool CanCompare => false;
	public override bool Search(string[]? querry) => throw new NotSupportedException();
}
