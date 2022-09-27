// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Tags;

using System.Collections.Generic;
using System.Threading.Tasks;
using XivToolsWpf.Selectors;

public abstract class TagFilterBase : MultiThreadedFilterBase
{
	public string[]? SearchTags { get; private set; }
	public TagCollection Tags { get; init; } = new();

	public TagCollection AvailableTags { get; init; } = new();

	public virtual void OnTagsChanged()
	{
		this.OnPropertyChanged(new(nameof(this.Tags)));

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

	protected override async Task<IEnumerable<object>?> Filter()
	{
		IEnumerable<object>? results = await base.Filter();

		// Get all tags from the results and put them in the available tags
		// list.
		if (results != null)
		{
			this.AvailableTags.Clear();
			foreach (object obj in results)
			{
				if (obj is ITagged tagged)
				{
					this.AvailableTags.AddRange(tagged.Tags);
				}
			}
		}

		return results;
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
