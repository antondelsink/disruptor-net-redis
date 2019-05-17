using DisruptorNetRedis.Databases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RedisServerProtocol;

namespace DisruptorNetRedis.DotNetRedis.Commands
{
    internal class SetCommands
    {
        SetsDatabase _db = null;

        public SetCommands(SetsDatabase db)
        {
            _db = db;
        }

        public byte[] Exec_SUNION(List<byte[]> data)
        {
            var keys = from arr in data
                       select new RedisKey(arr);

            var result = _db.SUnion(keys.Skip(1).ToArray());

            return RedisValue.ToRedisArrayAsByteArray(result.ToArray());
        }

        public byte[] Exec_SCARD(List<byte[]> data)
        {
            var key = new RedisKey(data[1]);

            return Encoding.UTF8.GetBytes(RESP.AsRedisNumber(_db.SCard(key)));
        }

        public byte[] Exec_SADD(List<byte[]> data)
        {
            var key = new RedisKey(data[1]);

            var vals = from arr in data.Skip(2) // skip 'SADD' and the Key
                       select new RedisValue(arr);

            return Encoding.UTF8.GetBytes(RESP.AsRedisNumber(_db.SAdd(key, vals)));
        }
    }
}