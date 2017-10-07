﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;

namespace DisruptorNetRedis.LongRunningTests
{
    [TestClass]
    public class Server_Tests
    {
        [TestMethod]
        public void Test_Server_Startup()
        {
            var listenOn = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 55001);

            using (var s = new Server(listenOn))
            {
                s.Start();

                Thread.Sleep(250);
            }
        }

        [TestMethod]
        public void Test_Server_Run5mins()
        {
            var listenOn = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 55001);

            using (var s = new Server(listenOn))
            {
                s.Start();

                Thread.Sleep(5 * 60 * 1000);
            }
        }
    }
}