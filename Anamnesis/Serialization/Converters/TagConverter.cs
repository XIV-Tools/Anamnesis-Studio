// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Serialization.Converters;

using Anamnesis.Tags;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

public class TagConverter : JsonConverter<Tag>
{
	public override Tag Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		string? str = reader.GetString();

		if (str == null)
			throw new Exception("Cannot convert null to Vector");

		return new Tag(str);
	}

	public override void Write(Utf8JsonWriter writer, Tag value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(value.Name);
	}
}
