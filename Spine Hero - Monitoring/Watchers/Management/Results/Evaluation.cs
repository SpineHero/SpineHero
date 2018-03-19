using System;

namespace SpineHero.Monitoring.Watchers.Management.Results
{
    public class Evaluation
    {
        public Evaluation()
        {
            EvaluatedAt = DateTime.Now;
        }

        public Evaluation(int sittingQuality) : this()
        {
            SittingQuality = sittingQuality;
            Posture = PostureHelper.GetPosture(sittingQuality);
        }

        public Evaluation(int sittingQuality, Posture posture) : this()
        {
            SittingQuality = sittingQuality;
            Posture = posture;
        }

        public override string ToString()
        {
            return base.ToString() + $" - SittingQuality: {SittingQuality,3}, Posture: {Posture}";
        }

        public int SittingQuality { get; set; }

        public Posture Posture { get; set; }

        public DateTime EvaluatedAt { get; set; }

        public bool IsWrong()
        {
            return SittingQualityLevelHelper.GetLevel(SittingQuality) == SittingQualityLevel.Wrong &&
                   Posture != Posture.Unknown;
        }

        public bool IsCorrect()
        {
            return SittingQualityLevelHelper.GetLevel(SittingQuality) == SittingQualityLevel.Correct &&
                   Posture != Posture.Unknown;
        }

        public bool IsUnknown()
        {
            return Posture == Posture.Unknown;
        }
    }
}