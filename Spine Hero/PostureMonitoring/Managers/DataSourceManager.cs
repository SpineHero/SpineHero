using SpineHero.Common.Logging;
using SpineHero.Monitoring.DataSources;
using SpineHero.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

namespace SpineHero.PostureMonitoring.Managers
{
    public class DataSourceManager : IDataSourceManager
    {
        private readonly object locker = new object();
        private readonly List<Type> availableDatasources = new List<Type>();
        private IDataSource dataSource;

        public DataSourceManager()
        {
            Settings.Default.PropertyChanged += UseDepthCameraChanged;
            AvailableDataSourcesChanged(Settings.Default.UseDepthCamera);
        }

        private void AvailableDataSourcesChanged(bool useDepth)
        {
            lock (locker)
            {
                availableDatasources.Clear();
                if (useDepth)
                {
                    availableDatasources.Add(typeof(IntelF200DataSource)); // Podľa poradia spúšťania
                    availableDatasources.Add(typeof(Senz3DDataSource));
                }
                availableDatasources.Add(typeof(WebCameraDataSource));
            }
        }

        private void UseDepthCameraChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == nameof(Settings.Default.UseDepthCamera))
            {
                AvailableDataSourcesChanged(Settings.Default.UseDepthCamera);
            }
        }

        public IDataSource DataSource
        {
            get { return dataSource; }
            private set
            {
                dataSource = value;
                Settings.Default.DataSource = DataSource.GetType().Name;
            }
        }

        public ImageWrapper Images => DataSource?.Images;

        public bool Running => DataSource?.Running ?? false;

        [LogMethodCall]
        public void StartDataSource(bool useDepthCamIfAvailable)
        {
            lock (locker)
            {
                if (Running) return;
                foreach (var dsType in availableDatasources)
                {
                    var ds = DataSourceFactory.CreateNewIfNeeded(DataSource, dsType);
                    if (TryStartDataSource(ds))
                    {
                        DataSource = ds;
                        return;
                    }
                }
                throw new IOException("No camera device available!");
            }
        }

        public void StopDataSource()
        {
            lock (locker)
            {
                DataSource.Stop();
            }
        }

        public ImageWrapper LoadNext()
        {
            lock (locker)
            {
                if (!Running || !DataSource.LoadNext()) return null;
                return DataSource.Images;
            }
        }

        public void StartDataSource(IDataSource dataSource)
        {
            try
            {
                dataSource.Start();
            }
            catch (IOException)
            {
                throw;
            }
            catch (Exception)
            {
                throw new IOException($"Can't initialize data Source {nameof(dataSource)}.");
            }
        }

        [LogMethodCall]
        public bool TryStartDataSource(IDataSource dataSource)
        {
            try
            {
                dataSource.Start();
                return true;
            }
            catch (Exception) { return false; }
        }
    }
}