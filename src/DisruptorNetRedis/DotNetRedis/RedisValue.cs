using System;

namespace DisruptorNetRedis
{
    public struct RedisValue
    {
        private byte[] _Value;

        public RedisValue(byte[] buffer)
        {
            _Value = buffer ?? throw new ArgumentException($"byte[] {nameof(buffer)} must not be null");
        }

        public static implicit operator RedisValue(byte[] raw) => new RedisValue(raw);
        public static implicit operator byte[] (RedisValue val) => val._Value;
    }
}