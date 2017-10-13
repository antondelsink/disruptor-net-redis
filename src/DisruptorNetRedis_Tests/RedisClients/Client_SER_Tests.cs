using Microsoft.VisualStudio.TestTools.UnitTesting;
using NFluent;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace DNR.Tests.RedisClients
{
    [TestClass]
    public class Client_StackExchange_Tests
    {
        [TestMethod]
        public void Test_StackExchange_Connect_SET_GET()
        {
            var ep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 55001);

            var s = new DisruptorNetRedis.Server(ep);
            s.Start();

            var cfg = new ConfigurationOptions()
            {
                EndPoints = { ep }
            };

            var cmx = ConnectionMultiplexer.Connect(cfg);

            var redis = cmx.GetDatabase();

            redis.StringSet("__key__", "__value__");

            var response = redis.StringGet("__key__");
            Check.That(response).IsEqualTo("__value__");
        }
    }
}
