﻿// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Navigation;

using Anamnesis.Extensions;
using Anamnesis.Services;
using FontAwesome.Sharp;
using PropertyChanged;
using Serilog;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using XivToolsWpf;
using XivToolsWpf.DependencyProperties;

[AddINotifyPropertyChangedInterface]
public partial class Shortcut : UserControl
{
	public static readonly IBind<string> LabelKeyDp = Binder.Register<string, Shortcut>(nameof(LabelKey), BindMode.OneWay);
	public static readonly IBind<string> ToolTipKeyDp = Binder.Register<string, Shortcut>(nameof(ToolTipKey), BindMode.OneWay);
	public static readonly IBind<IconChar> IconDp = Binder.Register<IconChar, Shortcut>(nameof(Icon), BindMode.OneWay);
	public static readonly IBind<string> DestinationDp = Binder.Register<string, Shortcut>(nameof(Destination), BindMode.OneWay);
	public static readonly IBind<object> ContextDp = Binder.Register<object, Shortcut>(nameof(Context), BindMode.OneWay);
	public static readonly IBind<bool> ShowTextDp = Binder.Register<bool, Shortcut>(nameof(ShowText), OnShowTextChanged, BindMode.OneWay);
	public static readonly IBind<Thickness> ContentPaddingDp = Binder.Register<Thickness, Shortcut>(nameof(ContentPadding));

	public Shortcut()
	{
		this.InitializeComponent();
		this.ButtonContent.DataContext = this;
	}

	public string LabelKey
	{
		get => LabelKeyDp.Get(this);
		set => LabelKeyDp.Set(this, value);
	}

	public string ToolTipKey
	{
		get => ToolTipKeyDp.Get(this);
		set => ToolTipKeyDp.Set(this, value);
	}

	public IconChar Icon
	{
		get => IconDp.Get(this);
		set => IconDp.Set(this, value);
	}

	public string Destination
	{
		get => DestinationDp.Get(this);
		set => DestinationDp.Set(this, value);
	}

	public object Context
	{
		get => ContextDp.Get(this);
		set => ContextDp.Set(this, value);
	}

	public bool ShowText
	{
		get => ShowTextDp.Get(this);
		set => ShowTextDp.Set(this, value);
	}

	public Thickness ContentPadding
	{
		get => ContentPaddingDp.Get(this);
		set => ContentPaddingDp.Set(this, value);
	}

	private static void OnShowTextChanged(Shortcut sender, bool value)
	{
		if (value)
		{
			sender.BeginStoryboard("ShowLabel");
		}
		else
		{
			sender.BeginStoryboard("HideLabel");
		}
	}

	private void OnClicked(object sender, RoutedEventArgs e)
	{
		try
		{
			NavigationService.Navigate(new(this, this.Destination, this.Context));
		}
		catch (Exception ex)
		{
			Log.Error(ex, "Failed to perform navigation");
		}
	}

	private void OnMouseEnter(object sender, MouseEventArgs e)
	{
		if (!this.ShowText)
		{
			this.BeginStoryboard("ShowLabel");
		}
	}

	private void OnMouseLeave(object sender, MouseEventArgs e)
	{
		if (!this.ShowText)
		{
			this.BeginStoryboard("HideLabel");
		}
	}
}
