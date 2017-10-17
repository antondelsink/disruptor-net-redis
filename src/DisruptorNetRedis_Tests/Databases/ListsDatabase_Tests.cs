using DisruptorNetRedis.Databases;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NFluent;
using System.Collections.Generic;

namespace DisruptorNetRedis.Tests.Databases
{
    [TestClass]
    public class ListsDatabase_Tests
    {
        ListsDatabase _db = null;

        [TestInitialize]
        public void Test_Init()
        {
            _db = new ListsDatabase();
        }

        /// <summary>
        /// https://redis.io/commands/lpush
        /// </summary>
        [TestMethod]
        public void Test_ListsDatabase_LPush()
        {
            var key = new RedisKey("key");
            var vals = new RedisValue[] { "a", "b", "c" };

            _db.LPush(key, vals);

            Check.That(_db.ListsDictionary[key]).ContainsExactly(new List<RedisValue>() { "c", "b", "a" });
        }

        /// <summary>
        /// https://redis.io/commands/rpush
        /// </summary>
        [TestMethod]
        public void Test_ListsDatabase_RPush()
        {
            var key = new RedisKey("key");
            var vals = new RedisValue[] { "a", "b", "c" };

            _db.RPush(key, vals);

            Check.That(_db.ListsDictionary[key]).ContainsExactly(new RedisValue[] { "a", "b", "c" });
        }

        /// <summary>
        /// https://redis.io/commands/lrange
        /// </summary>
        [TestMethod]
        public void Test_ListsDatabase_LRange()
        {
            var key = new RedisKey("key");
            var vals = new RedisValue[] { "a", "b", "c", "x", "y", "z" };

            _db.LPush(key, vals);

            Check.That(_db.LRange(key, 0, 0).Count).IsEqualTo(1);
            Check.That(_db.LRange(key, 0, 1).Count).IsEqualTo(2);
            Check.That(_db.LRange(key, 0, -1).Count).IsEqualTo(6);
            Check.That(_db.LRange(key, -4, -3).Count).IsEqualTo(2);
            Check.That(_db.LRange(key, 3, 2).Count).IsEqualTo(0);
        }

        /// <summary>
        /// https://redis.io/commands/lindex
        /// </summary>
        [TestMethod]
        public void Test_ListsDatabase_LIndex()
        {
            var key = new RedisKey("key");
            var vals = new RedisValue[] { "a", "b", "c", "x", "y", "z" };

            _db.LPush(key, vals);

            Check.That(_db.LIndex(key, 0)).IsEqualTo("z");
            Check.That(_db.LIndex(key, -1)).IsEqualTo("a");
        }
    }
}