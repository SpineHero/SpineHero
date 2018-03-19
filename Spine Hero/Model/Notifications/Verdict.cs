using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Caliburn.Micro;
using SpineHero.Model.Notifications.Rules;
using SpineHero.Monitoring.Watchers.Management.Results;
using SpineHero.PostureMonitoring.Managers;

namespace SpineHero.Model.Notifications
{
    public class Verdict : IVerdict, IHandle<PostureMonitoringStatusChange>
    {
        private readonly INotificationsSettings notificationsSettings;
        private readonly ISchedule schedule;

        public Verdict(IEventAggregator eventAggregator, INotificationsSettings notificationsSettings, ISchedule schedule)
        {
            eventAggregator.Subscribe(this);
            this.notificationsSettings = notificationsSettings;
            this.schedule = schedule;
            ShowNotificationRules = new RulesCollection(new List<IRule>
            {
                new Rules.StatisticsAreReady(),
                new Rules.Negation(new NotificationIsActivated(schedule)),
                new Rules.WrongSitting(),
                new Rules.FulfilledTimeLimit(notificationsSettings),
            });
            HideNotificationRules = new RulesCollection(new List<IRule>
            {
                new Rules.StatisticsAreReady(),
                new Rules.NotificationIsActivated(schedule),
                new Rules.CorrectSitting(),
            }, new List<IRule>
            {
                new Rules.StatisticsAreReady(),
                new Rules.NotificationIsActivated(schedule),
                new Rules.UserNotDetected(),
            });
        }

        public RulesCollection ShowNotificationRules { get; set; }

        public RulesCollection HideNotificationRules { get; set; }

        public NotificationStatistics Statistics { get; set; } = new NotificationStatistics();

        public void Consume(BlockingCollection<Evaluation> queue)
        {
            try
            {
                while (!queue.IsCompleted)
                {
                    MakeVerdictFor(queue.Take());
                }
            }
            catch (InvalidOperationException) { } // IOE means that Take() was called on a completed collection.
        }

        public void MakeVerdictFor(Evaluation evaluation)
        {
            notificationsSettings.ReloadIfNecessary();
            Statistics = Statistics.Add(evaluation);

            if (ShowNotificationRules.Check(Statistics))
                Notify();
            else if (HideNotificationRules.Check(Statistics))
                Hide();
        }

        private void Notify()
        {
            var nextNotificationToShow = notificationsSettings.NextNotification(Statistics.LastUsedNotification);
            schedule.ScheduleNotification(nextNotificationToShow);
            Statistics.LastUsedNotification = nextNotificationToShow;
        }

        private void Hide()
        {
            schedule.HideAllNotifications();
            Statistics = new NotificationStatistics();
        }

        public void Handle(PostureMonitoringStatusChange message)
        {
            if (!message.IsMonitoring)
                Statistics = new NotificationStatistics();
        }
    }
}