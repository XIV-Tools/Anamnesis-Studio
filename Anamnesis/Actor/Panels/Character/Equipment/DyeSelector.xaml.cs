// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Views;

using System.Threading.Tasks;
using System.Windows.Controls;
using Anamnesis.Actor.Utilities;
using Anamnesis.GameData;
using Anamnesis.Memory;
using Anamnesis.Services;
using XivToolsWpf;
using XivToolsWpf.DependencyProperties;
using XivToolsWpf.Selectors;

public partial class DyeSelector : UserControl
{
	public static readonly IBind<IEquipmentItemMemory?> ItemModelDp = Binder.Register<IEquipmentItemMemory?, DyeSelector>(nameof(ItemModel), BindMode.TwoWay);

	public DyeSelector()
	{
		this.InitializeComponent();
		this.Selector.DataContext = this;
	}

	public DyeFilter Filter { get; init; } = new();

	public IEquipmentItemMemory? ItemModel
	{
		get => ItemModelDp.Get(this);
		set => ItemModelDp.Set(this, value);
	}

	protected Task LoadItems()
	{
		this.Selector.AddItem(DyeUtility.NoneDye);

		if (GameDataService.Instance.Dyes != null)
			this.Selector.AddItems(GameDataService.Instance.Dyes);

		return Task.CompletedTask;
	}

	private void OnSelectionChanged(bool close)
	{
		IDye? dye = this.Selector.Value as IDye;

		if (dye == null || this.ItemModel == null)
			return;

		this.ItemModel.Dye = dye.Id;
	}

	public class DyeFilter : FilterBase<IDye>
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
