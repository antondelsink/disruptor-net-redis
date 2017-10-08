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
                    new ClientRequestTranslator(_commands),
                    new ClientRequestHandler(),
                    new IWorkHandler<RingBufferSlot>[] { new ResponseHandler() });

            _SessionManager = new SessionManager(listenOn);

            _SessionManager.DataAvailable += _DisruptorRedis.OnDataAvailable;
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
            _DisruptorRedis?.Dispose();

            _SessionManager = null;
            _DisruptorRedis = null;

            GC.SuppressFinalize(this);
        }
    }
}