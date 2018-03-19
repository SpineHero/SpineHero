using System;
using SpineHero.Monitoring.DataSources;
using SpineHero.Monitoring.Watchers.Management.Results;

namespace SpineHero.PostureMonitoring.Managers
{
    public interface IPostureMonitoringManager
    {
        bool IsMonitoring { get; }

        void StartDataSource();

        void StopDataSource();

        void StartMonitoring();

        void StopMonitoring(bool stopDataSource);

        void GetCopyOfLastData(out ImageWrapper image, out Evaluation evaluation);
    }

    public class PostureMonitoringStatusChange
    {
        public PostureMonitoringStatusChange(bool isMonitoring)
        {
            IsMonitoring = isMonitoring;
        }

        public PostureMonitoringStatusChange(bool isMonitoring, bool isError)
        {
            IsError = isError;
            IsMonitoring = isMonitoring;
            ErrorText = "Unknow error";
        }

        public PostureMonitoringStatusChange(bool isMonitoring, bool isError, String errorText)
        {
            IsError = isError;
            IsMonitoring = isMonitoring;
            ErrorText = errorText;
        }

        public bool IsMonitoring { get; private set; }

        public bool IsError { get; private set; } = false;

        public String ErrorText { get; private set; }
    }
}