using System.Linq;
using Caliburn.Micro;
using Moq;
using NUnit.Framework;
using SpineHero.Model.Notifications;
using SpineHero.Model.Notifications.DimNotification;
using SpineHero.Model.Notifications.PopupNotification;
using SpineHero.Model.Notifications.Rules;
using SpineHero.Monitoring.Watchers.Management.Results;

namespace SpineHero.UnitTests.Model.Notifications.NotificationPlanner
{
    [TestFixture]
    public class VerdictTest : AssertionHelper
    {
        private Mock<ISchedule> notificationManagerMock;
        private SpineHero.Model.Notifications.NotificationsSettings notificationSettings;
        private Verdict verdict;

        [SetUp]
        public void SetUp()
        {
            notificationManagerMock = new Mock<ISchedule>();
            var popup = new Mock<IPopupNotification>();
            var dim = new Mock<IDimNotification>();
            var ea = new Mock<IEventAggregator>();
            notificationSettings = new SpineHero.Model.Notifications.NotificationsSettings(popup.Object, dim.Object);
            verdict = new Verdict(ea.Object, notificationSettings, notificationManagerMock.Object);
        }

        [Test]
        public void CreatesShowNotificationRulesList()
        {
            Expect(verdict.ShowNotificationRules, Not.Null);
            Expect(verdict.ShowNotificationRules.ListsOfRules.First(), Not.Empty);
        }

        [Test]
        public void CreatesHideNotificationRulesList()
        {
            Expect(verdict.HideNotificationRules, Not.Null);
            Expect(verdict.HideNotificationRules.ListsOfRules.First(), Not.Empty);
        }

        [Test]
        public void ChecksRulesWhenProcessingEvaluation()
        {
            var rules = new Mock<RulesCollection>();
            var stats = new NotificationStatistics {Evaluation = {Current = new Evaluation(), BeforeCurrent = new Evaluation()}};
            rules.Setup(r => r.Check(It.IsAny<NotificationStatistics>())).Returns(false);
            verdict.ShowNotificationRules = rules.Object;
            verdict.HideNotificationRules = rules.Object;
            verdict.Statistics = stats;

            verdict.MakeVerdictFor(new Evaluation());

            rules.Verify(r => r.Check(It.IsAny<NotificationStatistics>()), Times.Exactly(2));
        }
    }
}