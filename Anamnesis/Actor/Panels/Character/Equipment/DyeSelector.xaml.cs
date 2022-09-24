// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Views;

using System.Threading.Tasks;
using System.Windows.Controls;
using Anamnesis.Actor.Utilities;
using Anamnesis.GameData;
using Anamnesis.Services;
using XivToolsWpf;
using XivToolsWpf.Selectors;

public partial class DyeSelector : UserControl
{
	public DyeSelector()
	{
		this.InitializeComponent();
		this.Selector.DataContext = this;
	}

	protected Task LoadItems()
	{
		this.Selector.AddItem(DyeUtility.NoneDye);

		if (GameDataService.Instance.Dyes != null)
			this.Selector.AddItems(GameDataService.Instance.Dyes);

		return Task.CompletedTask;
	}

	public class DyeFilter : Selector.FilterBase<IDye>
	{
		public override bool FilterItem(IDye dye)
		{
			// skip items without names
			if (string.IsNullOrEmpty(dye.Name))
				return false;

			if (!SearchUtility.Matches(dye.Name, this.SearchQuery))
				return false;

			return true;
		}

		public override int CompareItems(IDye dyeA, IDye dyeB)
		{
			if (dyeA == DyeUtility.NoneDye && dyeB != DyeUtility.NoneDye)
				return -1;

			if (dyeA != DyeUtility.NoneDye && dyeB == DyeUtility.NoneDye)
				return 1;

			if (dyeA.IsFavorite && !dyeB.IsFavorite)
				return -1;

			if (!dyeA.IsFavorite && dyeB.IsFavorite)
				return 1;

			return -dyeB.RowId.CompareTo(dyeA.RowId);
		}
	}
}
