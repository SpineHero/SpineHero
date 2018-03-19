using System.Collections.Concurrent;
using System.Threading.Tasks;
using Caliburn.Micro;
using SpineHero.Monitoring.Watchers.Management.Results;

namespace SpineHero.Model.Notifications
{
    public class EvaluationQueue : IHandle<Evaluation>
    {
        private readonly BlockingCollection<Evaluation> queue = new BlockingCollection<Evaluation>(new ConcurrentQueue<Evaluation>());

        public EvaluationQueue(IEventAggregator eventAggregator, IVerdict verdict)
        {
            eventAggregator.Subscribe(this);
            Task.Run(() => verdict.Consume(queue));
        }

        public void Handle(Evaluation message)
        {
            queue.Add(message);
        }
    }
}