using System;
using Caliburn.Micro;
using SpineHero.ViewModels.Notifications;

namespace SpineHero.Model.Notifications.PopupNotification
{
    public class PopupNotification : PropertyChangedBase, IPopupNotification
    {
        private readonly PopupNotificationViewModel popupViewModel;
        private readonly IWindowManager windowManager;

        public PopupNotification(PopupNotificationViewModel popupViewModel, IWindowManager windowManager)
        {
            this.popupViewModel = popupViewModel;
            this.windowManager = windowManager;
        }

        public TimeSpan TimeLimit => TimeSpan.FromMinutes(Properties.Notifications.Default.PopupNotificationTimeLimit);

        public void Show(Schedule.NotificationWasHidden callback)
        {
            popupViewModel.NotificationWasHidden = callback;
            Execute.OnUIThreadAsync(() => windowManager.ShowWindow(popupViewModel));
            popupViewModel.AutomaticallyCloseWindow();
        }

        public void Hide()
        {
            popupViewModel.FadeOutAndCloseWindow();
        }
    }
}