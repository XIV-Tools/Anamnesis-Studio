// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Serialization.Converters;

using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;

public class SizeConverter : JsonConverter<Size>
{
	public override Size Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		string? str = reader.GetString();

		if (str == null)
			throw new Exception("Cannot convert null to Size");

		string[] parts = str.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);

		if (parts.Length != 2)
			throw new FormatException();

		Size v = default;
		v.Width = float.Parse(parts[0], CultureInfo.InvariantCulture);
		v.Height = float.Parse(parts[1], CultureInfo.InvariantCulture);

		return v;
	}

	public override void Write(Utf8JsonWriter writer, Size value, JsonSerializerOptions options)
	{
		var formatString = string.Format(CultureInfo.InvariantCulture, "{0}, {1}", value.Width, value.Height);
		writer.WriteStringValue(formatString);
	}
}
