using System;
using System.Collections.Generic;

namespace DisruptorNetRedis.DotNetRedis
{
    internal class DotNetRedisServer
    {
        public Dictionary<long, byte[]> _clientNames = new Dictionary<long, byte[]>();
        public void Client_SetName(long clientID, byte[] name)
        {
            _clientNames.Add(clientID, name);
        }
    }
}
