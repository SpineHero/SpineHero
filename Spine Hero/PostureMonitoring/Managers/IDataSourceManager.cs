using SpineHero.Monitoring.DataSources;

namespace SpineHero.PostureMonitoring.Managers
{
    public interface IDataSourceManager
    {
        IDataSource DataSource { get; }

        ImageWrapper Images { get; }

        bool Running { get; }

        void StartDataSource(bool useDepthCamIfAvailable);

        void StopDataSource();

        ImageWrapper LoadNext();
    }
}