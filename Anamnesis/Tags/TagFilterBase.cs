// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Tags;

using System.Collections.Generic;
using XivToolsWpf.Selectors;

public abstract class TagFilterBase : Selector.FilterBase
{
	public string[]? SearchTags { get; private set; }
	public TagCollection Tags { get; init; } = new();

	public virtual void OnTagsChanged()
	{
		this.NotifyChanged(nameof(this.Tags));

		List<string> searchTags = new();
		foreach(Tag tag in this.Tags)
		{
			if (tag is SearchTag searchTag && !string.IsNullOrEmpty(searchTag.Name))
			{
				searchTags.Add(searchTag.Name);
			}
		}

		this.SearchTags = searchTags.ToArray();
	}
}

public abstract class TagFilterBase<T> : TagFilterBase
	where T : ITagged
{
	public sealed override bool FilterItem(object obj)
	{
		if (obj is T tObj)
		{
			if (!tObj.Tags.Matches(this.Tags))
				return false;

			if (this.SearchQuery != null && !tObj.Search(this.SearchQuery))
				return false;

			if (this.SearchTags != null && !tObj.Search(this.SearchTags))
				return false;

			if (!this.FilterItem(tObj))
			{
				return false;
			}

			return true;
		}

		return false;
	}

	public sealed override int CompareItems(object a, object b)
	{
		if (a is T tA && b is T tB)
			return this.CompareItems(tA, tB);

		return 0;
	}

	public abstract bool FilterItem(T obj);
	public abstract int CompareItems(T a, T b);
}
