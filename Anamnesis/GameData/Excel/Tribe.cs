// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Excel;

using Anamnesis.Memory;
using Lumina.Data;
using Lumina.Excel;
using Lumina.Text;

using ExcelRow = Anamnesis.GameData.Sheets.ExcelRow;
using CustomizeTribes = Anamnesis.Memory.ActorCustomizeMemory.Tribes;
using CustomizeAges = Anamnesis.Memory.ActorCustomizeMemory.Ages;
using System.Collections.Generic;

[Sheet("Tribe", 0xe74759fb)]
public class Tribe : ExcelRow
{
	public string Name => this.TribeId.ToString();
	public string DisplayName => this.Feminine;
	public string Feminine { get; private set; } = string.Empty;
	public string Masculine { get; private set; } = string.Empty;

	// Customize options
	public List<CustomizeAges> Ages { get; private set; } = new();

	// Customize Flags
	public ActorCustomizeMemory.Tribes TribeId => (CustomizeTribes)this.RowId;

	public bool Equals(Tribe? other)
	{
		if (other is null)
			return false;

		return this.TribeId == other.TribeId;
	}

	public override void PopulateData(RowParser parser, Lumina.GameData gameData, Language language)
	{
		base.PopulateData(parser, gameData, language);

		this.Masculine = parser.ReadColumn<SeString>(0) ?? string.Empty;
		this.Feminine = parser.ReadColumn<SeString>(1) ?? string.Empty;
	}
}
