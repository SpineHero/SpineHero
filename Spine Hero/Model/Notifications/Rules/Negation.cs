namespace SpineHero.Model.Notifications.Rules
{
    public class Negation : IRule
    {
        private readonly IRule rule;

        public Negation(IRule rule)
        {
            this.rule = rule;
        }

        public bool Check(NotificationStatistics statistics)
        {
            return ! rule.Check(statistics);
        }
    }
}