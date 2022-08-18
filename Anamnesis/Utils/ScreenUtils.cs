// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Utils;

using System.Windows.Forms;
using System.Windows;
using System.Windows.Media;

public static class ScreenUtils
{
	public static bool IsOnScreen(Point val, Window owner)
	{
		DpiScale dpi = VisualTreeHelper.GetDpi(owner);

		bool found = false;
		foreach (Screen screen in Screen.AllScreens)
		{
			found |= screen.Bounds.Contains(new System.Drawing.Point((int)(val.X * dpi.DpiScaleX), (int)(val.Y * dpi.DpiScaleY)));
		}

		return found;
	}
}
