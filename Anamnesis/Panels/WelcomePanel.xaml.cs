// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Panels;

using Anamnesis.Windows;
using System.Windows;

public partial class WelcomePanel : PanelBase
{
	public override void OnActivated()
	{
		base.OnActivated();

		if (VersionInfo.Date.Year <= 2000)
		{
			this.VersionLabel.Text = "Developer";
		}
		else
		{
			this.VersionLabel.Text = "version " + VersionInfo.Date.ToString("yyyy-MM-dd HH:mm");
		}
	}

	private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
	{
		this.Close();
    }
}