using System;
using System.Linq;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

namespace DisruptorNetRedis.Networking
{
    internal class SessionManager : IDisposable
    {
        private TcpListener _tcpListener = null;

        private ConcurrentBag<ClientSession> _sessions = new ConcurrentBag<ClientSession>();

        private Thread _backgroundThread = null;

        internal event Action<ClientSession, List<byte[]>> OnDataAvailable;

        public SessionManager(IPEndPoint listenOn)
        {
            _tcpListener = new TcpListener(listenOn);

            _backgroundThread = new Thread(new ThreadStart(MonitorNetworkStreams))
            {
                Name = $"Network Socket Monitoring Thread",
                IsBackground = true
            };
        }

        public void Start()
        {
            if (_tcpListener == null)
                throw new InvalidOperationException();

            _tcpListener.Start(512);

            AcceptSocketAsync();

            _backgroundThread.Start();
        }

        public void Shutdown()
        {
            Dispose();
        }

        public void Dispose()
        {
            _tcpListener?.Stop();
            _tcpListener = null;

            _sessions?.Clear();
            _sessions = null;

            GC.SuppressFinalize(this);
        }

        private void AcceptSocketAsync()
        {
            _tcpListener?.AcceptSocketAsync().ContinueWith(OnTcpSocketAccept);
        }

        private void OnTcpSocketAccept(Task<Socket> task)
        {
            if (!task.IsFaulted)
                CreateSession(task.Result);

            AcceptSocketAsync();
        }

        private void CreateSession(Socket socket)
        {
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);

            _sessions.Add(
                new ClientSession()
                {
                    RemoteEndPoint = (IPEndPoint)socket.RemoteEndPoint,
                    ClientDataStream = new NetworkStream(socket, true)
                });
        }

        private void MonitorNetworkStreams()
        {
            while (_sessions != null)
            {
                var localSessions = _sessions;
                if (localSessions != null)
                {
                    foreach (var s in localSessions)
                    {
                        if (s.ClientDataStream == null)
                            continue;

                        var ns = (NetworkStream)s.ClientDataStream;
                        if (ns.DataAvailable)
                        {
                            try
                            {
                                RESP.ReadOneArray(ns, out List<byte[]> data);

                                if (data != null)
                                    OnDataAvailable?.Invoke(s, data);
                            }
                            catch (System.Net.ProtocolViolationException)
                            {
                                s.ClientDataStream = null;
                            }
                            catch (System.IO.IOException)
                            {
                                s.ClientDataStream = null;
                            }
                        }
                    }
                }
            }
            Thread.Yield();
        }
    }
}