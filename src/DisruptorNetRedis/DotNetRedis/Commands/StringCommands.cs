using DisruptorNetRedis.Databases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DisruptorNetRedis.DotNetRedis.Commands
{
    internal class StringCommands
    {
        StringsDatabase _db = null;

        public StringCommands(StringsDatabase db)
        {
            _db = db;
        }

        public bool Is_SET(List<byte[]> data)
        {
            return
                data.Count == 3 &&
                RESP.StringCompare(data[0], "SET");
        }

        public byte[] Exec_SET(List<byte[]> data)
        {
            // TODO: clear Key from non-strings key collections (list, set, etc.)

            var key = new RedisKey(data[1]);
            var val = new RedisValue(data[2]);

            return
                _db.Set(key, val)
                ?
                Constants.OK_SimpleStringAsByteArray
                :
                Constants.GenericError_SimpleStringAsByteArray;
        }

        public bool Is_GET(List<byte[]> data)
        {
            return
                data.Count == 2 &&
                RESP.StringCompare(data[0], "GET");
        }

        public byte[] Exec_GET(List<byte[]> data)
        {
            var key = new RedisKey(data[1]);

            return
                _db.Get(key, out RedisValue val)
                ?
                RESP.ToBulkStringAsByteArray(val)
                :
                Constants.NULL_Binary;

            // TODO: if key NOT found, check if exists elsewhere, and if so return an error because GET should only be used on strings.
        }
    }
}
