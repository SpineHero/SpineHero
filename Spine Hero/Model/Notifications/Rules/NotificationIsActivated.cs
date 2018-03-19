namespace SpineHero.Model.Notifications.Rules
{
    /// <summary>
    /// Check if there is any displayed or scheduled notification
    /// </summary>
    public class NotificationIsActivated : IRule
    {
        private readonly ISchedule schedule;

        public NotificationIsActivated(ISchedule schedule)
        {
            this.schedule = schedule;
        }

        public bool Check(NotificationStatistics statistics)
        {
            return schedule.IsScheduledOrDisplayed();
        }
    }
}