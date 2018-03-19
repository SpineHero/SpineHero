using System.Collections.Concurrent;
using SpineHero.Model.Notifications.Rules;
using SpineHero.Monitoring.Watchers.Management.Results;

namespace SpineHero.Model.Notifications
{
    public interface IVerdict
    {
        RulesCollection ShowNotificationRules { get; set; }

        RulesCollection HideNotificationRules { get; set; }

        void Consume(BlockingCollection<Evaluation> queue);

        void MakeVerdictFor(Evaluation evaluation);
    }
}