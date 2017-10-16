using Disruptor;
using DisruptorNetRedis.DisruptorRedis;
using System;
using System.Collections.Generic;
using System.Text;

namespace DisruptorNetRedis.Tests.Mocks
{
    class MockRequestParser : IEventHandler<RingBufferSlot>
    {
        public void OnEvent(RingBufferSlot data, long sequence, bool endOfBatch)
        {
        }
    }
}
