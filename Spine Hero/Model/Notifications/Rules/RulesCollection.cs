using System.Collections.Generic;
using System.Linq;

namespace SpineHero.Model.Notifications.Rules
{
    /// <summary>
    /// At least in one list all rules must return true if RulesCollection should returns true.
    /// In other words, between lists is OR operator.
    /// </summary>
    public class RulesCollection
    {
        public RulesCollection(params IList<IRule>[] listsOfRules)
        {
            this.ListsOfRules = listsOfRules;
        }

        public IList<IRule>[] ListsOfRules { get; }

        public virtual bool Check(NotificationStatistics statistics)
        {
            return ListsOfRules.Any(list => CheckList(list, statistics));
        }

        public virtual bool CheckList(IList<IRule> rules, NotificationStatistics statistics)
        {
            return rules.All(rule => rule.Check(statistics));
        }
    }
}