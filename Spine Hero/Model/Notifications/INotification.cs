using System;

namespace SpineHero.Model.Notifications
{
    public interface INotification
    {
        TimeSpan TimeLimit { get; }

        void Show(Schedule.NotificationWasHidden callback);

        void Hide();
    }
}