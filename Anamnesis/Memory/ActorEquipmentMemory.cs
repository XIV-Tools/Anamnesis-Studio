// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

using Anamnesis.GameData;

public class ActorEquipmentMemory : MemoryBase
{
	[Bind(0x000)] public ItemMemory? Head { get => this.GetValue<ItemMemory?>(); set => this.SetValue(value); }
	[Bind(0x004)] public ItemMemory? Body { get => this.GetValue<ItemMemory?>(); set => this.SetValue(value); }
	[Bind(0x008)] public ItemMemory? Hands { get => this.GetValue<ItemMemory?>(); set => this.SetValue(value); }
	[Bind(0x00C)] public ItemMemory? Legs { get => this.GetValue<ItemMemory?>(); set => this.SetValue(value); }
	[Bind(0x010)] public ItemMemory? Feet { get => this.GetValue<ItemMemory?>(); set => this.SetValue(value); }
	[Bind(0x014)] public ItemMemory? Ears { get => this.GetValue<ItemMemory?>(); set => this.SetValue(value); }
	[Bind(0x018)] public ItemMemory? Neck { get => this.GetValue<ItemMemory?>(); set => this.SetValue(value); }
	[Bind(0x01C)] public ItemMemory? Wrists { get => this.GetValue<ItemMemory?>(); set => this.SetValue(value); }
	[Bind(0x020)] public ItemMemory? RightRing { get => this.GetValue<ItemMemory?>(); set => this.SetValue(value); }
	[Bind(0x024)] public ItemMemory? LeftRing { get => this.GetValue<ItemMemory?>(); set => this.SetValue(value); }

	public ItemMemory? GetSlot(ItemSlots slot)
	{
		switch (slot)
		{
			case ItemSlots.Head: return this.Head;
			case ItemSlots.Body: return this.Body;
			case ItemSlots.Hands: return this.Hands;
			case ItemSlots.Legs: return this.Legs;
			case ItemSlots.Feet: return this.Feet;
			case ItemSlots.Ears: return this.Ears;
			case ItemSlots.Neck: return this.Neck;
			case ItemSlots.Wrists: return this.Wrists;
			case ItemSlots.RightRing: return this.RightRing;
			case ItemSlots.LeftRing: return this.LeftRing;
		}

		return null;
	}
}
