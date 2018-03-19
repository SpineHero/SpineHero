using System;

namespace SpineHero.Monitoring.DataSources
{
    public abstract class DataSource : IDataSource
    {
        protected readonly object locker = new object();

        public bool Running { get; protected set; } = false;

        public ImageWrapper Images { get; protected set; }

        public abstract void Start();

        public abstract void Stop();

        public abstract bool LoadNext();

        #region IDisposable Support

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable Support

        #region Creation

        public static IDataSource CreateOfType(Type dataSourceType)
        {
            return (IDataSource)Activator.CreateInstance(dataSourceType);
        }

        public static IDataSource CreateNewIfNeeded(IDataSource current, Type dataSourceType)
        {
            return (current?.GetType() == dataSourceType) ? current : CreateOfType(dataSourceType);
        }

        #endregion Creation
    }
}