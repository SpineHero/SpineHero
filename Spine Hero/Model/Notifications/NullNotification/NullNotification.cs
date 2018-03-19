using System;

namespace SpineHero.Model.Notifications.NullNotification
{
    public class NullNotification : INotification
    {
        public TimeSpan TimeLimit => TimeSpan.MaxValue;

        public void Show(Schedule.NotificationWasHidden callback)
        {
            throw new NotImplementedException();
        }

        public void Hide()
        {
            throw new NotImplementedException();
        }
    }
}