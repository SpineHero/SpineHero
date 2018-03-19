using System;
using System.Runtime.InteropServices;

namespace SpineHero.Utils.WinAPI
{
	public class InputHelper
	{
		[DllImport("User32.dll", SetLastError = true)]
		public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

		[DllImport("User32.dll", SetLastError = true)]
		public static extern bool UnregisterHotKey(IntPtr hWnd, int id);
	}
}