using System.Collections.Generic;
using System.Linq;
using PostSharp.Patterns.Contracts;

namespace SpineHero.Monitoring.Watchers.Management.Results
{
    public class AverageResultMessageProcessor : IResultMessageProcessor
    {
        public Evaluation ProcessResults([NotNull] IList<ResultMessage> results)
        {
            if (!results.Any()) return null;
            int sum = 0, cnt = 0;
            Posture posture = 0;
            foreach (var item in results.Where(x => x != null && x.DetectedPosture != Posture.Unknown))
            {
                sum += item.SittingQuality;
                cnt++;
                posture = (item.DetectedPosture > posture) ? item.DetectedPosture : posture; // TODO : Use buckets
            }
            if (cnt == 0) return new Evaluation(-1, Posture.Unknown);
            var avg = sum / results.Count;
            if (SittingQualityLevelHelper.GetLevel(avg) == SittingQualityLevel.Correct) return new Evaluation(avg, Posture.Correct);
            return (posture == 0) ? new Evaluation(avg) : new Evaluation(avg, posture);
        }
    }
}