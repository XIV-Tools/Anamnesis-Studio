// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Panels;

using Anamnesis.Services;
using Anamnesis.Windows;
using System;
using System.Collections.Generic;
using System.Windows;

public class PanelService : ServiceBase<PanelService>
{
	private static readonly List<PanelBase> OpenPanels = new();

	public static T Show<T>(object? context = null)
		where T : PanelBase
	{
		T? panel = Show(typeof(T), context) as T;

		if (panel == null)
			throw new Exception($"Failed to create instance of panel: {typeof(T)}");

		return panel;
	}

	public static List<PanelBase> GetPanels(Type panelType)
	{
		List<PanelBase> results = new();

		foreach (PanelBase panel in OpenPanels)
		{
			if (panel.GetType() == panelType)
			{
				results.Add(panel);
			}
		}

		return results;
	}

	public static PanelBase Show(Type panelType, object? context = null)
	{
		IPanelHost panelHost = CreateHost();
		PanelBase? panel;

		try
		{
			panel = Activator.CreateInstance(panelType, panelHost) as PanelBase;
		}
		catch (Exception)
		{
			panel = Activator.CreateInstance(panelType, panelHost, context) as PanelBase;
		}

		if (panel == null)
			throw new Exception($"Failed to create instance of panel: {panelType}");

		panel.DataContext = context;
		panelHost.AddPanel(panel);
		panelHost.Show();

		// Copy width and height values from the inner panel to the host
		if (panel.CanResize && (double.IsNormal(panel.Width) || double.IsNormal(panel.Height)))
		{
			Rect rect = panelHost.Rect;

			if (double.IsNormal(panel.Width))
				rect.Width = panel.Width;

			if (double.IsNormal(panel.Height))
				rect.Height = panel.Height + 28; // height of panel titlebar

			panel.Width = double.NaN;
			panel.Height = double.NaN;
			panelHost.Rect = rect;
		}

		Rect? lastPos = GetLastPosition(panelHost);
		if (lastPos != null)
		{
			panelHost.RelativeRect = (Rect)lastPos;
		}
		else
		{
			// Center screen
			panelHost.RelativeRect = new Rect(0.5, 0.5, 0, 0);
		}

		OpenPanels.Add(panel);
		return panel;
	}

	public static void OnPanelClosed(PanelBase panel)
	{
		OpenPanels.Remove(panel);
	}

	public static IPanelHost CreateHost()
	{
		// TODO: if OverlayMode!
		return new OverlayWindow();
	}

	public static Rect? GetLastPosition(IPanelHost panel)
	{
		if (SettingsService.Current.Panels.TryGetValue(panel.Id, out PanelsData? data) && data != null)
		{
			return data.Position;
		}

		return null;
	}

	public static void SavePosition(IPanelHost panel)
	{
		if (SettingsService.Current.Panels.TryGetValue(panel.Id, out PanelsData? data) && data != null)
		{
			data.Position = panel.RelativeRect;
		}
		else
		{
			SettingsService.Current.Panels.Add(panel.Id, new(panel.RelativeRect));
		}

		SettingsService.Save();
	}

	[Serializable]
	public class PanelsData
	{
		public PanelsData()
		{
		}

		public PanelsData(Rect pos)
		{
			this.Position = pos;
		}

		public Rect Position { get; set; }
	}
}
