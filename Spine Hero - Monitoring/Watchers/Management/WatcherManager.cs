using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenCvSharp;
using SpineHero.Monitoring.DataSources;
using SpineHero.Monitoring.Watchers.Management.Results;

namespace SpineHero.Monitoring.Watchers.Management
{
    public abstract class WatcherManager : IWatcherManager
    {
        protected readonly ConcurrentBag<IWatcher> watchers = new ConcurrentBag<IWatcher>();
        protected ConcurrentBag<Task> tasks = new ConcurrentBag<Task>();

        protected WatcherManager()
        {
            FillWatchers();
        }

        public IResultMessageProcessor ResultMessageProcessor { get; set; } = new AverageResultMessageProcessor();

        protected virtual void FillWatchers() // Maybe using Assembly to find all watchers?
        {
            watchers.Add(new HeadWatcher.HeadWatcher());
            watchers.Add(new DepthWatcher.DepthWatcher());
        }

        public Evaluation AnalyzeInWatchers(ImageWrapper images)
        {
            var results = new List<ResultMessage>();
            AnalyzeInWatchers(images, results);
            WaitWatchers();
            return ResultMessageProcessor.ProcessResults(results);
        }

        public abstract void AnalyzeInWatchers(ImageWrapper images, IList<ResultMessage> results);

        public void AnalyzeInWatchers<T>(ImageWrapper images, IList<ResultMessage> results) where T : IWatcher
        {
            foreach (var item in watchers.Where(x => x is T))
            {
                var task = Task.Run(() =>
                {
                    var result = item.AnalyzeImages(images);
                    if (result != null) results.Add(result);
                });
                tasks.Add(task);
            }
        }

        public void EnableVisualization(bool enable)
        {
            foreach (var watcher in watchers.Cast<ICanVisualize>())
            {
                watcher.VisualizationEnabled = enable;
            }
        }

        public IList<Mat> GetVisualizationImages()
        {
            var list = new List<Mat>();
            list.AddRange(watchers.Select(w => (w as ICanVisualize)?.Visualization));
            list.RemoveAll(x => x == null);
            return list;
        }

        public void SetCalibrationImages(ImageWrapper calibrationImages)
        {
            foreach (var w in watchers)
            {
                (w as CalibrationDependentWatcher)?.CalibrationImagesChanged(calibrationImages);
            }
        }
        public void WaitWatchers()
        {
            foreach (var item in tasks)
            {
                item.Wait();
            }
            tasks = new ConcurrentBag<Task>();
        }
    }
}