using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using Caliburn.Micro;

namespace SpineHero.Utils.WinAPI
{
    public class WindowStyle
    {
        private readonly Window window;

        public WindowStyle(Window window)
        {
            this.window = window;
            if (window == null)
                throw new WindowStyleException("Window instance can not be null");
        }

        public WindowStyle(Screen screen) : this((Window) screen.GetView())
        {}

        public async void ApplyClickThrough()
        {
            var windowHandle = IntPtr.Zero;
            await Execute.OnUIThreadAsync(() => windowHandle = new WindowInteropHelper(window).Handle);
            if (windowHandle == IntPtr.Zero)
                throw new WindowStyleException("Unable to get window handle.");

            if (WindowHelper.SetWindowExtendedStyleToTransparent(windowHandle) == 0)
            {
                throw new WindowStyleException(
                    $"Can not set window as click-through. Unable to change extended window style. Error {Marshal.GetLastWin32Error()}");
            }
        }
    }

    public class WindowStyleException : Exception
    {
        public WindowStyleException() { }

        public WindowStyleException(string message) : base(message) { }

        public WindowStyleException(string message, Exception inner) : base(message, inner) { }
    }
}