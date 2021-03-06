﻿using DisruptorNetRedis.Networking;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using RedisServerProtocol;

namespace DisruptorNetRedis.LongRunningTests
{
    [TestClass]
    public class TCP_Tests
    {
        /// <summary>
        /// Round-trip the SET command from the client through TCP read & write, excluding server-side processing.
        /// </summary>
        /// <remarks>
        /// Tested on Intel Xeon CPU E3-1505M v5 @ 2.80GHz, 4 Core(s), 8 Logical Processor(s)
        /// Once the test is running (max 5mins by default) launch either:
        /// for a single client connecting, noting response times less than 3ms, and 20,000reqs/s:
        ///     "redis-benchmark -c 1 -n 100000 -P 1 -t SET -d 128 -r 8 -p 55001"
        ///     "redis-benchmark -c 1 -n 100000 -P 10 -t SET -d 128 -r 8 -p 55001"
        ///     "redis-benchmark -c 1 -n 1000000 -P 100 -t SET -d 128 -r 8 -p 55001"
        ///     or
        /// for 100 clients connecting simultaneously, note throughput should be above 200,000reqs/sec:
        ///     "redis-benchmark -c 100 -n 1000000 -P 10 -t SET -d 128 -r 8 -p 55001"
        /// </remarks>
        [TestMethod]
        public void Test_01_TCP_NoOp_RoundTrip_Run5mins()
        {
            var endpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 55001);

            var sessionManager = new SessionManager(endpoint);

            sessionManager.OnNewSession += (ClientSession newSession) =>
            {
                newSession.Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, 8 * 1024);

                newSession.ClientDataStream = new NetworkStream(newSession.Socket, true);

                newSession.Buffer = new byte[1];

                newSession.ClientDataStream
                    .ReadAsync(newSession.Buffer, 0, newSession.Buffer.Length)
                    .ContinueWith(OnReadContinueWithNewArray, newSession);
            };

            sessionManager.Start();

            Thread.Sleep(5 * 60 * 1000);
        }

        private static void OnReadContinueWithNewArray(Task<int> t, object state)
        {
            if (t.IsFaulted || t.IsCanceled)
                return;

            var session = state as ClientSession ?? throw new InvalidOperationException();

            var bytesRead = t.Result;
            if (bytesRead == 1)
            {
                if (session.Buffer[0] == '*')
                {
                    try
                    {
                        List<byte[]> data = RESP.ReadRespArray(session.ClientDataStream);

                        OnRespArrayAvailable(session, data);
                    }
                    catch (System.IO.IOException)
                    {
                        return;
                    }
                    catch (System.Net.ProtocolViolationException)
                    {
                        return;
                    }
                }
                else
                {
                    return;
                }
            }

            session.ClientDataStream
                .ReadAsync(session.Buffer, 0, session.Buffer.Length)
                .ContinueWith(OnReadContinueWithNewArray, session);
        }

        private static void OnRespArrayAvailable(ClientSession session, List<byte[]> data)
        {
            var buffer = Constants.OK_SimpleStringAsByteArray;
            session.ClientDataStream.Write(buffer, 0, buffer.Length);
            session.ClientDataStream.Flush();
        }
    }
}