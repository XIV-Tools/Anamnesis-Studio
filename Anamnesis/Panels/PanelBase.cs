// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Panels;

using Anamnesis.Keyboard;
using Anamnesis.Services;
using Anamnesis.Windows;
using FontAwesome.Sharp;
using Serilog;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using XivToolsWpf;
using XivToolsWpf.DependencyProperties;
using XivToolsWpf.Localization;
using static Anamnesis.Panels.PanelService;

public abstract class PanelBase : UserControl, INotifyPropertyChanged
{
	public static readonly IBind<string?> TitleDp = Binder.Register<string?, PanelBase>(nameof(Title), BindMode.OneWay);
	private FloatingWindow? window;

	public PanelBase()
	{
		TextBlockHook.Attach();

		this.GetType().GetMethod("InitializeComponent")?.Invoke(this, null);
		this.DataContext = this;

		if (SettingsService.Current == null || !SettingsService.Current.Panels.TryGetValue(this.Id, out PanelSettings? settings) || settings == null)
		{
			this.Settings = new();
			SettingsService.Current?.Panels.Add(this.Id, this.Settings);
		}
		else
		{
			this.Settings = settings;
		}
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	public ServiceManager Services => App.Services;
	public PanelService.PanelSettings Settings { get; private set; }
	public bool IsOpen { get; private set; } = true;
	public virtual string Id => this.GetType().ToString();
	public IconChar Icon { get; set; }
	public Rect Rect => this.Window.Rect;
	public bool ShowBackground { get; set; } = true;
	public bool CanResize { get; set; }
	public bool CanScroll { get; set; } = false;

	public bool IsActive
	{
		get => this.Window.IsActive;
		set => this.Window.Activate();
	}

	public string? Title
	{
		get => TitleDp.Get(this);
		set => TitleDp.Set(this, value);
	}

	public FloatingWindow Window
	{
		get
		{
			if (this.window == null)
				throw new Exception("Attempt to access panel host before it has been initialized");

			return this.window;
		}
	}

	public object? PanelContext { get; private set; }

	protected ILogger Log => Serilog.Log.ForContext(this.GetType());

	public virtual void SetContext(FloatingWindow host, object? context)
	{
		this.window = host;
		this.PanelContext = context;
	}

	public void DragMove() => this.Window.DragMove();

	public void Close()
	{
		this.IsOpen = false;
		this.Services.Panels.OnPanelClosed(this);
		this.Settings.Save();
	}

	public async Task WhileOpen()
	{
		await Dispatch.NonUiThread();

		while (this.IsOpen)
		{
			await Task.Delay(500);
		}
	}

	public virtual void OnActivated()
	{
		this.Services.Panels.OnPanelActivated(this);
	}

	public virtual void OnDeactivated()
	{
		this.Services.Panels.OnPanelDeactivated(this);
	}

	public bool HandleKey(Key key, ModifierKeys modifiers, KeyboardKeyStates state)
	{
		return false;
	}

	protected void RaisePropertyChanged(string propertyName)
	{
		this.PropertyChanged?.Invoke(this, new(propertyName));
	}
}