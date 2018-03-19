using System;
using System.Collections.Generic;

namespace SpineHero.Model.Notifications
{
    public interface INotificationsSettings
    {
        List<INotification> EnabledNotifications { get; }

        INotification NextNotification(INotification lastUsedNotification);

        void ReloadIfNecessary();
    }
}