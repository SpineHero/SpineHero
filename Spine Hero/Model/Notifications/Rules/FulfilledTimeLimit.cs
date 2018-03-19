namespace SpineHero.Model.Notifications.Rules
{
    /// <summary>
    /// Check if the time of sitting in current position is over the time limit for next notification
    /// </summary>
    public class FulfilledTimeLimit : IRule
    {
        private readonly INotificationsSettings notificationsSettings;

        public FulfilledTimeLimit(INotificationsSettings notificationsSettings)
        {
            this.notificationsSettings = notificationsSettings;
        }

        public bool Check(NotificationStatistics statistics)
        {
            var duration = statistics.Evaluation.Current.EvaluatedAt - statistics.Evaluation.FirstWrong.EvaluatedAt;
            var timeLimit = notificationsSettings.NextNotification(statistics.LastUsedNotification).TimeLimit;
            return duration >= timeLimit;
        }
    }
}