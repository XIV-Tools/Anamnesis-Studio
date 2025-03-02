﻿// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Navigation;

using Anamnesis.Panels;
using PropertyChanged;
using System.Windows;
using System.Windows.Input;
using XivToolsWpf.Extensions;
using Anamnesis.Files;

[AddINotifyPropertyChangedInterface]
public partial class NavigationPanel : PanelBase
{
	public bool IsExpanded { get; set; } = true;

	protected override void OnMouseUp(MouseButtonEventArgs e)
	{
		base.OnMouseUp(e);

		if (this.IsMouseOver)
			return;

		this.IsExpanded = false;
	}

	private void OnIconMouseDown(object sender, MouseButtonEventArgs e)
	{
		if (e.LeftButton == MouseButtonState.Pressed)
		{
			this.DragMove();
		}
	}

	private void OnSaveSceneClicked(object sender, RoutedEventArgs e)
	{
		this.Services.Scene.Save();
	}

	private void OnOpenSceneClicked(object sender, RoutedEventArgs e)
	{
		this.Services.Scene.Open();
	}

	private void OnImportClicked(object sender, RoutedEventArgs e)
	{
		FileService.Import().Run();
	}

	private void OnExportClicked(object sender, RoutedEventArgs e)
	{
		FileService.Export().Run();
	}

	private void OnExpandClicked(object sender, RoutedEventArgs e)
	{
	}

	private async void OnQuitClicked(object sender, RoutedEventArgs e)
	{
		bool? result = await GenericDialogPanel.ShowLocalizedAsync("WarningQuit", "Common_Confirm", MessageBoxButton.YesNo);
		if (result == true)
		{
			this.Close();

			Application.Current.Dispatcher.Invoke(() =>
			{
				Application.Current.Shutdown();
			});
		}
	}
}