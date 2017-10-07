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
        public void Test_DNR_Init()
        {
            var _core = new DotNetRedisServer();
            var _dbStrings = new StringsDatabase();

            var session = new ClientSession()
            {
                ClientDataStream = null,
                RemoteEndPoint = null
            };

            var _commands = new RedisCommandDefinitions(_core, _dbStrings);
            var _translator = new MockClientRequestTranslator(_commands);

            using (var dnr = new DisruptorRedis.DisruptorRedis(
                _translator,
                new ClientRequestHandler(),
                new IWorkHandler<RingBufferSlot>[] { new MockResponseHandler() }))
            {
                dnr.Start();

                var data = new List<byte[]>()
                {
                    Encoding.UTF8.GetBytes("SET"),
                    Encoding.UTF8.GetBytes("_KEY_"),
                    Encoding.UTF8.GetBytes("_VALUE_")
                };

                dnr.OnDataAvailable(session, data);

                AssertWithTimeout.IsTrue(() => _dbStrings.StringsDictionary.Count == 1, "too slow", TimeSpan.FromMilliseconds(100));
            }
        }
    }
}