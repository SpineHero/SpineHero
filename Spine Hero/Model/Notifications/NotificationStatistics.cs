using SpineHero.Monitoring.Watchers.Management.Results;

namespace SpineHero.Model.Notifications
{
    public class NotificationStatistics
    {
        public NotificationStatistics()
        {
            Evaluation = new EvaluationStatistics();
        }

        public INotification LastUsedNotification { get; set; }

        public EvaluationStatistics Evaluation { get; set; }

        public NotificationStatistics Add(Evaluation currentEvaluation)
        {
            return new NotificationStatistics
            {
                Evaluation = new EvaluationStatistics
                {
                    Current = currentEvaluation,
                    BeforeCurrent = this.Evaluation.Current,
                    FirstWrong = FindFirstWrong(currentEvaluation),
                    FirstUnknown = FindFirstUnknown(currentEvaluation)
                },
                LastUsedNotification = this.LastUsedNotification
            };
        }

        private Evaluation FindFirstWrong(Evaluation currentEvaluation)
        {
            if (Evaluation.FirstWrong == null && currentEvaluation.IsWrong())
                return currentEvaluation;
            if (!currentEvaluation.IsWrong() && Evaluation.Current != null && !Evaluation.Current.IsWrong())
                    return null;
            return Evaluation.FirstWrong;
        }

        private Evaluation FindFirstUnknown(Evaluation currentEvaluation)
        {
            if (Evaluation.FirstUnknown == null && currentEvaluation.IsUnknown())
                return currentEvaluation;
            if (!currentEvaluation.IsUnknown() && Evaluation.Current != null && !Evaluation.Current.IsUnknown())
                return null;
            return Evaluation.FirstUnknown;
        }
    }

    public class EvaluationStatistics
    {
        public Evaluation Current { get; set; }

        public Evaluation BeforeCurrent { get; set; }

        public Evaluation FirstUnknown { get; set; }

        public Evaluation FirstWrong { get; set; }
    }
}