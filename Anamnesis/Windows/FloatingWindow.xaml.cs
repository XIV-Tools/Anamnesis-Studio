// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Windows;

using Anamnesis.Extensions;
using Anamnesis.Panels;
using Anamnesis.Services;
using FontAwesome.Sharp;
using MaterialDesignThemes.Wpf;
using PropertyChanged;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using XivToolsWpf;
using XivToolsWpf.Windows;
using MediaColor = System.Windows.Media.Color;

[AddINotifyPropertyChangedInterface]
public partial class FloatingWindow : Window
{
	private const double MaxSizeScreenPadding = 25;

	private readonly WindowInteropHelper windowInteropHelper;
	private readonly List<PanelBase> panels = new();

	private bool canResize = true;

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
	public PanelService.PanelsData PanelsData { get; private set; } = new();

	public IEnumerable<PanelBase> Panels => this.panels.AsReadOnly();

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

	public virtual Rect Rect => new Rect(this.Left, this.Top, this.Width, this.Height);

	public string Id
	{
		get
		{
			StringBuilder sb = new();
			foreach (PanelBase panel in this.panels)
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

	public MediaColor? TitleColor { get; set; }
	public virtual bool CanPopOut => false;
	public virtual bool CanPopIn => true;
	public bool AutoClose { get; set; } = false;

	public virtual new void Show()
	{
		Rect screen = this.ScreenRect;

		Rect? desiredPosition = this.PanelsData.GetLastPosition();

		// Center screen
		if (desiredPosition == null)
		{
			this.MaxWidth = Math.Clamp(this.MaxWidth, this.MinWidth, screen.Width - MaxSizeScreenPadding);
			this.MaxHeight = Math.Clamp(this.MaxHeight, this.MinHeight, screen.Height - MaxSizeScreenPadding);

			this.Left = screen.Left + ((screen.Width / 2) - (this.ActualWidth / 2));
			this.Top = screen.Top + ((screen.Height / 2) - (this.ActualHeight / 2));
		}
		else
		{
			this.MaxWidth = Math.Max(screen.Width - ((Rect)desiredPosition).Left, 0);
			this.MaxHeight = Math.Max(screen.Height - ((Rect)desiredPosition).Top, 0);
			this.MinWidth = 64;
			this.MinHeight = 64;

			foreach (PanelBase panel in this.Panels)
			{
				this.MinWidth = Math.Max(this.MinWidth, panel.MinWidth + 20);
				this.MinHeight = Math.Max(this.MinHeight, panel.MinHeight);
				this.MaxHeight = Math.Min(this.MaxHeight, panel.MaxHeight);
				this.MaxWidth = Math.Min(this.MaxWidth, panel.MaxWidth);
			}

			this.MinWidth = Math.Clamp(this.MinWidth, 0, screen.Width);
			this.MinHeight = Math.Clamp(this.MinHeight, 0, screen.Height);
			this.MaxWidth = Math.Clamp(this.MaxWidth, this.MinWidth, screen.Width - MaxSizeScreenPadding);
			this.MaxHeight = Math.Clamp(this.MaxHeight, this.MinHeight, screen.Height - MaxSizeScreenPadding);

			this.Left = screen.Left + (((Rect)desiredPosition).X * screen.Width);
			this.Top = screen.Top + (((Rect)desiredPosition).Y * screen.Height);

			double height = ((Rect)desiredPosition).Height;
			double width = ((Rect)desiredPosition).Width;

			if (double.IsNaN(height) && double.IsNaN(width))
			{
				this.SizeToContent = SizeToContent.WidthAndHeight;
			}
			else if (double.IsNaN(height))
			{
				this.Width = Math.Clamp(width, this.MinWidth, this.MaxWidth);
				this.SizeToContent = SizeToContent.Height;
			}
			else if (double.IsNaN(width))
			{
				this.Height = Math.Clamp(height, this.MinHeight, this.MaxHeight);
				this.SizeToContent = SizeToContent.Width;
			}
			else
			{
				this.SizeToContent = SizeToContent.Manual;
				this.Width = Math.Clamp(width, this.MinWidth, this.MaxWidth);
				this.Height = Math.Clamp(height, this.MinHeight, this.MaxHeight);
			}
		}

		this.UpdatePosition();
		base.Show();
		this.IsOpen = true;
	}

	public virtual void Show(FloatingWindow copy)
	{
		throw new NotImplementedException();
	}

	public virtual new bool Activate()
	{
		return base.Activate();
	}

	public void AddPanel(PanelBase panel)
	{
		// TODO: panel docking.
		this.PanelGroupArea.Content = panel as PanelBase;

		this.panels.Add(panel);
		this.PanelsData = App.Services.Panels.GetData(this);
		this.PanelsData.PanelIds.Add(panel.Id);
		this.PanelsData.IsOpen = true;
		this.PanelsData.Save();

		panel.PropertyChanged += this.OnPanelPropertyChanged;
		this.OnPanelPropertyChanged(this, null);
	}

	public void RemovePanel(PanelBase panel)
	{
		panel.PropertyChanged -= this.OnPanelPropertyChanged;
		this.panels.Remove(panel);
		this.PanelsData = App.Services.Panels.GetData(this);
		this.PanelsData.PanelIds.Remove(panel.Id);
		this.PanelsData.IsOpen = true;
		this.PanelsData.Save();

		if (this.IsOpen && this.panels.Count <= 0)
		{
			this.Close();
		}
	}

	public new void Close()
	{
		this.PanelsData.IsOpen = false;
		this.PanelsData.SavePosition(this);
		this.PanelsData.Save();

		this.BeginStoryboard("CloseStoryboard");
		this.IsOpen = false;

		foreach (PanelBase panel in this.Panels.ToArray())
		{
			panel.Close();
		}
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

		this.BeginStoryboard("OpenStoryboard");
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

	private async void OnPanelPropertyChanged(object? sender, PropertyChangedEventArgs? e = null)
	{
		if (this.panels.Count <= 0)
			throw new Exception("Panel host reciving panel events without any panel children");

		await this.Dispatcher.MainThread();

		if (this.panels.Count == 1)
		{
			this.ShowBackground = this.panels[0].ShowBackground;
			this.CanResize = this.panels[0].CanResize;
			this.CanScroll = this.panels[0].CanScroll;
			this.TitleIcon.Icon = this.panels[0].Icon;
			this.TitleColor = this.panels[0].TitleColor;

			if (!string.IsNullOrEmpty(this.panels[0].Title))
				this.Title = this.panels[0].Title;

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

	// Swallow all keyboard events, as we have the global keyboard hook handling
	// keys for us.
	private void OnPreviewKeyDown(object sender, KeyEventArgs e)
	{
		if (Keyboard.FocusedElement is not TextBox)
		{
			e.Handled = true;
		}
	}

	// Swallow all keyboard events, as we have the global keyboard hook handling
	// keys for us.
	private void OnPreviewKeyUp(object sender, KeyEventArgs e)
	{
		if (Keyboard.FocusedElement is not TextBox)
		{
			e.Handled = true;
		}
	}

	private void OnActivated(object sender, EventArgs e)
	{
		foreach (PanelBase panel in this.panels)
		{
			panel.OnActivated();
		}
	}

	private void OnDeactivated(object sender, EventArgs e)
	{
		foreach (PanelBase panel in this.panels)
		{
			panel.OnDeactivated();
		}
	}
}
