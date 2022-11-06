// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Tags;

using PropertyChanged;
using System;
using System.Collections.Generic;
using XivToolsWpf;

[AddINotifyPropertyChangedInterface]
public class Tag : IEquatable<Tag?>
{
	public Tag(string name)
	{
		this.Name = name;
	}

	public string? Name { get; init; }

	public virtual bool CanCompare => true;

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

	public virtual bool Search(string[]? querry)
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
