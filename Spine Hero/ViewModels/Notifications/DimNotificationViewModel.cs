using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;
using Caliburn.Micro;
using SpineHero.Model.Notifications;
using SpineHero.Utils.Logging;
using SpineHero.Utils.WinAPI;
using SpineHero.Views.Notifications;
using WindowStyle = SpineHero.Utils.WinAPI.WindowStyle;

namespace SpineHero.ViewModels.Notifications
{
    public sealed class DimNotificationViewModel : Screen
    {
        private static readonly ILogger Logger = Utils.Logging.Logger.GetLogger<DimNotificationViewModel>();
        private readonly Rect screen;
        private readonly object closeWindowLocker = new object();
        private bool windowClosed;

        public DimNotificationViewModel(Rect screen, GlobalHotkey hotkey)
        {
            Hotkey = hotkey;
            this.screen = screen;
        }

        public Schedule.NotificationWasHidden NotificationWasHidden;

        public double PositionTop
        {
            get { return screen.Top; }
            set { } // Binding for Window.Top property doesn´t work unless the binding is in TwoWay mode
        }

        public double PositionLeft => screen.Left;

        public double Width => screen.Width;

        public double Height
        {
            get { return screen.Height; }
            set { } // Binding for Window.Height property doesn´t work unless the binding is in TwoWay mode
        }

        public GlobalHotkey Hotkey { get; set; }

        public async void SetupWindow()
        {
            DimNotificationView window = null;
            await Execute.OnUIThreadAsync(() => window = (DimNotificationView) GetView());

            try
            {
                new WindowStyle(window).ApplyClickThrough();
            }
            catch (WindowStyleException e)
            {
                Logger.Error(e);
                MessageBox.Show(
                    $"Problem with window configuration. Info: {e.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            var animation = await SetupFadeInAnimation();
            await Execute.OnUIThreadAsync(() => window.WindowGrid.BeginAnimation(UIElement.OpacityProperty, animation));
        }

        public async Task<DoubleAnimation> SetupFadeInAnimation()
        {
            DoubleAnimation animation = null;
            await Execute.OnUIThreadAsync(() =>
            {
                animation = new DoubleAnimation
                {
                    From = 0,
                    To = 1,
                    Duration = new Duration(Properties.Notifications.Default.DimNotificationFadeIn)
                };
            });
            return animation;
        }

        public async Task<DoubleAnimation> SetupFadeOutAnimation(DimNotificationView window)
        {
            DoubleAnimation animation = null;
            await Execute.OnUIThreadAsync(() =>
            {
                animation = new DoubleAnimation
                {
                    From = window.WindowGrid.Opacity,
                    To = 0,
                    Duration = new Duration(Properties.Notifications.Default.DimNotificationFadeOut)
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

        public async void FadeOutAndCloseWindow()
        {
            lock (closeWindowLocker)
            {
                if (windowClosed)
                    return;
                windowClosed = true;
            }
            var window = (DimNotificationView)GetView();
            var animation = await SetupFadeOutAnimation(window);
            await Execute.OnUIThreadAsync(() => window.WindowGrid.BeginAnimation(UIElement.OpacityProperty, animation));
        }
    }
}