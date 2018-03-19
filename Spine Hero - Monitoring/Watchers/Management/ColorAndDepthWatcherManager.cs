using System.Collections.Generic;
using SpineHero.Monitoring.DataSources;
using SpineHero.Monitoring.Watchers.Management.Results;

namespace SpineHero.Monitoring.Watchers.Management
{
    public class ColorAndDepthWatcherManager : WatcherManager
    {
        public override void AnalyzeInWatchers(ImageWrapper images, IList<ResultMessage> results)
        {
            if (images == null || results == null) return;

            if (images.ColorImage != null)
            {
                AnalyzeInWatchers<IColorImageWatcher>(images, results);
            }
            if (images.DepthImage != null)
            {
                AnalyzeInWatchers<IDepthImageWatcher>(images, results);
            }
        }
    }
}