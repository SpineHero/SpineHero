using Microsoft.Win32;

namespace SpineHero.Model
{
    internal static class AutoStartup
    {
        private const string KEY_NAME = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string APP_NAME = "Spine Hero"; //TODO use assembly instead?

        public static bool AutoStart
        {
            get
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(KEY_NAME, true))
                {
                    var val = key?.GetValue(APP_NAME);
                    return val != null;
                }
            }
            set
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(KEY_NAME, true))
                {
                    if (key == null)
                    {
                        // Key doesn't exist. Do whatever you want to handle
                        // this case
                    }
                    else
                    {
                        var path = System.Reflection.Assembly.GetExecutingAssembly().Location;
                        if (value)
                            key.SetValue(APP_NAME, path, RegistryValueKind.String);
                        else
                            key.DeleteValue(APP_NAME);
                    }
                }
            }
        }
    }
}