// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Excel;

using Anamnesis.GameData.Sheets;
using Lumina.Data;
using Lumina.Excel;

using ExcelRow = Anamnesis.GameData.Sheets.ExcelRow;
using LuminaData = Lumina.GameData;

[Sheet("Lobby", columnHash: 0x54075F2E)]
public class Lobby : ExcelRow
{
	public string? Text { get; set; }
	public string? Unknown4 { get; set; }
	public string? Unknown5 { get; set; }

	public override void PopulateData(RowParser parser, LuminaData gameData, Language language)
	{
		base.PopulateData(parser, gameData, language);
		////TYPE = parser.ReadColumn<uint>(0);
		////PARAM = parser.ReadColumn<uint>(1);
		////LINK = parser.ReadColumn<uint>(2);
		this.Text = parser.ReadString(3);
		this.Unknown4 = parser.ReadString(4);
		this.Unknown5 = parser.ReadString(5);
	}
}