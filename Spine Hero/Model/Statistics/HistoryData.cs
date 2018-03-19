using SpineHero.Model.Store;
using System;
using System.Linq;
using SpineHero.Monitoring.Watchers.Management.Results;

namespace SpineHero.Model.Statistics
{
    public class HistoryData
    {
        public HistoryData()
        {
        }

        public HistoryData(Bucket bucket)
        {
            Time = bucket.Time;
            Correct = bucket.Posture[(int)Posture.Correct];
            Unknown = bucket.Posture[(int)Posture.Unknown];
            Wrong = bucket.Posture.Skip(2).Sum();
            SittingQuality = bucket.SittingQuality;
        }

        public DateTime Time { get; set; }
        public long Correct { get; set; }
        public long Unknown { get; set; }
        public long Wrong { get; set; }
        public long Total => Correct + Wrong;
        public int SittingQuality { get; set; }
    }
}