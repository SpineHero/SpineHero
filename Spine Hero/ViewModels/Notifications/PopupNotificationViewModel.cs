using Caliburn.Micro;
using SpineHero.Model.Notifications;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;
using SpineHero.Model.Statistics;
using SpineHero.Utils.Extentions;
using SpineHero.Monitoring.Watchers.Management.Results;

namespace SpineHero.ViewModels.Notifications
{
    public class PopupNotificationViewModel : Screen, IHandle<Posture>
    {
        private const int WindowWidthWithBorder = 230;
        private const int WindowHeightWithBorder = 120;
        private readonly StatisticsModule statisticsModule;
        private readonly object closeWindowLocker = new object();
        private bool windowClosed = true;

        public PopupNotificationViewModel(IEventAggregator eventAggregator, StatisticsModule statisticsModule)
        {
            this.statisticsModule = statisticsModule;
            eventAggregator.Subscribe(this);
        }

        public string CurrentPostureImagePath => statisticsModule.LastPosture.GetImageRepresentation();

        public int PositionLeft => (int)(SystemParameters.PrimaryScreenWidth - WindowWidthWithBorder);

        public int PositionTop
        {
            get { return (int)(SystemParameters.PrimaryScreenHeight - WindowHeightWithBorder); }
            set { } // It´s sad, but binding for Window.Top property doesn´t work unless the binding is in TwoWay mode
        }

        public Schedule.NotificationWasHidden NotificationWasHidden;

        public async void AutomaticallyCloseWindow()
        {
            lock (closeWindowLocker)
            {
                windowClosed = false;
            }
            await Task.Delay(TimeSpan.FromSeconds(Properties.Notifications.Default.PopupNotificationDisplayInterval));
            FadeOutAndCloseWindow();
        }

        public async void FadeOutAndCloseWindow()
        {
            lock (closeWindowLocker)
            {
                if (windowClosed)
                    return;
                windowClosed = true;
            }
            var window = (Window)GetView();
            var animation = await SetupFadeOutAnimation();

            await Execute.OnUIThreadAsync(() =>
            {
                // We must remove all trigers, because they could start another animation and thus break our animation.
                window.Triggers.Clear();
                window.BeginAnimation(UIElement.OpacityProperty, animation);
            });
        }

        private async Task<DoubleAnimation> SetupFadeOutAnimation()
        {
            DoubleAnimation animation = null;
            await Execute.OnUIThreadAsync(() =>
            {
                animation = new DoubleAnimation
                {
                    From = 0.9,
                    To = 0,
                    Duration = new Duration(TimeSpan.FromMilliseconds(Properties.Notifications.Default.PopupNotificationFadeOutMiliseconds))
                };
                animation.Completed += (sender, args) => { CloseWindow(); };
            });
            return animation;
        }

        private void CloseWindow()
        {
            TryClose();
            NotificationWasHidden();
        }

        public void Handle(Posture message)
        {
            NotifyOfPropertyChange(() => CurrentPostureImagePath);
        }
    }
}