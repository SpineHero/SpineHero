using System;
using Caliburn.Micro;
using Moq;
using NUnit.Framework;
using SpineHero.Model.Notifications;
using SpineHero.PostureMonitoring;
using Action = System.Action;

namespace SpineHero.UnitTests.PostureMonitoring
{
    [TestFixture]
    public class AnalyzePeriodManagerTests : AssertionHelper
    {
        Mock<IEventAggregator> ea;
        Mock<ISchedule> schedule;

        [SetUp]
        public void SetUp()
        {
            ea = new Mock<IEventAggregator>();
            schedule = new Mock<ISchedule>();
        }

        [Test]
        public void ChangeNormalAnalyzePeriodWhenRelevantSettingsChanged()
        {
            var manager = new AnalyzePeriodManager(ea.Object, schedule.Object);

            Properties.Settings.Default.DataSource = Properties.Settings.Default.DataSource;    // not relevant settings
            ea.Verify(e => e.Publish(It.IsAny<AnalyzePeriodChange>(), It.IsAny<Action<Action>>()), Times.Never);

            Properties.Settings.Default.AnalyzePeriod = 5;  // relevant settings
            ea.Verify(e => e.Publish(It.Is<AnalyzePeriodChange>(arg => arg.PeriodTime == 5), It.IsAny<Action<Action>>()), Times.Once);
        }

        [Test]
        public void NotChangeNormalAnalyzePeriodWhenNotificationIsDisplayed()
        {
            schedule.Setup(s => s.IsDisplayedNotification()).Returns(true);
            var manager = new AnalyzePeriodManager(ea.Object, schedule.Object);
            Properties.Settings.Default.AnalyzePeriod = 5;

            ea.Verify(e => e.Publish(It.Is<AnalyzePeriodChange>(arg => arg.PeriodTime == 5), It.IsAny<Action<Action>>()), Times.Never);
        }

        [Test]
        public void ChangePeriodWhenNotificationIsDisplayed()
        {
            var scheduleReal = new Schedule(ea.Object);
            var manager = new AnalyzePeriodManager(ea.Object, scheduleReal);
            scheduleReal.DisplayedNotification = new Mock<INotification>().Object;

            ea.Verify(e => e.Publish(
                It.Is<AnalyzePeriodChange>(arg => arg.PeriodTime == Properties.Settings.Default.AnalyzePeriodWithNotification), 
                It.IsAny<Action<Action>>()), 
                Times.Once);
        }

        [Test]
        public void ChangePeriodWhenNotificationIsHidden()
        {
            var scheduleReal = new Schedule(ea.Object);
            var manager = new AnalyzePeriodManager(ea.Object, scheduleReal);
            scheduleReal.DisplayedNotification = null;

            ea.Verify(e => e.Publish(
                It.Is<AnalyzePeriodChange>(arg => arg.PeriodTime == Properties.Settings.Default.AnalyzePeriod), 
                It.IsAny<Action<Action>>()), 
                Times.Once);
        }
    }
}