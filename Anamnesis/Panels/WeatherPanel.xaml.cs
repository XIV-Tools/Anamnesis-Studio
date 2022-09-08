// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Panels;

using Anamnesis.GameData.Excel;
using Anamnesis.Services;
using System.Threading.Tasks;
using System.Windows;
using XivToolsWpf;
using XivToolsWpf.Selectors;

public partial class WeatherPanel : PanelBase
{
	private static readonly WeatherFilter FilterInstance = new();

	public WeatherFilter Filter => FilterInstance;

	protected Task LoadItems()
	{
		if (TerritoryService.Instance.CurrentTerritory == null)
			return Task.CompletedTask;

		this.WeatherSelector.AddItems(GameDataService.Instance.Weathers);

		return Task.CompletedTask;
	}

	private void OnWeatherClicked(object sender, RoutedEventArgs e)
	{
		this.WeatherSelectorPopup.IsOpen = true;
	}

	private void OnWeatherSelected(bool close)
	{
		if (close)
			this.WeatherSelectorPopup.IsOpen = false;

		if (this.WeatherSelector.Value is Weather weather)
		{
			this.Services.Territory.CurrentWeather = weather;
		}
	}

	public class WeatherFilter : Selector.FilterBase<Weather>
	{
		public bool NaturalWeathers { get; set; } = true;

		public override int CompareItems(Weather a, Weather b)
		{
			return a.RowId.CompareTo(b.RowId);
		}

		public override bool FilterItem(Weather weather, string[]? search)
		{
			if (weather.RowId == 0)
				return false;

			if (TerritoryService.Instance.CurrentTerritory != null)
			{
				bool isNatural = TerritoryService.Instance.CurrentTerritory.Weathers.Contains(weather);

				if (this.NaturalWeathers && !isNatural)
				{
					return false;
				}
			}

			bool matches = SearchUtility.Matches(weather.Name, search);
			matches |= SearchUtility.Matches(weather.Description, search);
			matches |= SearchUtility.Matches(weather.WeatherId.ToString(), search);

			if (!matches)
				return false;

			return true;
		}
	}
}
