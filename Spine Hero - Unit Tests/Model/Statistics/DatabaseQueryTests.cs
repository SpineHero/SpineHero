using NUnit.Framework;
using SpineHero.Model.Statistics;
using SpineHero.Model.Store;
using SpineHero.Monitoring.Watchers.Management.Results;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SpineHero.UnitTests.Model.Statistics
{
    [TestFixture]
    internal class DatabaseQueryTests : AssertionHelper
    {
        private static readonly string path = Path.GetTempPath() + "SpineHeroDatabaseQueryTests.db";
        private readonly Database db = new Database(path);
        private DatabaseQuery databaseQuery;
        private DateTime time = new DateTime(2016, 1, 1);

        [OneTimeSetUp]
        public void Init()
        {
            FillDatabase();
            FillDatabasePostureTime(time);
            FillDatabasePostureTime(time.AddMinutes(100));
        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            File.Delete(path);
        }

        [SetUp]
        public void InitBeforeTest()
        {
            databaseQuery = new DatabaseQuery
            {
                Database = db
            };
        }

        [Test]
        public void QueryHistoryDataTestDay()
        {
            var result = databaseQuery.QueryHistoryData(time, DateRange.Day);
            Expect(result.Count, EqualTo(9));
            Expect(result.First().Time, EqualTo(time.AddHours(8)));
            Expect(result.Last().Time, EqualTo(time.AddHours(16)));
            for (int i = 0; i < 8; i++)
            {
                var total = 1800000 + i * 100000;
                ExpectHistoryData(result[i], total, 64 + i + 8);
            }
        }

        [Test]
        public void QueryHistoryDataTestWeek()
        {
            var result = databaseQuery.QueryHistoryData(time, DateRange.Week);
            Expect(result.Count, EqualTo(7));
            Expect(result.First().Time, EqualTo(time.AddHours(8)));
            Expect(result.Last().Time, EqualTo(time.AddDays(6).AddHours(8)));
            for (int i = 0; i < 7; i++)
            {
                var total = (1800000 + 2600000) * 9 / 2;
                ExpectHistoryData(result[i], total, 64 + (8 + 16) / 2);
            }
        }

        [Test]
        public void QueryHistoryDataTestMonth()
        {
            var result = databaseQuery.QueryHistoryData(time, DateRange.Month);
            Expect(result.Count, EqualTo(31));
            Expect(result.First().Time, EqualTo(time.AddHours(8)));
            Expect(result.Last().Time, EqualTo(time.AddMonths(1).AddDays(-1).AddHours(8)));
            for (int i = 0; i < 31; i++)
            {
                var total = (1800000 + 2600000) * 9 / 2;
                ExpectHistoryData(result[i], total, 64 + (8 + 16) / 2);
            }
        }

        [Test]
        public void QueryHistoryDataTestYear()
        {
            var result = databaseQuery.QueryHistoryData(time, DateRange.Year);
            Expect(result.Count, EqualTo(12));
            Expect(result.First().Time, EqualTo(time.AddHours(8)));
            Expect(result.Last().Time, EqualTo(time.AddYears(1).AddMonths(-1).AddHours(8)));
            for (int i = 0; i < 12; i++)
            {
                var days = DateTime.DaysInMonth(2016, i + 1);
                var total = (1800000 + 2600000) * 9 / 2 * days;
                ExpectHistoryData(result[i], total, 64 + (8 + 16) / 2);
            }
        }

        private void ExpectHistoryData(HistoryData result, int total, int sq)
        {
            Expect(result.SittingQuality, EqualTo(sq));
            Expect(result.Total, EqualTo(total));
            Expect(result.Unknown, EqualTo(total / 10));
            Expect(result.Correct, EqualTo(total / 2));
            Expect(result.Wrong, EqualTo(total / 2));
        }

        private static Bucket CreateBucket(DateTime time, long total, int sittingQuality)
        {
            return new Bucket
            {
                Time = time,
                SittingQuality = sittingQuality,
                SittingTimeSum = total,
                SittingQualitySum = sittingQuality * total,
                Posture = new[] { total / 10, total / 2, total / 4, total / 20, total / 20, total / 20, total / 20, total / 20 }
            };
        }

        private void FillDatabase()
        {
            var list = new List<Bucket>();
            var diff = (time.AddYears(1) - time).TotalDays;
            for (int j = 0; j < diff; j++)
            {
                for (int i = 8; i <= 16; i++)
                {
                    var total = 1000000 + i * 100000;
                    var bucket = CreateBucket(time.AddDays(j).AddHours(i), total, 64 + i);
                    list.Add(bucket);
                }
            }
            db.Insert<Bucket>(list);
        }

        private void FillDatabasePostureTime(DateTime time)
        {
            var postures = new List<PostureTime>();
            for (int j = 0; j < 8; j++)
            {
                postures.Add(new PostureTime((Posture)j, time.AddMinutes(j * 10)));
            }
            postures.Add(new PostureTime(null, time.AddMinutes(80)));
            db.Insert<PostureTime>(postures);
        }

        [Test]
        public void QueryLongestPostureTimeTest()
        {
            for (int i = 0; i < 8; i++)
            {
                var length = databaseQuery.QueryLongestPostureTime((Posture)i, time, time.AddDays(1));
                Expect(length, EqualTo(TimeSpan.FromMinutes(10)));
            }
        }

        [Test]
        public void QueryCountPostureOccurrencesOfMinimalLength()
        {
            for (int i = 0; i < 8; i++)
            {
                var count = databaseQuery.QueryCountPostureOccurrencesOfMinimalLength(Posture.Correct, time, time.AddDays(1), TimeSpan.FromMinutes(5));
                Expect(count, EqualTo(2));
            }
        }
    }
}