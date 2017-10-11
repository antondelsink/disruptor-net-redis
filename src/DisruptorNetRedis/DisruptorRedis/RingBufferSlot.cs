using DisruptorNetRedis.Networking;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DisruptorNetRedis.DisruptorRedis
{
    internal class RingBufferSlot
    {
        // populated by Translator
        public ClientSession Session = null; 
        public List<byte[]> Data;

        // Populated by ClientRequestParser
        public Func<List<byte[]>, byte[]> Command;

        // populated by ResponseHandler
        public byte[] Response = null;
    }
}