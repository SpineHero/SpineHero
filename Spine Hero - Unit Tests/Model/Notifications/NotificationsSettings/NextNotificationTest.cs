using System;
using Moq;
using NUnit.Framework;
using SpineHero.Model.Notifications.DimNotification;
using SpineHero.Model.Notifications.NullNotification;
using SpineHero.Model.Notifications.PopupNotification;

namespace SpineHero.UnitTests.Model.Notifications.NotificationsSettings
{
    [TestFixture]
    public class NextNotificationTest : AssertionHelper
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
        public void NextNotificationWhenNoNotificationEnabled()
        {
            Properties.Notifications.Default.PopupNotification = false;
            Properties.Notifications.Default.DimNotification = false;
            settings.Reload();
            
            Expect(settings.NextNotification(popup), Is.InstanceOf<NullNotification>());
        }

        [Test]
        public void NextNotificationWhenNoNotificationWasUsed()
        {
            Expect(settings.NextNotification(null), EqualTo(popup), "First notification should be used");
        }

        [Test]
        public void NextNotificationWhenSomeNotificationWasUsedBefore()
        {
            Expect(settings.NextNotification(popup), EqualTo(dim), "Next notification should be used");
        }

        [Test]
        public void NextNotificationWhenAllTypesOfNotificationsWereShown()
        {
            Expect(settings.NextNotification(dim), EqualTo(popup), "First notification should be used");
        }

        [Test]
        public void NextNotificationWhenLastUsedNotificationWasDisabled()
        {
            Expect(settings.NextNotification(dim), EqualTo(popup), "First notification should be used");
        }
    }
}