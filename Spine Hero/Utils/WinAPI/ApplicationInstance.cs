using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;

namespace SpineHero.Utils.WinAPI
{
    public class ApplicationInstance
    {
        [DllImport("user32", CharSet = CharSet.Unicode)]
        static extern IntPtr FindWindow(string cls, string win);

        [DllImport("user32")]
        static extern IntPtr SetForegroundWindow(IntPtr hWnd);


        [DllImport("user32")]
        static extern bool IsIconic(IntPtr hWnd);

        [DllImport("user32")]
        static extern bool OpenIcon(IntPtr hWnd);

        public void PreserveSingleInstance()
        {
            var thisProc = Process.GetCurrentProcess();
            if (Process.GetProcessesByName(thisProc.ProcessName).Length > 1)
            {
                ActivateOtherWindow();
                Application.Current.Shutdown();
            }
        }

        private void ActivateOtherWindow()
        {
            var other = FindWindow(null, "Spine Hero");
            if (other != IntPtr.Zero)
            {
                SetForegroundWindow(other);
                if (IsIconic(other))    // "Iconic" is other word for "minimized"
                    OpenIcon(other);
            }
        }
    }
}