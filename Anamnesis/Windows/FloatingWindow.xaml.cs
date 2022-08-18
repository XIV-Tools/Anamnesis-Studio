// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Windows;

using Anamnesis.Extensions;
using Anamnesis.Memory;
using Anamnesis.Panels;
using Anamnesis.Services;
using FontAwesome.Sharp;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json.Linq;
using PropertyChanged;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using XivToolsWpf;

using MediaColor = System.Windows.Media.Color;

[AddINotifyPropertyChangedInterface]
public partial class FloatingWindow : Window, IPanelHost
{
	private readonly WindowInteropHelper windowInteropHelper;
	private readonly List<IPanel> panels = new();

	private bool canResize = true;
	private bool playOpenAnimation = true;
	private bool isOpeningAnimation = true;

	public FloatingWindow()
	{
		this.windowInteropHelper = new(this);
		this.InitializeComponent();
		this.ContentArea.DataContext = this;
		this.WindowContextMenu.DataContext = this;
		base.Topmost = false;

		this.TitleColor = Application.Current.Resources.GetTheme().ToolForeground;
	}

	public ContentPresenter PanelGroupArea => this.ContentPresenter;
	public bool ShowBackground { get; set; } = true;
	public bool IsOpen { get; private set; }

	public IEnumerable<IPanel> Panels => this.panels.AsReadOnly();

	public new bool Topmost
	{
		get => base.Topmost;
		set
		{
			base.Topmost = value;
			this.UpdatePosition();
		}
	}

	public bool CanResize
	{
		get => this.canResize;
		set
		{
			this.canResize = value;
			this.ResizeMode = this.canResize ? ResizeMode.CanResize : ResizeMode.NoResize;
			this.SizeToContent = value ? SizeToContent.Manual : SizeToContent.WidthAndHeight;
		}
	}

	public bool CanScroll
	{
		get
		{
			return this.ScrollArea.VerticalScrollBarVisibility == ScrollBarVisibility.Auto;
		}
		set
		{
			this.ScrollArea.VerticalScrollBarVisibility = value ? ScrollBarVisibility.Auto : ScrollBarVisibility.Disabled;
		}
	}

	public virtual Rect Rect
	{
		get
		{
			return new Rect(this.Left, this.Top, this.Width, this.Height);
		}
		set
		{
			if (value.Height != this.Height || value.Width != this.Width)
			{
				this.SizeToContent = SizeToContent.Manual;
			}

			this.Left = (int)value.X;
			this.Top = (int)value.Y;
			this.Width = value.Width;
			this.Height = value.Height;
			this.UpdatePosition();
		}
	}

	public string Id
	{
		get
		{
			StringBuilder sb = new();
			foreach (IPanel panel in this.panels)
			{
				sb.Append(panel.Id);
			}

			return sb.ToString();
		}
	}

	public virtual Rect ScreenRect
	{
		get
		{
			// We uuuuh... might need to know what screen we are on to begin with? idk.
			return new Rect(0, 0, SystemParameters.WorkArea.Width, SystemParameters.WorkArea.Height);
		}
	}

	public IPanelHost Host => this;
	public MediaColor? TitleColor { get; set; }
	public virtual bool CanPopOut => false;
	public virtual bool CanPopIn => true;
	public bool AutoClose { get; set; } = false;

	public Rect RelativeRect
	{
		get
		{
			Rect screen = this.ScreenRect;
			Rect pos = this.Rect;
			Rect value = new();
			value.X = (pos.X - screen.X) / (screen.Width - pos.Width);
			value.Y = (pos.Y - screen.Y) / (screen.Height - pos.Height);
			value.Width = pos.Width;
			value.Height = pos.Height;

			return value;
		}

		set
		{
			Rect screen = this.ScreenRect;
			Rect pos = this.Rect;
			pos.X = screen.X + (screen.Width * value.X) - (pos.Width * value.X);
			pos.Y = screen.Y + (screen.Height * value.Y) - (pos.Height * value.Y);

			if (this.CanResize)
			{
				if (value.Height > 0)
				{
					pos.Height = value.Height;
				}

				if (value.Width > 0)
				{
					pos.Width = value.Width;
				}
			}

			this.Rect = pos;
		}
	}

	public virtual new void Show()
	{
		base.Show();

		Rect screen = this.ScreenRect;
		Rect pos = this.Rect;

		double maxHeight = Math.Max(screen.Height - pos.Top, 0);

		if (maxHeight == 0)
			maxHeight = 1080;

		this.MaxHeight = maxHeight;
		this.IsOpen = true;
	}

