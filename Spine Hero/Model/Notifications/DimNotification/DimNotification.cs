using System;
using System.Collections.Generic;
using System.Windows;
using Caliburn.Micro;
using SpineHero.Utils.WinAPI;
using SpineHero.ViewModels.Notifications;

namespace SpineHero.Model.Notifications.DimNotification
{
    public class DimNotification : IDimNotification
    {
        private readonly IWindowManager windowManager;
        private readonly List<DimNotificationViewModel> viewModels = new List<DimNotificationViewModel>();
        private readonly GlobalHotkey hotkey;

        public DimNotification(IWindowManager windowManager)
        {
            this.windowManager = windowManager;
            try
            {
                hotkey = new GlobalHotkey(new List<Shortcut>
                {
                    new Shortcut(Keys.Shift, Keys.Escape),
                    new Shortcut(Keys.Control, Keys.Escape),
                    new Shortcut(Keys.Shift, Keys.Backspace),
                    new Shortcut(Keys.Control, Keys.Backspace),
                }, Hide);
            }
            catch (GlobalHotkeyException e)
            {
                MessageBox.Show(
                    $"Unable to register global hotkey for Dim notification. This feature will be disabled. Info: {e.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        public TimeSpan TimeLimit => TimeSpan.FromMinutes(Properties.Notifications.Default.DimNotificationTimeLimit);

        public void Show(Schedule.NotificationWasHidden callback)
        {
            viewModels.Clear();
            var allScreens = WpfScreenHelper.Screen.AllScreens;
            foreach (var screen in allScreens)
            {
                var viewModel = new DimNotificationViewModel(screen.Bounds, hotkey) {NotificationWasHidden = callback};
                Execute.OnUIThreadAsync(() => windowManager.ShowWindow(viewModel));
                viewModel.SetupWindow();
                viewModels.Add(viewModel);
            }
        }

        public void Hide()
        {
            viewModels.ForEach(v => v.FadeOutAndCloseWindow());
            viewModels.Clear();
        }
    }
}