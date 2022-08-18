// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Serialization.Converters;

using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;

public class RectConverter : JsonConverter<Rect>
{
	public override Rect Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		string? str = reader.GetString();

		if (str == null)
			throw new Exception("Cannot convert null to Vector");

		string[] parts = str.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);

		if (parts.Length != 4)
			throw new FormatException();

		Rect v = default;
		v.X = float.Parse(parts[0], CultureInfo.InvariantCulture);
		v.Y = float.Parse(parts[1], CultureInfo.InvariantCulture);
		v.Width = float.Parse(parts[2], CultureInfo.InvariantCulture);
		v.Height = float.Parse(parts[3], CultureInfo.InvariantCulture);
		return v;
	}

	public override void Write(Utf8JsonWriter writer, Rect value, JsonSerializerOptions options)
	{
		writer.WriteStringValue($"{value.X}, {value.Y}, {value.Width}, {value.Height}");
	}
}
