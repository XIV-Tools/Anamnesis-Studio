// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Dalamud;

using AnamneisisDalamud;
using global::Dalamud.Logging;
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

	public enum EquipIndex : uint
	{
		Head,
		Chest,
		Hands,
		Legs,
		Feet,
		Earring,
		Necklace,
		Bracelet,
		RingRight,
		RingLeft
	}

	[StructLayout(LayoutKind.Explicit, Size = 0x4)]
	public struct EquipItem
	{
		[FieldOffset(0)] public ushort Id;
		[FieldOffset(2)] public byte Variant;
		[FieldOffset(3)] public byte Dye;
	}

	public ActorRefresh()
	{
		MessageHandler.Register(MessageTypes.ActorRefresh, this.Refresh);

		var changeEquip = Plugin.SigScanner.ScanText("E8 ?? ?? ?? ?? 41 B5 01 FF C3");
		ChangeEquip = Marshal.GetDelegateForFunctionPointer<ChangeEquipDelegate>(changeEquip);
	}

	public void Dispose()
	{
		ChangeEquip = null;
	}

	private Task Refresh(IntPtr address)
	{
		PluginLog.Information($"Refresh actor: {address}");

		if (ChangeEquip == null)
			return Task.CompletedTask;

		EquipItem item = new();
		item.Id = 9903;
		item.Variant = 0;
		item.Dye = 0;

		ChangeEquip(address + 0x6D0, EquipIndex.Chest, item);

		return Task.CompletedTask;
	}
}
