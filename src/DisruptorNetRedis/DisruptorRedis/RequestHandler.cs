using Disruptor;

namespace DisruptorNetRedis.DisruptorRedis
{
    internal class RequestHandler : IEventHandler<RingBufferSlot>
    {
        public void OnEvent(RingBufferSlot slot, long sequence, bool endOfBatch)
        {
            slot.Response = slot.Command(slot.Data);
        }
    }
}