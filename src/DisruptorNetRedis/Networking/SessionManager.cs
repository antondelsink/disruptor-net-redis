using System;
using System.Linq;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace DisruptorNetRedis.Networking
{
    internal class SessionManager : IDisposable
    {
        private TcpListener _tcpListener = null;

        private ConcurrentBag<ClientSession> _sessions = new ConcurrentBag<ClientSession>();

        internal event Action<ClientSession, List<byte[]>> DataAvailable;

        public SessionManager(IPEndPoint listenOn)
        {
            _tcpListener = new TcpListener(listenOn);

            var _backgroundThread = new Thread(new ThreadStart(MonitorNetworkStreams))
            {
                Name = $"Network Socket Monitoring Thread",
                IsBackground = true
            };
            _backgroundThread.Start();
        }

        public void Start()
        {
            if (_tcpListener == null)
                throw new InvalidOperationException();

            _tcpListener.Start();
            _tcpListener.AcceptSocketAsync().ContinueWith(OnTcpSocketAccept);
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

        private void OnTcpSocketAccept(Task<Socket> task)
        {
            if (!task.IsFaulted)
                CreateSession(task.Result);

            AcceptSocketAsync();
        }

        private void AcceptSocketAsync()
        {
            if (_tcpListener != null)
            {
                _tcpListener.AcceptSocketAsync().ContinueWith(OnTcpSocketAccept);
            }
        }

        private void CreateSession(Socket socket)
        {
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
                        {
                            // TODO: change _sessions to Concurrent-?
                        }
                        else
                        {
                            var ns = (NetworkStream)s.ClientDataStream;
                            if (ns.DataAvailable)
                            {
                                try
                                {
                                    RESP.ReadOneArray(ns, out List<byte[]> data);

                                    if (data != null)
                                        DataAvailable?.Invoke(s, data);
                                }
                                catch (System.IO.EndOfStreamException)
                                {
                                    s.ClientDataStream = null;
                                }
                                catch (System.Net.ProtocolViolationException)
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
}