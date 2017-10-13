using DisruptorNetRedis.Networking;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace DisruptorNetRedis.LongRunningTests
{
    [TestClass]
    public class TCP_Tests
    {
        /// <summary>
        /// Round-trip the SET command from the client through TCP read & write, excluding server-side processing.
        /// </summary>
        /// <remarks>
        /// Once the test is running (max 5mins by default) launch: "redis-benchmark -c 1 -n 1000000 -P 100 -t SET -d 128 -r 8 -p 55001"
        /// </remarks>
        [TestMethod]
        public void Test_TCP()
        {
            var endpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 55001);

            var sessionManager = new SessionManager(endpoint);

            sessionManager.OnNewSession += (ClientSession newSession) =>
            {
                newSession.ClientDataStream = new NetworkStream(newSession.Socket, true);
            };

            sessionManager.OnDataAvailable += Session_OnDataAvailable;
            sessionManager.OnRespArrayAvailable += Session_OnRespArrayAvailable;
            sessionManager.Start();

            Thread.Sleep(5 * 60 * 1000);
        }

        private List<byte[]> Session_OnDataAvailable(ClientSession session)
        {
            RESP.ReadOneArray(session.ClientDataStream, out List<byte[]> data);
            return data;
        }

        private void Session_OnRespArrayAvailable(ClientSession session, List<byte[]> data)
        {
            var buffer = Constants.OK_SimpleStringAsByteArray;
            session.ClientDataStream.Write(buffer, 0, buffer.Length);
        }
    }
}