using Disruptor;
using DisruptorNetRedis.Databases;
using DisruptorNetRedis.DisruptorRedis;
using DisruptorNetRedis.DotNetRedis;
using DisruptorNetRedis.Networking;
using DisruptorNetRedis.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace DisruptorNetRedis.LongRunningTests
{
    [TestClass]
    public class DisruptorRedis_Tests_Init
    {
        [TestMethod]
        public void Test_Init_DisruptorRedis()
        {
            var _core = new DotNetRedisServer();
            var _dbStrings = new StringsDatabase();
            var _dbLists = new ListsDatabase();
            var _dbSets = new Databases.SetsDatabase();

            var _commands = new RedisCommandDefinitions(_core, _dbStrings, _dbLists, _dbSets);

            using (var dnr = new DisruptorRedis.DisruptorRedis(
                new MockClientRequestTranslator(),
                new RequestParser(_commands),
                new RequestHandler(),
                new MockResponseHandler()))
            {
                dnr.Start();

                var data = new List<byte[]>()
                {
                    Encoding.UTF8.GetBytes("SET"),
                    Encoding.UTF8.GetBytes("_KEY_"),
                    Encoding.UTF8.GetBytes("_VALUE_")
                };

                dnr.OnDataAvailable(null, data);

                AssertWithTimeout.IsTrue(() => _dbStrings.StringsDictionary.Count == 1, "too slow", TimeSpan.FromMilliseconds(100));
            }
        }
    }
}