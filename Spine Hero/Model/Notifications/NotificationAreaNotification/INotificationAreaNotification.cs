using System.ComponentModel;

namespace SpineHero.Model.Notifications.NotificationAreaNotification
{
	public interface INotificationAreaNotification : INotifyPropertyChanged
    {
	    string SelectedIcon { get; }

        string StartStopMonitoringText { get; }

        string SelectedToolTipText { get; }
    }
}