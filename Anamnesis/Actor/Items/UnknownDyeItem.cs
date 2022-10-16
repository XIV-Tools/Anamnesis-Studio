// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Items;

using System.Windows.Media;
using Anamnesis.GameData;
using Anamnesis.GameData.Sheets;

public class UnknownDye : IDye
{
	public UnknownDye(byte id)
	{
		this.RowId = id;
		this.Id = id;
	}

	public uint RowId { get; private set; }
	public byte Id { get; private set; }
	public string Name => $"Unknown ({this.Id})";
	public string? Description => null;
	public ImageReference? Icon => null;
	public Brush? Color => null;

	public bool IsFavorite
	{
		get => false;
		set { }
	}
}
