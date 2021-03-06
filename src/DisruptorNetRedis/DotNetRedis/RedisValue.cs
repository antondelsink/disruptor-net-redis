﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RedisServerProtocol;

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
                n = n * 10 + (b - RESP.Constants.ZeroDigitByte);
            }
            return n;
        }

        public bool IsInteger
        {
            get
            {
                if (_Value == null || _Value.Length == 0)
                    return false;

                foreach (var b in _Value)
                {
                    if (!char.IsDigit((char)b))
                        return false;
                }
                return true;
            }
        }

        public override string ToString()
        {
            return (_Value == null) ? string.Empty : Encoding.UTF8.GetString(_Value);
        }

        public byte[] ToRedisBulkStringByteArray()
        {
            var prefix = Encoding.UTF8.GetBytes("$" + _Value.Length.ToString() + Environment.NewLine);
            var suffix = Encoding.UTF8.GetBytes(Environment.NewLine);

            return Enumerable.Concat<byte>(Enumerable.Concat<byte>(prefix, _Value), suffix).ToArray();
        }
        public static byte[] ToRedisArrayAsByteArray(params RedisValue[] lst)
        {
            string prefix = "*" + lst.Length.ToString() + Environment.NewLine;

            IEnumerable<byte> result = Encoding.UTF8.GetBytes(prefix);

            foreach (RedisValue element in lst)
            {
                result = Enumerable.Concat<byte>(result, element.ToRedisBulkStringByteArray());
            }
            return result.ToArray();
        }
    }
}