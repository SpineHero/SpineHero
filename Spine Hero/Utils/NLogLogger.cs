using System;

namespace SpineHero.Utils
{
    internal class NLogLogger : ILogger
    {
        private readonly NLog.Logger innerLogger;

        private NLogLogger(Type type)
        {
            innerLogger = NLog.LogManager.GetLogger(type.FullName);
        }

        public static ILogger GetLogger<T>()
        {
            return new NLogLogger(typeof(T));
        }

        public static ILogger GetLogger(Type type)
        {
            return new NLogLogger(type);
        }

        public void Info(string format, params object[] args)
        {
            innerLogger.Info(format, args);
        }

        public void Warn(string format, params object[] args)
        {
            innerLogger.Warn(format, args);
        }

        public void Error(Exception exception)
        {
            innerLogger.Error(exception, exception.Message);
        }

        public void Debug(string format, params object[] args)
        {
            innerLogger.Debug(format, args);
        }
        
    }
}