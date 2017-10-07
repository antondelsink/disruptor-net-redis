using System.Collections.Generic;

namespace DisruptorNetRedis.Databases
{
    public class StringsDatabase
    {
        public Dictionary<RedisKey, RedisValue> StringsDictionary { get; private set; } = new Dictionary<RedisKey, RedisValue>();

        public bool Set(RedisKey key, RedisValue val)
        {
            if (StringsDictionary.ContainsKey(key))
                StringsDictionary.Remove(key);

            StringsDictionary.Add(key, val);

            return true; // TODO: NX, XX -> bool
        }

        public bool Get(RedisKey key, out RedisValue val)
        {
            return StringsDictionary.TryGetValue(key, out val);
        }
    }
}
