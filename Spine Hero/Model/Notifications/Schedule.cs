using Caliburn.Micro;
using LiteDB;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using SpineHero.PostureMonitoring.Managers;

namespace SpineHero.Model.Notifications
{
    public class Schedule : PropertyChangedBase, ISchedule, IHandle<PostureMonitoringStatusChange>
    {
        private readonly IEventAggregator eventAggregator;
        private INotification displayedNotification;

        public delegate void NotificationWasHidden();

        public Schedule(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            this.eventAggregator.Subscribe(this);
            Properties.Notifications.Default.PropertyChanged += OnSettingsChanged;
        }

        public INotification DisplayedNotification
        {
            get { return displayedNotification; }
            set
            {
                displayedNotification = value;
                NotifyOfPropertyChange();
            }
        }

        public void ScheduleNotification(INotification notification)
        {
            // Tu sa zapíšem na User Activity Monitoring a počkám na vhodný čas
            // Dokym nie je Activity Monitoring implementovane, ihned zobrazim notifikaciu
            DisplayedNotification = notification;
            notification.Show(DisplayedNotificationWasHiddenCallback);
            eventAggregator.PublishOnUIThread(new NotificationShownEvent(notification.GetType(), DateTime.Now));
        }

        public void DisplayedNotificationWasHiddenCallback()
        {
            DisplayedNotification = null;
        }

        public bool IsDisplayedNotification()
        {
            return DisplayedNotification != null;
        }

        public bool IsScheduledOrDisplayed()
        {
            return IsDisplayedNotification();
        }

        public void HideAllNotifications()
        {
            DisplayedNotification?.Hide();
        }

        private void OnSettingsChanged(object sender, PropertyChangedEventArgs args)
        {
            var notifications = new List<string> {"PopupNotification", "DimNotification"};
            if (notifications.Contains(args.PropertyName) && Properties.Notifications.Default[args.PropertyName].Equals(false))
                HideAllNotifications();
        }

        public void Handle(PostureMonitoringStatusChange message)
        {
            if (!message.IsMonitoring)
                HideAllNotifications();
        }
    }

    public class NotificationShownEvent
    {
        public NotificationShownEvent()
        {
        }

        public NotificationShownEvent(string notificationType, DateTime shownAt)
        {
            NotificationType = notificationType;
            ShownAt = shownAt;
        }

        public NotificationShownEvent(Type notificationType, DateTime shownAt)
        {
            NotificationType = notificationType.Name;
            ShownAt = shownAt;
        }

        [BsonId(true)]
        public int Id { get; set; }

        public string NotificationType { get; set; }

        public DateTime ShownAt { get; set; }
    }
}