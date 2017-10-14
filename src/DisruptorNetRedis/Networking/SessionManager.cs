using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace DisruptorNetRedis.Networking
{
    internal class SessionManager : IDisposable
    {
        private TcpListener _tcpListener = null;

        internal event Action<ClientSession> OnNewSession;

        public SessionManager(IPEndPoint listenOn)
        {
            _tcpListener = new TcpListener(listenOn);
        }

        public void Start()
        {
            if (_tcpListener == null)
                throw new InvalidOperationException();

            _tcpListener.Start();

            AcceptSocketAsync();
        }

        public void Shutdown()
        {
            Dispose();
        }

        public void Dispose()
        {
            _tcpListener?.Stop();
            _tcpListener = null;

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
        }
    }
}