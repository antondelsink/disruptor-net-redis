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

        internal event Action<ClientSession> OnNewSession;
        internal event Func<ClientSession, List<byte[]>> OnDataAvailable;
        internal event Action<ClientSession, List<byte[]>> OnRespArrayAvailable;

        private CancellationTokenSource _cancel = null;

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

            _cancel = new CancellationTokenSource();

            AcceptSocketAsync();

            _backgroundThread.Start();
        }

        public void Shutdown()
        {
            Dispose();
        }

        public void Dispose()
        {
            _cancel?.Cancel();

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
            var newSession = new ClientSession(socket);

            OnNewSession?.Invoke(newSession);

            _sessions.Add(newSession); // must be after event OnNewSession otherwise session object not initialized during foreach of method MonitorNetworkStreams
        }

        private void MonitorNetworkStreams()
        {
            while (true)
            {
                if (_cancel.IsCancellationRequested)
                    return;

                var localSessions = _sessions;
                if (localSessions != null)
                {
                    foreach (var s in localSessions)
                    {
                        var ns = s.ClientDataStream as NetworkStream;

                        if (ns != null && ns.DataAvailable)
                        {
                            try
                            {
                                var data = OnDataAvailable?.Invoke(s);

                                if (data != null)
                                    OnRespArrayAvailable?.Invoke(s, data);
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
                Thread.Yield();
            }
        }
    }
}