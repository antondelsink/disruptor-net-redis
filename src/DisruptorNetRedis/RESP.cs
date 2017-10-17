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
            if (buffer == null)
                return Constants.NULL_Binary;

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

        private static void AdvanceIndexOnChar(char c, byte[] buffer, ref int ix)
        {
            var firstByte = buffer[ix];
            if (firstByte != (byte)c)
                throw new System.Net.ProtocolViolationException($"during {nameof(AdvanceIndexOnChar)} the byte read was '{(char)firstByte}' instead of the required '{c}' ");
            ix++;
        }

        private static int GetInteger(byte[] buffer, ref int ix)
        {
            int countArrayElements = 0;
            for (; ix < buffer.Length && buffer[ix] != '\r'; ix++)
            {
                if (!char.IsDigit((char)buffer[ix]))
                    throw new System.Net.ProtocolViolationException($"during {nameof(GetInteger)} a digit was expected while determining count of array elements; instead the byte read was: '{(char)buffer[ix]}'.");

                countArrayElements = countArrayElements * 10 + (buffer[ix] - Constants.ZeroDigitByte);
            }

            return countArrayElements;
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

        public static bool StringCompare(byte[] byteArray, string charArray)
        {
            if (byteArray.Length != charArray.Length)
                return false;

            for (int ix = 0; ix < byteArray.Length; ix++)
            {
                if (charArray[ix] != byteArray[ix] &&
                    charArray[ix] != byteArray[ix] - 32)
                    return false;
            }
            return true;
        }

        internal static List<byte[]> ReadRespArray(Stream stream)
        {
            int redisArrayLength = ReadInteger(stream);

            var data = new List<byte[]>(redisArrayLength);

            for (int n = 0; n < redisArrayLength; n++)
            {
                AdvanceIndexOnChar('$', stream);
                var bulkStringLength = ReadInteger(stream);
                data.Add(GetBulkString(bulkStringLength, stream));
                AdvanceIndexOnChar('\r', stream);
                AdvanceIndexOnChar('\n', stream);
            }

            return data;
        }

        internal static byte[] GetBulkString(int len, Stream s)
        {
            var buffer = new byte[len];
            s.Read(buffer, 0, len);
            return buffer;
        }

        internal static void AdvanceIndexOnChar(char c, Stream s)
        {
            var firstByte = (byte)s.ReadByte();
            if (firstByte != (byte)c)
                throw new System.Net.ProtocolViolationException($"during {nameof(AdvanceIndexOnChar)} the byte read was '{(char)firstByte}' instead of the required '{c}' ");
        }

        internal static int ReadInteger(Stream stream)
        {
            int countArrayElements = 0;
            int b;
            while ((b = stream.ReadByte()) != (byte)'\r')
            {
                if (!char.IsDigit((char)b))
                    throw new System.Net.ProtocolViolationException($"during {nameof(ReadInteger)} a digit was expected while determining count of array elements; instead the byte read was: '{(char)b}'.");

                countArrayElements = countArrayElements * 10 + (b - Constants.ZeroDigitByte);
            }
            if (stream.ReadByte() != (byte)'\n')
                throw new System.Net.ProtocolViolationException($"during {nameof(ReadInteger)} an expected NewLine character was missing");
            return countArrayElements;
        }
    }
}
