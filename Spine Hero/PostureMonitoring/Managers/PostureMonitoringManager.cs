using System.Collections.Generic;
using System.Diagnostics;
using Caliburn.Micro;
using SpineHero.Properties;
using System.Threading;
using OpenCvSharp;
using SpineHero.Common.Logging;
using SpineHero.Monitoring.DataSources;
using SpineHero.Monitoring.Watchers.Management;
using SpineHero.Monitoring.Watchers.Management.Results;
using Timer = System.Timers.Timer;

namespace SpineHero.PostureMonitoring.Managers
{
    public class PostureMonitoringManager : IPostureMonitoringManager, IHandle<AnalyzePeriodChange>, IHandle<ImageWrapper>
    {
        private readonly object locker = new object();
        private readonly object monitoringLocker = new object();
        private readonly IEventAggregator eventAggregator;
        private readonly IDataSourceManager dataSourceManager;
        private readonly IWatcherManager watcherManager;

        private Timer timer;
        private ImageWrapper lastImage;
        private Evaluation lastEvaluation;
        private bool isMonitoring;

        public PostureMonitoringManager(IEventAggregator ea, IDataSourceManager dsm, IWatcherManager wm)
        {
            eventAggregator = ea;
            eventAggregator.Subscribe(this);
            dataSourceManager = dsm;
            watcherManager = wm;
#if DEBUG
            watcherManager.EnableVisualization(true);
#endif
        }

        public bool IsMonitoring
        {
            get
            {
                lock (monitoringLocker)
                {
                    return isMonitoring;
                }
            }
            private set
            {
                lock (monitoringLocker)
                {
                    isMonitoring = value;
                }
            }
        }

        public void RunAnalysis()
        {
            if (!IsMonitoring || !Monitor.TryEnter(locker)) return;
            try
            {
                lastImage?.Dispose();
                lastImage = dataSourceManager.LoadNext();
                if (lastImage == null)
                {
                    eventAggregator.PublishOnUIThread(new PostureMonitoringStatusChange(false, true, "Monitoring was stopped. Camera is not available."));
                    return;
                }

                lastEvaluation = watcherManager.AnalyzeInWatchers(lastImage);
                if (lastEvaluation != null) eventAggregator.PublishOnUIThread(lastEvaluation);
                PublishVisualizationImages();
            }
            finally
            {
                Monitor.Exit(locker);
            }
        }

        [Conditional("DEBUG")]
        private void PublishVisualizationImages()
        {
            var images = new List<Mat> { lastImage.ColorImage?.Flip(FlipMode.Y), lastImage.DepthImage?.Flip(FlipMode.Y) };
            images.AddRange(watcherManager.GetVisualizationImages());
            eventAggregator.PublishOnUIThread(images);
            images.RemoveAll(x => x == null);
            images.Apply(x => x.Dispose());
        }


        public void StartDataSource()
        {
            if (!dataSourceManager.Running) dataSourceManager.StartDataSource(Settings.Default.UseDepthCamera);
        }

        public void StopDataSource()
        {
            if (IsMonitoring) StopMonitoring(true);
            else dataSourceManager.StopDataSource();
        }

        [LogMethodCall("Starting monitoring.")]
        public void StartMonitoring()
        {
            if (IsMonitoring) return;
            lock (locker)
            {
                if (!dataSourceManager.Running) dataSourceManager.StartDataSource(Settings.Default.UseDepthCamera);
                timer = new Timer(Settings.Default.AnalyzePeriod);
                timer.Elapsed += delegate { RunAnalysis(); };
                timer.Start();
            }
            IsMonitoring = true;
            eventAggregator.BeginPublishOnUIThread(new PostureMonitoringStatusChange(true));
        }

        [LogMethodCall("Stopping monitoring.")]
        public void StopMonitoring(bool stopDataSource)
        {
            if (!IsMonitoring) return;
            IsMonitoring = false;
            lock (locker)
            {
                if (stopDataSource) dataSourceManager.StopDataSource();
                timer.Stop();
                timer.Dispose();
                timer = null;
            }
            eventAggregator.BeginPublishOnUIThread(new PostureMonitoringStatusChange(false));
        }

        public void GetCopyOfLastData(out ImageWrapper image, out Evaluation evaluation)
        {
            lock (locker)
            {
                image = lastImage.Clone();
                evaluation = lastEvaluation;
            }
        }

        public void Handle(AnalyzePeriodChange analyzePeriod)
        {
            lock (locker)
            {
                if (timer != null)
                    timer.Interval = analyzePeriod.PeriodTime;
            }
        }

        public void Handle(ImageWrapper calibrationImages)
        {
            watcherManager.SetCalibrationImages(calibrationImages);
        }
    }
}