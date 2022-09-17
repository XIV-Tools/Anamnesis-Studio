// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Windows;

using Anamnesis.Memory;
using Anamnesis.Panels;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using XivToolsWpf;
using XivToolsWpf.Extensions;

public class OverlayWindow : FloatingWindow
{
	private readonly WindowInteropHelper windowInteropHelper;

	public OverlayWindow()
	{
		this.windowInteropHelper = new(this);
	}

	public override Rect Rect => new Rect(this.Left, this.Top, this.Width, this.Height);

	public override Rect ScreenRect
	{
		get
		{
			if (MemoryService.Process == null)
				return new Rect(0, 0, 0, 0);

			GetClientRect(MemoryService.Process.MainWindowHandle, out Win32Rect clientRect);
			GetWindowRect(MemoryService.Process.MainWindowHandle, out Win32Rect gameRect);

			int chromeWidth = ((gameRect.Right - gameRect.Left) - clientRect.Right) / 2;
			int titleBarHeight = ((gameRect.Bottom - gameRect.Top) - clientRect.Bottom) - chromeWidth;

			return new Rect(
				gameRect.Left + chromeWidth,
				gameRect.Top + titleBarHeight,
				(gameRect.Right - gameRect.Left) - (chromeWidth * 2),
				(gameRect.Bottom - gameRect.Top) - chromeWidth);
		}
	}

	public override bool CanPopOut => true;
	public override bool CanPopIn => false;

	protected override void OnWindowLoaded()
	{
		if (MemoryService.Process == null)
			return;

		SetParent(this.windowInteropHelper.Handle, MemoryService.Process.MainWindowHandle);

		const uint WS_POPUP = 0x80000000;
		const uint WS_CHILD = 0x40000000;
		const int GWL_STYLE = -16;

		int style = GetWindowLong(this.windowInteropHelper.Handle, GWL_STYLE);
		style = (int)((style & ~WS_POPUP) | WS_CHILD);
		SetWindowLong(this.windowInteropHelper.Handle, GWL_STYLE, style);

		this.UpdatePosition();

		this.Activate();
	}

	protected override void UpdatePosition()
	{
		base.UpdatePosition();

		if (MemoryService.Process == null)
			return;

		Rect screenRect = this.ScreenRect;

		int w = (int)this.ActualWidth - 1;
		int h = (int)this.ActualHeight - 1;

		int x = (int)(this.Left - screenRect.Left);
		int y = (int)(this.Top - screenRect.Top);

		x = Math.Clamp(x, 0, Math.Max((int)screenRect.Width - w, 0));
		y = Math.Clamp(y, 0, Math.Max((int)screenRect.Height - h, 0));

		// SHOWWINDOW
		SetWindowPos(this.windowInteropHelper.Handle, IntPtr.Zero, x, y, w, h,  0x0040);

		// NOSIZE
		SetWindowPos(this.windowInteropHelper.Handle, IntPtr.Zero, x, y, w, h,  0x0001);

		if (this.Topmost)
		{
			// NOSIZE | NOMOVE
			SetWindowPos(this.windowInteropHelper.Handle, (IntPtr)(-1), x, y, w, h,  0x0001 | 0x0003);
		}
	}

	[DllImport("user32.dll", SetLastError = true)]
	private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

	[DllImport("user32.dll", SetLastError = true)]
	private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

	[DllImport("user32.dll", SetLastError = true)]
	private static extern bool GetWindowRect(IntPtr hwnd, out Win32Rect rect);

	[DllImport("user32.dll", SetLastError = true)]
	private static extern bool GetClientRect(IntPtr hwnd, out Win32Rect rect);

	[DllImport("user32.dll")]
	private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

	[DllImport("user32.dll", EntryPoint = "GetWindowLong")]
	private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

	[StructLayout(LayoutKind.Sequential)]
	public struct Win32Rect
	{
		public int Left;        // x position of upper-left corner
		public int Top;         // y position of upper-left corner
		public int Right;       // x position of lower-right corner
		public int Bottom;      // y position of lower-right corner
	}
}
