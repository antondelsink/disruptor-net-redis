using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace RedisServerProtocol
{
    public static class RESP
    {
        public static class Constants
        {
            public static readonly string NewLine = "\r\n";

            public static readonly byte[] NEWLINE_Binary = NewLine.ToUtf8Bytes();

            public const string NULL = "-1";
            public const string NULL_BulkString = "$-1\r\n";
            public static byte[] NULL_Binary = NULL_BulkString.ToUtf8Bytes();

            public const char RedisArrayPrefixChar = '*';
            public const byte RedisArrayPrefixByte = (byte)'*';
            public const string RedisArrayPrefixString = "*";
            public static byte[] RedisArrayPrefixByteArray = RedisArrayPrefixString.ToUtf8Bytes();

            public const char BulkStringPrefixChar = '$';
            public const byte BulkStringPrefixByte = (byte)'$';
            public const string BulkStringPrefixString = "$";
            public static byte[] BulkStringPrefixByteArray = BulkStringPrefixString.ToUtf8Bytes();

            public const string EmptyArray = "*0\r\n";
            public static byte[] EmptyArrayAsByteArray = EmptyArray.ToUtf8Bytes();

            public static byte ZeroDigitByte = (byte)'0';
        }

        /// <summary>
        /// Concatenate all provided strings and prefix with a RESP Array Header.
        /// </summary>
        /// <param name="redisArrayElements">usually a BulkString; not modfied during concatenation</param>
        /// <returns>RESP Array</returns>
        public static string AsRedisArray(params string[] redisArrayElements)
        {
            string redisArrayHeader = Constants.RedisArrayPrefixChar + redisArrayElements.Length.ToString() + Constants.NewLine;

            var sb = new StringBuilder(redisArrayHeader, redisArrayHeader.Length + redisArrayElements.Length * 3); // min length 3 per element
            foreach (string element in redisArrayElements)
            {
                sb.Append(element);
            }
            return sb.ToString();
        }

        public static string AsRedisBulkString(string s)
        {
            switch (s.Length)
            {
                case int n when n >= 1 && n <= 9:

                    Span<char> tmp = stackalloc char[n + 6];

                    tmp[3] = tmp[n + 6 - 1] = '\n';
                    tmp[2] = tmp[n + 6 - 2] = '\r';

                    tmp[0] = '$';
                    tmp[1] = (char)(n + 48);

                    for (int ix = 4; ix < tmp.Length - 2; ix++)
                    {
                        tmp[ix] = s[ix - 4];
                    }

                    return new string(tmp);

                default:
                    return "$" + s.Length.ToString() + Constants.NewLine + s + Constants.NewLine;
            }
        }

        public static byte[] AsRedisBulkString(byte[] data)
        {
            var prefix = Encoding.UTF8.GetBytes("$" + data.Length.ToString() + Constants.NewLine);
            var suffix = Encoding.UTF8.GetBytes(Constants.NewLine);

            return Enumerable.Concat<byte>(Enumerable.Concat<byte>(prefix, data), suffix).ToArray();
        }

        public static string AsRedisNumber(int i)
        {
            return ":" + i.ToString() + Constants.NewLine;
        }

        public static string AsRedisSimpleString(string f)
        {
            return "+" + f + Constants.NewLine;
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

        /// <summary>
        /// Read a decimal integer from a multi-segment sequence of read-only memory.
        /// </summary>
        public static int ReadNumber(ReadOnlySequence<byte> buffer)
        {
            // TODO: Minus sign for NULL

            if (buffer.Length == 1)
                return (buffer.First.Span[0] - Constants.ZeroDigitByte);

            int result = 0;
            if (buffer.IsSingleSegment)
            {
                foreach (byte b in buffer.First.Span)
                {
                    result = result * 10 + (b - Constants.ZeroDigitByte);
                }
            }
            else
            {
                foreach (var segment in buffer)
                {
                    foreach (byte b in segment.Span)
                    {
                        result = result * 10 + (b - Constants.ZeroDigitByte);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Read a decimal integer from a multi-segment sequence of read-only memory terminated by '\r'.
        /// </summary>
        public static int ReadNumberUpToEOL(ReadOnlySequence<byte> buffer)
        {
            // TODO: Minus sign for NULL

            int result = 0;
            if (buffer.IsSingleSegment)
            {
                foreach (byte b in buffer.First.Span)
                {
                    if (b == (byte)'\r')
                        return result;

                    result = result * 10 + (b - Constants.ZeroDigitByte);
                }
            }
            else
            {
                foreach (var segment in buffer)
                {
                    foreach (byte b in segment.Span)
                    {
                        if (b == (byte)'\r')
                            return result;

                        result = result * 10 + (b - Constants.ZeroDigitByte);
                    }
                }
            }
            return result;
        }

        public static string ArrayOfBulkStringsFromStrings(params string[] s)
        {
            return RESP.AsRedisArray(RESP.AsRedisBulkStrings(s));
        }
        private static string AsRedisBulkStrings(params string[] arrs)
        {
            var sb = new StringBuilder();
            foreach (var s in arrs)
            {
                sb.Append(RESP.AsRedisBulkString(s));
            }
            return sb.ToString();
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

        public static List<byte[]> ReadRespArray(Stream stream)
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

        public static byte[] GetBulkString(int len, Stream s)
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

        public static string ToRedisBulkString(this string s)
        {
            return RESP.AsRedisBulkString(s);
        }

        public static string ToRedisArray(this string s)
        {
            return RESP.AsRedisArray(s);
        }

        public static string ToRedisArray(this string[] s)
        {
            return RESP.AsRedisArray(s);
        }

        public static string[] ToRedisBulkStrings(this string[] dotNetStrings)
        {
            return dotNetStrings.Select(s => s.ToRedisBulkString()).ToArray();
        }

        public static byte[] ToUtf8Bytes(this string s)
        {
            return Encoding.UTF8.GetBytes(s);
        }
    }
}
