using Disruptor;
using System;
using System.Collections.Generic;
using System.Text;

namespace DisruptorNetRedis.DisruptorRedis
{
    internal class RequestParser : IEventHandler<RingBufferSlot>
    {
        RedisCommandDefinitions _commands = null;

        public RequestParser(RedisCommandDefinitions commands)
        {
            _commands = commands;
        }

        public void OnEvent(RingBufferSlot slot, long sequence, bool endOfBatch)
        {
            slot.Command = _commands.GetCommand(slot.Data);
        }
    }
}
