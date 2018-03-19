using SpineHero.PostureMonitoring;

namespace SpineHero.Model.Notifications.Rules
{
    public class WrongSitting : IRule
    {
        public bool Check(NotificationStatistics statistics)
        {
            return statistics.Evaluation.Current.IsWrong() &&
                   statistics.Evaluation.BeforeCurrent.IsWrong();
        }
    }
}