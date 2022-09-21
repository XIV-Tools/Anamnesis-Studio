// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Panels;

using Anamnesis.Navigation;
using Anamnesis.Windows;
using FontAwesome.Sharp;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;

public partial class ExceptionPanel : PanelBase
{
	public static async Task Show(ExceptionDispatchInfo error, bool isCritical)
	{
		ErrorInfo info = new(error, isCritical);
		PanelBase? panel = await App.Services.Navigation.Navigate(new("Exception", info));

		while (panel.IsOpen)
		{
			await Task.Delay(500);
		}
	}

	public override void SetContext(FloatingWindow host, object? context)
	{
		base.SetContext(host, context);

		if (context is not ErrorInfo data)
			return;

		Window? hostWindow = this.Window as Window;

		this.ContentArea.Content = new XivToolsWpf.Dialogs.ErrorDialog(hostWindow, data.Error, data.IsCritical);
		this.Title = "Anamnesis v" + VersionInfo.Date.ToString("yyyy-MM-dd HH:mm");
		this.TitleColor = data.IsCritical ? Colors.Red : Colors.Yellow;
		this.Icon = data.IsCritical ? IconChar.ExclamationCircle : IconChar.ExclamationTriangle;
	}

	public class ErrorInfo
	{
		public ExceptionDispatchInfo Error;
		public bool IsCritical = false;

		public ErrorInfo(ExceptionDispatchInfo error, bool isCritical)
		{
			this.Error = error;
			this.IsCritical = isCritical;
		}
	}
}
