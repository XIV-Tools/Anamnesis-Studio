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
	private static readonly List<Type> PreLoadPanels = new()
	{
		typeof(CharacterPanel),
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

	public List<PanelBase> OpenPanels { get; init; } = new();
	public List<PanelBase> ActivePanels { get; init; } = new();
	private Dictionary<Type, PanelBase?> Panelcache { get; init; } = new();

	public async Task<PanelBase> Show(string panelId, object? context = null, PanelThreadingMode threadMode = PanelThreadingMode.CustomThread)
	{
		Type? panelType = Type.GetType(panelId);

		if (panelType == null)
			throw new Exception($"Failed to locate panel type: {panelId}");

		return await this.Show(panelType, context, threadMode);
	}

	public async Task<T> Show<T>(object? context = null, PanelThreadingMode threadMode = PanelThreadingMode.CustomThread)
		where T : PanelBase
	{
		PanelBase panel = await this.Show(typeof(T), context, threadMode);

		if (panel is not T tPanel)
			throw new Exception("Panel was wrong type");

		return tPanel;
	}

	public async Task<T> Show<T>(PanelThreadingMode threadMode = PanelThreadingMode.CustomThread)
		where T : PanelBase
	{
		PanelBase panel = await this.Show(typeof(T), null, threadMode);

		if (panel is not T tPanel)
			throw new Exception("Panel was wrong type");

		return tPanel;
	}

	public List<PanelBase> GetPanels(Type panelType)
	{
		List<PanelBase> results = new();

		foreach (PanelBase panel in this.OpenPanels)
		{
			if (panel.GetType() == panelType)
			{
				results.Add(panel);
			}
		}

		return results;
	}

	public async Task<PanelBase> Spawn(Type panelType, PanelThreadingMode threadMode = PanelThreadingMode.CustomThread)
	{
		if (!this.Panelcache.ContainsKey(panelType))
		{
			this.Panelcache.Add(panelType, null);

			if (threadMode == PanelThreadingMode.CustomThread)
			{
				Thread panelMainThread = new Thread(this.PanelMainThread);
				panelMainThread.SetApartmentState(ApartmentState.STA);
				panelMainThread.Start(panelType);
			}
			else
			{
				PanelBase? newPanel = Activator.CreateInstance(panelType) as PanelBase;

				if (newPanel == null)
					throw new Exception($"Failed to create instance of panel: {panelType}");

				this.Panelcache[panelType] = newPanel;
			}
		}

		while (this.Panelcache[panelType] == null)
			await Task.Delay(1);

		if (this.Panelcache[panelType] == null)
			throw new Exception("Panel not loaded!");

		PanelBase panel = this.Panelcache[panelType]!;
		return panel;
	}

	public async Task<PanelBase> Show(Type panelType, object? context = null, PanelThreadingMode threadMode = PanelThreadingMode.CustomThread)
	{
		foreach (PanelBase otherPanel in this.OpenPanels)
		{
			if (otherPanel.GetType() == panelType)
			{
				// This panel is open, swap to it instead of opening another.
				await otherPanel.Dispatcher.MainThread();
				otherPanel.SetContext(otherPanel.Window, context);
				otherPanel.Window.Activate();
				return otherPanel;
			}
		}

		PanelBase panel = await this.Spawn(panelType, threadMode);

		await panel.Dispatcher.MainThread();
		FloatingWindow panelHost = panelHost = this.CreateWindow();
		panelHost.AddPanel(panel);
		panel.SetContext(panelHost, context);
		panelHost.Show();

		this.OpenPanels.Add(panel);
		return panel;
	}

	public void OnPanelClosed(PanelBase panel)
	{
		this.OpenPanels.Remove(panel);
	}

	public PanelsData GetData(FloatingWindow panelHost)
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
		this.CompleteStart().Run();
		return base.Start();
	}

	public override Task Shutdown()
	{
		foreach (PanelBase? panel in this.Panelcache.Values)
		{
			if (panel?.Dispatcher != App.Current?.Dispatcher)
			{
				panel?.Dispatcher.BeginInvokeShutdown(DispatcherPriority.Normal);
			}
		}

		return base.Shutdown();
	}

	public void OnPanelActivated(PanelBase panel)
	{
		this.ActivePanels.Add(panel);
	}

	public void OnPanelDeactivated(PanelBase panel)
	{
		this.ActivePanels.Remove(panel);
	}

	private FloatingWindow CreateWindow()
	{
		// TODO: if OverlayMode!
		return new OverlayWindow();
	}

	private void PanelMainThread(object? panelTypeObj)
	{
		if (panelTypeObj is not Type panelType)
			return;

		PanelBase? newPanel = Activator.CreateInstance(panelType) as PanelBase;

		if (newPanel == null)
			throw new Exception($"Failed to create instance of panel: {panelType}");

		this.Panelcache[panelType] = newPanel;

		Log.Information($"Panel: {panelType} has started");

		Dispatcher.Run();

		Log.Information($"Panel: {panelType} has shutdown");
	}

	private async Task CompleteStart()
	{
		await Dispatch.NonUiThread();

		// Open last used panels
		foreach (PanelsData data in SettingsService.Current.Panels.Values)
		{
			if (data.IsOpen)
			{
				foreach (string panelId in data.PanelIds.ToArray())
				{
					if (panelId == "Anamnesis.Panels.GenericDialogPanel" || panelId == "Anamnesis.Panels.ExceptionPanel")
						continue;

					try
					{
						await this.Show(panelId);
					}
					catch (Exception ex)
					{
						Log.Warning(ex, $"Failed to reopen panel: {panelId}");
					}
				}
			}
		}

		// Preload panels
		/*try
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
		}*/
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

		public void SavePosition(FloatingWindow panel)
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
