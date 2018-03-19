using Caliburn.Micro;

namespace SpineHero.Utils.Logging
{
    internal class EventLoggerManager : IHandle<object>
    {
        private readonly ILogger log = Logger.GetLogger<EventLoggerManager>();

        public EventLoggerManager(IEventAggregator aggregator)
        {
            aggregator.Subscribe(this);
        }

        public void Handle(object message)
        {
            log.Debug("Event triggered " + message);
        }
    }
}