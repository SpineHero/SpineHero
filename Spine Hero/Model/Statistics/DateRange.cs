using System;

namespace SpineHero.Model.Statistics
{
    public enum DateRange
    {
        Day,
        Week,
        Month,
        Year,
    }

    public static class DateRangeExtensions
    {
        public static DateTime GetEndTime(this DateRange dateRange, DateTime startTime)
        {
            DateTime endDate;
            switch (dateRange)
            {
                case DateRange.Day:
                    endDate = startTime.AddHours(24);
                    break;

                case DateRange.Week:
                    endDate = startTime.AddDays(7);
                    break;

                case DateRange.Month:
                    endDate = startTime.AddMonths(1);
                    break;

                case DateRange.Year:
                    endDate = startTime.AddYears(1);
                    break;

                default:
                    endDate = startTime;
                    break;
            }
            return endDate;
        }
    }
}