	public virtual void Show(IPanelHost copy)
	{
		this.playOpenAnimation = false;
		this.Show();

		throw new NotImplementedException();

		/*if (this.PanelGroupArea.Content is PanelBase panel)
			panel.Host = this;

		this.TitleKey = copy.TitleKey;
		this.Title = copy.Title;
		this.Icon = copy.Icon;
		this.TitleColor = copy.TitleColor;
		this.ShowBackground = copy.ShowBackground;
		this.Topmost = copy.Topmost;
		this.CanResize = copy.CanResize;

		if (copy is FloatingWindow wnd)
		{
			this.AutoClose = wnd.AutoClose;
		}

		this.Rect = copy.Rect;*/
	}

	public void AddPanel(IPanel panel)
	{
		// TODO: panel docking.
		this.PanelGroupArea.Content = panel as PanelBase;

		this.panels.Add(panel);
		panel.PropertyChanged += this.OnPanelPropertyChanged;
		this.OnPanelPropertyChanged(this, null);
	}

	public void RemovePanel(IPanel panel)
	{
		panel.PropertyChanged -= this.OnPanelPropertyChanged;
		this.panels.Remove(panel);

		if (this.panels.Count <= 0)
		{
			this.Close();
		}
	}

	public new void Close()
	{
		if (!this.isOpeningAnimation)
			PanelService.SavePosition(this);

		this.BeginStoryboard("CloseStoryboard");
		this.IsOpen = false;
	}

	protected virtual void OnWindowLoaded()
	{
	}

	protected virtual void UpdatePosition()
	{
	}

	protected override void OnDeactivated(EventArgs e)
	{
		base.OnDeactivated(e);

		if (this.AutoClose)
		{
			this.Close();
		}
	}

	private void OnWindowLoaded(object sender, RoutedEventArgs e)
	{
		this.UpdatePosition();
		this.Activate();

		this.OnWindowLoaded();

		if (this.playOpenAnimation)
		{
			this.BeginStoryboard("OpenStoryboard");
		}
		else
		{
			this.Opacity = 1.0;
		}
	}

	private void OnTitleMouseDown(object sender, MouseButtonEventArgs e)
	{
		if (e.LeftButton == MouseButtonState.Pressed)
		{
			this.DragMove();
		}
	}

	private void OnTitlebarContextButtonClicked(object sender, RoutedEventArgs e)
	{
		this.WindowContextMenu.IsOpen = true;
	}

	private void OnTitlebarCloseButtonClicked(object sender, RoutedEventArgs e)
	{
		this.Close();
	}

	private void OnMouseEnter(object sender, MouseEventArgs e)
	{
		if (this.Opacity == 1.0)
			return;

		this.Animate(Window.OpacityProperty, 1.0, 100);
	}

	private void OnMouseLeave(object sender, MouseEventArgs e)
	{
		if (SettingsService.Current.Opacity != 1.0)
		{
			this.Animate(Window.OpacityProperty, SettingsService.Current.Opacity, 250);
		}
	}

	private void OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
	{
		this.Activate();

		////this.UpdatePosition();
		////PanelService.SavePosition(this);
	}

	private void OnCloseStoryboardCompleted(object sender, EventArgs e)
	{
		base.Close();
	}

	private void OnOpenStoryboardCompleted(object sender, EventArgs e)
	{
		this.isOpeningAnimation = false;
	}

	private void OnPopOutClicked(object sender, RoutedEventArgs e)
	{
		FloatingWindow wnd = new();
		wnd.Show(this);
		base.Close();
	}

	private void OnPopInClicked(object sender, RoutedEventArgs e)
	{
		OverlayWindow wnd = new();
		wnd.Show(this);
		base.Close();
	}

	private void OnPanelPropertyChanged(object? sender, PropertyChangedEventArgs? e = null)
	{
		if (this.panels.Count <= 0)
			throw new Exception("Panel host reciving panel events without any panel children");

		if (this.panels.Count == 1)
		{
			this.ShowBackground = this.panels[0].ShowBackground;
			this.CanResize = this.panels[0].CanResize;
			this.CanScroll = this.panels[0].CanScroll;
			this.TitleIcon.Icon = this.panels[0].Icon;
			this.TitleColor = this.panels[0].TitleColor;
			this.Title = this.panels[0].FinalTitle;
			this.TitleText.Text = this.Title;
		}
		else
		{
			this.ShowBackground = true;
			this.CanResize = true;
			this.CanScroll = true;
			this.TitleIcon.Icon = IconChar.None;
			this.TitleColor = Colors.White;
			this.Title = string.Empty;
			this.TitleText.Text = string.Empty;
		}
	}
}
