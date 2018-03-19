using System.ComponentModel;
using System.Threading.Tasks;
using Caliburn.Micro;
using SpineHero.Model.Notifications;
using SpineHero.Properties;

namespace SpineHero.PostureMonitoring
{
    public class AnalyzePeriodManager
    {
        private readonly IEventAggregator eventAggregator;
        private readonly ISchedule notificationsSchedule;

        public AnalyzePeriodManager(IEventAggregator eventAggregator, ISchedule notificationsSchedule)
        {
            this.eventAggregator = eventAggregator;
            this.notificationsSchedule = notificationsSchedule;
            Settings.Default.PropertyChanged += OnSettingsChanged;
            notificationsSchedule.PropertyChanged += OnDisplayedNotificationChanged;
        }

        private void OnSettingsChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == nameof(Settings.Default.AnalyzePeriod) && !notificationsSchedule.IsDisplayedNotification())
                PublishNewTime(Settings.Default.AnalyzePeriod);
        }

        private void OnDisplayedNotificationChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == nameof(ISchedule.DisplayedNotification))
            {
                if (notificationsSchedule.IsDisplayedNotification())
                    PublishNewTime(Settings.Default.AnalyzePeriodWithNotification);
                else
                    PublishNewTime(Settings.Default.AnalyzePeriod);
            }
        }

        private void PublishNewTime(int analyzePeriod)
        {
            eventAggregator.Publish(new AnalyzePeriodChange(analyzePeriod), action => Task.Factory.StartNew(action));
        }
    }
}