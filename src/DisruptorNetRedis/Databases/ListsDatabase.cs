using System;
using System.Collections.Generic;

namespace DisruptorNetRedis.Databases
{
    public class ListsDatabase
    {
        public Dictionary<RedisKey, List<RedisValue>> ListsDictionary { get; private set; } = new Dictionary<RedisKey, List<RedisValue>>();

        public ListsDatabase()
        {
            throw new NotImplementedException();
        }
    }
}
