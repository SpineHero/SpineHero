using Caliburn.Micro;
using NUnit.Framework;
using SpineHero.Model.Notifications;
using SpineHero.Model.Notifications.PopupNotification;
using SpineHero.Model.Statistics;
using SpineHero.Model.Store;
using SpineHero.PostureMonitoring;
using SpineHero.PostureMonitoring.Managers;
using SpineHero.Utils.Extentions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SpineHero.Monitoring.Watchers.Management.Results;

namespace SpineHero.UnitTests.Model.Store
{
    [TestFixture]
    internal class StatisticsTests : AssertionHelper
    {
        private readonly string path = Path.GetTempPath() + "SpineHeroStatisticsTests.db";
        private SpineHero.Model.Store.Statistics statistics;

        [SetUp]
        public void InitBeforeTest()
        {
            statistics = new SpineHero.Model.Store.Statistics(new EventAggregator())
            {
                Database = { ConnectionString = path }
            };
        }

        [TearDown]
        public void CleanupAfterTest()
        {
            File.Delete(path);
        }

        [Test]
        public void HandleEvaluationTest()
        {
            var time = DateTime.Now;
            statistics.Handle(new PostureMonitoringStatusChange(true), time);
            for (int i = 1; i <= 10; i++)
            {
                statistics.Handle(new Evaluation(100) { EvaluatedAt = time.AddSeconds(i) });
            }
            statistics.Handle(new PostureMonitoringStatusChange(false));
            var bucket = statistics.Database.FindOne<Bucket>(x => x.Time == time.DayAndHour());
            Expect(bucket.SittingQuality, EqualTo(100));
            Expect(bucket.Posture[(int)Posture.Correct], EqualTo(10000));

            statistics.Handle(new PostureMonitoringStatusChange(true), time.AddSeconds(10));
            for (int i = 11; i <= 20; i++)
            {
                statistics.Handle(new Evaluation(0) { EvaluatedAt = time.AddSeconds(i) });
            }
            statistics.Handle(new PostureMonitoringStatusChange(false));
            bucket = statistics.Database.FindOne<Bucket>(x => x.Time == time.DayAndHour());
            Expect(bucket.SittingQuality, EqualTo(50));
            Expect(bucket.Posture[(int)Posture.Correct], EqualTo(10000));
            Expect(bucket.Posture[(int)Posture.Wrong], EqualTo(10000));
        }

        [Test]
        public void HandleNotificationShownEventTest()
        {
            statistics.Handle(new NotificationShownEvent(nameof(PopupNotification), DateTime.Today));
            var list = statistics.Database.FindAll<NotificationShownEvent>();
            var ret = list.Last();
            Expect(ret, Not.Null);
            Expect(ret.ShownAt, EqualTo(DateTime.Today));
            Expect(ret.NotificationType, EqualTo(nameof(PopupNotification)));
            Expect(ret.Id, EqualTo(1));
        }

        [Test]
        public void HandlePostureTest()
        {
            var time = DateTime.Today;
            statistics.Handle(Posture.Correct, time);
            statistics.Handle(Posture.Wrong, time.AddMinutes(1));
            statistics.Handle(new PostureMonitoringStatusChange(false), time.AddMinutes(2));
            var postures = statistics.Database.FindAll<PostureTime>();
            Expect(postures.Count(), EqualTo(3));
        }

        [Test]
        public void UpdateCurrentBucketInHistoryDataTest()
        {
            var now = DateTime.Now.DayAndHour();
            var data = new List<HistoryData>() { new HistoryData { Time = now } };

            statistics.Handle(new PostureMonitoringStatusChange(true), now);
            statistics.Handle(new Evaluation(100) { EvaluatedAt = now.AddSeconds(1) });
            statistics.UpdateCurrentBucketInHistoryData(data);
            Expect(data.Count, EqualTo(1));
            Expect(data.Last().SittingQuality, EqualTo(100));

            data.Last().Time = now.AddHours(-1);
            statistics.Handle(new Evaluation(0) { EvaluatedAt = now.AddSeconds(2) });
            statistics.UpdateCurrentBucketInHistoryData(data);
            Expect(data.Count, EqualTo(2));
            Expect(data.Last().SittingQuality, EqualTo(50));
        }
    }
}