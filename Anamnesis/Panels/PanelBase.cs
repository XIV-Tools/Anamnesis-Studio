// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Panels;

using Anamnesis.Services;
using FontAwesome.Sharp;
using Serilog;
using System;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using XivToolsWpf;
using XivToolsWpf.DependencyProperties;

public abstract class PanelBase : UserControl, IPanel, INotifyPropertyChanged
{
	public static readonly IBind<string?> TitleDp = Binder.Register<string?, PanelBase>(nameof(Title), BindMode.OneWay);

	public PanelBase(IPanelHost host)
	{
		this.Host = host;
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	public ServiceManager Services => App.Services;
	public IPanelHost Host { get; set; }
	public bool IsOpen { get; private set; } = true;
	public virtual string Id => this.GetType().ToString();
	public string? TitleKey { get; set; }
	public string? Title { get; set; }
	public IconChar Icon { get; set; }
	public Rect Rect => this.Host.Rect;
	public bool ShowBackground { get; set; } = true;
	public bool CanResize { get; set; }
	public Color? TitleColor { get; set; } = Colors.Gray;

	public string FinalTitle
	{
		get
		{
			StringBuilder sb = new();

			if (this.TitleKey != null)
				sb.Append(LocalizationService.GetString(this.TitleKey, true));

			if (this.TitleKey != null && this.Title != null)
				sb.Append(" ");

			if (this.Title != null)
				sb.Append(this.Title);

			return sb.ToString();
		}
	}

	protected ILogger Log => Serilog.Log.ForContext(this.GetType());

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
}
