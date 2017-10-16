using DisruptorNetRedis.Databases;
using DisruptorNetRedis.DisruptorRedis;
using DisruptorNetRedis.DotNetRedis;
using DisruptorNetRedis.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace DisruptorNetRedis.LongRunningTests
{
    [TestClass]
    public class DisruptorRedis_Tests_Perf
    {
        private static ulong ITERATIONS = 1_000_000;

        private static DisruptorRedis.DisruptorRedis _dnr = null;

        private static Random rnd = new Random();

        private static List<List<byte[]>> dataArray = new List<List<byte[]>>();

        private static DotNetRedisServer _core = new DotNetRedisServer();
        private static StringsDatabase _strings = new StringsDatabase();
        private static ListsDatabase _dbLists = new Databases.ListsDatabase();
        private static SetsDatabase _dbSets = new Databases.SetsDatabase();

        private static RedisCommandDefinitions _commands = new RedisCommandDefinitions(_core, _strings, _dbLists, _dbSets);
        private static MockResponseHandler _commandLogger = new MockResponseHandler();
        private static MockClientRequestTranslator _translator = new MockClientRequestTranslator();
        private static RequestParser _parser = new RequestParser(_commands);

        [ClassInitialize]
        public static void Class_Init(TestContext ctx)
        {
            GenerateSetCommands();

            _dnr = new DisruptorRedis.DisruptorRedis(
                _translator,
                _parser,
                new RequestHandler(),
                _commandLogger);

            _dnr.Start();

            Thread.Sleep(1000);
        }

        [ClassCleanup]
        public static void Class_Cleanup()
        {
            _dnr.Dispose();
        }

        /// <summary>
        /// Invoke the SET operation 1million times against the disruptor-based Redis server, excluding TCP (no network IO in this test).
        /// </summary>
        /// <remarks>
        /// Tested on Intel Xeon CPU E3-1505M v5 @ 2.80GHz, 4 Core(s), 8 Logical Processor(s).
        /// The test completes in approx 700ms on average, approx 1.4M requests per second.
        /// </remarks>
        [TestMethod]
        public void Test_03_DisruptorRedis_1million_SET()
        {
            var sw = Stopwatch.StartNew();
            foreach (var req in dataArray)
            {
                _dnr.OnDataAvailable(null, req);
            }
            sw.Stop();
            Debug.WriteLine($"stopwatch: {sw.ElapsedMilliseconds.ToString()}ms");

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