using Caliburn.Micro;
using SpineHero.Common.Logging;
using SpineHero.Common.Resources;
using SpineHero.Model;
using SpineHero.Model.Notifications;
using SpineHero.Model.Notifications.PopupNotification;
using SpineHero.Model.Store;
using SpineHero.Monitoring.Watchers.Management.Results;
using SpineHero.PostureMonitoring.Managers;
using SpineHero.Properties;
using SpineHero.ViewModels.DialogWindows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Timers;
using System.Windows.Controls.Primitives;
using Const = SpineHero.Monitoring.Properties.Const;

namespace SpineHero.ViewModels.MainMenuItems
{
    internal class DebugViewModel : Screen, IMainMenuItem
    {
        private readonly IEventAggregator eventAggregator;
        private readonly IWindowManager windowManager;
        private readonly CalibrationManager calibrationManager;
        private Timer timer = new Timer(Settings.Default.AnalyzePeriod);
        private bool repeat = true;
        private int mode;

        public DebugViewModel(IEventAggregator ea, IWindowManager wm, CalibrationManager cm)
        {
            DisplayName = "Debug";
            eventAggregator = ea;
            ea.Subscribe(this);
            windowManager = wm;
            calibrationManager = cm;
        }

        public bool Repeat
        {
            get { return repeat; }
            set
            {
                if (!value)
                    Publish(0, 0, mode);
                repeat = value;
                NotifyOfPropertyChange();
            }
        }

        [Conditional("Debug")]
        public void ShowVisualization()
        {
            windowManager.ShowWindow(new VisualizationImagesViewModel(eventAggregator));
        }

        [Conditional("DEBUG")]
        public void PublishCorrect()
        {
            Publish(Const.Default.CorrectLimit, 100, 1);
        }

        [Conditional("DEBUG")]
        public void PublishWrong()
        {
            Publish(0, Const.Default.WarningLimit - 1, 2);
        }

        [Conditional("DEBUG")]
        public void Publish(int min, int max, int m)
        {
            var random = new Random(260);
            if (Repeat)
            {
                if (m != mode)
                {
                    timer.Dispose();
                    timer = new Timer(Settings.Default.AnalyzePeriod);
                    timer.Elapsed += delegate { eventAggregator.PublishOnUIThread(new Evaluation(random.Next(min, max))); };
                    eventAggregator.PublishOnUIThread(new PostureMonitoringStatusChange(true));
                    mode = m;
                    timer.Start();
                }
                else
                {
                    timer.Dispose();
                    mode = 0;
                    eventAggregator.PublishOnUIThread(new PostureMonitoringStatusChange(false));
                }
            }
            else
            {
                eventAggregator.PublishOnUIThread(new PostureMonitoringStatusChange(true));
                eventAggregator.PublishOnUIThread(new Evaluation(random.Next(min, max)));
                eventAggregator.PublishOnUIThread(new PostureMonitoringStatusChange(false));
            }
        }

        [Conditional("DEBUG")]
        public void DeleteCalibration()
        {
            calibrationManager.DeleteAll();
        }

        [LogMethodCall]
        [Conditional("DEBUG")]
        public void CreateAndFillDatabase()
        {
            var path = ResourceHelper.GetAppDataLocalResourcePath(Settings.Default.DatabasePath);
            if (File.Exists(path)) File.Delete(path);
            var db = new Database(path);
            var list = new List<Bucket>();
            var to = DateTime.Today.AddDays(1);
            var from = to.AddYears(-2);
            var diff = (to - from).TotalDays;
            var random = new Random(26);
            for (int i = 0; i < diff; i++)
            {
                if (random.Next(0, 100) > 90) continue;
                var today = from.AddDays(i);
                for (int j = 0; j < 3; j++)
                {
                    if (random.Next(0, 100) < 90 - i * 10) continue;
                    var start = random.Next(6 + j * 6, 8 + j * 6);
                    var duration = random.Next(1, 5);
                    var ss = 6 + (j + 1) * 6;
                    duration = start + duration >= ss ? ss - start - 1 : duration;

                    var now = today.AddHours(start);
                    for (int k = 0; k < duration; k++)
                    {
                        var total = random.Next(1800000, 3600000);
                        var sq = random.Next(0, 100);
                        var bucket = new Bucket
                        {
                            Time = now.AddHours(k),
                            SittingQuality = sq,
                            SittingTimeSum = total,
                            SittingQualitySum = sq * total,
                            Posture = new long[] { total / 10, total / 2, total / 4, total / 20, total / 20, total / 20, total / 20, total / 20 }
                        };
                        list.Add(bucket);
                    }
                }
            }
            db.Insert<Bucket>(list);

            var postures = new List<PostureTime>();
            for (int i = 0; i < diff; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    postures.Add(new PostureTime((Posture)j, from.AddDays(i).AddHours(j + 1)));
                }
                postures.Add(new PostureTime(null, from.AddDays(i).AddHours(9)));
            }
            db.Insert<PostureTime>(postures);

            var notifications = new List<NotificationShownEvent>();
            for (int i = 0; i < 30; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    notifications.Add(new NotificationShownEvent(nameof(PopupNotification), DateTime.Today.AddDays(i).AddHours(j)));
                }
            }
            db.Insert<NotificationShownEvent>(notifications);
        }

        public void ShowCustomPopup()
        {
            ((IShell)Parent).ShowNotification("Sitting without break", $"You are sitting for {60} minutes.\nTake a break.", PopupAnimation.Fade, 10000);
        }
    }
}