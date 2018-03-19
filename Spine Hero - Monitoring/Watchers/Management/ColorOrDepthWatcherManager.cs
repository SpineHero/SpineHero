using System.Collections.Generic;
using SpineHero.Monitoring.DataSources;
using SpineHero.Monitoring.Watchers.Management.Results;

namespace SpineHero.Monitoring.Watchers.Management
{
    public class ColorOrDepthWatcherManager : WatcherManager
    {
        public override void AnalyzeInWatchers(ImageWrapper images, IList<ResultMessage> results)
        {
            if (images == null || results == null) return;

            if (images.DepthImage != null)
            {
                AnalyzeInWatchers<IDepthImageWatcher>(images, results);
            }
            else if (images.ColorImage != null)
            {
                AnalyzeInWatchers<IColorImageWatcher>(images, results);
            }
        }
    }
}