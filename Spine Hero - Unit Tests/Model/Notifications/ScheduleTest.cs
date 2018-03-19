using Caliburn.Micro;
using Moq;
using NUnit.Framework;
using SpineHero.Model.Notifications;

namespace SpineHero.UnitTests.Model.Notifications
{
    [TestFixture]
    public class ScheduleTest : AssertionHelper
    {
        [Test]
        public void CanScheduleNotification()
        {
            var notification = new Mock<INotification>();
            var ea = new Mock<IEventAggregator>();
            var schedule = new Schedule(ea.Object);

            schedule.ScheduleNotification(notification.Object);

            notification.Verify(n => n.Show(It.IsAny<Schedule.NotificationWasHidden>()), Times.Once);
            Expect(schedule.IsDisplayedNotification(), Is.True);
        }

        [Test]
        public void IsScheduledOrDisplayed()
        {
            var notification = new Mock<INotification>();
            var ea = new Mock<IEventAggregator>();
            var schedule = new Schedule(ea.Object);

            Expect(schedule.IsDisplayedNotification(), Is.False);
            Expect(schedule.IsScheduledOrDisplayed(), Is.False);

            schedule.ScheduleNotification(notification.Object);

            Expect(schedule.IsDisplayedNotification(), Is.True);
            Expect(schedule.IsScheduledOrDisplayed(), Is.True);
        }

        [Test]
        public void DisplayedNotificationIsNullifiedWhenNotificationIsHidden()
        {
            var notification = new Mock<INotification>();
            var ea = new Mock<IEventAggregator>();
            var schedule = new Schedule(ea.Object);
            schedule.ScheduleNotification(notification.Object);

            schedule.DisplayedNotificationWasHiddenCallback();

            Expect(schedule.IsDisplayedNotification(), Is.False);
        }
    }
}