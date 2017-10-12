using System;
using System.Collections.Generic;
using System.Text;

namespace DisruptorNetRedis.Databases
{
    public class SetsDatabase
    {
        public Dictionary<RedisKey, HashSet<RedisValue>> SetsDictionary = new Dictionary<RedisKey, HashSet<RedisValue>>();

        /// <summary>
        /// https://redis.io/commands/sadd
        /// </summary>
        /// <returns>number of elements added to the set</returns>
        public int SAdd(RedisKey key, params RedisValue[] vals)
        {
            return SAdd(key, (IEnumerable<RedisValue>)vals);
        }

        /// <summary>
        /// https://redis.io/commands/sadd
        /// </summary>
        public int SAdd(RedisKey key, IEnumerable<RedisValue> vals)
        {
            HashSet<RedisValue> theSet = null;
            if (SetsDictionary.ContainsKey(key))
            {
                theSet = SetsDictionary[key];
            }
            else
            {
                theSet = new HashSet<RedisValue>();
                SetsDictionary.Add(key, theSet);
            }

            int count = 0;
            foreach (var v in vals)
            {
                if (theSet.Add(v))
                    count++;
            }
            return count;
        }

        /// <summary>
        /// https://redis.io/commands/scard
        /// </summary>
        public int SCard(RedisKey key)
        {
            return (SetsDictionary.ContainsKey(key) ?  SetsDictionary[key].Count : 0);
        }

        /// <summary>
        /// https://redis.io/commands/sunion
        /// </summary>
        public ISet<RedisValue> SUnion(params RedisKey[] keys)
        {
            var results = new HashSet<RedisValue>();
            foreach (var k in keys)
            {
                if (SetsDictionary.ContainsKey(k))
                    results.UnionWith(SetsDictionary[k]);
            }
            return results;
        }
    }
}