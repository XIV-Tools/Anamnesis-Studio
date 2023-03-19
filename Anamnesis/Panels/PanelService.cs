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
using Anamnesis.Memory;

public class PanelService : ServiceBase<PanelService>
{
	private static readonly object ComponentLock = new();

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
		typeof(BoneTransformPanel),
	};

	public enum PanelThreadingMode
	{
		ApplicationThread,
		CustomThread,
	}

	public List<PanelBase> OpenPanels { get; init; } = new();
	public List<PanelBase> ActivePanels { get; init; } = new();
	private Dictionary<Type, PanelBase?> ClosedPanelCache { get; init; } = new();

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
		// Do we have a cached version of this panel?
		if (this.ClosedPanelCache.TryGetValue(panelType, out PanelBase? cached))
		{
			this.ClosedPanelCache.Remove(panelType);

			if (cached != null)
			{
				return cached;
			}
		}

		return await new PanelThread().Start(panelType, threadMode);
	}

	public async Task<PanelBase> Show(Type panelType, object? context = null, PanelThreadingMode threadMode = PanelThreadingMode.CustomThread)
	{
		foreach (PanelBase otherPanel in this.OpenPanels)
		{
			if (otherPanel?.GetType() == panelType)
			{
				// This panel is open, swap to it instead of opening another.
				await otherPanel.Dispatcher.MainThread();
				otherPanel.SetContext(otherPanel.Window, context);
				otherPanel.Window.Activate();
				return otherPanel;
			}
		}

		PanelBase panel = await this.Spawn(panelType, threadMode);
		panel.Open();

		await panel.Dispatcher.MainThread();
		FloatingWindow panelHost = panelHost = this.CreateWindow();
		panelHost.SetPanel(panel);
		panel.SetContext(panelHost, context);
		panelHost.Show();

		this.OpenPanels.Add(panel);
		return panel;
	}

	public void OnPanelClosed(PanelBase panel)
	{
		lock (this)
		{
			this.OpenPanels.Remove(panel);

			Type panelType = panel.GetType();

			if (!this.ClosedPanelCache.ContainsKey(panelType))
			{
				this.ClosedPanelCache.Add(panelType, panel);
			}
		}
	}

	public override Task Start()
	{
		this.CompleteStart().Run();
		return base.Start();
	}

	public override Task Shutdown()
	{
		foreach (PanelBase? panel in this.OpenPanels)
		{
			if (panel?.Dispatcher != App.Current?.Dispatcher)
			{
				panel?.Dispatcher.BeginInvokeShutdown(DispatcherPriority.Normal);
			}
		}

		foreach (PanelBase? panel in this.ClosedPanelCache.Values)
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
		if (MemoryService.Process == null)
			return new FloatingWindow();

		// TODO: if OverlayMode!
		return new OverlayWindow();
		////return new FloatingWindow();
	}

	private async Task CompleteStart()
	{
		await Dispatch.NonUiThread();

		try
		{
			List<Task> tasks = new();

			foreach (Type panelType in PreLoadPanels)
			{
				Log.Information($"Spawning panel: {panelType}");
				PanelBase panel = await this.Spawn(panelType);
				this.ClosedPanelCache.Add(panelType, panel);
			}

			await Task.WhenAll(tasks);
		}
		catch (Exception ex)
		{
			Log.Error(ex, "Failed to preload panels");
		}
	}

	[Serializable]
	public class PanelSettings
	{
		public PanelSettings()
		{
		}

		public Point? Position { get; set; } = null;
		public Size? Size { get; set; } = null;

		public void Save()
		{
			SettingsService.Save();
		}
	}

	private class PanelThread
	{
		private PanelBase? panel;
		private Type? panelType;

		public async Task<PanelBase> Start(Type panelType, PanelThreadingMode threadMode)
		{
			this.panelType = panelType;

			// Create a new panel;
			if (threadMode == PanelThreadingMode.CustomThread)
			{
				Thread panelMainThread = new Thread(this.PanelMainThread);
				panelMainThread.SetApartmentState(ApartmentState.STA);
				panelMainThread.Start(this);
			}
			else
			{
				this.panel = Activator.CreateInstance(this.panelType) as PanelBase;
			}

			// Wait for the panel to load for up to 5 seconds.
			int timeOut = 5000;
			while (this.panel == null && timeOut > 0)
			{
				await Task.Delay(10);
				timeOut -= 10;
			}

			if (this.panel == null)
				throw new Exception($"Failed to start panel {this.panelType}");

			return this.panel;
		}

		private void PanelMainThread(object? param)
		{
			if (this.panelType == null)
				throw new Exception("No panel type in panel thread");

			if (Application.Current == null)
				return;

			try
			{
				// Even though we're doing this on another thread, we can still only do one panel
				// at a time since WPF's LoadComponent system isn't thread safe.
				lock (PanelService.ComponentLock)
				{
					this.panel = Activator.CreateInstance(this.panelType) as PanelBase;
				}
			}
			catch(Exception ex)
			{
				if (Application.Current == null)
					return;

				Log.Error(ex, $"Exception during panel construction: {this.panelType}");
				return;
			}

			Log.Information($"Panel: {this.panelType} has started");
			Dispatcher.Run();
			Log.Information($"Panel: {this.panelType} has shutdown");
		}
	}
}
