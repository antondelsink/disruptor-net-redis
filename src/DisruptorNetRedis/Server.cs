using DisruptorNetRedis.DisruptorRedis;
using DisruptorNetRedis.Networking;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace DisruptorNetRedis
{
    public class Server : IDisposable
    {
        private SessionManager _SessionManager = null;

        private DisruptorRedis.DisruptorRedis _DisruptorRedis = null;

        internal event Action<ClientSession, List<byte[]>> OnDataAvailable;

        internal Server(IPEndPoint listenOn, DisruptorRedis.DisruptorRedis disruptor = null)
        {
            var _commands =
                new RedisCommandDefinitions(
                    new DotNetRedis.DotNetRedisServer(),
                    new Databases.StringsDatabase(),
                    new Databases.ListsDatabase(),
                    new Databases.SetsDatabase());

            _DisruptorRedis = disruptor ??
                new DisruptorRedis.DisruptorRedis(
                    new RequestTranslator(),
                    new RequestParser(_commands),
                    new RequestHandler(),
                    new ClientResponseHandler());

            _SessionManager = new SessionManager(listenOn);

            this.OnDataAvailable += _DisruptorRedis.OnDataAvailable;

            _SessionManager.OnNewSession +=
                (newSession) =>
                {
                    newSession.Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, 4 * 1024);

                    newSession.ClientDataStream = new NetworkStream(newSession.Socket, true);

                    newSession.Buffer = new byte[1];

                    newSession.ClientDataStream
                        .ReadAsync(newSession.Buffer, 0, newSession.Buffer.Length)
                        .ContinueWith(OnReadContinueWithNewArray, newSession);
                };
        }

        private void OnReadContinueWithNewArray(Task<int> t, object state)
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
                        OnDataAvailable?.Invoke(session, RESP.ReadRespArray(session.ClientDataStream));
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

        public void Start()
        {
            _DisruptorRedis?.Start();
            _SessionManager?.Start();
        }

        public void Stop()
        {
            Dispose();
        }

        public void Dispose()
        {
            _SessionManager?.Shutdown();
            _SessionManager = null;

            _DisruptorRedis?.Dispose();
            _DisruptorRedis = null;

            GC.SuppressFinalize(this);
        }
    }
}