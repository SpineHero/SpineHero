using System;
using SpineHero.Common.Logging;

namespace SpineHero.Utils.Logging
{
    public interface ILogger : ILog
    {
        void LogPageView(string pageName);

        void LogEvent(string eventName);

        void LogMonitoringTime(TimeSpan elapsed, string dataSource = null);
    }
}