using Disruptor;
using DisruptorNetRedis.DisruptorRedis;
using DisruptorNetRedis.Networking;
using System;
using System.Net;

namespace DisruptorNetRedis
{
    public class Server : IDisposable
    {
        private SessionManager _SessionManager = null;

        private DisruptorRedis.DisruptorRedis _DisruptorRedis = null;

        public Server(IPEndPoint listenOn)
        {
            var _commands =
                new RedisCommandDefinitions(
                    new DotNetRedis.DotNetRedisServer(),
                    new Databases.StringsDatabase(),
                    new Databases.ListsDatabase());

            _DisruptorRedis =
                new DisruptorRedis.DisruptorRedis(
                    new RequestTranslator(),
                    new RequestParser(_commands),
                    new RequestHandler(),
                    new ClientResponseHandler());

            _SessionManager = new SessionManager(listenOn);

            _SessionManager.OnDataAvailable += _DisruptorRedis.OnDataAvailable;
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