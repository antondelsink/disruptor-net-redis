using System;
using System.Collections.Generic;
using System.Text;

using RedisServerProtocol;

namespace DisruptorNetRedis.DotNetRedis.Commands
{
    internal class PubSubCommands
    {
        public byte[] Exec_SUBSCRIBE(List<byte[]> data)
        {
            // TODO: implement pub/sub

            var subscribe = RESP.AsRedisBulkString("subscribe");
            var channel = RESP.AsRedisBulkString(Encoding.UTF8.GetString(data[1]));
            var one = RESP.AsRedisNumber(1);

            return Encoding.UTF8.GetBytes(RESP.AsRedisArray(subscribe, channel, one));
        }
    }
}
