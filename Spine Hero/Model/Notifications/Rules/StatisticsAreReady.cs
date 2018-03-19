namespace SpineHero.Model.Notifications.Rules
{
    public class StatisticsAreReady : IRule
    {
        public bool Check(NotificationStatistics statistics)
        {
            return statistics.Evaluation.Current != null && statistics.Evaluation.BeforeCurrent != null;
        }
    }
}