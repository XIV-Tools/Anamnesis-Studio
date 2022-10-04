// © Anamnesis.
// Licensed under the MIT license.

using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Game.Gui;
using Dalamud.Game;
using Anamnesis.Dalamud;
using EasyTcp4;
using EasyTcp4.ServerUtils;
using System.Text;
using Dalamud.Logging;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace AnamneisisDalamud;
public sealed class Plugin : IDalamudPlugin
{
	private readonly EasyTcpServer server = new();

	public string Name => "Anamnesis Dalamud";
	
	[PluginService][RequiredVersion("1.0")] public static DalamudPluginInterface PluginInterface { get; private set; } = null!;
	[PluginService][RequiredVersion("1.0")] public static CommandManager CommandManager { get; private set; } = null!;
	[PluginService][RequiredVersion("1.0")] public static ChatGui ChatGui { get; private set; } = null!;
	[PluginService][RequiredVersion("1.0")] public static SigScanner SigScanner { get; private set; } = null!;

	private readonly ActorRefresh refresh;

	public Plugin()
    {
		this.server.EnableServerKeepAlive();
		this.server = this.server.Start(DalamudIpcConfig.Port);
		this.server.OnDataReceiveAsync += OnDataReceive;
		this.server.OnError += (s, e) => PluginLog.Error(e, "IPC error");
		this.server.OnConnect += (s, e) => PluginLog.Information("IPC client connected");

		this.refresh = new();

		PluginLog.Information($"Anamnesis Dalamud started on port: {DalamudIpcConfig.Port}");
	}

	public void Dispose()
	{
		this.refresh.Dispose();
		this.server.Dispose();
	}

	private async Task OnDataReceive(object? sender, Message e)
	{
		string json = Encoding.UTF8.GetString(e.Data);
		IpcMessage? msg = JsonConvert.DeserializeObject<IpcMessage>(json);

		if (msg == null)
			return;

		IpcMessage response = await MessageHandler.Invoke(msg);

		if (response == null)
			return;

		string responseJson = JsonConvert.SerializeObject(response);
		this.server.SendAll(responseJson);
	}
}