using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace DisruptorNetRedis.Networking
{
    internal class ClientSession
    {
        public IPEndPoint RemoteEndPoint = null;

        public Stream ClientDataStream = null; // normally a NetworkStream; for tests it's a MemoryStream.
    }
}
