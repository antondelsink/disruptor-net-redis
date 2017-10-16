using Microsoft.VisualStudio.TestTools.UnitTesting;
using NFluent;
using Sider;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace DNR.Tests.RedisClients
{
    [TestClass]
    public class Client_Sider_Tests
    {
        [TestMethod]
        public void Test_Sider_Connect_SET_GET()
        {
            var port = 55002;
            var ep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), port);

            var s = new DisruptorNetRedis.Server(ep);
            s.Start();
            
            var redis = new RedisClient(port:port);

            redis.Set("__key__", "__value__");

            var response = redis.Get("__key__");
            Check.That(response).IsEqualTo("__value__");

            s.Stop();
        }
    }
}
