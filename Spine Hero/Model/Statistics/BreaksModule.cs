using Caliburn.Micro;
using SpineHero.Model.Store;
using SpineHero.Monitoring.Watchers.Management.Results;
using SpineHero.PostureMonitoring.Managers;
using System;

namespace SpineHero.Model.Statistics
{
    public class BreaksModule : IHandle<Evaluation>, IHandle<PostureMonitoringStatusChange>
    {
        private PostureTime firstUnknown;
        private PostureTime lastUnknown;
        private PostureTime firstSitting;
        private PostureTime lastSitting;

        private TimeSpan breakLength;
        private TimeSpan timeLimit;
        private uint timeBetweenNotifications;
        private bool enabled;

        private uint notificationShownCount = 0;

        public BreaksModule(IEventAggregator aggregator)
        {
            aggregator.Subscribe(this);
            var settings = Properties.Notifications.Default;
            settings.PropertyChanged += BreakNotificationPropertyChanged;
            breakLength = settings.BreakNotificationBreakLength;
            timeLimit = TimeSpan.FromMinutes(settings.BreakNotificationTimeLimit);
            timeBetweenNotifications = settings.BreakNotificationTimeLimit / 2;
            enabled = settings.BreakNotification;
        }

        public DateTime SittingStart => firstSitting?.StartAt ?? DateTime.MinValue;

        public bool SittingWithoutBreakForTooLong()
        {
            var result = firstSitting != null && lastSitting.StartAt - firstSitting.StartAt >= timeLimit + TimeSpan.FromMinutes(notificationShownCount * timeBetweenNotifications);
            if (result) notificationShownCount++;
            return result;
        }

        private void BreakNotificationPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var settings = (Properties.Notifications)sender;
            switch (e.PropertyName)
            {
                case nameof(settings.BreakNotificationBreakLength):
                    breakLength = settings.BreakNotificationBreakLength;
                    break;

                case nameof(settings.BreakNotificationTimeLimit):
                    timeLimit = TimeSpan.FromMinutes(settings.BreakNotificationTimeLimit);
                    timeBetweenNotifications = settings.BreakNotificationTimeLimit / 2;
                    break;

                case nameof(settings.BreakNotification):
                    enabled = settings.BreakNotification;
                    break;
            }
        }

        public void Handle(Evaluation message)
        {
            if (!enabled) return;
            if (firstSitting != null && message.Posture == Posture.Unknown)
            {
                if (firstUnknown == null) firstUnknown = new PostureTime(message.Posture, message.EvaluatedAt);
                lastUnknown = new PostureTime(message.Posture, message.EvaluatedAt);
                if (lastUnknown.StartAt - firstUnknown.StartAt >= breakLength)
                {
                    Reset();
                }
            }
            else if (message.Posture != Posture.Unknown)
            {
                if (firstSitting == null) firstSitting = new PostureTime(message.Posture, message.EvaluatedAt);
                lastSitting = new PostureTime(message.Posture, message.EvaluatedAt);
                if (lastSitting.StartAt - firstSitting.StartAt >= breakLength)
                {
                    firstUnknown = null;
                    lastUnknown = null;
                }
            }
        }

        public void Handle(PostureMonitoringStatusChange message)
        {
            if (!message.IsMonitoring)
            {
                Reset();
            }
        }

        private void Reset()
        {
            firstUnknown = null;
            lastUnknown = null;
            firstSitting = null;
            lastSitting = null;
            notificationShownCount = 0;
        }
    }
}