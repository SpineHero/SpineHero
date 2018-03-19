using Caliburn.Micro;
using SpineHero.Common.Logging;
using SpineHero.Common.Resources;
using SpineHero.Monitoring.DataSources;
using SpineHero.Properties;
using System.ComponentModel;
using System.IO;

namespace SpineHero.Model
{
    public class CalibrationManager : IHandle<ImageWrapper>
    {
        private static readonly string EXT = Monitoring.Properties.Const.Default.IWFileExtension;
        private readonly IEventAggregator eventAggregator;
        private bool dontSave;

        public CalibrationManager(IEventAggregator aggregator)
        {
            eventAggregator = aggregator;
            eventAggregator.Subscribe(this);
            Settings.Default.PropertyChanged += DataSourceChanged;
            //DataSourceChanged(Settings.Default, new PropertyChangedEventArgs(nameof(Settings.Default.DataSource)));
        }

        public bool IsCalibrated { get; private set; }

        public ImageWrapper CalibrationImage { get; private set; }

        private void DataSourceChanged(object sender, PropertyChangedEventArgs e) // Load
        {
            var settings = (sender as Settings);
            if (settings == null || !e.PropertyName.Equals(nameof(settings.DataSource))) return;

            IsCalibrated = false;
            var images = LoadCalibrationImages(settings.DataSource);
            if (images != null)
            {
                IsCalibrated = true;
                dontSave = true;
                eventAggregator.PublishOnBackgroundThread(images);
            }
        }

        [LogMethodCall("Saving calibration image.")]
        public void Handle(ImageWrapper images) // Save
        {
            CalibrationImage = images.Clone();
            if (dontSave) // There is no need to save currently loaded calibration images.
            {
                dontSave = false;
                return;
            }

            IsCalibrated = true;
            var name = Settings.Default.DataSource + EXT;
            var path = ResourceHelper.GetAppDataLocalResourcePath(Settings.Default.CalibrationPath + name);
            images.Save(path);
        }

        [LogMethodCall("Loading calibration image.")]
        public ImageWrapper LoadCalibrationImages(string ds)
        {
            var name = ds + EXT;
            var path = ResourceHelper.GetAppDataLocalResourcePath(Settings.Default.CalibrationPath + name);
            try
            {
                return ImageWrapper.Load(path);
            }
            catch (IOException) { return null; }
        }

        public void DeleteAll()
        {
            IsCalibrated = false;
            CalibrationImage = null;
            var path = ResourceHelper.GetAppDataLocalResourcePath(Settings.Default.CalibrationPath);
            var info = new DirectoryInfo(path);
            if (info.Exists) info.Delete(true);
            info.Create();
        }
    }
}