using LiteDB;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SpineHero.Model.Store;

namespace SpineHero.UnitTests.Model.Store
{
    [TestFixture]
    internal class DatabaseTests : AssertionHelper
    {
        private readonly string path = Path.GetTempPath() + "SpineHeroDatabaseTests.db";
        private Database db;

        [SetUp]
        public void InitBeforeTest()
        {
            db = new Database(path);
        }

        [TearDown]
        public void CleanupAfterTest()
        {
            File.Delete(path);
        }

        [Test]
        public void FindTest()
        {
            var data = new Data { Value = 50 };

            Expect(db.FindOne(data), Null);
            Expect(db.FindOne<Data>(x => x.Value == data.Value), Null);
            Expect(db.Find<Data>(x => x.Value == data.Value).Count(), EqualTo(0));
            Expect(db.FindAll<Data>().Count(), EqualTo(0));

            Expect(db.Insert(data), Not.Null);

            Expect(db.FindOne(data).Value, EqualTo(data.Value));
            Expect(db.FindOne<Data>(x => x.Value == data.Value)?.Value, EqualTo(data.Value));
            Expect(db.Find<Data>(x => x.Value == data.Value).Count(), EqualTo(1));
            Expect(db.FindAll<Data>().Count(), EqualTo(1));
        }

        [Test]
        public void InsertTest()
        {
            var data = new Data { Value = 50 };
            Expect(db.Insert(data), Not.Null);
            try
            {
                Expect(db.Insert(data), Null);
            }
            catch (Exception) { /* TODO Application insights fix needed */ }
            data.Id = 2;
            Expect(db.Insert(data), Not.Null);
        }

        [Test]
        public void InsertListTest()
        {
            var list = FillList();
            Expect(db.Insert<Data>(list), EqualTo(list.Count));
        }

        private List<Data> FillList()
        {
            var list = new List<Data>();
            for (int i = 0; i < 10; i++)
            {
                list.Add(new Data() { Id = i + 1, Value = i });
            }
            return list;
        }

        [Test]
        public void InsertOrUpdateTest()
        {
            var data = new Data { Value = 50 };
            Expect(db.InsertOrUpdate(data), True);
            Expect(db.FindOne(data).Value, EqualTo(data.Value));

            data.Value = 60;
            Expect(db.InsertOrUpdate(data), True);
            Expect(db.FindOne(data).Value, EqualTo(data.Value));
        }

        [Test]
        public void DeleteTest()
        {
            var data = new Data { Value = 5 };
            Expect(db.FindOne(data), Null);
            Expect(db.Insert(data), Not.Null);
            Expect(db.FindOne(data), Not.Null);
            Expect(db.Delete(data), True);
            Expect(db.FindOne(data), Null);
            Expect(db.Delete(data), False);
        }

        [Test]
        public void CountTest()
        {
            var list = FillList();
            db.Insert<Data>(list);

            var cnt = db.Count<Data>();
            Expect(cnt, EqualTo(list.Count));

            var queryCnt = db.Count<Data>(x => x.Value < 5);
            Expect(queryCnt, EqualTo(list.Count(x => x.Value < 5)));
        }

        [Test]
        public void AggregateSumTest()
        {
            var list = FillList();
            db.Insert<Data>(list);
            var sum = db.Aggregate<int, Data>(0, (i, d) => d.Value + i);
            Expect(sum, EqualTo(list.Sum(x => x.Value)));

            var querySum = db.Aggregate<int, Data>(0, (i, d) => d.Value + i, x => x.Value < 5);
            Expect(querySum, EqualTo(list.FindAll(x => x.Value < 5).Sum(x => x.Value)));
        }
    }

    public class Data
    {
        public Data()
        {
        }

        [BsonId]
        public int Id { get; set; }

        public int Value { get; set; }
    }
}