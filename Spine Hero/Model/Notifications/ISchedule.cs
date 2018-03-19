using Caliburn.Micro;

namespace SpineHero.Model.Notifications
{
    public interface ISchedule : INotifyPropertyChangedEx
    {
        INotification DisplayedNotification { get; set; }

        void HideAllNotifications();

        bool IsDisplayedNotification();

        bool IsScheduledOrDisplayed();

        void ScheduleNotification(INotification notification);
    }
}