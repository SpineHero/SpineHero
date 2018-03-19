using Caliburn.Micro;
using LiteDB;
using SpineHero.Common.Logging;
using SpineHero.Common.Resources;
using SpineHero.Model.Notifications;
using SpineHero.Model.Statistics;
using SpineHero.Monitoring.Watchers.Management.Results;
using SpineHero.PostureMonitoring.Managers;
using SpineHero.Properties;
using SpineHero.Utils.Extentions;
using SpineHero.Utils.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using Logger = SpineHero.Utils.Logging.Logger;

namespace SpineHero.Model.Store
{
    public class Statistics : PropertyChangedBase, IHandle<Evaluation>, IHandle<Posture>, IHandle<NotificationShownEvent>, IHandle<PostureMonitoringStatusChange>
    {
        private static readonly ILogger logger = Logger.GetLogger<Statistics>();
        private DateTime time;
        private IList<PostureTime> postureTime = new List<PostureTime>();

        public Statistics(IEventAggregator ea)
        {
            ea.Subscribe(this);
        }

        public Database Database { get; set; } = new Database(ResourceHelper.GetAppDataLocalResourcePath(Settings.Default.DatabasePath));

        public Bucket CurrentBucket { get; private set; }

        #region Handle events

        public void Handle(Evaluation message)
        {
            if (CurrentBucket == null) throw new ArgumentException(nameof(CurrentBucket));

            var evalAt = message.EvaluatedAt.DayAndHour();
            if (time.Minute != message.EvaluatedAt.Minute) SaveData();
            if (CurrentBucket.Time != evalAt)
            {
                SaveData();
                CurrentBucket = new Bucket();
            }

            var diff = (long)message.EvaluatedAt.Subtract(time).TotalMilliseconds;
            time = message.EvaluatedAt;

            CurrentBucket.Posture[(int)message.Posture] += diff;

            if (message.Posture != Posture.Unknown)
            {
                CurrentBucket.SittingTimeSum += diff;
                CurrentBucket.SittingQualitySum += diff * message.SittingQuality;
                CurrentBucket.SittingQuality = CurrentBucket.SittingTimeSum == 0 ? 0 : (int)(CurrentBucket.SittingQualitySum / CurrentBucket.SittingTimeSum);
            }

            NotifyOfPropertyChange(() => CurrentBucket);
        }

        public void Handle(NotificationShownEvent message)
        {
            Database.Insert(message);
        }

        public void Handle(Posture posture)
        {
            Handle(posture, DateTime.Now);
        }

        public void Handle(Posture posture, DateTime startAt)
        {
            postureTime.Add(new PostureTime(posture, startAt));
        }

        public void Handle(PostureMonitoringStatusChange message)
        {
            Handle(message, DateTime.Now);
        }

        public void Handle(PostureMonitoringStatusChange message, DateTime setTime)
        {
            if (message.IsMonitoring)
            {
                time = setTime;
                CurrentBucket = Database.FindOne<Bucket>(x => x.Time == DateTime.Now.DayAndHour()) ?? new Bucket();
            }
            else
            {
                postureTime.Add(new PostureTime(null, setTime));
                SaveData();
            }
        }

        #endregion Handle events

        [LogMethodCall]
        public void SaveData()
        {
            if (CurrentBucket != null)
            {
                logger.Debug(CurrentBucket.ToString());
                Database.InsertOrUpdate(CurrentBucket);
            }
            if (postureTime.Any())
            {
                Database.Insert<PostureTime>(postureTime);
                postureTime.Clear();
            }
        }

        public void UpdateCurrentBucketInHistoryData(List<HistoryData> data)
        {
            var bucket = CurrentBucket;
            if (bucket != null && data != null)
            {
                if (bucket.Time == data.LastOrDefault()?.Time) data.RemoveAt(data.Count - 1);
                data.Add(new HistoryData(bucket));
            }
        }
    }

    public class Bucket
    {
        public Bucket()
        {
            Time = DateTime.Today.AddHours(DateTime.Now.Hour);
        }

        [BsonId]
        public DateTime Time { get; set; }

        public long[] Posture { get; set; } = new long[8];

        public int SittingQuality { get; set; }

        public long SittingTimeSum { get; set; }

        public long SittingQualitySum { get; set; }

        public override string ToString()
        {
            return base.ToString() + $" {Time}: SittingQuality = {SittingQuality}, SittingQualitySum = {SittingQualitySum}, TimeSum = {SittingTimeSum}, Posture = [{string.Join(", ", Posture)}]";
        }
    }

    public class PostureTime
    {
        public PostureTime()
        {
        }

        public PostureTime(Posture? posture, DateTime startAt)
        {
            StartAt = startAt;
            Posture = posture;
        }

        [BsonId]
        public DateTime StartAt { get; set; }

        public Posture? Posture { get; set; }
    }
}