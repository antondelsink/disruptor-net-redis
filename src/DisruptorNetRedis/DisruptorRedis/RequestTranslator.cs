using Disruptor;
using DisruptorNetRedis.Networking;
using System.Collections.Generic;

namespace DisruptorNetRedis.DisruptorRedis
{
    internal class RequestTranslator : IEventTranslatorTwoArg<RingBufferSlot, ClientSession, List<byte[]>>
    {
        public void TranslateTo(RingBufferSlot slot, long sequence, ClientSession session, List<byte[]> data)
        {
            slot.Session = session;

            slot.Data = data;
        }
    }
}