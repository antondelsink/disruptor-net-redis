using Disruptor;
using DisruptorNetRedis.DisruptorRedis;
using DisruptorNetRedis.Networking;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace DisruptorNetRedis
{
    internal class MockClientRequestTranslator : IEventTranslatorTwoArg<RingBufferSlot, ClientSession, List<byte[]>>
    {
        public long? LastSeenSequenceNumber { get; private set; } = null;

        public void TranslateTo(RingBufferSlot slot, long sequence, ClientSession session, List<byte[]> data)
        {
            slot.Session = session;
            slot.Data = data;
            slot.RedisCommand = RedisCommandDefinitions.GetCommand(slot.Data);

            this.LastSeenSequenceNumber = sequence;
        }
    }
}
