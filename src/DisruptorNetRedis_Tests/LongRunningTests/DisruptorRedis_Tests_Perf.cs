using Disruptor;
using DisruptorNetRedis.Databases;
using DisruptorNetRedis.DisruptorRedis;
using DisruptorNetRedis.DotNetRedis;
using DisruptorNetRedis.Networking;
using DisruptorNetRedis.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace DisruptorNetRedis.LongRunningTests
{
    [TestClass]
    public class DisruptorRedis_Tests_Perf
    {
        private static ulong ITERATIONS = 1_000_000;

        private static DisruptorRedis.DisruptorRedis _dnr = null;

        private static Random rnd = new Random();

        private static List<List<byte[]>> dataArray = new List<List<byte[]>>();

        private static DotNetRedisCore _core = new DotNetRedisCore();
        private static StringsDatabase _strings = new StringsDatabase();
        private static MockResponseHandler _commandLogger = new MockResponseHandler();
        private static MockClientRequestTranslator _translator = new MockClientRequestTranslator();

        [ClassInitialize]
        public static void Class_Init(TestContext ctx)
        {
            GenerateSetCommands();

            _dnr = new DisruptorRedis.DisruptorRedis(
                _translator,
                new ClientRequestHandler(_core, _strings),
                new IWorkHandler<RingBufferSlot>[] { _commandLogger });

            _dnr.Start();
        }

        [ClassCleanup]
        public static void Class_Cleanup()
        {
            _dnr.Dispose();
        }

        [TestMethod]
        public void Run_DNR_Perf__Set()
        {
            var session = new ClientSession()
            {
                ClientDataStream = null,
                RemoteEndPoint = null
            };

            foreach (var req in dataArray)
            {
                _dnr.OnDataAvailable(session, req);
            }

            AssertWithTimeout.IsTrue(() => ITERATIONS == (ulong)_strings.StringsDictionary.Count, "TOO SLOW!", TimeSpan.FromMilliseconds(1_000));
        }

        private static void GenerateSetCommands()
        {
            for (ulong ix = 0; ix < ITERATIONS; ix++)
            {
                byte[] key = new byte[8];
                rnd.NextBytes(key);

                byte[] val = new byte[128];
                rnd.NextBytes(val);

                dataArray.Add(new List<byte[]>()
                {
                    Encoding.UTF8.GetBytes("SET"),
                    key,
                    val
                });
            };
        }
    }
}
