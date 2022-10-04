// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Dalamud;

using System;

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

	public IpcMessage(MessageTypes type, IntPtr param)
	{
		this.Id = new Guid().ToString();
		this.Type = type;
		this.PointerParam = param;
	}

	public IpcMessage(MessageTypes type, double param)
	{
		this.Id = new Guid().ToString();
		this.Type = type;
		this.NumberParam = param;
	}

	public IpcMessage(MessageTypes type, string param)
	{
		this.Id = new Guid().ToString();
		this.Type = type;
		this.StringParam = param;
	}

	public string Id { get; set; }
	public MessageTypes Type { get; set; }
	public double NumberParam { get; set; }
	public string StringParam { get; set; }
	public IntPtr PointerParam { get; set; }
}
