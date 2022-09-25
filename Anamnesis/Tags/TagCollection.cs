// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Tags;
using System.Collections.Generic;

public class TagCollection : HashSet<Tag>
{
	public TagCollection()
	{
	}

	public TagCollection(TagCollection other)
	{
		this.AddRange(other);
	}

	public void Add(string name)
	{
		base.Add(new(name));
	}

	public void AddRange(IEnumerable<string> names)
	{
		foreach (string name in names)
		{
			this.Add(name);
		}
	}

	public void AddRange(IEnumerable<Tag> tags)
	{
		foreach (Tag tag in tags)
		{
			this.Add(tag);
		}
	}

	public void Replace(IEnumerable<Tag> tags)
	{
		this.Clear();
		this.AddRange(tags);
	}

	public bool Matches(TagCollection other)
	{
		foreach(Tag tag in other)
		{
			if (!tag.CanCompare)
				continue;

			if (!this.Contains(tag))
			{
				return false;
			}
		}

		return true;
	}
}