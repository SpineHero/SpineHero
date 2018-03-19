using Caliburn.Micro;
using NUnit.Framework;
using SpineHero.Model.Notifications.NotificationAreaNotification;
using SpineHero.Monitoring.Watchers.Management.Results;
using SpineHero.PostureMonitoring;
using SpineHero.PostureMonitoring.Managers;

namespace SpineHero.UnitTests.Model.Notifications
{
    [TestFixture]
    public class NotificationAreaNotificationTest : AssertionHelper
    {
        private readonly IEventAggregator eventAggregator;
        private INotificationAreaNotification notificationAreaNotification;

        public NotificationAreaNotificationTest()
        {
            eventAggregator = new EventAggregator();
        }

        [SetUp]
        public void Init()
        {
            notificationAreaNotification = new NotificationAreaNotification(eventAggregator);
        }

        [Test]
        public void HasDefaultState()
        {
            Expect(notificationAreaNotification.SelectedIcon, EqualTo(NotificationAreaNotification.DefaultIcon));
            Expect(notificationAreaNotification.SelectedToolTipText, EqualTo("Spine Hero"));
        }

        [Test]
        public void HandleEvaluationAndChangeIconAndTooltip()
        {
            // Correct
            var evaluation = new Evaluation(70);
            eventAggregator.PublishOnCurrentThread(evaluation);

            Expect(notificationAreaNotification.SelectedIcon, EqualTo(NotificationAreaNotification.CorrectIcon), "Correct Limit icon");
            Expect(notificationAreaNotification.SelectedToolTipText, EqualTo("You are sitting correct!\nPercent: 70"), "Correct Limit");

            // Warning
            evaluation = new Evaluation(40);
            eventAggregator.PublishOnCurrentThread(evaluation);

            Expect(notificationAreaNotification.SelectedIcon, EqualTo(NotificationAreaNotification.WarningIcon), "Warning Limit icon");
            Expect(notificationAreaNotification.SelectedToolTipText, EqualTo("You can sit better!\nPercent: 40"), "Warning Limit");

            // Wrong
            evaluation = new Evaluation(39);
            eventAggregator.PublishOnCurrentThread(evaluation);

            Expect(notificationAreaNotification.SelectedIcon, EqualTo(NotificationAreaNotification.WrongIcon), "Wrong Limit icon");
            Expect(notificationAreaNotification.SelectedToolTipText, EqualTo("You are sitting WRONG!\nPercent: 39"), "Wrong Limit");
        }

        [Test]
        public void ReturnsToDefaultStateWhenMonitoringStops()
        {
            eventAggregator.PublishOnCurrentThread(new Evaluation(0));
            Expect(notificationAreaNotification.SelectedIcon, EqualTo(NotificationAreaNotification.WrongIcon), "Wrong Limit icon");

            eventAggregator.PublishOnCurrentThread(new PostureMonitoringStatusChange(false));
            HasDefaultState();
        }
    }
}