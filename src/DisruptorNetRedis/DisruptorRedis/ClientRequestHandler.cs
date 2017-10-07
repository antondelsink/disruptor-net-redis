using Disruptor;
using System;

namespace DisruptorNetRedis.DisruptorRedis
{
    internal class ClientRequestHandler : IEventHandler<RingBufferSlot>
    {
        public void OnEvent(RingBufferSlot slot, long sequence, bool endOfBatch)
        {
            slot.Response = slot.Command(slot.Data);
        }
    }
}