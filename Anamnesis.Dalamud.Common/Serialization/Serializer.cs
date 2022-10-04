// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Dalamud.Serialization;

using Anamnesis.Dalamud.Serialization.Converters;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

public static class Serializer
{
	public static JsonSerializerOptions Options = new JsonSerializerOptions();

	static Serializer()
	{
		Options.WriteIndented = true;
		Options.PropertyNameCaseInsensitive = false;
		////Options.IgnoreNullValues = true;
		Options.AllowTrailingCommas = true;
		Options.ReadCommentHandling = JsonCommentHandling.Skip;
		Options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;

		Options.Converters.Add(new JsonStringEnumConverter());
		Options.Converters.Add(new IntPtrConverter());
	}

	public static string Serialize(object obj)
	{
		return JsonSerializer.Serialize(obj, Options);
	}

	public static T Deserialize<T>(string json)
		where T : notnull
	{
		T? result = JsonSerializer.Deserialize<T>(json, Options);

		if (result == null)
			throw new Exception("Failed to deserialize object");

		return result;
	}

	public static object Deserialize(string json, Type type)
	{
		object? result = JsonSerializer.Deserialize(json, type, Options);

		if (result == null)
			throw new Exception("Failed to deserialize object");

		return result;
	}
}
