using Caliburn.Micro;
using SpineHero.PostureMonitoring.Managers;
using SpineHero.Properties;
using System.Collections.Generic;
using System.Linq;
using SpineHero.Monitoring.Watchers.Management.Results;

namespace SpineHero.Model.Statistics
{
    public class StatisticsModule : IHandle<Evaluation>, IHandle<PostureMonitoringStatusChange>
    {
        private static readonly int EVALUATION_MOVING_AVG_COUNT = Const.Default.EvaluationMovingAvgCount;
        private static readonly int EVALUATION_LIST_MAX_LIMIT = Const.Default.EvaluationListMaxLimit;
        private static readonly int EVALUATION_LIST_LOWER_LIMIT = Const.Default.EvaluationListLowerLimit;
        private static readonly int TO_REMOVE = EVALUATION_LIST_MAX_LIMIT - EVALUATION_LIST_LOWER_LIMIT;
        private readonly IEventAggregator eventAggregator;

        private bool started;

        public StatisticsModule(IEventAggregator aggregator)
        {
            eventAggregator = aggregator;
            eventAggregator.Subscribe(this);
        }

        public List<Evaluation> Evaluations { get; } = new List<Evaluation>();

        public List<int> SittingQualityAveraged { get; } = new List<int>();

        public Evaluation LastEvaluation
        {
            get
            {
                return Evaluations.LastOrDefault();
            }
        }

        public Posture LastPosture
        {
            get
            {
                return LastEvaluation?.Posture ?? Posture.Unknown;
            }
        }

        public int LastSittingQuality
        {
            get
            {
                return LastEvaluation?.SittingQuality ?? 0;
            }
        }

        public int LastSittingQualityAveraged
        {
            get
            {
                return SittingQualityAveraged.LastOrDefault();
            }
        }

        public void Handle(Evaluation eval)
        {
            if (LastPosture != eval.Posture)
            {
                eventAggregator.PublishOnUIThreadAsync(eval.Posture);
            }
            else if (started)
            {
                started = false;
                eventAggregator.PublishOnUIThread(eval.Posture);
            }
            if (eval.Posture == Posture.Unknown && Evaluations.Any() && Evaluations.Last()?.Posture == Posture.Unknown) return; // 2 Unknown posture evaluations in a row.

            Evaluations.Add(eval);
            var adj = CalculateMovingAvg();
            SittingQualityAveraged.Add(adj);

            RemoveElementsIfOverLimit();
        }

        public void Handle(PostureMonitoringStatusChange message)
        {
            if (message.IsMonitoring) started = true;
        }

        private int CalculateMovingAvg()
        {
            var cnt = Evaluations.Count;
            if (cnt > EVALUATION_MOVING_AVG_COUNT) cnt = EVALUATION_MOVING_AVG_COUNT;

            var sum = 0;
            for (int i = 1; i <= cnt; i++)
            {
                sum += Evaluations[Evaluations.Count - i].SittingQuality;
            }
            sum /= cnt;
            return sum;
        }

        private void RemoveElementsIfOverLimit()
        {
            if (Evaluations.Count > EVALUATION_LIST_MAX_LIMIT)
            {
                Evaluations.RemoveRange(0, TO_REMOVE);
                SittingQualityAveraged.RemoveRange(0, TO_REMOVE);
            }
        }
    }
}