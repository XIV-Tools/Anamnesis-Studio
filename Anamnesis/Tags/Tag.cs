// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Tags;

using Anamnesis.Actor;
using System;
using System.Collections.Generic;
using XivToolsWpf;

public class Tag : IEquatable<Tag?>
{
	public Tag(string name)
	{
		this.Name = name;
	}

	public string? Name { get; init; }

	public static implicit operator Tag(string name)
	{
		return new Tag(name);
	}

	public static bool operator ==(Tag? left, Tag? right)
	{
		return EqualityComparer<Tag>.Default.Equals(left, right);
	}

	public static bool operator !=(Tag? left, Tag? right)
	{
		return !(left == right);
	}

	public bool Search(string[]? querry)
	{
		if (SearchUtility.Matches(this.Name, querry))
			return true;

		return false;
	}

	public override bool Equals(object? obj)
	{
		return this.Equals(obj as Tag);
	}

	public bool Equals(Tag? other)
	{
		return other is not null && this.Name == other.Name;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(this.Name);
	}
}

public class TagCollection : HashSet<Tag>
{
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
}