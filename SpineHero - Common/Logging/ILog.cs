using System;

namespace SpineHero.Common.Logging
{
    public interface ILog
    {
        void Debug(string format, params object[] args);

        void Info(string format, params object[] args);

        void Warn(string format, params object[] args);

        void Error(Exception exception);

        void Error(Exception exception, string message);
    }
}