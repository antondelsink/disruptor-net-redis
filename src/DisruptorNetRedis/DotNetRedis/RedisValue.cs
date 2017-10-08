using System;
using System.Linq;
using System.Text;

namespace DisruptorNetRedis
{
    public struct RedisValue
    {
        private byte[] _Value;

        public RedisValue(byte[] buffer)
        {
            _Value = buffer ?? throw new ArgumentException($"byte[] {nameof(buffer)} must not be null");
        }

        public RedisValue(string data)
            : this(Encoding.UTF8.GetBytes(data))
        {
        }

        public override bool Equals(object obj)
        {
            return
                (obj is RedisValue) &&
                Enumerable.SequenceEqual<byte>(this._Value, ((RedisValue)obj)._Value);
        }

        public static implicit operator RedisValue(byte[] raw) => new RedisValue(raw);
        public static implicit operator byte[] (RedisValue val) => val._Value;

        public static implicit operator RedisValue(string val) => new RedisValue(val);
        public static explicit operator string(RedisValue rv) => rv.ToString();

        public static explicit operator int(RedisValue rv)
        {
            int n = 0;
            foreach (var b in rv._Value)
            {
                n = n * 10 + (b - Constants.ZeroDigitByte);
            }
            return n;
        }

        public override string ToString()
        {
            return Encoding.UTF8.GetString(_Value);
        }

        public byte[] ToRedisBulkStringByteArray()
        {
            var prefix = Encoding.UTF8.GetBytes("$" + _Value.Length.ToString() + Environment.NewLine);
            var suffix = Encoding.UTF8.GetBytes(Environment.NewLine);

            return Enumerable.Concat<byte>(Enumerable.Concat<byte>(prefix, _Value), suffix).ToArray();
        }
    }
}