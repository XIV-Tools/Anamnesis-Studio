// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Dalamud.Serialization.Converters;

using System;
using System.Text.Json.Serialization;
using System.Text.Json;

public class IntPtrConverter : JsonConverter<IntPtr>
{
	public override IntPtr Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		string? str = reader.GetString();

		if (str == null)
			throw new Exception("Cannot convert null to Quaternion");

		return IntPtr.Parse(str);
	}

	public override void Write(Utf8JsonWriter writer, IntPtr value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(value.ToString());
	}
}
