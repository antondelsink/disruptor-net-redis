using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace DisruptorNetRedis
{
    public struct RedisKey
    {
        private static HashAlgorithm _hasher = MD5.Create();

        private readonly byte[] _Key;
        private readonly int _HashCode;

        public RedisKey(byte[] buffer)
        {
            this._Key = buffer ?? throw new ArgumentException($"byte[] {nameof(buffer)} must not be null");
            this._HashCode = BitConverter.ToInt32(_hasher.ComputeHash(this._Key), 0);
        }

        public static implicit operator RedisKey(byte[] buffer) => new RedisKey(buffer);
        public static implicit operator byte[] (RedisKey key) => key._Key;

        public static implicit operator RedisKey(string key) => new RedisKey(Encoding.UTF8.GetBytes(key));
        public static explicit operator string(RedisKey key) => key.ToString();

        public override string ToString()
        {
            return Encoding.UTF8.GetString(_Key);
        }

        public override bool Equals(object obj)
        {
            return 
                (obj is RedisKey) &&
                Enumerable.SequenceEqual<byte>(this._Key, ((RedisKey)obj)._Key);
        }

        public override int GetHashCode()
        {
            return _HashCode;
        }
    }
}