namespace SpineHero.Model.Notifications.Rules
{
    public interface IRule
    {
        bool Check(NotificationStatistics statistics);
    }
}