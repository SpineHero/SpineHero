using Caliburn.Micro;
using SpineHero.Model.Notifications;
using SpineHero.Model.Statistics;
using SpineHero.Model.Store;
using System;
using System.ComponentModel;
using System.Linq;
using System.Timers;
using SpineHero.Common.Logging;
using Statistics = SpineHero.Model.Store.Statistics;
using System.Resources;
using System.Reflection;
using Infralution.Localization.Wpf;

namespace SpineHero.ViewModels.MainMenuItems
{
    public sealed class StatisticsViewModel : Screen, IMainMenuItem, IHandle<NotificationShownEvent>
    {
        private readonly Timer timer = new Timer(DateTime.Today.AddDays(1).Subtract(DateTime.Now).TotalMilliseconds);
        private readonly DatabaseQuery databaseQuery;
        private readonly Statistics statistics;
        private int notificationShownCount;

        public StatisticsViewModel(IEventAggregator ea, Statistics stats, DatabaseQuery dq)
        {
            ResourceManager rm = new ResourceManager("SpineHero.Views.Translation", Assembly.GetExecutingAssembly());
            DisplayName = rm.GetString("Statistics");

            ea.Subscribe(this);
            statistics = stats;
            databaseQuery = dq;

            timer.Elapsed += TimerElapsed;
            timer.AutoReset = true;
            CultureManager.UICultureChanged += (o, e) => { DisplayName = rm.GetString("Statistics"); };
        }

        public TimeSpan TodaySittingTime { get; private set; }

        public int TodayCorrectSitting { get; private set; }

        public int TodayChallangeSuccess { get; private set; } = 50; // TODO

        public int NotificationShownCount
        {
            get
            {
                return notificationShownCount;
            }
            private set
            {
                notificationShownCount = value;
                NotifyOfPropertyChange();
            }
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            timer.Interval = DateTime.Today.AddDays(1).Subtract(DateTime.Now).TotalMilliseconds;
            NotificationShownCount = databaseQuery.QueryNotificationShownCount(DateTime.Today, DateTime.Today.AddDays(1));
        }

        [LogMethodCall]
        private void UpdateSittingTimeStatistics()
        {
            var data = databaseQuery.QueryHistoryData(DateTime.Today, DateRange.Day);
            statistics.UpdateCurrentBucketInHistoryData(data);

            var total = data.Sum(x => x.Total);
            TodaySittingTime = TimeSpan.FromMilliseconds(total);
            var correct = data.Sum(x => x.Correct);
            TodayCorrectSitting = (total == 0) ? 0 : (int)(correct * 100 / total);

            NotifyOfPropertyChange(() => TodaySittingTime);
            NotifyOfPropertyChange(() => TodayCorrectSitting);
        }

        private void StatisticsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(statistics.CurrentBucket))
                UpdateSittingTimeStatistics();
        }

        protected override void OnActivate()
        {
            base.OnActivate();
            statistics.PropertyChanged += StatisticsPropertyChanged;
            timer.Interval = DateTime.Today.AddDays(1).Subtract(DateTime.Now).TotalMilliseconds;
            timer.Start();
            NotificationShownCount = databaseQuery.QueryNotificationShownCount(DateTime.Today, DateTime.Today.AddDays(1));
            UpdateSittingTimeStatistics();
        }

        protected override void OnDeactivate(bool close)
        {
            base.OnDeactivate(close);
            statistics.PropertyChanged -= StatisticsPropertyChanged;
            timer.Stop();
        }

        public void Handle(NotificationShownEvent message)
        {
            if (!IsActive) return;
            NotificationShownCount++;
        }
    }
}