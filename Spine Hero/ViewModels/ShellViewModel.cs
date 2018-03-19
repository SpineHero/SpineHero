using Caliburn.Micro;
using Hardcodet.Wpf.TaskbarNotification;
using Newtonsoft.Json;
using SpineHero.Common.Resources;
using SpineHero.Model;
using SpineHero.Monitoring.DataSources;
using SpineHero.Monitoring.Watchers.Management.Results;
using SpineHero.PostureMonitoring.Managers;
using SpineHero.Utils.CloudStorage;
using SpineHero.Utils.Logging;
using SpineHero.ViewModels.MainMenuItems;
using SpineHero.ViewModels.Notifications;
using SpineHero.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using SpineHero.Views.Notifications;
using System.Resources;
using System.Reflection;
using Infralution.Localization.Wpf;

namespace SpineHero.ViewModels
{
    public class ShellViewModel : Conductor<IScreen>, IShell, IHandle<PostureMonitoringStatusChange>
    {
        private const string reportBtnDefaultText = "Report\nmistake";
        private readonly ILogger log = Logger.GetLogger<ShellViewModel>();
        private readonly IWindowManager windowManager;
        private readonly IPostureMonitoringManager postureAnalysis;
        private readonly CalibrationManager calibrationManager;
        private readonly IEnumerable<IMainMenuItem> menuItems;

        private Type lastActiveType;
        private string startStopButtonImagePath;
        private string startStopMonitoringButtonText;
        private bool canStartStopMonitoring = true;
        private bool isMonitoring;
        private DateTime startMonitoringDateTime;
        private string reportBtnText;
        private ResourceManager translation;

        public ShellViewModel(IEventAggregator ea, IWindowManager wm, IPostureMonitoringManager pam, IEnumerable<IMainMenuItem> items, NotificationAreaIconViewModel naiv, CalibrationManager calibMa)
        {
            ea.Subscribe(this);
            windowManager = wm;
            postureAnalysis = pam;
            menuItems = items;
            NotificationAreaIconViewModel = naiv;
            calibrationManager = calibMa;
            translation = new ResourceManager("SpineHero.Views.Translation", Assembly.GetExecutingAssembly());

            DisplayName = "Spine Hero";
            ChangeStartStopMonitoringIconAndText(false);
            ReportBtnText = reportBtnDefaultText;
            ShowDashboard();

            CultureManager.UICultureChanged += (o, e) =>
            {
                if (isMonitoring)
                    StartStopMonitoringButtonText = translation.GetString("Pause");
                else
                    StartStopMonitoringButtonText = translation.GetString("Start");
            };
        }

        public NotificationAreaIconViewModel NotificationAreaIconViewModel { get; private set; }

        public string StartStopButtonImagePath
        {
            get { return startStopButtonImagePath; }
            private set
            {
                startStopButtonImagePath = value;
                NotifyOfPropertyChange(() => StartStopButtonImagePath);
            }
        }

        public string StartStopMonitoringButtonText
        {
            get { return startStopMonitoringButtonText; }
            set
            {
                startStopMonitoringButtonText = value;
                NotifyOfPropertyChange(() => StartStopMonitoringButtonText);
            }
        }

        public string ReportBtnText
        {
            get { return reportBtnText; }
            set
            {
                reportBtnText = value;
                NotifyOfPropertyChange(() => ReportBtnText);
            }
        }

        private void ChangeStartStopMonitoringIconAndText(bool value)
        {
            if (value)
            {
                StartStopButtonImagePath = ResourceHelper.GetResourcePath("Resources/Images/Buttons/PauseButton.png");
                StartStopMonitoringButtonText = translation.GetString("Pause");
            }
            else
            {
                StartStopButtonImagePath = ResourceHelper.GetResourcePath("Resources/Images/Buttons/PlayButton.png");
                StartStopMonitoringButtonText = translation.GetString("Start");
            }
        }

        public void ShowDashboard()
        {
            var dashboard = menuItems.FirstOrDefault(w => w is DashboardViewModel);
            ActivateItem(dashboard);
            log.LogPageView(dashboard.DisplayName);
        }

        public void ShowStatistics()
        {
            var statistics = menuItems.FirstOrDefault(w => w is StatisticsViewModel);
            ActivateItem(statistics);
            log.LogPageView(statistics.DisplayName);
        }

        public void ShowHistoryStatistics()
        {
            var statisticsHistory = menuItems.FirstOrDefault(w => w is HistoryStatisticsViewModel);
            ActivateItem(statisticsHistory);
            log.LogPageView(statisticsHistory.DisplayName);
        }

        public void ShowSettings()
        {
            var settings = menuItems.FirstOrDefault(w => w is SettingsViewModel);
            ActivateItem(settings);
            log.LogPageView(settings.DisplayName);
        }

        public void ShowFeedback()
        {
            var feedback = menuItems.FirstOrDefault(w => w is FeedbackViewModel);
            ActivateItem(feedback);
            log.LogPageView(feedback.DisplayName);
        }

        [Conditional("DEBUG")]
        public void ShowDebug()
        {
            var debug = menuItems.FirstOrDefault(w => w is DebugViewModel);
            ActivateItem(debug);
        }

