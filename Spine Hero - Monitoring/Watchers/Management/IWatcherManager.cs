using System.Collections.Generic;
using OpenCvSharp;
using SpineHero.Monitoring.DataSources;
using SpineHero.Monitoring.Watchers.Management.Results;

namespace SpineHero.Monitoring.Watchers.Management
{
    public interface IWatcherManager
    {
        Evaluation AnalyzeInWatchers(ImageWrapper images);

        void AnalyzeInWatchers(ImageWrapper images, IList<ResultMessage> results);

        void AnalyzeInWatchers<T>(ImageWrapper images, IList<ResultMessage> results) where T : IWatcher;

        void WaitWatchers();

        void EnableVisualization(bool enable);

        IList<Mat> GetVisualizationImages();

        void SetCalibrationImages(ImageWrapper calibrationImages);

        IResultMessageProcessor ResultMessageProcessor { get; set; }
    }
}