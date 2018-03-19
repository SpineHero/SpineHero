using System.ComponentModel;
using Caliburn.Micro;
using SpineHero.Model.Notifications.NotificationAreaNotification;

namespace SpineHero.ViewModels.Notifications
{
    public class NotificationAreaIconViewModel : PropertyChangedBase
    {
        private readonly INotificationAreaNotification notificationAreaNotification;
        public NotificationAreaIconViewModel(INotificationAreaNotification notificationAreaNotification)
        {
            this.notificationAreaNotification = notificationAreaNotification;
            notificationAreaNotification.PropertyChanged += NotificationAreaNotificationOnPropertyChanged;
        }

        public string SelectedIcon => notificationAreaNotification.SelectedIcon;

        public string StartStopMonitoringText => notificationAreaNotification.StartStopMonitoringText;

        public string SelectedToolTipText => notificationAreaNotification.SelectedToolTipText;

        private void NotificationAreaNotificationOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            switch (propertyChangedEventArgs.PropertyName)
            {
                case "SelectedIcon":
                    NotifyOfPropertyChange(() => SelectedIcon);
                    break;
                case "StartStopMonitoringText":
                    NotifyOfPropertyChange(() => StartStopMonitoringText);
                    break;
                case "SelectedToolTipText":
                    NotifyOfPropertyChange(() => SelectedToolTipText);
                    break;
            }
        }
    }
}
