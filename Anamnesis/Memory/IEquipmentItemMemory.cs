// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

using Anamnesis.GameData;
using System.ComponentModel;

public interface IEquipmentItemMemory : INotifyPropertyChanged
{
	ushort Set { get; set; }
	byte Dye { get; set; }

	void Equip(IItem item);
	bool Is(IItem? item);
}
