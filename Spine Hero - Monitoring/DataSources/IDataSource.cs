using System;

namespace SpineHero.Monitoring.DataSources
{
    public interface IDataSource : IDisposable
    {
        ImageWrapper Images { get; }

        bool Running { get; }

        void Start();

        void Stop();

        bool LoadNext();
    }
}