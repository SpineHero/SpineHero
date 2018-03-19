using System;
using Moq;
using NUnit.Framework;
using SpineHero.Model.Notifications;
using SpineHero.Model.Notifications.DimNotification;
using SpineHero.Model.Notifications.Rules;
using SpineHero.Monitoring.Watchers.Management.Results;

namespace SpineHero.UnitTests.Model.Notifications.Rules
{
    [TestFixture]
    public class FulfilledTimeLimitTest : AssertionHelper
    {
        private IDimNotification dim;

        [SetUp]
        public void SetUp()
        {
            var dimMock = new Mock<IDimNotification>();
            dimMock.SetupGet(p => p.TimeLimit)
                .Returns(TimeSpan.FromMinutes(Properties.Notifications.Default.DimNotificationTimeLimit));
            dim = dimMock.Object;
        }

        [Test]
        public void NextNotificationIsFetchedFromNotificationSettings()
        {
            var settings = new Mock<INotificationsSettings>();
            settings.Setup(s => s.NextNotification(null)).Returns(dim);
            var rule = new FulfilledTimeLimit(settings.Object);
            var stats = new NotificationStatistics { LastUsedNotification = null, Evaluation = new EvaluationStatistics
            {
                Current = new Evaluation(),
                FirstWrong = new Evaluation()
            } };

            rule.Check(stats);

            settings.Verify(s => s.NextNotification(null), Times.Once);
        }

        [Test]
        public void CheckWhenIsSittingShortly()
        {
            Properties.Notifications.Default.DimNotificationTimeLimit = 10;
            var settings = new Mock<SpineHero.Model.Notifications.INotificationsSettings>();
            settings.Setup(s => s.NextNotification(null)).Returns(dim);
            var rule = new FulfilledTimeLimit(settings.Object);
            var stats = new SpineHero.Model.Notifications.NotificationStatistics
            {
                Evaluation =
                {
                    Current = new Evaluation {EvaluatedAt = DateTime.Now},
                    FirstWrong = new Evaluation {EvaluatedAt = DateTime.Now.AddSeconds(-1)}
                }
            };

            Expect(rule.Check(stats), Is.False);
        }

        [Test]
        public void CheckWhenIsSittingSameTimeAsTimeLimit()
        {
            Properties.Notifications.Default.DimNotificationTimeLimit = 10;
            var settings = new Mock<SpineHero.Model.Notifications.INotificationsSettings>();
            settings.Setup(s => s.NextNotification(null)).Returns(dim);
            var rule = new FulfilledTimeLimit(settings.Object);
            var datetime = DateTime.Now;
            var stats = new SpineHero.Model.Notifications.NotificationStatistics
            {
                Evaluation =
                {
                    Current = new Evaluation {EvaluatedAt = datetime},
                    FirstWrong = new Evaluation {EvaluatedAt = datetime.AddMinutes(-10)}
                }
            };

            Expect(rule.Check(stats), Is.True);
        }

        [Test]
        public void CheckWhenIsSittingLongEnough()
        {
            Properties.Notifications.Default.PopupNotificationTimeLimit = 10;
            var settings = new Mock<SpineHero.Model.Notifications.INotificationsSettings>();
            settings.Setup(s => s.NextNotification(null)).Returns(dim);
            var rule = new FulfilledTimeLimit(settings.Object);
            var datetime = DateTime.Now;
            var stats = new SpineHero.Model.Notifications.NotificationStatistics
            {
                Evaluation =
                {
                    Current = new Evaluation {EvaluatedAt = datetime},
                    FirstWrong = new Evaluation {EvaluatedAt = datetime.AddMinutes(-50)}
                }
            };

            Expect(rule.Check(stats), Is.True);
        }
    }
}