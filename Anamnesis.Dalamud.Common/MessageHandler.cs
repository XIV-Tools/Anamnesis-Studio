// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Dalamud;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public static class MessageHandler
{
	private static readonly Dictionary<MessageTypes, Func<IpcMessage, Task>> callbacks = new();

	public static void Register(MessageTypes type, Func<Task> callback) => RegisterIpcCallback(type, (m) => callback.Invoke());
	public static void Register(MessageTypes type, Func<string, Task> callback) => RegisterIpcCallback(type, (m) => callback.Invoke(m.StringParam));
	public static void Register(MessageTypes type, Func<double, Task> callback) => RegisterIpcCallback(type, (m) => callback.Invoke(m.NumberParam));
	public static void Register(MessageTypes type, Func<IntPtr, Task> callback) => RegisterIpcCallback(type, (m) => callback.Invoke(m.PointerParam));

	public static async Task<IpcMessage> Invoke(IpcMessage msg)
	{
		IpcMessage response = new(MessageTypes.Response);
		response.Id = msg.Id;

		if (!callbacks.ContainsKey(msg.Type))
			return response;

		await callbacks[msg.Type].Invoke(msg);

		// TODO: add return value support!
		return response;
	}

	private static void RegisterIpcCallback(MessageTypes type, Func<IpcMessage, Task> callback)
	{
		if (callbacks.ContainsKey(type))
			throw new Exception($"Attempt to register multiple handlers for message type: {type}");

		callbacks.Add(type, callback);
	}
}
