using DisruptorNetRedis.Networking;
using System.Collections.Generic;

namespace DisruptorNetRedis.DisruptorRedis
{
    internal class RingBufferSlot
    {
        public ClientSession Session = null;

        public RedisCommands RedisCommand;
        public List<byte[]> Data;

        public byte[] Response = null;
    }
}