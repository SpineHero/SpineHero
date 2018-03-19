namespace SpineHero.Monitoring.DataSources
{
    internal class TestDataSource : DataSource
    {
        public override bool LoadNext()
        {
            return true;
        }

        public override void Start()
        {
            Running = true;
        }

        public override void Stop()
        {
            Running = false;
        }
    }
}