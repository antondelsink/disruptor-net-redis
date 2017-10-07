using Disruptor;
using DisruptorNetRedis.DisruptorRedis;

namespace DisruptorNetRedis.Tests
{
    internal class MockCommandProducer : IEventHandler<RingBufferSlot>
    {
        public long? LastSeenSequenceNumber { get; private set; } = null;

        public void OnEvent(RingBufferSlot data, long sequence, bool endOfBatch)
        {
            this.LastSeenSequenceNumber = sequence;
        }
    }
}