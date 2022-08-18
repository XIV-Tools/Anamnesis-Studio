// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Navigation;
using Anamnesis.Panels;
using PropertyChanged;
using System.Windows;
using System.Windows.Input;
using XivToolsWpf.Extensions;
using Anamnesis.Files;

/// <summary>
/// Interaction logic for Navigation.xaml.
/// </summary>
[AddINotifyPropertyChangedInterface]
public partial class NavigationPanel : PanelBase
{
	public NavigationPanel(IPanelHost host)
		: base(host)
	{
		this.InitializeComponent();
		this.ContentArea.DataContext = this;
	}

	public bool Expanded { get; set; } = true;

	private async void OnIconMouseDown(object sender, MouseButtonEventArgs e)
	{
		if (e.LeftButton == MouseButtonState.Pressed)
		{
			this.DragMove();
		}
		else if (e.RightButton == MouseButtonState.Pressed)
		{
			bool? result = await GenericDialogPanel.ShowLocalizedAsync("Pose_WarningQuit", "Common_Confirm", MessageBoxButton.YesNo);
			if (result == true)
			{
				this.Close();
				Application.Current.Shutdown();
			}
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
}