using Caliburn.Micro;
using Caliburn.Micro.KeyBinding.Input;
using SpineHero.Common.Logging;
using SpineHero.Model;
using SpineHero.Model.Notifications;
using SpineHero.Model.Notifications.NotificationAreaNotification;
using SpineHero.Model.Notifications.PopupNotification;
using SpineHero.Model.Statistics;
using SpineHero.Model.Store;
using SpineHero.PostureMonitoring;
using SpineHero.PostureMonitoring.Managers;
using SpineHero.Properties;
using SpineHero.Utils.Logging;
using SpineHero.ViewModels;
using SpineHero.ViewModels.DialogWindows;
using SpineHero.ViewModels.MainMenuItems;
using SpineHero.ViewModels.Notifications;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;
using AutoUpdaterDotNET;
using SpineHero.Monitoring.Watchers.Management;
using SpineHero.Model.Notifications.DimNotification;
using SpineHero.Utils.WinAPI;
using Key = System.Windows.Input.Key;
using Statistics = SpineHero.Model.Store.Statistics;
using Infralution.Localization.Wpf;
using System.Globalization;

namespace SpineHero
{
    public class AppBootstrapper : BootstrapperBase
    {
        private readonly ILogger log = Logger.GetLogger<AppBootstrapper>();
        private SimpleContainer container;

        public AppBootstrapper()
        {
            new ApplicationInstance().PreserveSingleInstance();
            Initialize();
        }

        protected override void Configure()
        {
            //LogManager.GetLog = NLogger.GetLogger;

            ConfigureKeyTrigger();

            container = new SimpleContainer();

            container.Singleton<IEventAggregator, EventAggregator>();
            container.Singleton<IWindowManager, WindowManager>();

            container.Singleton<EventLoggerManager>().GetInstance<EventLoggerManager>();

            container.Singleton<DatabaseQuery>();
            container.Singleton<Statistics>();

            container.Singleton<StatisticsModule>();
            container.Singleton<BreaksModule>();

            container.Singleton<IDataSourceManager, DataSourceManager>();
            container.Singleton<IPostureMonitoringManager, PostureMonitoringManager>();
            container.Singleton<IWatcherManager, ColorOrDepthWatcherManager>();

            container.Singleton<EvaluationQueue>();
            container.Singleton<IVerdict, Verdict>();
            container.Singleton<INotificationsSettings, NotificationsSettings>();
            container.Singleton<ISchedule, Schedule>();
            container.Singleton<NotificationAreaIconViewModel>();
            container.Singleton<PopupNotificationViewModel>();
            container.Singleton<INotificationAreaNotification, NotificationAreaNotification>();
            container.Singleton<IPopupNotification, PopupNotification>();
            container.Singleton<IDimNotification, DimNotification>();

            container.Singleton<AnalyzePeriodManager>();
            container.Singleton<CalibrationManager>();

            container.Singleton<IShell, ShellViewModel>();
            container.PerRequest<CalibrationWindowViewModel>();
            container.PerRequest<WelcomeViewModel>();
            container.AllTypesOf<IMainMenuItem>(Assembly.GetExecutingAssembly());
        }

        private void ConfigureKeyTrigger()
        {
            var trigger = Parser.CreateTrigger;

            Parser.CreateTrigger = (target, triggerText) =>
            {
                if (triggerText == null)
                {
                    var defaults = ConventionManager.GetElementConvention(target.GetType());
                    return defaults.CreateTrigger();
                }

                var triggerDetail = triggerText
                    .Replace("[", string.Empty)
                    .Replace("]", string.Empty);

                var splits = triggerDetail.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
                if (splits[0] == "Key")
                {
                    var key = (Key)Enum.Parse(typeof(Key), splits[1], true);
                    return new KeyTrigger { Key = key };
                }
                else if (splits[0] == "Gesture")
                {
                    var mkg = (MultiKeyGesture)(new MultiKeyGestureConverter()).ConvertFrom(splits[1]);
                    return new KeyTrigger { Modifiers = mkg.KeySequences[0].Modifiers, Key = mkg.KeySequences[0].Keys[0] };
                }

                return trigger(target, triggerText);
            };
        }

        protected override object GetInstance(Type service, string key)
        {
            var instance = container.GetInstance(service, key);
            if (instance != null)
                return instance;

            throw new InvalidOperationException("Could not locate any instances.");
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return container.GetAllInstances(service);
        }

        protected override void BuildUp(object instance)
        {
            container.BuildUp(instance);
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            // Select application language
            if (Settings.Default.AppLanguage == string.Empty)
            {
                Settings.Default.AppLanguage = CultureManager.UICulture.ToString();
            }
            CultureManager.UICulture = new CultureInfo(Settings.Default.AppLanguage);

            // Show window
            DisplayRootViewFor<IShell>();

            // Then load other things
            container.GetInstance<Statistics>();
            container.GetInstance<EvaluationQueue>();
            container.GetInstance<CalibrationManager>();
            container.GetInstance<AnalyzePeriodManager>();
            container.GetInstance<IWatcherManager>();

            // Show Welcome screen
            if (Settings.Default.DisplayWelcomeScreenOnStart)
            {
                var windowManager = container.GetInstance<IWindowManager>();
                var welcomeViewModel = container.GetInstance<WelcomeViewModel>();
                windowManager.ShowDialog(welcomeViewModel);
            }

            // Autolaunch on startup
            if (!Settings.Default.WasAutolaunchSetBefore)
            {
                AutoStartup.AutoStart = true;
                Settings.Default.WasAutolaunchSetBefore = true;
            }

#if !DEBUG
            AutoUpdater.Start(Settings.Default.UpdateUrl);
#endif
        }

        protected override void OnExit(object sender, EventArgs e)
        {
            base.OnExit(sender, e);
            Settings.Default.Save();
            Notifications.Default.Save();
            container.GetInstance<Statistics>().SaveData();
        }

        protected override void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            log.Error(e.Exception);
#if DEBUG
            var msg = e.Exception.ToString();
#else
            var msg = "Ups, something went wrong. \nIf application isn't working correctly or the problem persist, try restarting Spine Hero.";
#endif
            MessageBox.Show(msg, "Error occurred", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }
    }
}