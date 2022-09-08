// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Panels;

using Anamnesis.Services;
using Anamnesis.Windows;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Anamnesis.Actor.Panels;
using Anamnesis.Libraries.Panels;
using XivToolsWpf;
using XivToolsWpf.Extensions;

public class PanelService : ServiceBase<PanelService>
{
	private static readonly List<PanelBase> OpenPanels = new();
	private static readonly Dictionary<Type, PanelBase> Panelcache = new();

	private static readonly List<Type> PreLoadPanels = new()
	{
		typeof(CustomizePanel),
		typeof(LibraryPanel),
		typeof(CameraPanel),
		typeof(ExceptionPanel),
		typeof(GenericDialogPanel),
		typeof(SettingsPanel),
		typeof(WeatherPanel),
		typeof(BonesPanel),
		typeof(TransformPanel),
	};

	public enum PanelThreadingMode
	{
		ApplicationThread,
		CustomThread,
	}

	public static async Task<T> Show<T>(object? context = null, PanelThreadingMode threadMode = PanelThreadingMode.CustomThread)
		where T : PanelBase
	{
		PanelBase panel = await Show(typeof(T), context, threadMode);

		if (panel is not T tPanel)
			throw new Exception("Panel was wrong type");

		return tPanel;
	}

	public static async Task<T> Show<T>(PanelThreadingMode threadMode = PanelThreadingMode.CustomThread)
		where T : PanelBase
	{
		PanelBase panel = await Show(typeof(T), null, threadMode);

		if (panel is not T tPanel)
			throw new Exception("Panel was wrong type");

		return tPanel;
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

	public static async Task<PanelBase> Spawn(Type panelType, PanelThreadingMode threadMode = PanelThreadingMode.CustomThread)
	{
		if (!Panelcache.ContainsKey(panelType))
		{
			if (threadMode == PanelThreadingMode.CustomThread)
			{
				Thread panelMainThread = new Thread(PanelMainThread);
				panelMainThread.SetApartmentState(ApartmentState.STA);
				panelMainThread.Start(panelType);

				while (!Panelcache.ContainsKey(panelType))
				{
					await Task.Delay(1);
				}
			}
			else
			{
				PanelBase? newPanel = Activator.CreateInstance(panelType) as PanelBase;

				if (newPanel == null)
					throw new Exception($"Failed to create instance of panel: {panelType}");

				Panelcache.Add(panelType, newPanel);
			}
		}

		return Panelcache[panelType];
	}

	public static async Task<PanelBase> Show(Type panelType, object? context = null, PanelThreadingMode threadMode = PanelThreadingMode.CustomThread)
	{
		PanelBase panel = await Spawn(panelType, threadMode);

		await panel.Dispatcher.MainThread();
		IPanelHost panelHost = panelHost = CreateHost();
		panelHost.AddPanel(panel);
		panel.SetContext(panelHost, context);
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

	public override Task Start()
	{
		this.PreloadPanels().Run();
		return base.Start();
	}

	public override Task Shutdown()
	{
		foreach (PanelBase panel in Panelcache.Values)
		{
			if (panel.Dispatcher != App.Current?.Dispatcher)
			{
				panel.Dispatcher.BeginInvokeShutdown(DispatcherPriority.Normal);
			}
		}

		return base.Shutdown();
	}

	private static void PanelMainThread(object? panelTypeObj)
	{
		if (panelTypeObj is not Type panelType)
			return;

		PanelBase? newPanel = Activator.CreateInstance(panelType) as PanelBase;

		if (newPanel == null)
			throw new Exception($"Failed to create instance of panel: {panelType}");

		Panelcache.Add(panelType, newPanel);

		Log.Information($"Panel: {panelType} has started");

		Dispatcher.Run();

		Log.Information($"Panel: {panelType} has shutdown");
	}

	private async Task PreloadPanels()
	{
		try
		{
			foreach (Type panelType in PreLoadPanels)
			{
				Log.Information($"Spwning panel: {panelType}");
				await Spawn(panelType);
			}
		}
		catch (Exception ex)
		{
			Log.Error(ex, "Failed to spawn panels");
		}
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
