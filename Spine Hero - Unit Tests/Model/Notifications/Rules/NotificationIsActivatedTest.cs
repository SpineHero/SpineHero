using Moq;
using NUnit.Framework;
using SpineHero.Model.Notifications;
using SpineHero.Model.Notifications.Rules;

namespace SpineHero.UnitTests.Model.Notifications.Rules
{
    [TestFixture]
    public class NotificationIsActivatedTest : AssertionHelper
    {
        [Test]
        public void ChecksNotificationsManager()
        {
            var manager1 = new Mock<ISchedule>();
            var manager2 = new Mock<ISchedule>();
            manager1.Setup(m => m.IsScheduledOrDisplayed()).Returns(true);
            manager2.Setup(m => m.IsScheduledOrDisplayed()).Returns(false);
            var rule1 = new NotificationIsActivated(manager1.Object);
            var rule2 = new NotificationIsActivated(manager2.Object);
            var stats = new SpineHero.Model.Notifications.NotificationStatistics();

            Expect(rule1.Check(stats), Is.True);
            Expect(rule2.Check(stats), Is.False);
        }
    }
}