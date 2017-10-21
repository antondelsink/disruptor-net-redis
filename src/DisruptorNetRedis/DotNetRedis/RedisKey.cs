using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace DisruptorNetRedis
{
    public struct RedisKey
    {
        private readonly byte[] _Key;
        private int? _HashCode;

        public RedisKey(byte[] buffer)
        {
            this._Key = buffer ?? throw new ArgumentException($"byte[] {nameof(buffer)} must not be null");
            this._HashCode = null;
        }

        public RedisKey(string key)
            : this(Encoding.UTF8.GetBytes(key))
        {
        }

        public static implicit operator RedisKey(byte[] buffer) => new RedisKey(buffer);
        public static implicit operator byte[] (RedisKey key) => (key._Key == null) ? new byte[0] : key._Key;

        public static implicit operator RedisKey(string key) => new RedisKey(key);
        public static explicit operator string(RedisKey key) => key.ToString();

        public override string ToString()
        {
            return (_Key == null) ? string.Empty : Encoding.UTF8.GetString(_Key);
        }

        public override bool Equals(object obj)
        {
            return
                (obj is RedisKey) &&
                Enumerable.SequenceEqual<byte>(this._Key, ((RedisKey)obj)._Key);
        }

        public override int GetHashCode()
        {
            // TODO: profile/test

            if (!_HashCode.HasValue)
            {
                //_HashCode = BitConverter.ToInt32(_hasher.ComputeHash(this._Key), 0);

                switch (_Key.Length)
                {
                    case 0:
                        _HashCode = 0;
                        break;
                    case 1:
                        _HashCode = _Key[0];
                        break;
                    case 2:
                    case 3:
                        _HashCode = BitConverter.ToInt16(_Key, 0);
                        break;
                    default:
                        _HashCode = BitConverter.ToInt32(this._Key, 0);
                        break;
                }
            }
            return _HashCode.Value;
        }
    }
}