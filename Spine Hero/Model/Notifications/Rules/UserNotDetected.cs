using System;

namespace SpineHero.Model.Notifications.Rules
{
    public class UserNotDetected : IRule
    {
        public bool Check(NotificationStatistics statistics)
        {
            return IsTwoUnknownPostureInRow(statistics) && IsFulfilledTimeLimit(statistics);
        }

        public virtual bool IsTwoUnknownPostureInRow(NotificationStatistics statistics)
        {
            return statistics.Evaluation.Current.IsUnknown() && statistics.Evaluation.BeforeCurrent.IsUnknown();
        }

        public virtual bool IsFulfilledTimeLimit(NotificationStatistics statistics)
        {
            var limit = TimeSpan.FromSeconds(Properties.Notifications.Default.UnknownPostureLimit);
            return limit <= statistics.Evaluation.Current.EvaluatedAt - statistics.Evaluation.FirstUnknown.EvaluatedAt;
        }
    }
}