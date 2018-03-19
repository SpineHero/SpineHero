using System;
using System.Collections.Generic;
using System.Linq;
using SpineHero.Model.Notifications.DimNotification;
using SpineHero.Model.Notifications.PopupNotification;

namespace SpineHero.Model.Notifications
{
    public class NotificationsSettings : INotificationsSettings
    {
        private readonly IPopupNotification popupNotification;
        private readonly IDimNotification dimNotification;

        public NotificationsSettings(IPopupNotification popupNotification, IDimNotification dimNotification)
        {
            this.popupNotification = popupNotification;
            this.dimNotification = dimNotification;
            Properties.Notifications.Default.PropertyChanged += (sender, args) => SettingsWasChanged = true;
            Reload();
        }

        public bool SettingsWasChanged { get; set; }

        public List<INotification> EnabledNotifications { get; } = new List<INotification>();

        public INotification NextNotification(INotification lastUsedNotification)
        {
            if (lastUsedNotification != null && EnabledNotifications.Any() && EnabledNotifications.Last() != lastUsedNotification)
            {
                var index = EnabledNotifications.IndexOf(lastUsedNotification);
                return EnabledNotifications[index + 1];
            }
            return EnabledNotifications.Any() ? EnabledNotifications.First() : new NullNotification.NullNotification();
        }

        public void ReloadIfNecessary()
        {
            if (SettingsWasChanged)
            {
                Reload();
                SettingsWasChanged = false;
            }
        }

        public virtual void Reload()
        {
            EnabledNotifications.Clear();

            if (Properties.Notifications.Default.PopupNotification)
                EnabledNotifications.Add(popupNotification);
            if (Properties.Notifications.Default.DimNotification)
                EnabledNotifications.Add(dimNotification);

            // Sort notifications by TimeLimit property
            EnabledNotifications.Sort((l, r) => TimeSpan.Compare(l.TimeLimit, r.TimeLimit));
        }
    }
}