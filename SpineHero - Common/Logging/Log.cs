using System;

namespace SpineHero.Common.Logging
{
    public class Log : ILog
    {
        protected readonly NLog.Logger innerLogger;

        public delegate void ExceptionEventHandler(Exception sender);
        public static event ExceptionEventHandler OnError;

        protected Log(Type type)
        {
            innerLogger = NLog.LogManager.GetLogger(type.FullName);
        }

        public static ILog GetLogger<T>()
        {
            return new Log(typeof(T));
        }

        public static ILog GetLogger(Type type)
        {
            return new Log(type);
        }

        public virtual void Debug(string format, params object[] args)
        {
            innerLogger.Debug(format, args);
        }

        public virtual void Info(string format, params object[] args)
        {
            innerLogger.Info(format, args);
        }

        public virtual void Warn(string format, params object[] args)
        {
            innerLogger.Warn(format, args);
        }

        public virtual void Error(Exception exception)
        {
            innerLogger.Error(exception, exception.Message);
            OnError?.Invoke(exception);
        }

        public virtual void Error(Exception exception, string message)
        {
            innerLogger.Error(exception, message);
            OnError?.Invoke(exception);
        }
    }
}