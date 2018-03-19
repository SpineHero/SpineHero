using System;

namespace SpineHero.Monitoring.DataSources
{
    public static class DataSourceFactory
    {
        public static IDataSource CreateOfType(Type dataSourceType)
        {
            return (IDataSource)Activator.CreateInstance(dataSourceType);
        }

        public static IDataSource CreateNewIfNeeded(IDataSource current, Type dataSourceType)
        {
            return (current?.GetType() == dataSourceType) ? current : CreateOfType(dataSourceType);
        }
    }
}
