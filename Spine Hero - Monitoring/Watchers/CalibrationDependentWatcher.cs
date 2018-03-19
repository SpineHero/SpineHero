using OpenCvSharp;
using SpineHero.Monitoring.DataSources;
using SpineHero.Monitoring.Watchers.Management.Results;

namespace SpineHero.Monitoring.Watchers
{
    public abstract class CalibrationDependentWatcher : IWatcher, ICanVisualize
    {
        protected ImageWrapper calibrationImages = null;

        protected CalibrationDependentWatcher()
        {
        }

        protected CalibrationDependentWatcher(ImageWrapper calibrationImages)
        {
            CalibrationImagesChanged(calibrationImages);
        }

        public virtual void CalibrationImagesChanged(ImageWrapper calibImages)
        {
            calibrationImages = calibImages;
        }

        public Mat Visualization { get; protected set; }

        public bool VisualizationEnabled { get; set; }

        public bool CalibrationIsSet => calibrationImages != null;

        public abstract ResultMessage AnalyzeImages(ImageWrapper images);
    }
}