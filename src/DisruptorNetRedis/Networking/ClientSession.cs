using System.IO;
using System.Net;

namespace DisruptorNetRedis.Networking
{
    internal class ClientSession
    {
        public IPEndPoint RemoteEndPoint = null;

        public Stream ClientDataStream = null; // normally a NetworkStream; for tests it's a MemoryStream.
    }
}
