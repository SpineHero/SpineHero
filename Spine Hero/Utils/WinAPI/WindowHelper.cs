using System;
using System.Runtime.InteropServices;

namespace SpineHero.Utils.WinAPI
{
	// Zdroj: http://joelabrahamsson.com/detecting-mouse-and-keyboard-input-with-net/
	public static class WindowHelper
	{
	    private const int WS_EX_TRANSPARENT = 0x00000020;
	    private const int GWL_EXSTYLE = (-20);

	    public delegate IntPtr HookDelegate(Int32 code, IntPtr wParam, IntPtr lParam);

		public delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, 
			int idChild, uint dwEventThread, uint dwmsEventTime);

		[DllImport("User32.dll", SetLastError = true)]
		public static extern IntPtr SetWindowsHookEx(Int32 idHook, HookDelegate lpfn, IntPtr hmod, Int32 dwThreadId);

		[DllImport("User32.dll", SetLastError = true)]
		public static extern IntPtr CallNextHookEx(IntPtr hHook, Int32 nCode, IntPtr wParam, IntPtr lParam);

		[DllImport("User32.dll", SetLastError = true)]
		public static extern bool UnhookWindowsHookEx(IntPtr hHook);

		[DllImport("User32.dll", SetLastError = true)]
		public static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc,
			WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwflags);

		[DllImport("User32.dll", SetLastError = true)]
		public static extern bool UnhookWinEvent(IntPtr hWinEventHook);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

        public static int SetWindowExtendedStyleToTransparent(IntPtr hwnd)
        {
            var extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
            return SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_TRANSPARENT);
        }
	}
}