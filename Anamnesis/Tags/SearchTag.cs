// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Tags;

using FontAwesome.Sharp.Pro;
using System;

public class SearchTag : Tag
{
	public SearchTag(string search)
		: base(search)
	{
	}

	public override ProIcons Icon => ProIcons.Search;
	public override bool CanCompare => false;
	public override bool Search(string[]? querry) => throw new NotSupportedException();
}
