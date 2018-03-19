using SpineHero.Common.Logging;
using SpineHero.Common.Resources;
using SpineHero.Model.Notifications;
using SpineHero.Model.Statistics;
using SpineHero.Monitoring.Watchers.Management.Results;
using SpineHero.Properties;
using SpineHero.Utils.Extentions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpineHero.Model.Store
{
    public class DatabaseQuery
    {
        public Database Database { get; set; } = new Database(ResourceHelper.GetAppDataLocalResourcePath(Settings.Default.DatabasePath));

        #region HistoryData

        private IEnumerable<Bucket> historyData;
        private DateTime historyDataStartTime;
        private DateTime historyDataEndTime;
        private DateTime queryCreated = DateTime.MinValue;

        [LogMethodCall]
        public List<HistoryData> QueryHistoryData(DateTime startTime, DateRange timeRange)
        {
            var endTime = timeRange.GetEndTime(startTime);
            var now = DateTime.Now.DayAndHour();
            if (queryCreated.AddHours(1) == now) InvalidateHistoryData();

            if (historyData == null || !historyData.Any() || startTime < historyDataStartTime || endTime > historyDataEndTime)
            {
                historyDataStartTime = startTime;
                historyDataEndTime = endTime;
                historyData = Database.Find<Bucket>(x => x.Time >= startTime && x.Time < endTime);
                queryCreated = now;
            }
            long c, w;
            return (from bucket in historyData
                    where bucket.Time >= startTime && bucket.Time < endTime
                    group bucket by
                    (timeRange == DateRange.Day)
                        ? bucket.Time.Hour
                        : (timeRange == DateRange.Year) ? bucket.Time.Month : bucket.Time.Day
                into grouping
                    select new HistoryData
                    {
                        Time = grouping.First().Time,
                        Correct = c = grouping.Sum(x => x.Posture[(int)Posture.Correct]),
                        Unknown = grouping.Sum(x => x.Posture[(int)Posture.Unknown]),
                        Wrong = w = grouping.Sum(x => x.Posture.Skip(2).Sum()),
                        SittingQuality = (c + w == 0) ? 0 : (int)(grouping.Sum(x => x.SittingQualitySum) / (c + w))
                    }).ToList();
        }

        [LogMethodCall]
        public void InvalidateHistoryData()
        {
            historyData = null;
            historyDataEndTime = DateTime.MinValue;
            historyDataEndTime = DateTime.MinValue;
        }

        public TimeSpan QueryLongestPostureTime(Posture posture, DateTime startTime, DateTime endTime)
        {
            var data = Database.Find<PostureTime>(x => x.StartAt >= startTime && x.StartAt < endTime);
            var longest = TimeSpan.Zero;
            for (int i = 0; i < data.Length; i++)
            {
                var current = data[i];
                if (current.Posture == posture)
                {
                    if (i + 1 >= data.Length) break; ;
                    var next = data[i + 1];
                    var duration = next.StartAt - current.StartAt;
                    if (duration > longest) longest = duration;
                }
            }
            return longest;
        }

        public int QueryCountPostureOccurrencesOfMinimalLength(Posture posture, DateTime startTime, DateTime endTime, TimeSpan minimum)
        {
            int count = 0;
            var data = Database.Find<PostureTime>(x => x.StartAt >= startTime && x.StartAt < endTime);
            for (int i = 0; i < data.Length; i++)
            {
                var current = data[i];
                if (current.Posture == posture)
                {
                    if (i + 1 >= data.Length) break; ;
                    var next = data[i + 1];
                    var duration = next.StartAt - current.StartAt;
                    if (duration > minimum) count++;
                }
            }
            return count;
        }

        #endregion HistoryData

        #region Notifications

        [LogMethodCall]
        public int QueryNotificationShownCount(DateTime startTime, DateTime endTime)
        {
            return Database.Count<NotificationShownEvent>(x => x.ShownAt >= startTime && x.ShownAt < endTime);
        }

        #endregion Notifications
    }
}