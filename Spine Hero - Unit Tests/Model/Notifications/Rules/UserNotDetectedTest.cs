using System;
using Moq;
using NUnit.Framework;
using SpineHero.Model.Notifications;
using SpineHero.Model.Notifications.Rules;
using SpineHero.Monitoring.Watchers.Management.Results;

namespace SpineHero.UnitTests.Model.Notifications.Rules
{
    [TestFixture]
    public class UserNotDetectedTest : AssertionHelper
    {
        [Test]
        public void MustBeTwoUnknownPostureInRow()
        {
            var unknown = new Evaluation {Posture = Posture.Unknown};
            var stats1 = new SpineHero.Model.Notifications.NotificationStatistics {
                Evaluation = new EvaluationStatistics {Current = unknown, BeforeCurrent = unknown}};
            var stats2 = new SpineHero.Model.Notifications.NotificationStatistics {
                Evaluation = new EvaluationStatistics { Current = unknown, BeforeCurrent = new Evaluation {Posture = Posture.Wrong}}};
            var rule = new UserNotDetected();

            Expect(rule.IsTwoUnknownPostureInRow(stats1), Is.True);
            Expect(rule.IsTwoUnknownPostureInRow(stats2), Is.False);
        }

        [Test]
        public void MustFulfillTimeLimit()
        {
            var limit = Properties.Notifications.Default.UnknownPostureLimit;
            var now = DateTime.Now;
            var stats1 = new SpineHero.Model.Notifications.NotificationStatistics {
                Evaluation = new EvaluationStatistics {
                    Current = new Evaluation { EvaluatedAt = now},
                    FirstUnknown = new Evaluation { EvaluatedAt = now.AddSeconds(- limit - 1) }}};
            var stats2 = new SpineHero.Model.Notifications.NotificationStatistics {
                Evaluation = new EvaluationStatistics {
                    Current = new Evaluation { EvaluatedAt = now},
                    FirstUnknown = new Evaluation { EvaluatedAt = DateTime.Now.AddSeconds(-limit) }}};
            var stats3 = new SpineHero.Model.Notifications.NotificationStatistics {
                Evaluation = new EvaluationStatistics {
                    Current = new Evaluation { EvaluatedAt = now},
                    FirstUnknown = new Evaluation { EvaluatedAt = DateTime.Now.AddSeconds(- limit + 1) }}};
            var rule = new UserNotDetected();

            Expect(rule.IsFulfilledTimeLimit(stats1), Is.True);
            Expect(rule.IsFulfilledTimeLimit(stats2), Is.True);
            Expect(rule.IsFulfilledTimeLimit(stats3), Is.False);
        }

        [Test]
        public void CorrectMethodsAreCalled()
        {
            var rule = new Mock<UserNotDetected>();
            rule.Setup(r => r.IsFulfilledTimeLimit(It.IsAny<SpineHero.Model.Notifications.NotificationStatistics>())).Returns(true);
            rule.Setup(r => r.IsTwoUnknownPostureInRow(It.IsAny<SpineHero.Model.Notifications.NotificationStatistics>())).Returns(true);

            rule.Object.Check(new SpineHero.Model.Notifications.NotificationStatistics());

            rule.Verify(r => r.IsFulfilledTimeLimit(It.IsAny<SpineHero.Model.Notifications.NotificationStatistics>()), Times.Once);
            rule.Verify(r => r.IsTwoUnknownPostureInRow(It.IsAny<SpineHero.Model.Notifications.NotificationStatistics>()), Times.Once);
        }
    }
}