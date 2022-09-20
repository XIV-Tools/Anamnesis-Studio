// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Keyboard;

using System.Runtime.InteropServices;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using static System.Runtime.CompilerServices.RuntimeHelpers;
using System.Collections.Generic;
using Anamnesis.Memory;
using static Lumina.Data.Parsing.Layer.LayerCommon;
using System.Reflection.Emit;

// © Anamnesis.
// Licensed under the MIT license.

// © NonInvasiveKeyboardHook library
// Licensed under the MIT license.
// https://github.com/kfirprods/NonInvasiveKeyboardHook

// boy I hope our users trust me, cause this catches /all/ keyboard inputs, system wide.
// This could be used to create a Keylogger. =(
// https://blogs.msdn.microsoft.com/toub/2006/05/03/low-level-keyboard-hook-in-c/
public class KeyboardService : ServiceBase<KeyboardService>
{
	private const int WhKeyboardLl = 13;
	private const int WmKeyDown = 0x0100;
	private const int WmKeyUp = 0x0101;
	private const int WmSysKeyDown = 0x0104;
	private const int WmSysKeyUp = 0x0105;

	private static IntPtr hookId = IntPtr.Zero;
	private static LowLevelKeyboardProc? hook;
	private readonly HashSet<Key> keysDown = new();

	private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

	public override Task Start()
	{
		var userLibrary = LoadLibrary("User32");
		hook = this.HookCallback;
		hookId = SetWindowsHookEx(WhKeyboardLl, hook, userLibrary, 0);

		return base.Start();
	}

	public override Task Shutdown()
	{
		UnhookWindowsHookEx(hookId);
		return base.Shutdown();
	}

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool UnhookWindowsHookEx(IntPtr hhk);

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

	[DllImport("kernel32.dll")]
	private static extern IntPtr LoadLibrary(string lpFileName);

	private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
	{
		int vkCode = Marshal.ReadInt32(lParam);
		if (this.HandleSingleKeyboardInput(wParam, vkCode))
		{
			return new IntPtr(-1);
		}

		return CallNextHookEx(hookId, nCode, wParam, lParam);
	}

	private bool HandleSingleKeyboardInput(IntPtr wParam, int vkCode)
	{
		// get key event type
		KeyboardKeyStates state = (int)wParam switch
		{
			WmKeyDown => KeyboardKeyStates.Pressed,
			WmSysKeyDown => KeyboardKeyStates.Pressed,

			WmKeyUp => KeyboardKeyStates.Released,
			WmSysKeyUp => KeyboardKeyStates.Released,

			_ => throw new Exception($"Unknown key event type: {wParam}")
		};

		// convert from VKeys
		Key key = KeyInterop.KeyFromVirtualKey(vkCode);

		// Reieving multple 'pressed' events meand the key is held down.
		if (state == KeyboardKeyStates.Pressed)
		{
			if (!this.keysDown.Contains(key))
			{
				this.keysDown.Add(key);
			}
			else
			{
				state = KeyboardKeyStates.Down;
			}
		}
		else if (state == KeyboardKeyStates.Released)
		{
			this.keysDown.Remove(key);
		}

		// Don't attempt to handle _just_ modifier key presses
		if (key >= Key.LeftShift && key <= Key.RightAlt)
			return false;

		if (key >= Key.LWin && key <= Key.Sleep)
			return false;

		// Get modifiers
		ModifierKeys modifiers = ModifierKeys.None;

		if (this.keysDown.Contains(Key.LeftCtrl) || this.keysDown.Contains(Key.RightCtrl))
			modifiers |= ModifierKeys.Control;

		if (this.keysDown.Contains(Key.LeftAlt) || this.keysDown.Contains(Key.RightAlt))
			modifiers |= ModifierKeys.Alt;

		if (this.keysDown.Contains(Key.LeftShift) || this.keysDown.Contains(Key.RightShift))
			modifiers |= ModifierKeys.Shift;

		if (this.keysDown.Contains(Key.LWin) || this.keysDown.Contains(Key.RWin))
			modifiers |= ModifierKeys.Windows;

		return this.HandleKey(key, modifiers, state);
	}

	private bool HandleKey(Key key, ModifierKeys modifiers, KeyboardKeyStates state)
	{
		// TODO: Keyboard events need to be passed to the current windows, and if no window handles it,
		// then checked through the global hotkeys somewhere, and if an anamnesis window has focus, but the event
		// was not handled, pass it back to ffxiv...
		// This all needs to work alongside the window native OnPreviewKeyDown and OnPreviewKeyUp events...
		////Log.Information($"{key} - {modifiers} - {state}");

		// If we have focus, but key was not handled, pass the event to ffxiv
		// Pressed - PostMessage(0x100, (IntPtr)vkey, IntPtr.Zero);
		// Released - PostMessage(0x0101, (IntPtr)vkey, IntPtr.Zero);
		// TODO: modifier keys don't work this way, but other keys do, so figure out how to send shift and stuff to ffxiv...
		if (state == KeyboardKeyStates.Pressed || state == KeyboardKeyStates.Down)
		{
			MemoryService.PostMessage(WmKeyDown, (IntPtr)KeyInterop.VirtualKeyFromKey(key), IntPtr.Zero);
		}
		else if (state == KeyboardKeyStates.Released)
		{
			MemoryService.PostMessage(WmKeyUp, (IntPtr)KeyInterop.VirtualKeyFromKey(key), IntPtr.Zero);
		}

		return false;
	}
}
