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
using System.Data;
using Octokit;
using System.Runtime.CompilerServices;
using System.Linq;

public class PanelService : ServiceBase<PanelService>
{
	private static readonly List<PanelBase> OpenPanels = new();
	private static readonly Dictionary<Type, PanelBase?> Panelcache = new();

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

	public static async Task<PanelBase> Show(string panelId, object? context = null, PanelThreadingMode threadMode = PanelThreadingMode.CustomThread)
	{
		Type? panelType = Type.GetType(panelId);

		if (panelType == null)
			throw new Exception($"Failed to locate panel type: {panelId}");

		return await Show(panelType, context, threadMode);
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
			Panelcache.Add(panelType, null);

			if (threadMode == PanelThreadingMode.CustomThread)
			{
				Thread panelMainThread = new Thread(PanelMainThread);
				panelMainThread.SetApartmentState(ApartmentState.STA);
				panelMainThread.Start(panelType);
			}
			else
			{
				PanelBase? newPanel = Activator.CreateInstance(panelType) as PanelBase;

				if (newPanel == null)
					throw new Exception($"Failed to create instance of panel: {panelType}");

				Panelcache[panelType] = newPanel;
			}
		}

		while (Panelcache[panelType] == null)
			await Task.Delay(1);

		if (Panelcache[panelType] == null)
			throw new Exception("Panel not loaded!");

		PanelBase panel = Panelcache[panelType]!;
		return panel;
	}

	public static async Task<PanelBase> Show(Type panelType, object? context = null, PanelThreadingMode threadMode = PanelThreadingMode.CustomThread)
	{
		foreach (PanelBase otherPanel in OpenPanels)
		{
			if (otherPanel.GetType() == panelType)
			{
				// This panel is open, swap to it instead of opening another.
				await otherPanel.Dispatcher.MainThread();
				otherPanel.SetContext(otherPanel.Host, context);
				otherPanel.Host.Activate();
				return otherPanel;
			}
		}

		PanelBase panel = await Spawn(panelType, threadMode);

		await panel.Dispatcher.MainThread();
		IPanelHost panelHost = panelHost = CreateHost();
		panelHost.AddPanel(panel);
		panel.SetContext(panelHost, context);
		panelHost.Show();

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

	public PanelsData GetData(IPanelHost panelHost)
	{
		if (!SettingsService.Current.Panels.TryGetValue(panelHost.Id, out PanelsData? data) || data == null)
		{
			data = new();
			SettingsService.Current.Panels.Add(panelHost.Id, data);
		}

		return data;
	}

	public override Task Start()
	{
		this.PreloadPanels().Run();
		return base.Start();
	}

	public override Task Shutdown()
	{
		foreach (PanelBase? panel in Panelcache.Values)
		{
			if (panel?.Dispatcher != App.Current?.Dispatcher)
			{
				panel?.Dispatcher.BeginInvokeShutdown(DispatcherPriority.Normal);
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

		Panelcache[panelType] = newPanel;

		Log.Information($"Panel: {panelType} has started");

		Dispatcher.Run();

		Log.Information($"Panel: {panelType} has shutdown");
	}

	private async Task PreloadPanels()
	{
		await Dispatch.NonUiThread();

		// Open last used panels
		foreach (PanelsData data in SettingsService.Current.Panels.Values)
		{
			if (data.IsOpen)
			{
				foreach (string panelId in data.PanelIds.ToArray())
				{
					try
					{
						await PanelService.Show(panelId);
					}
					catch (Exception ex)
					{
						Log.Error(ex, $"Failed to reopen panel: {panelId}");
					}
				}
			}
		}

		// Preload panels
		try
		{
			List<Task> tasks = new();

			foreach (Type panelType in PreLoadPanels)
			{
				Log.Information($"Spwning panel: {panelType}");
				tasks.Add(Spawn(panelType));
			}

			await Task.WhenAll(tasks);
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

		public List<string> PanelIds { get; init; } = new();
		public Rect Position { get; set; } = default;
		public SizeToContent SizeToContent { get; set; } = SizeToContent.WidthAndHeight;
		public bool IsOpen { get; set; } = false;

		public Rect? GetLastPosition()
		{
			Rect pos = this.Position;

			if (this.SizeToContent == SizeToContent.WidthAndHeight)
			{
				pos.Width = double.NaN;
				pos.Height = double.NaN;
			}
			else if (this.SizeToContent == SizeToContent.Width)
			{
				pos.Width = double.NaN;
			}
			else if (this.SizeToContent == SizeToContent.Height)
			{
				pos.Height = double.NaN;
			}

			return pos;
		}

		public void SavePosition(IPanelHost panel)
		{
			Rect pos = new();
			pos.Width = panel.Rect.Width;
			pos.Height = panel.Rect.Height;
			pos.X = (panel.Rect.X - panel.ScreenRect.Left) / panel.ScreenRect.Width;
			pos.Y = (panel.Rect.Y - panel.ScreenRect.Top) / panel.ScreenRect.Height;

			this.Position = pos;
			this.SizeToContent = panel.SizeToContent;

			this.Save();
		}

		public void Save()
		{
			SettingsService.Save();
		}
	}
}
