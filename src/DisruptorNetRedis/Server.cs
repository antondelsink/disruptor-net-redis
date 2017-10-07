﻿using Disruptor;
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
            var _core = new DotNetRedis.DotNetRedisCore();
            var _dbStrings = new Databases.StringsDatabase();

            _DisruptorRedis =
                new DisruptorRedis.DisruptorRedis(
                    new ClientRequestTranslator(),
                    new ClientRequestHandler(_core, _dbStrings),
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