using SpineHero.Monitoring.DataSources;
using SpineHero.Monitoring.Watchers.Management.Results;

namespace SpineHero.Monitoring.Watchers
{
    public interface IWatcher
    {
        ResultMessage AnalyzeImages(ImageWrapper images);
    }

    public interface IColorImageWatcher : IWatcher { }

    public interface IDepthImageWatcher : IWatcher { }
}