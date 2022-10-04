// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Dalamud;

using Anamnesis.Dalamud.Serialization;
using EasyTcp4;
using EasyTcp4.ClientUtils;
using EasyTcp4.ClientUtils.Async;
using EasyTcp4.ServerUtils;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class AnamesisDalamudIPC : IDisposable
{
	private const int Port = 1145;

	private EasyTcpServer? server;
	private EasyTcpClient? client;
	private readonly Dictionary<string, IpcMessage> responses = new();
	private readonly Dictionary<MessageTypes, Delegate> callbacks = new();

	public Action<Exception, string>? LogError;
	public Action<string>? LogMessage;

	public void StartServer()
	{
		if (this.client != null || this.server != null)
			throw new Exception("Attempt to start IPC server while it is already running");

		this.server = new();
		this.server.EnableServerKeepAlive();
		this.server.OnDataReceiveAsync += OnDataReceived;
		this.server.OnError += (s, e) => this.LogError?.Invoke(e, "IPC error");
		this.server.OnConnect += (s, e) => this.LogMessage?.Invoke("IPC client connected");

		this.server.Start(Port);
	}

	public async Task<bool> StartClient()
	{
		if (this.client != null || this.server != null)
			throw new Exception("Attempt to start IPC client while it is already running");

		this.client = new();
		this.client.OnError += (s, e) => LogError?.Invoke(e, "IPC error");
		this.client.OnDataReceiveAsync += this.OnDataReceived;

		return await this.client.ConnectAsync(IPAddress.Loopback, Port);
	}

	public void Dispose()
	{
		this.server?.Dispose();
		this.client?.Dispose();
	}

	public void RegisterIpcMethod(MessageTypes type, Delegate d)
	{
		if (this.callbacks.ContainsKey(type))
			throw new Exception($"Attempt to register multiple handlers for message type: {type}");

		this.callbacks.Add(type, d);
	}

	/// <summary>
	/// Invoke an IPC method and wait for it to complete.
	/// </summary>
	public Task Invoke(MessageTypes type, params object[] param)
	{
		IpcMessage request = new(type);
		request.SetParameters(param);
		return this.SendRequest(request);
	}

	/// <summary>
	/// Invoke an IPC method and get its return value.
	/// </summary>
	public async Task<T> Invoke<T>(MessageTypes type, params object[] param)
	{
		IpcMessage request = new(type);
		request.SetParameters(param);
		return await this.SendRequest<T>(request);
	}

	private async Task<T> SendRequest<T>(IpcMessage msg)
	{
		IpcMessage response = await this.SendRequest(msg);

		object? obj = response.Result?.ToValue();

		if (obj is T tObj)
			return tObj;

		throw new Exception($"IPC response result: {obj} was not type: {typeof(T)}");
	}

	private async Task<IpcMessage> SendRequest(IpcMessage msg)
	{
		string json = JsonSerializer.Serialize(msg);
		this.client.Send(json);

		while (!this.responses.ContainsKey(msg.Id))
			await Task.Delay(50);

		IpcMessage response = this.responses[msg.Id];
		this.responses.Remove(msg.Id);
		return response;
	}

	private async Task OnDataReceived(object? sender, Message e)
	{
		string json = Encoding.UTF8.GetString(e.Data);
		IpcMessage? msg = Serializer.Deserialize<IpcMessage>(json);

		if (msg == null)
			return;

		if (msg.Type == MessageTypes.Response)
		{
			this.responses.Add(msg.Id, msg);
		}
		else
		{
			await this.HandleRequest(msg);
		}
	}

	private async Task HandleRequest(IpcMessage msg)
	{
		IpcMessage response = new(MessageTypes.Response);
		response.Id = msg.Id;

		if (this.callbacks.ContainsKey(msg.Type))
		{
			Delegate d = this.callbacks[msg.Type];

			object[] param = msg.GetParameters();
			object? returnValue = d.DynamicInvoke(param);

			this.LogMessage?.Invoke($"Invoke: {d}");

			// If this method returned a task, await it and get its response.
			if (returnValue is Task task)
			{
				await task;

				PropertyInfo? resultProperty = task.GetType().GetProperty("Result", BindingFlags.Public | BindingFlags.Instance);
				if (resultProperty != null)
				{
					returnValue = resultProperty.GetValue(task);
				}
				else
				{
					returnValue = null;
				}

				this.LogMessage?.Invoke($"Got result: {returnValue}");
			}

			// Add the method return value to the response
			if (returnValue != null)
			{
				response.Result = IpcMessage.MessageParameter.FromObject(returnValue);
			}
		}

		string responseJson = Serializer.Serialize(response);
		this.server.SendAll(responseJson);
	}
}
