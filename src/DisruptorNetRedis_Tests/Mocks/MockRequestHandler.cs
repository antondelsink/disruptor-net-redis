using Disruptor;
using DisruptorNetRedis.DisruptorRedis;
using System;
using System.Collections.Generic;
using System.Text;

namespace DisruptorNetRedis.Tests.Mocks
{
    class MockRequestHandler : IEventHandler<RingBufferSlot>
    {
        public void OnEvent(RingBufferSlot slot, long sequence, bool endOfBatch)
        {
            slot.Response = Constants.OK_SimpleStringAsByteArray;
        }
    }
}
