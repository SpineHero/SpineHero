namespace SpineHero.PostureMonitoring
{

    public class AnalyzePeriodChange
    {
        public AnalyzePeriodChange(int time)
        {
            PeriodTime = time;
        }

        public int PeriodTime { get; private set; }
    }
}