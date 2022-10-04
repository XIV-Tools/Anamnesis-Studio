// © Anamnesis.
// Licensed under the MIT license.

using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Game.Gui;
using Dalamud.Game;
using Anamnesis.Dalamud;
using Dalamud.Logging;

namespace Anamneisis.Dalamud;

public sealed class Plugin : IDalamudPlugin
{
	public string Name => "Anamnesis Dalamud";

	public static AnamesisDalamudIPC IPC { get; private set; } = null!;
	[PluginService][RequiredVersion("1.0")] public static DalamudPluginInterface PluginInterface { get; private set; } = null!;
	[PluginService][RequiredVersion("1.0")] public static CommandManager CommandManager { get; private set; } = null!;
	[PluginService][RequiredVersion("1.0")] public static ChatGui ChatGui { get; private set; } = null!;
	[PluginService][RequiredVersion("1.0")] public static SigScanner SigScanner { get; private set; } = null!;

	private readonly ActorRefresh refresh;

	public Plugin()
    {
		IPC = new();
		IPC.LogMessage = (msg) => PluginLog.Information(msg);
		IPC.LogError = (e, msg) => PluginLog.Error(e, msg);
		IPC.StartServer();

		this.refresh = new();

		PluginLog.Information($"Anamnesis Dalamud started");
	}

	public void Dispose()
	{
		this.refresh.Dispose();
	}
}