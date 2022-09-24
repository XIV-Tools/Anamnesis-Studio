// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.Sheets;

using Anamnesis.Services;
using System;

public class ExcelRow : Lumina.Excel.ExcelRow, IEquatable<ExcelRow>
{
	public static GameDataService GameData => App.Services.GameData;

	public static bool operator ==(ExcelRow? lhs, ExcelRow? rhs)
	{
		if (ReferenceEquals(lhs, rhs))
			return true;

		if (lhs is null)
			return false;

		if (rhs is null)
			return false;

		return lhs.Equals(rhs);
	}

	public static bool operator !=(ExcelRow? lhs, ExcelRow? rhs)
	{
		return !(lhs == rhs);
	}

	public bool Equals(ExcelRow? other)
	{
		if (other is null)
			return false;

		if (ReferenceEquals(this, other))
			return true;

		return this.RowId.Equals(other.RowId)
			   && this.SubRowId.Equals(other.SubRowId)
			   && this.SheetName.Equals(other.SheetName)
			   && this.SheetLanguage.Equals(other.SheetLanguage);
	}

	public override bool Equals(object? obj)
	{
		return this.Equals(obj as ExcelRow);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(this.RowId, this.SubRowId, this.SheetName, this.SheetLanguage);
	}
}
