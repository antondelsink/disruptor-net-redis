using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace DisruptorNetRedis
{
    internal static class RESP
    {
        public static string AsRedisArray(params string[] redisArrayElements)
        {
            string arr = "*" + redisArrayElements.Length.ToString() + Environment.NewLine;
            foreach (string element in redisArrayElements)
            {
                arr += element;
            }
            return arr;
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

        public static string AsRedisBulkString(string s)
        {
            return "$" + s.Length.ToString() + Environment.NewLine + s + Environment.NewLine;
        }

        public static byte[] AsRedisBulkString(byte[] data)
        {
            var prefix = Encoding.UTF8.GetBytes("$" + data.Length.ToString() + Environment.NewLine);
            var suffix = Encoding.UTF8.GetBytes(Environment.NewLine);

            return Enumerable.Concat<byte>(Enumerable.Concat<byte>(prefix, data), suffix).ToArray();
        }

        public static string AsRedisNumber(int i)
        {
            return ":" + i.ToString() + Environment.NewLine;
        }

        public static string AsRedisSimpleString(string f)
        {
            return "+" + f + Environment.NewLine;
        }

        public static string CommandInfo(string commandName, int arity, string[] flags, int firstKeyPosition, int lastKeyPosition, int stepCount)
        {
            var lst = new List<string>();
            foreach (var f in flags)
            {
                lst.Add(RESP.AsRedisSimpleString(f));
            }

            return
                RESP.AsRedisArray(
                    RESP.AsRedisBulkString(commandName),
                    RESP.AsRedisNumber(arity),
                    RESP.AsRedisArray(lst.ToArray()),
                    RESP.AsRedisNumber(firstKeyPosition),
                    RESP.AsRedisNumber(lastKeyPosition),
                    RESP.AsRedisNumber(stepCount));
        }

        public static byte[] ToBulkStringAsByteArray(byte[] buffer)
        {
            var prefix = Constants.BulkStringPrefixByteArray;

            var dataLen = buffer.Length;
            var dataLenString = dataLen.ToString();
            var dataLenByteArray = Encoding.UTF8.GetBytes(dataLenString);

            var newlineLen = Constants.NEWLINE_Binary.Length;

            byte[] result = new byte[prefix.Length + dataLenByteArray.Length + newlineLen + buffer.Length + newlineLen];

            int ixStart = 0;
            Array.Copy(prefix, 0, result, ixStart, prefix.Length);
            ixStart += prefix.Length;

            Array.Copy(dataLenByteArray, 0, result, ixStart, dataLenByteArray.Length);
            ixStart += dataLenByteArray.Length;

            Array.Copy(Constants.NEWLINE_Binary, 0, result, ixStart, Constants.NEWLINE_Binary.Length);
            ixStart += Constants.NEWLINE_Binary.Length;

            Array.Copy(buffer, 0, result, ixStart, buffer.Length);
            ixStart += buffer.Length;

            Array.Copy(Constants.NEWLINE_Binary, 0, result, ixStart, Constants.NEWLINE_Binary.Length);
            ixStart += Constants.NEWLINE_Binary.Length;

            return result;
        }

        public static void ReadOneArray(System.IO.Stream stream, out List<byte[]> data)
        {
            data = null;

            var firstByte = stream.ReadByte();
            if (firstByte == -1) // end of stream; possible during testing with NetworkStream
                throw new System.IO.EndOfStreamException();

            if (firstByte != Constants.RedisArrayPrefixByte)
                throw new System.Net.ProtocolViolationException($"during {nameof(ReadOneArray)} the first byte read was '{(char)firstByte}' instead of the required '*' ");

            int countArrayElements = 0;
            int b;
            while ((b = stream.ReadByte()) != (byte)'\r')
            {
                if (!char.IsDigit((char)b))
                    throw new System.Net.ProtocolViolationException($"during {nameof(ReadOneArray)} a digit was expected while determining count of array elements; instead the byte read was: '{(char)b}'.");

                countArrayElements = countArrayElements * 10 + (b - Constants.ZeroDigitByte);
            }

            if (stream.ReadByte() != (byte)'\n')
                throw new System.Net.ProtocolViolationException($"during {nameof(ReadOneArray)} an expected NewLine character was missing");

            if (countArrayElements > 0)
            {
                data = new List<byte[]>(countArrayElements);

                for (int ix = 0; ix < countArrayElements; ix++)
                {
                    if (stream.ReadByte() != Constants.BulkStringPrefixByte)
                        throw new System.Net.ProtocolViolationException();


                    int lenBulkString = 0;
                    while ((b = stream.ReadByte()) != (byte)'\r')
                    {
                        if (!char.IsDigit((char)b))
                            throw new System.Net.ProtocolViolationException($"during {nameof(ReadOneArray)} a digit was expected while determining count of bytes in a bulkstring; instead the byte read was: '{(char)b}'.");

                        lenBulkString = lenBulkString * 10 + (b - Constants.ZeroDigitByte);
                    }

                    if (stream.ReadByte() != (byte)'\n')
                        throw new System.Net.ProtocolViolationException($"during {nameof(ReadOneArray)} an expected NewLine character was missing");

                    var buffer = new byte[lenBulkString];
                    var readCount = stream.Read(buffer, 0, lenBulkString);
                    //Debug.Assert(readCount == lenBulkString);
                    data.Add(buffer);
                    if (stream.ReadByte() != (byte)'\r')
                        throw new System.Net.ProtocolViolationException($"during {nameof(ReadOneArray)} an expected NewLine character was missing");

                    if (stream.ReadByte() != (byte)'\n')
                        throw new System.Net.ProtocolViolationException($"during {nameof(ReadOneArray)} an expected NewLine character was missing");
                }
            }
            else
            {
                data = new List<byte[]>();
            }
        }
    }
}
