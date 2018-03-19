using System;

namespace SpineHero.Monitoring.Watchers.Management.Results
{
    public class ResultMessage
    {
        public ResultMessage(string watchersName)
        {
            TimeStamp = DateTime.Now;
            WatchersName = watchersName;
        }

        public ResultMessage(string watchersName, int sittingQuality, int certainty)
            : this(watchersName)
        {
            SittingQuality = sittingQuality;
            Certainty = certainty;
            DetectedPosture = PostureHelper.GetPosture(sittingQuality);
        }

        public ResultMessage(string watchersName, int wrongSitting, int certainty, Posture detectedPosture)
            : this(watchersName, wrongSitting, certainty)
        {
            DetectedPosture = detectedPosture;
        }

        public string WatchersName { get; private set; }

        public int SittingQuality { get; private set; }

        public int Certainty { get; private set; }

        public Posture DetectedPosture { get; private set; }

        public DateTime TimeStamp { get; private set; }

        public override string ToString()
        {
            return $"{base.ToString()}: {WatchersName}, {SittingQuality}, {DetectedPosture}";
        }
    }
}