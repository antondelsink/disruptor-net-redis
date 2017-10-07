using Disruptor;
using DisruptorNetRedis.Networking;
using System;
using System.Collections.Generic;

namespace DisruptorNetRedis.DisruptorRedis
{
    internal class ClientRequestTranslator : IEventTranslatorTwoArg<RingBufferSlot, ClientSession, List<byte[]>>
    {
        RedisCommandDefinitions _commands = null;

        public ClientRequestTranslator(RedisCommandDefinitions commands)
        {
            _commands = commands;
        }

        public void TranslateTo(RingBufferSlot slot, long sequence, ClientSession session, List<byte[]> data)
        {
            slot.Session = session;

            slot.Data = data;
            slot.Command = _commands.GetCommand(slot.Data);
        }
    }
}