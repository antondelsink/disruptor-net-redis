﻿using System.Linq;
using System.Collections.Generic;

namespace DisruptorNetRedis.Databases
{
    public class ListsDatabase
    {
        public Dictionary<RedisKey, List<RedisValue>> ListsDictionary { get; private set; } = new Dictionary<RedisKey, List<RedisValue>>();

        /// <summary>
        /// https://redis.io/commands/lpush
        /// </summary>
        public int LPush(RedisKey key, params RedisValue[] vals)
        {
            if (ListsDictionary.ContainsKey(key))
            {
                var lst = ListsDictionary[key];
                lst.InsertRange(0, vals.Reverse());
                return lst.Count;
            }
            else
            {
                ListsDictionary.Add(key, new List<RedisValue>(vals.Reverse()));
                return vals.Length;
            }
        }

        /// <summary>
        /// https://redis.io/commands/rpush
        /// </summary>
        public int RPush(RedisKey key, params RedisValue[] vals)
        {
            if (ListsDictionary.ContainsKey(key))
            {
                var lst = ListsDictionary[key];
                lst.InsertRange(lst.Count - 1, vals.Reverse());
                return lst.Count;
            }
            else
            {
                ListsDictionary.Add(key, new List<RedisValue>(vals));
                return vals.Length;
            }
        }

        /// <summary>
        /// https://redis.io/commands/lrange
        /// </summary>
        public List<RedisValue> LRange(RedisKey key, int start, int stop)
        {
            if (ListsDictionary.TryGetValue(key, out List<RedisValue> lst))
            {
                int len = lst.Count;

                if (start > len) return new List<RedisValue>();

                int ixStart = (start < 0) ? (len + start) : start;
                int ixEnd = (stop < 0) ? (len + stop) : stop;

                if (ixEnd < ixStart) return new List<RedisValue>();

                int count = ixEnd - ixStart + 1;

                return lst.GetRange(ixStart, count);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// https://redis.io/commands/lindex
        /// </summary>
        public RedisValue? LIndex(RedisKey key, int ix)
        {
            // TODO: "When the value at key is not a list, an error is returned."

            if (ListsDictionary.TryGetValue(key, out List<RedisValue> lst))
            {
                ix = (ix < 0) ? lst.Count + ix : ix;

                if (ix >= 0 && ix < lst.Count)
                    return lst[ix];
                else
                    return null; // "nil when index is out of range"
            }
            else // match behavior of redis-cli: return null if key does not exist.
            {
                return null; 
            }
        }
    }
}