using System;

namespace SpineHero.Utils.Extentions
{
    public static class DateTimeExtentions
    {
        public static DateTime DayAndHour(this DateTime time)
        {
            return time.Date.AddHours(time.Hour);
        }
    }
}