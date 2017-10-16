using DisruptorNetRedis.DisruptorRedis;
using DisruptorNetRedis.Tests.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;

namespace DisruptorNetRedis.LongRunningTests
{
    [TestClass]
    public class RoundTrip_noOp_Tests
    {
        /// <summary>
        /// Round-trip the SET command from the client through TCP read & write, including mock (no-op) server-side processing.
        /// </summary>
        /// <remarks>
        /// Tested on Intel Xeon CPU E3-1505M v5 @ 2.80GHz, 4 Core(s), 8 Logical Processor(s)
        /// Once the test is running (max 5mins by default) launch either:
        /// for a single client connecting, noting response times less than 10ms, and 20,000reqs/s:
        ///     "redis-benchmark -c 1 -n 100000 -P 1 -t SET -d 128 -r 8 -p 55001"
        ///     "redis-benchmark -c 1 -n 100000 -P 10 -t SET -d 128 -r 8 -p 55001"
        ///     "redis-benchmark -c 1 -n 1000000 -P 100 -t SET -d 128 -r 8 -p 55001"
        ///     or
        /// for 100 clients connecting simultaneously, note throughput should be approx 200,000reqs/sec:
        ///     "redis-benchmark -c 100 -n 1000000 -P 10 -t SET -d 128 -r 8 -p 55001"
        /// </remarks>
        [TestMethod]
        public void Test_02_TCP_NoOpDisruptor_RoundTrip_Run5mins()
        {
            var listenOn = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 55001);

            var disruptor =
                new DisruptorRedis.DisruptorRedis(
                    new MockClientRequestTranslator(),
                    new MockRequestParser(),
                    new MockRequestHandler(),
                    new ClientResponseHandler());

            using (var s = new Server(listenOn, disruptor))
            {
                s.Start();

                Thread.Sleep(5 * 60 * 1000);
            }
        }
    }
}
