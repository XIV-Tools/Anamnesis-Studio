// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Panels;

using FontAwesome.Sharp.Pro;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using System.Windows;
using XivToolsWpf;

public partial class ExceptionPanel : PanelBase
{
	public static async Task Show(ExceptionDispatchInfo error, bool isCritical)
	{
		ExceptionPanel? panel = await App.Services.Panels.Show<ExceptionPanel>();

		await panel.Dispatcher.MainThread();

		Window? hostWindow = panel.Window as Window;

		panel.ContentArea.Content = new XivToolsWpf.Dialogs.ErrorDialog(hostWindow, error, isCritical);
		panel.Title = "Anamnesis v" + VersionInfo.Date.ToString("yyyy-MM-dd HH:mm");
		////this.TitleColor = data.IsCritical ? Colors.Red : Colors.Yellow;
		panel.Icon = isCritical ? ProIcons.ExclamationCircle : ProIcons.ExclamationTriangle;

		while (panel.IsOpen)
		{
			await Task.Delay(500);
		}
	}
}