// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Windows;

using Anamnesis.Extensions;
using Anamnesis.Panels;
using Anamnesis.Services;
using PropertyChanged;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using XivToolsWpf;

[AddINotifyPropertyChangedInterface]
public partial class FloatingWindow : Window
{
	protected readonly WindowInteropHelper windowInteropHelper;

	private const double MaxSizeScreenPadding = 25;

	private bool canResize = true;

	public FloatingWindow()
	{
		this.windowInteropHelper = new(this);
		this.InitializeComponent();
		this.ContentArea.DataContext = this;
		base.Topmost = false;
	}

	public ServiceManager Services => App.Services;
	public ContentPresenter PanelGroupArea => this.ContentPresenter;
	public bool ShowBackground { get; set; } = true;
	public bool IsOpen { get; private set; }

	public PanelBase? Panel { get; private set; }

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

	public string Id => this.Panel?.Id ?? string.Empty;

	public virtual Rect ScreenRect
	{
		get
		{
			// We uuuuh... might need to know what screen we are on to begin with? idk.
			return new Rect(0, 0, SystemParameters.WorkArea.Width, SystemParameters.WorkArea.Height);
		}
	}

	public virtual bool CanPopOut => false;
	public virtual bool CanPopIn => true;
	public bool AutoClose { get; set; } = false;

	public virtual new void Show()
	{
		Rect screen = this.ScreenRect;

		if (this.Panel == null)
			throw new Exception("No panel in window");

		Rect? desiredPosition = this.Panel.Settings.GetLastPosition();

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

			// foreach (PanelBase panel in this.Panels)
			this.MinWidth = Math.Max(this.MinWidth, this.Panel.MinWidth + 20);
			this.MinHeight = Math.Max(this.MinHeight, this.Panel.MinHeight);
			this.MaxHeight = Math.Min(this.MaxHeight, this.Panel.MaxHeight);
			this.MaxWidth = Math.Min(this.MaxWidth, this.Panel.MaxWidth);

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
		if (copy.Panel != null)
			this.SetPanel(copy.Panel);

		this.Show();
	}

	public virtual new bool Activate()
	{
		return base.Activate();
	}

	public void SetPanel(PanelBase panel)
	{
		this.PanelGroupArea.Content = panel as PanelBase;

		this.Panel = panel;

		panel.PropertyChanged += this.OnPanelPropertyChanged;
		this.OnPanelPropertyChanged(this, null);
	}

	public new void Close()
	{
		this.Panel?.Close();

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

	private void OnTitlebarCloseButtonClicked(object sender, RoutedEventArgs e)
	{
		this.Close();
	}

	private void OnMouseEnter(object sender, MouseEventArgs e)
	{
		if (this.WindowContentArea.Opacity == 1.0)
			return;

		this.WindowContentArea.Animate(Grid.OpacityProperty, 1.0, 100);
	}

	private void OnMouseLeave(object sender, MouseEventArgs e)
	{
		if (SettingsService.Current.Opacity != 1.0)
		{
			this.WindowContentArea.Animate(Grid.OpacityProperty, SettingsService.Current.Opacity, 250);
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
		await this.Dispatcher.MainThread();

		if (this.Panel == null)
			return;

		this.ShowBackground = this.Panel.ShowBackground;
		this.CanResize = this.Panel.CanResize;
		this.CanScroll = this.Panel.CanScroll;
		this.TitleIcon.Icon = this.Panel.Icon;

		if (!string.IsNullOrEmpty(this.Panel.Title))
			this.Title = this.Panel.Title;

		string title = this.Title;
		if (title.StartsWith('['))
		{
			title = title.Trim('[', ']');
			title = LocalizationService.GetLocalizedText(title);
		}

		this.TitleText.Text = title;
		this.Title = title;
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
		this.Panel?.OnActivated();
	}

	private void OnDeactivated(object sender, EventArgs e)
	{
		this.Panel?.OnDeactivated();
	}

	private void OnLocationChanged(object sender, EventArgs e)
	{
		this.Panel?.Settings?.SavePosition(this);
	}
}
