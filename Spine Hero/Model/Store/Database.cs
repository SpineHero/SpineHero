using LiteDB;
using PostSharp.Patterns.Contracts;
using SpineHero.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using SpineHero.Utils.Logging;
using Logger = SpineHero.Utils.Logging.Logger;

namespace SpineHero.Model.Store
{
    public class Database
    {
        private readonly ILogger logger = Logger.GetLogger<Database>();

        public Database(string connectionString)
        {
            ConnectionString = connectionString;
            Directory.CreateDirectory(Path.GetDirectoryName(connectionString));
        }

        public string ConnectionString { get; set; }

        public LiteDatabase WrappedDatabase => new LiteDatabase(ConnectionString);

        public T[] Find<T>([NotNull] Expression<Func<T, bool>> func, string collectionName = null) where T : new()
        {
            using (var db = new LiteDatabase(ConnectionString))
            {
                collectionName = collectionName ?? typeof(T).Name;
                var col = db.GetCollection<T>(collectionName);
                return col.Find(func).ToArray();
            }
        }

        public T FindOne<T>([NotNull] Expression<Func<T, bool>> func, string collectionName = null) where T : new()
        {
            using (var db = new LiteDatabase(ConnectionString))
            {
                collectionName = collectionName ?? typeof(T).Name;
                var col = db.GetCollection<T>(collectionName);
                return col.FindOne(func);
            }
        }

        public T FindOne<T>([NotNull] T item, string collectionName = null) where T : new()
        {
            using (var db = new LiteDatabase(ConnectionString))
            {
                collectionName = collectionName ?? typeof(T).Name;
                var col = db.GetCollection<T>(collectionName);
                var property = typeof(T).GetProperties().FirstOrDefault(x => x.GetCustomAttribute(typeof(BsonIdAttribute)) != null);
                var id = new BsonValue(property?.GetValue(item));
                return col.FindById(id);
            }
        }

        public T[] FindAll<T>(string collectionName = null) where T : new()
        {
            using (var db = new LiteDatabase(ConnectionString))
            {
                collectionName = collectionName ?? typeof(T).Name;
                var col = db.GetCollection<T>(collectionName);
                return col.FindAll().ToArray();
            }
        }

        public object Insert<T>([NotNull] T item, string collectionName = null) where T : new()
        {
            using (var db = new LiteDatabase(ConnectionString))
            {
                collectionName = collectionName ?? typeof(T).Name;
                var col = db.GetCollection<T>(collectionName);
                try
                {
                    return col.Insert(item);
                }
                catch (Exception e)
                {
                    logger.Error(e);
                    return null;
                }
            }
        }

        public int Insert<T>([NotNull] IEnumerable<T> item, string collectionName = null) where T : new()
        {
            using (var db = new LiteDatabase(ConnectionString))
            {
                collectionName = collectionName ?? typeof(T).Name;
                var col = db.GetCollection<T>(collectionName);
                try
                {
                    return col.Insert(item);
                }
                catch (Exception e)
                {
                    logger.Error(e);
                    return 0;
                }
            }
        }

        public bool InsertOrUpdate<T>([NotNull] T item, string collectionName = null) where T : new()
        {
            using (var db = new LiteDatabase(ConnectionString))
            {
                collectionName = collectionName ?? typeof(T).Name;
                var col = db.GetCollection<T>(collectionName);
                if (!col.Update(item)) return col.Insert(item) != null;
                return true;
            }
        }

        public bool Update<T>([NotNull] T item, string collectionName = null) where T : new()
        {
            using (var db = new LiteDatabase(ConnectionString))
            {
                collectionName = collectionName ?? typeof(T).Name;
                var col = db.GetCollection<T>(collectionName);
                return col.Update(item);
            }
        }

        public bool Delete<T>([NotNull] T item, string collectionName = null) where T : new()
        {
            using (var db = new LiteDatabase(ConnectionString))
            {
                collectionName = collectionName ?? typeof(T).Name;
                var col = db.GetCollection<T>(collectionName);
                var property = typeof(T).GetProperties().FirstOrDefault(x => x.GetCustomAttribute(typeof(BsonIdAttribute)) != null);
                var id = new BsonValue(property?.GetValue(item));
                return col.Delete(id);
            }
        }

        public int Count<T>(Expression<Func<T, bool>> func = null, string collectionName = null) where T : new()
        {
            using (var db = new LiteDatabase(ConnectionString))
            {
                collectionName = collectionName ?? typeof(T).Name;
                var col = db.GetCollection<T>(collectionName);
                return func == null ? col.Count() : col.Count(func);
            }
        }

        public TResult Aggregate<TResult, TSource>([NotNull]TResult seed, [NotNull] Func<TResult, TSource, TResult> aggregateFunc, Expression<Func<TSource, bool>> func = null, string collectionName = null) where TSource : new()
        {
            using (var db = new LiteDatabase(ConnectionString))
            {
                collectionName = collectionName ?? typeof(TSource).Name;
                var col = db.GetCollection<TSource>(collectionName);
                return func == null ? col.FindAll().Aggregate(seed, aggregateFunc) : col.Find(func).Aggregate(seed, aggregateFunc);
            }
        }
    }
}