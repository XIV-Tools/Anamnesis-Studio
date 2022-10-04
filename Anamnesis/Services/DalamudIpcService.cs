// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Services;

using Anamnesis.Dalamud;
using EasyTcp4;
using EasyTcp4.ClientUtils;
using EasyTcp4.ClientUtils.Async;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

public class DalamudIpcService : ServiceBase<DalamudIpcService>
{
	private readonly EasyTcpClient client = new();
	private readonly Dictionary<string, IpcMessage> responses = new();

	public bool IsConnected { get; private set; } = false;

	public override async Task Initialize()
	{
		await base.Initialize();

		this.client.OnError += (s, e) => Log.Error(e, "IPC Error");
		this.client.OnDataReceiveAsync += this.OnDataReceive;

		if (!await this.client.ConnectAsync(IPAddress.Loopback, DalamudIpcConfig.Port))
		{
			Log.Warning("Could not connect to IPC server");
			return;
		}

		this.IsConnected = true;
	}

	public override Task Shutdown()
	{
		this.client.Dispose();
		return base.Shutdown();
	}

	public async Task<bool> RefreshActor(IntPtr address)
	{
		await this.Send(new(MessageTypes.ActorRefresh, address));
		return true;
	}

	private Task OnDataReceive(object sender, Message message)
	{
		string json = Encoding.UTF8.GetString(message.Data);
		IpcMessage? msg = JsonConvert.DeserializeObject<IpcMessage>(json);

		if (msg != null && msg.Type == MessageTypes.Response)
			this.responses.Add(msg.Id, msg);

		return Task.CompletedTask;
	}

	private async Task Send(IpcMessage msg)
	{
		string json = JsonConvert.SerializeObject(msg);
		this.client.Send(json);

		while (!this.responses.ContainsKey(msg.Id))
			await Task.Delay(50);

		IpcMessage response = this.responses[msg.Id];
		this.responses.Remove(msg.Id);

		// TODO: return value
	}
}