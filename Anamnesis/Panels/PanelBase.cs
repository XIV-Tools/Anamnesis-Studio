// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Panels;

using Anamnesis.Services;
using FontAwesome.Sharp;
using Serilog;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using XivToolsWpf;
using XivToolsWpf.DependencyProperties;
using XivToolsWpf.Localization;

public abstract class PanelBase : UserControl, IPanel, INotifyPropertyChanged
{
	public static readonly IBind<string?> TitleDp = Binder.Register<string?, PanelBase>(nameof(Title), BindMode.OneWay);
	private IPanelHost? host;

	public PanelBase()
	{
		TextBlockHook.Attach();

		this.GetType().GetMethod("InitializeComponent")?.Invoke(this, null);
		this.DataContext = this;
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	public ServiceManager Services => App.Services;
	public bool IsOpen { get; private set; } = true;
	public virtual string Id => this.GetType().ToString();
	public string? Title { get; set; }
	public IconChar Icon { get; set; }
	public Rect Rect => this.Host.Rect;
	public bool ShowBackground { get; set; } = true;
	public bool CanResize { get; set; }
	public bool CanScroll { get; set; } = false;
	public Color? TitleColor { get; set; } = Colors.Gray;

	public IPanelHost Host
	{
		get
		{
			if (this.host == null)
				throw new Exception("Attempt to access panel host before it has been initialized");

			return this.host;
		}
	}

	public object? PanelContext { get; private set; }
	public PanelService.PanelsData PanelsData => this.Host.PanelsData;

	protected ILogger Log => Serilog.Log.ForContext(this.GetType());

	public virtual void SetContext(IPanelHost host, object? context)
	{
		this.host = host;
		this.PanelContext = context;
	}

	public void DragMove() => this.Host.DragMove();

	public void Close()
	{
		this.Host.RemovePanel(this);
		this.IsOpen = false;
		PanelService.OnPanelClosed(this);
	}

	public async Task WhileOpen()
	{
		await Dispatch.NonUiThread();

		while (this.IsOpen)
		{
			await Task.Delay(500);
		}
	}

	protected void RaisePropertyChanged(string propertyName)
	{
		this.PropertyChanged?.Invoke(this, new(propertyName));
	}
}