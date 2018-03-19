using SpineHero.Monitoring.Properties;

namespace SpineHero.Monitoring.Watchers.Management.Results
{
    public enum SittingQualityLevel
    {
        Unknown,
        Wrong,
        Warning,
        Correct,
    }

    public static class SittingQualityLevelHelper
    {
        private static readonly int CORRECT_LIMIT = Const.Default.CorrectLimit;
        private static readonly int WARNING_LIMIT = Const.Default.WarningLimit;

        public static SittingQualityLevel GetLevel(int sittingQuality)
        {
            if (sittingQuality > 100 || sittingQuality < 0) return SittingQualityLevel.Unknown;
            if (sittingQuality >= CORRECT_LIMIT)
                return SittingQualityLevel.Correct;
            if (sittingQuality >= WARNING_LIMIT)
                return SittingQualityLevel.Warning;
            return SittingQualityLevel.Wrong;
        }
    }

    public enum Posture
    {
        Unknown,
        Correct,
        Wrong,
        LeanBackward,
        LeanForward,
        LeanLeft,
        LeanRight,
        Slouch
    }

    public static class PostureHelper
    {
        public static Posture GetPosture(int sittingQuality)
        {
            var l = SittingQualityLevelHelper.GetLevel(sittingQuality);
            switch (l)
            {
                case SittingQualityLevel.Wrong:
                    return Posture.Wrong;

                case SittingQualityLevel.Unknown:
                    return Posture.Unknown;

                default:
                    return Posture.Correct;
            }
        }
    }
}