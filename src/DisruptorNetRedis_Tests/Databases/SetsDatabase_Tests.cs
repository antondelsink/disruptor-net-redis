using DisruptorNetRedis.Databases;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NFluent;
using System;
using System.Collections.Generic;
using System.Text;

namespace DisruptorNetRedis.Tests.Databases
{
    [TestClass]
    public class SetsDatabase_Tests
    {
        private SetsDatabase _db = new SetsDatabase();

        /// <summary>
        /// https://redis.io/commands/sadd
        /// </summary>
        [TestMethod]
        public void Test_SetsDatabase_SAdd()
        {
            var key = new RedisKey("theKey");
            var val = new RedisValue("theValue");

            _db.SAdd(key, new RedisValue[] { val });

            Check.That(_db.SetsDictionary.Count).IsEqualTo(1);
            Check.That(_db.SetsDictionary.ContainsKey(key)).IsTrue();
            Check.That(_db.SetsDictionary[key].Count).IsEqualTo(1);
            Check.That(_db.SetsDictionary[key].Contains(val));
        }

        /// <summary>
        /// https://redis.io/commands/scard
        /// </summary>
        [TestMethod]
        public void Test_SetsDatabase_SCard()
        {
            var key = new RedisKey("theKey");
            var val = new RedisValue("theValue");

            _db.SAdd(key, new RedisValue[] { val });

            var cardinality = _db.SCard(key);

            Check.That(cardinality).IsEqualTo(1);
        }

        [TestMethod]
        public void Test_SetsDatabase_SUnion()
        {
            _db.SAdd("kSetOne", new RedisValue[] { "s1v1" });
            _db.SAdd("kSetTwo", new RedisValue[] { "s2v1", "s2v2" });
            _db.SAdd("kSetThree", new RedisValue[] { "s3v1", "s3v2", "s3v3" });

            Check.That(_db.SetsDictionary["kSetOne"].Count).IsEqualTo(1);
            Check.That(_db.SetsDictionary["kSetTwo"].Count).IsEqualTo(2);
            Check.That(_db.SetsDictionary["kSetThree"].Count).IsEqualTo(3);

            var union = _db.SUnion("kSetOne", "kSetThree");
            Check.That(union.Count).IsEqualTo(4);
        }
    }
}
