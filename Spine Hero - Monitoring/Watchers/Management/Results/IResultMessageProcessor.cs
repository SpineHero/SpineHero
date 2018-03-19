using System.Collections.Generic;

namespace SpineHero.Monitoring.Watchers.Management.Results
{
    public interface IResultMessageProcessor
    {
        Evaluation ProcessResults(IList<ResultMessage> results);
    }
}