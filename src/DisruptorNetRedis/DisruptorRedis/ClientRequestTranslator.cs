using Disruptor;
using DisruptorNetRedis.Networking;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DisruptorNetRedis.DisruptorRedis
{
    internal class ClientRequestTranslator : IEventTranslatorTwoArg<RingBufferSlot, ClientSession, List<byte[]>>
    {
        public void TranslateTo(RingBufferSlot slot, long sequence, ClientSession session, List<byte[]> data)
        {
            slot.Session = session;

            slot.Data = data;
            slot.RedisCommand = RedisCommandDefinitions.GetCommand(slot.Data);

            Debug.WriteLine(nameof(ClientRequestTranslator) + " selected command " + slot.RedisCommand.ToString());
        }
    }
}