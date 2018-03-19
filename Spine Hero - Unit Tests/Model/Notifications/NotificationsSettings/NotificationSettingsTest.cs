using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using SpineHero.Model.Notifications;
using SpineHero.Model.Notifications.DimNotification;
using SpineHero.Model.Notifications.PopupNotification;

namespace SpineHero.UnitTests.Model.Notifications.NotificationsSettings
{
    [TestFixture]
    public class NotificationSettingsTest : AssertionHelper
    {
        private SpineHero.Model.Notifications.NotificationsSettings settings;
        private IPopupNotification popup;
        private IDimNotification dim;

        [SetUp]
        public void SetUp()
        {
            // Enable Popup and Dim notification
            Properties.Notifications.Default.PopupNotification = true;
            Properties.Notifications.Default.DimNotification = true;
            // ... and set time limit
            Properties.Notifications.Default.PopupNotificationTimeLimit = 5;
            Properties.Notifications.Default.DimNotificationTimeLimit = 10;

            var popupMock = new Mock<IPopupNotification>();
            popupMock.SetupGet(p => p.TimeLimit)
                .Returns(TimeSpan.FromMinutes(Properties.Notifications.Default.PopupNotificationTimeLimit));
            var dimMock = new Mock<IDimNotification>();
            dimMock.SetupGet(p => p.TimeLimit)
                .Returns(TimeSpan.FromMinutes(Properties.Notifications.Default.DimNotificationTimeLimit));

            popup = popupMock.Object;
            dim = dimMock.Object;

            settings = new SpineHero.Model.Notifications.NotificationsSettings(popup, dim);
        }

        [Test]
        public void HasListOfEnabledNotifications()
        {
            Expect(settings.EnabledNotifications, EqualTo(new List<INotification> { popup, dim, }));
        }

        [Test]
        public void CanReloadSettings()
        {
            Properties.Notifications.Default.PopupNotification = false;
            settings.ReloadIfNecessary();

            Expect(settings.EnabledNotifications, EqualTo(new List<INotification> { dim }));
        }

        [Test]
        public void DoReloadOnlyIfSettingsChanged()
        {
            Properties.Notifications.Default.PopupNotification = false;
            settings.SettingsWasChanged = false;
            settings.ReloadIfNecessary();
            Expect(settings.EnabledNotifications, Has.Count.EqualTo(2));


            settings.SettingsWasChanged = true;
            settings.ReloadIfNecessary();
            Expect(settings.EnabledNotifications, Has.Count.EqualTo(1));
        }

        [Test]
        public void EnabledNotificationsAreOrderedByTimeLimit()
        {
            Properties.Notifications.Default.PopupNotificationTimeLimit = 5;
            Properties.Notifications.Default.DimNotificationTimeLimit = 10;
            settings.Reload();

            Expect(settings.EnabledNotifications, EqualTo(new List<INotification> { popup, dim, }));

            Properties.Notifications.Default.PopupNotificationTimeLimit = 50;
            Properties.Notifications.Default.DimNotificationTimeLimit = 10;
            var popupMock = new Mock<IPopupNotification>();
            popupMock.SetupGet(p => p.TimeLimit)
                .Returns(TimeSpan.FromMinutes(Properties.Notifications.Default.PopupNotificationTimeLimit));
            settings = new SpineHero.Model.Notifications.NotificationsSettings(popupMock.Object, dim);
            settings.Reload();

            Expect(settings.EnabledNotifications, EqualTo(new List<INotification> { dim, popupMock.Object }));
        }
    }
}