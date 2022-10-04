// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Services;

using Anamnesis.Dalamud;
using Anamnesis.Memory;
using System;
using System.Threading.Tasks;

public class DalamudIpcService : ServiceBase<DalamudIpcService>
{
	private readonly AnamesisDalamudIPC ipc = new();

	public bool IsConnected { get; private set; } = false;

	public override async Task Initialize()
	{
		await base.Initialize();

		this.ipc.LogMessage = (msg) => Log.Information(msg);
		this.ipc.LogError = (e, msg) => Log.Error(e, msg);
		this.IsConnected = await this.ipc.StartClient();
	}

	public override Task Shutdown()
	{
		this.ipc.Dispose();
		return base.Shutdown();
	}

	public async Task<bool> ChangeGear(IntPtr address, EquipIndex equip, ItemMemory? item)
	{
		if (item == null)
			return false;

		return await this.ipc.Invoke<bool>(MessageTypes.ChangeGear, address, equip, item.Base, item.Variant, item.Dye);
	}

	public Task<bool> ChangeGear(IntPtr address, EquipIndex equip, ushort itemId, byte variant, byte dye)
	{
		return this.ipc.Invoke<bool>(MessageTypes.ChangeGear, address, equip, itemId, variant, dye);
	}
}