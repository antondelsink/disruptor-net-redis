using Disruptor;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace DisruptorNetRedis.DisruptorRedis
{
    internal class ClientResponseHandler : IEventHandler<RingBufferSlot>
    {
        public void OnEvent(RingBufferSlot slot, long sequence, bool endOfBatch)
        {
            if (slot.Response == null)
                return;

            try
            {
                slot.Session?.ClientDataStream?.Write(slot.Response, 0, slot.Response.Length);
            }
            catch (System.IO.IOException)
            {
            }

            slot.Response = null;
        }
    }
}