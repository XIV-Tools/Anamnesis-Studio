// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Dalamud;

using Anamneisis.Dalamud;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

// Stolen blatantly from Ktisis
// https://github.com/ktisis-tools/Ktisis
public class ActorRefresh : IDisposable
{
	// Change actor equipment
	// a1 = Actor + 0x6D0, a2 = EquipIndex, a3 = EquipItem
	internal delegate IntPtr ChangeEquipDelegate(IntPtr writeTo, EquipIndex index, EquipItem item);
	internal static ChangeEquipDelegate? ChangeEquip;

	public ActorRefresh()
	{
		IntPtr changeEquip = Plugin.SigScanner.ScanText("E8 ?? ?? ?? ?? 41 B5 01 FF C3");
		ChangeEquip = Marshal.GetDelegateForFunctionPointer<ChangeEquipDelegate>(changeEquip);

		Plugin.IPC.RegisterIpcMethod(MessageTypes.ChangeGear, this.ChangeGear);
	}

	public void Dispose()
	{
		ChangeEquip = null;
	}

	/// <summary>
	/// Change an actors equipment in the given slot.
	/// </summary>
	public Task<bool> ChangeGear(IntPtr address, EquipIndex equip, ushort itemId, byte variant, byte dye)
	{
		if (ChangeEquip != null)
		{
			EquipItem item = new();
			item.Id = itemId;
			item.Variant = variant;
			item.Dye = dye;

			ChangeEquip(address + 0x6D0, equip, item);
			return Task.FromResult(true);
		}

		return Task.FromResult(false);
	}

	[StructLayout(LayoutKind.Explicit, Size = 0x4)]
	public struct EquipItem
	{
		[FieldOffset(0)] public ushort Id;
		[FieldOffset(2)] public byte Variant;
		[FieldOffset(3)] public byte Dye;
	}
}