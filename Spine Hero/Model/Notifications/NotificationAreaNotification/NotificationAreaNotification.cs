using Caliburn.Micro;
using SpineHero.Common.Resources;
using SpineHero.PostureMonitoring.Managers;
using System;
using SpineHero.Monitoring.Watchers.Management.Results;

namespace SpineHero.Model.Notifications.NotificationAreaNotification
{
    public class NotificationAreaNotification : PropertyChangedBase, INotificationAreaNotification, IHandle<Evaluation>, IHandle<PostureMonitoringStatusChange>
    {
        private string selectedIcon;
        private string startStopMonitoringText;
        private string selectedToolTipText;
        public static readonly string CorrectIcon = ResourceHelper.GetResourcePath(@"Resources/Icons/CorrectIcon.ico");
        public static readonly string WarningIcon = ResourceHelper.GetResourcePath(@"Resources/Icons/WarningIcon.ico");
        public static readonly string WrongIcon = ResourceHelper.GetResourcePath(@"Resources/Icons/WrongIcon.ico");
        public static readonly string DefaultIcon = ResourceHelper.GetResourcePath(@"Resources/Icons/DefaultIcon.ico");

        public NotificationAreaNotification(IEventAggregator eventAggregator)
        {
            eventAggregator.Subscribe(this);
            ResetToDefault();
            ChangeStartStopMonitoringText(false);
        }

        public string SelectedIcon
        {
            get { return selectedIcon; }
            set
            {
                selectedIcon = value;
                NotifyOfPropertyChange(() => SelectedIcon);
            }
        }

        public string StartStopMonitoringText
        {
            get { return startStopMonitoringText; }
            set
            {
                startStopMonitoringText = value;
                NotifyOfPropertyChange(() => StartStopMonitoringText);
            }
        }

        public string SelectedToolTipText
        {
            get { return selectedToolTipText; }
            set
            {
                selectedToolTipText = value;
                NotifyOfPropertyChange(() => SelectedToolTipText);
            }
        }

        private void ResetToDefault()
        {
            SelectedToolTipText = "Spine Hero";
            SelectedIcon = DefaultIcon;
        }

        private void ChangeStartStopMonitoringText(bool isMonitoring)
        {
            StartStopMonitoringText = isMonitoring ? "Stop monitoring" : "Start monitoring";
        }

        public void ShowNotification(int sittingQuality)
        {
            SelectedIcon = ChooseProperIcon(sittingQuality);
            SelectedToolTipText = ComposeProperMessage(sittingQuality);
        }

        private string ChooseProperIcon(int sittingQuality)
        {
            var level = SittingQualityLevelHelper.GetLevel(sittingQuality);

            if (level == SittingQualityLevel.Correct)
                return CorrectIcon;
            if (level == SittingQualityLevel.Warning)
                return WarningIcon;
            return WrongIcon;
        }

        private string ComposeProperMessage(int sittingQuality)
        {
            var level = SittingQualityLevelHelper.GetLevel(sittingQuality);

            if (level == SittingQualityLevel.Correct)
                return String.Format("{0}\nPercent: {1}", "You are sitting correct!", sittingQuality);
            if (level == SittingQualityLevel.Warning)
                return String.Format("{0}\nPercent: {1}", "You can sit better!", sittingQuality);
            return String.Format("{0}\nPercent: {1}", "You are sitting WRONG!", sittingQuality);
        }

        public void Handle(PostureMonitoringStatusChange message)
        {
            ChangeStartStopMonitoringText(message.IsMonitoring);
            if (message.IsMonitoring == false)
                ResetToDefault();
        }

        public void Handle(Evaluation message)
        {
            if (message.Posture == Posture.Unknown) ResetToDefault();
            else ShowNotification(message.SittingQuality);
        }
    }
}