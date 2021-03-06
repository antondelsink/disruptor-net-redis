﻿using DisruptorNetRedis.Databases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RedisServerProtocol;

namespace DisruptorNetRedis.DotNetRedis.Commands
{
    internal class ListCommands
    {
        ListsDatabase _db = null;

        public ListCommands(ListsDatabase db)
        {
            _db = db;
        }

        public byte[] Exec_LRANGE(List<byte[]> data)
        {
            var key = new RedisKey(data[1]);
            var start = (int)new RedisValue(data[2]);
            var stop = (int)new RedisValue(data[3]);

            var results = _db.LRange(key, start, stop);

            return RedisValue.ToRedisArrayAsByteArray(results.ToArray());
        }

        public byte[] Exec_RPUSH(List<byte[]> data)
        {
            var key = new RedisKey(data[1]);

            data.RemoveRange(0, 2); // remove 'RPUSH' and the key from the array.

            var vals = from v in data
                       select new RedisValue(v);

            _db.RPush(key, vals.ToArray());

            return Constants.OK_SimpleStringAsByteArray;
        }

        public byte[] Exec_LPUSH(List<byte[]> data)
        {
            var key = new RedisKey(data[1]);

            data.RemoveRange(0, 2); // remove 'LPUSH' and the key from the array.

            var vals = from v in data
                       select new RedisValue(v);

            _db.LPush(key, vals.ToArray());

            return Constants.OK_SimpleStringAsByteArray;
        }

        public byte[] Exec_LINDEX(List<byte[]> data)
        {
            if (data.Count != 3) // TODO: improve parsing error messages
                return Constants.GenericError_SimpleStringAsByteArray;

            var key = new RedisKey(data[1]);
            var ix = new RedisValue(data[2]);

            if (ix.IsInteger)
            { 
                var result = _db.LIndex(key, (int)ix);

                return RESP.ToBulkStringAsByteArray(result);
            }
            else
                return Constants.GenericError_SimpleStringAsByteArray;
        }
    }
}
