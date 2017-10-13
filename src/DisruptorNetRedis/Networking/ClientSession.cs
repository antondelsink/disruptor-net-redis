using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace DisruptorNetRedis.Networking
{
    internal class ClientSession
    {
        public Socket Socket = null;
        public IPEndPoint RemoteEndPoint = null;

        public ClientSession(Socket socket)
        {
            Socket = socket;
            RemoteEndPoint = (IPEndPoint)socket.RemoteEndPoint;
        }

        public Stream ClientDataStream = null; // normally a NetworkStream; for tests it's a MemoryStream.
    }
}
