using Caliburn.Micro;
using OpenCvSharp.Extensions;
using SpineHero.Common.Logging;
using SpineHero.Monitoring.DataSources;
using SpineHero.PostureMonitoring.Managers;
using SpineHero.Properties;
using SpineHero.Utils.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using OpenCvSharp;
using Timer = System.Timers.Timer;
using Window = System.Windows.Window;
using System.Resources;
using System.Reflection;

namespace SpineHero.ViewModels.DialogWindows
{
    public class CalibrationWindowViewModel : Screen
    {
        private readonly ILogger log = Logger.GetLogger<CalibrationWindowViewModel>();
        private readonly object locker = new object();
        private readonly IEventAggregator eventAggregator;
        private readonly IDataSourceManager dataSourceManager;
        private readonly IWindowManager windowManager;
        private ImageWrapper calibration;
        private string calibrateButtonText;
        private Visibility textVisibility;
        private Visibility buttonVisibility;
        private Brush borderColor;
        private bool stopds;
        private Timer timer;
        private ImageSource calibrationImageSource;
        private bool closed = false;
        private int frame = 0;
        private ResourceManager translation;

        public CalibrationWindowViewModel(IEventAggregator aggregator, IDataSourceManager dsManager, IWindowManager windowManager)
        {
            eventAggregator = aggregator;
            dataSourceManager = dsManager;
            this.windowManager = windowManager;
            translation = new ResourceManager("SpineHero.Views.Translation", Assembly.GetExecutingAssembly());
            DisplayName = "Calibration";
        }

        public string CalibrateButtonText
        {
            get { return calibrateButtonText; }
            set
            {
                calibrateButtonText = value;
                NotifyOfPropertyChange(() => CalibrateButtonText);
            }
        }

        public Visibility TextVisibility
        {
            get { return textVisibility; }
            set
            {
                textVisibility = value;
                NotifyOfPropertyChange(() => TextVisibility);
            }
        }

        public Visibility ButtonVisibility
        {
            get { return buttonVisibility; }
            set
            {
                buttonVisibility = value;
                NotifyOfPropertyChange(() => ButtonVisibility);
            }
        }

        public Brush BorderColor
        {
            get { return borderColor; }
            set
            {
                borderColor = value;
                NotifyOfPropertyChange(() => BorderColor);
            }
        }

        public ImageSource CalibrationImageSource
        {
            get { return calibrationImageSource; }
            set
            {
                calibrationImageSource = value;
                CanCalibrate = value != null;
                NotifyOfPropertyChange(() => CalibrationImageSource);
            }
        }

        public async void Initialize(EventArgs e)
        {
            SetToDefault();
            calibration = null;
            CalibrationImageSource = null;
            closed = false;
            timer = new Timer(Const.Default.CalibrationPeriod);
            timer.Elapsed += delegate { Show(); };

            if (dataSourceManager.Running)
            {
                stopds = false;
                Calibrate();
            }
            else
            {
                try
                {
                    stopds = true;
                    await Task.Factory.StartNew(() => dataSourceManager.StartDataSource(Settings.Default.UseDepthCamera));
                    if (closed) return;
                    Calibrate();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    log.Error(ex);
                    TryClose();
                }
            }
        }

        private void SetToDefault()
        {
            BorderColor = new SolidColorBrush(Colors.Transparent);
            ButtonVisibility = Visibility.Hidden;
            TextVisibility = Visibility.Hidden;
            CalibrateButtonText = translation.GetString("Set");
            calibration = null;
        }

        public void CloseWindow()
        {
            TryClose();
        }

        public void OnClosed(EventArgs e)
        {
            closed = true;
            timer.Stop();
            timer.Dispose();
            if (stopds)
            {
                Task.Factory.StartNew(() => dataSourceManager.StopDataSource());
            }
            if (calibration != null)
            {
                eventAggregator.PublishOnUIThread(calibration);
            }
        }

        #region Setting calibration

        private bool canCalibrate;

        public bool CanCalibrate
        {
            get { return canCalibrate; }
            set
            {
                if (canCalibrate == value) return;
                canCalibrate = value;
                NotifyOfPropertyChange(() => CanCalibrate);
            }
        }

        public void Calibrate()
        {
            lock (locker)
            {
                if (!timer.Enabled)
                {
                    SetToDefault();
                    timer.Start();
                }
                else
                {
                    timer.Stop();
                    CalibrateButtonText = translation.GetString("TryAgain");
                    SetCalibration();
                }
            }
        }

        [LogMethodCall("Trying to set calibration.")]
        private void SetCalibration()
        {
            var images = dataSourceManager.Images;
            if (images?.Head != null)
            {
                DrawRectangleAroundHead(images);
                BorderColor = new SolidColorBrush(Color.FromRgb(160, 232, 56));
                ButtonVisibility = Visibility.Visible;
                TextVisibility = Visibility.Hidden;
                calibration?.Dispose();
                calibration = images;
            }
            else
            {
                BorderColor = new SolidColorBrush(Colors.Red);
                ButtonVisibility = Visibility.Hidden;
                TextVisibility = Visibility.Visible;
            }
        }

        private void DrawRectangleAroundHead(ImageWrapper imageWrapper)
        {
            var imageWithHead = imageWrapper.ColorImage.Clone();
            imageWithHead.Rectangle(imageWrapper.Head.Value, Scalar.Blue);
            CalibrationImageSource = imageWithHead.ToBitmapSource();
        }

        #endregion Setting calibration

        private void Show()
        {
            ImageWrapper images;
            if (Monitor.TryEnter(locker))
            {
                try
                {
                    if (!timer.Enabled) return;
                    images = dataSourceManager.LoadNext();
                    if (images?.ColorImage == null || images.ColorImage.IsDisposed) return;
                }
                finally
                {
                    Monitor.Exit(locker);
                }
            }
            else return;

            ((Window) GetView())?.Dispatcher?.Invoke(delegate
            {
                lock (locker)
                {
                    if (!timer.Enabled) return;
                    CalibrationImageSource = images.ColorImage.ToBitmapSource();
                }
                if (frame == Const.Default.CollectPeriod)
                {
                    frame = 0;
                    GC.Collect();
                }
                frame++;
            });
        }

        public void HowToSitCorrectly()
        {
            log.LogEvent("How to sit correctly - from calibration window");
            windowManager.ShowDialog(new HowToSitCorrectlyViewModel());
        }
    }
}