        public void MinimizeWindow()
        {
            if (!Properties.Settings.Default.WasMinimalized)
            {
                string title = "Spine Hero";
                string text = "is now minimized in the notification area";

                ShowNotification(title, text, BalloonIcon.Info);
                Properties.Settings.Default.WasMinimalized = true;
            }

            Application.Current.MainWindow.WindowState = WindowState.Minimized;
            Application.Current.MainWindow.ShowInTaskbar = false;
        }

        public void ShowNotification(string title, string text, BalloonIcon icon)
        {           
            var win = (ShellView)GetView();
            win.TaskbarIcon.ShowBalloonTip(title, text, icon);
        }

        public void ShowNotification(string title, string text, PopupAnimation animation = PopupAnimation.Fade,  int? timeout = 10000)
        {
            var win = (ShellView)GetView();
            var popup = new GenericPopupView(title, text);
            win.TaskbarIcon.ShowCustomBalloon(popup, animation, timeout);
        }

        public void ShowWindow()
        {
            Application.Current.MainWindow.WindowState = WindowState.Normal;
            Application.Current.MainWindow.ShowInTaskbar = true;
            Application.Current.MainWindow.Activate();
        }

        public void ExitApplication()
        {
            Application.Current.Shutdown();
        }

        public bool CanStartStopMonitoring
        {
            get { return canStartStopMonitoring; }
            private set
            {
                canStartStopMonitoring = value;
                NotifyOfPropertyChange(() => CanStartStopMonitoring);
            }
        }

        public async void StartStopMonitoring()
        {
            isMonitoring = postureAnalysis.IsMonitoring;

            CanStartStopMonitoring = false;
            try
            {
                if (!isMonitoring)
                {
                    ChangeStartStopMonitoringIconAndText(true);
                    await Task.Run(() => postureAnalysis.StartDataSource());
                    if (!calibrationManager.IsCalibrated)
                    {
                        MessageBoxResult result = MessageBox.Show(
                            translation.GetString("CalibrationMsg"), translation.GetString("CalibrationTitle"), MessageBoxButton.OKCancel
                         );
                        if (result == MessageBoxResult.OK)
                        {
                            var svm = (SettingsViewModel)menuItems.FirstOrDefault(w => w is SettingsViewModel);
                            svm.Recalibrate(); ;
                        }
                    }
                    if (!calibrationManager.IsCalibrated)
                    {
                        MessageBox.Show(translation.GetString("NoCalibrationMsg"), translation.GetString("NoCalibrationTitle"));
                        await Task.Run(() => postureAnalysis.StopDataSource());
                        ChangeStartStopMonitoringIconAndText(false);
                    }
                    else
                    {
                        startMonitoringDateTime = DateTime.Now;
                        await Task.Run(() => postureAnalysis.StartMonitoring());
                    }
                }
                else
                {
                    ChangeStartStopMonitoringIconAndText(false);
                    TimeSpan monitoringTime = DateTime.Now - startMonitoringDateTime;
                    log.LogMonitoringTime(monitoringTime);
                    await Task.Run(() => postureAnalysis.StopMonitoring(true));
                }
            }
            catch (IOException e)
            {
                ChangeStartStopMonitoringIconAndText(false);
                MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                log.Error(e);
            }
            CanStartStopMonitoring = true;
        }

        public void StateChanged(object sender, EventArgs e)
        {
            var state = ((Window)sender).WindowState;
            if (state == WindowState.Minimized)
            {
                lastActiveType = ActiveItem.GetType();
                ActivateItem(null);
            }
            else
            {
                var item = menuItems.FirstOrDefault(w => w.GetType() == lastActiveType);
                ActivateItem(item);
            }
        }

        public async void Recalibrate()
        {
            var svm = (SettingsViewModel)menuItems.FirstOrDefault(w => w is SettingsViewModel);
            svm.Recalibrate();
        }

        public void SendReport()
        {
            Task.Run(async () =>
            {
                try
                {
                    ReportBtnText = "Sending...";

                    ICloudStorage cloud = new SpineHeroWebApi();
                    await cloud.SaveData("Bad measurement", CollectDataForReport());

                    MessageBox.Show("Thank you. Your report was sent successfully.", "Sending report",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (CloudStorageException e)
                {
                    log.Error(e);
                    MessageBox.Show("Problem with sending report. Please try it later.", "Sending report",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                finally
                {
                    ReportBtnText = reportBtnDefaultText;
                }
            });
        }

        private string CollectDataForReport()
        {
            ImageWrapper image;
            Evaluation evaluation;
            postureAnalysis.GetCopyOfLastData(out image, out evaluation);
            var data = JsonConvert.SerializeObject(new Dictionary<string, object>()
            {
                {"Machine", Environment.MachineName},
                {"OS", Environment.OSVersion.ToString()},
                {"SittingQuality", evaluation.SittingQuality.ToString()},
                {"Posture", evaluation.Posture.ToString()},
                {"CurrentImage", image.ToJson()},
                {"CalibrationImage", calibrationManager.CalibrationImage.ToJson()}
            });
            image.Dispose();
            return data;
        }

        public void Handle(PostureMonitoringStatusChange message)
        {
            if (isMonitoring == message.IsMonitoring) return;
            if (message.IsError)
            {
                string title = "Spine Hero";
                string text = message.ErrorText;

                ShowNotification(title, text, BalloonIcon.Error);
                StartStopMonitoring();
            }
            else
            {
                isMonitoring = message.IsMonitoring;
                ChangeStartStopMonitoringIconAndText(message.IsMonitoring);
            }
        }
    }
}