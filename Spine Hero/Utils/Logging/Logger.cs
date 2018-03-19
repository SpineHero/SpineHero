using System;
using SpineHero.Common.Logging;

namespace SpineHero.Utils.Logging
{
    internal class Logger : Log, ILogger
    {
        protected Logger(Type type) : base(type)
        {
        }

        public new static ILogger GetLogger<T>()
        {
            return new Logger(typeof(T));
        }

        public new static ILogger GetLogger(Type type)
        {
            return new Logger(type);
        }

        public void LogPageView(string pageName)
        {
            Info("PageView: " + pageName);
        }

        public void LogEvent(string eventName)
        {
            Info("Event: " + eventName);
        }

        public void LogMonitoringTime(TimeSpan elapsed, string dataSource = null)
        {
            Info("MonitoringTime: " + elapsed.ToString());
        }
    }
}