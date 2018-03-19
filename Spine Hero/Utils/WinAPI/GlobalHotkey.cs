using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using SpineHero.Utils.Logging;

namespace SpineHero.Utils.WinAPI
{
    public class GlobalHotkey : IDisposable
    {
        private const int HotkeyPressedMsg = 0x0312;
        private static readonly ILogger Logger = Logging.Logger.GetLogger<GlobalHotkey>();
        private static int _counter = 0;
        private readonly GlobalHotkeyPressedCallback callback;
        private readonly int hotkeyId;
        private Window window;
        private IntPtr windowHandle;
        private HwndSource windowHandleSource;

        private GlobalHotkey(GlobalHotkeyPressedCallback callback)
        {
            this.callback = callback;
            this.hotkeyId = ++_counter;
        }

        public GlobalHotkey(Shortcut shortcut, GlobalHotkeyPressedCallback callback) : this(callback)
        {
            Register(shortcut);
            Shortcut = shortcut;
        }

        public GlobalHotkey(List<Shortcut> shortcuts, GlobalHotkeyPressedCallback callback) : this(callback)
        {
            bool registered = false;
            Exception lastException = null;
            foreach (var shortcut in shortcuts)
            {
                try
                {
                    Register(shortcut);
                    Shortcut = shortcut;
                    registered = true;
                    break;
                }
                catch (GlobalHotkeyException e)  // If I am unable to register this shortcut, I try another one
                {
                    lastException = e;
                }
            }
            if (!registered)
                throw new GlobalHotkeyException("Unable to register any provided shortcut", lastException);
        }

        public Shortcut Shortcut { get; set; }

        public delegate void GlobalHotkeyPressedCallback();

        private void Register(Shortcut shortcut)
        {
            if ((window = Application.Current.MainWindow) == null)
                throw new GlobalHotkeyException("Unable to get MainWindow instance.");
            if ((windowHandle = new WindowInteropHelper(window).Handle) == IntPtr.Zero)
                throw new GlobalHotkeyException("Unable to get window handle.");
            if ((windowHandleSource = HwndSource.FromHwnd(windowHandle)) == null)
                throw new GlobalHotkeyException("Unable to get handle source of window.");
            
            if (InputHelper.RegisterHotKey(windowHandle, hotkeyId, shortcut.Modifier.Code, shortcut.Key.Code))
                windowHandleSource.AddHook(Pressed);
            else
                throw new GlobalHotkeyException(
                    $"Unable to register global hotkey. Shortcut: {shortcut} Hotkey ID: {hotkeyId} " +
                    $"Error: {Marshal.GetLastWin32Error()}"
                    );
        }

        private IntPtr Pressed(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam, ref bool handled)
        {
            if (hwnd == windowHandle && msg == HotkeyPressedMsg && wparam.ToInt32() == hotkeyId)
                callback();
            return IntPtr.Zero;
        }

        public void Dispose()
        {
            Unregister();
        }

        private void Unregister()
        {
            if (windowHandle != IntPtr.Zero && windowHandleSource != null)
            {
                windowHandleSource.RemoveHook(Pressed);
                if (!InputHelper.UnregisterHotKey(windowHandle, hotkeyId))
                    Logger.Warn($"Unable to unregister global hotkey. Hotkey ID: {hotkeyId} Error: {1}",
                                hotkeyId, Marshal.GetLastWin32Error());
            }
        }

        ~GlobalHotkey()
        {
            Dispose();
        }
    }

    public class GlobalHotkeyException : Exception
    {
        public GlobalHotkeyException() {}

        public GlobalHotkeyException(string message) : base(message) {}

        public GlobalHotkeyException(string message, Exception inner) : base(message, inner) {}
    }
}