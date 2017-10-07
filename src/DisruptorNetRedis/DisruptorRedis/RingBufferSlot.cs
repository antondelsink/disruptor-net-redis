using DisruptorNetRedis.Networking;
using System;
using System.Collections.Generic;

namespace DisruptorNetRedis.DisruptorRedis
{
    internal class RingBufferSlot
    {
        public ClientSession Session = null;

        public Func<List<byte[]>, byte[]> Command;
        public List<byte[]> Data;

        public byte[] Response = null;
    }
}