// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Dalamud;

using Anamnesis.Dalamud.Serialization;
using System;
using System.Collections.Generic;

public class IpcMessage
{
	public IpcMessage()
	{
		this.Id = new Guid().ToString();
	}

	public IpcMessage(MessageTypes type)
	{
		this.Id = new Guid().ToString();
		this.Type = type;
	}

	public string Id { get; set; }
	public MessageTypes Type { get; set; }
	public List<MessageParameter> Params { get; set; } = new();
	public MessageParameter? Result { get; set; }

	public void SetParameters(params object[] parameters)
	{
		foreach (object param in parameters)
		{
			this.Params.Add(MessageParameter.FromObject(param));
		}
	}

	public object[] GetParameters()
	{
		List<object> parameters = new();

		foreach (MessageParameter param in this.Params)
		{
			parameters.Add(param.ToValue());
		}

		return parameters.ToArray();
	}

	public class MessageParameter
	{
		public string? Value { get; set; }
		public string? Type { get; set; }

		public static MessageParameter FromObject(object obj)
		{
			MessageParameter param = new();
			param.Type = obj.GetType().FullName;
			param.Value = Serializer.Serialize(obj);
			return param;
		}

		public object ToValue()
		{
			if (this.Type == null)
				throw new Exception("No type in message parameter");

			if (this.Value == null)
				throw new Exception("No value in message parameter");

			Type? type = System.Type.GetType(this.Type);

			if (type == null)
				throw new Exception($"Failed to find parameter type: {this.Type}. Is it implemented in the Anamnesis.Dalamud.Common assembly?");

			return Serializer.Deserialize(this.Value, type);
		}
	}
}
