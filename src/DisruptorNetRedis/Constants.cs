using System;
using System.Collections.Generic;
using System.Text;

namespace DisruptorNetRedis
{
    internal static class Constants
    {
        public static byte[] NEWLINE_Binary = Encoding.UTF8.GetBytes(Environment.NewLine);

        public const string OK = "OK";
        public const string OK_SimpleString = "+" + OK + "\r\n";
        public static byte[] OK_SimpleStringAsByteArray = Encoding.UTF8.GetBytes(OK_SimpleString);

        public const string ERR = "ERR";
        public const string ERR_SimpleString = "-" + ERR + "\r\n";
        public static byte[] ERR_Binary = Encoding.UTF8.GetBytes(ERR_SimpleString);

        public const string UnknownCommandError = "Command not supported.";
        public const string UnknownCommandError_SimpleString = "-" + UnknownCommandError + "\r\n";
        public static byte[] UnknownCommandError_Binary = Encoding.UTF8.GetBytes(UnknownCommandError_SimpleString);

        public const string NULL = "-1";
        public const string NULL_BulkString = "$-1\r\n";
        public static byte[] NULL_Binary = Encoding.UTF8.GetBytes(NULL_BulkString);

        public const string SET = "SET";
        public static byte[] SET_Binary = Encoding.UTF8.GetBytes(SET);
        public static byte[] set_Binary = Encoding.UTF8.GetBytes(SET.ToLower());

        public const string GET = "GET";
        public static byte[] GET_Binary = Encoding.UTF8.GetBytes(GET);
        public static byte[] get_Binary = Encoding.UTF8.GetBytes(GET.ToLower());

        public const string COMMAND = "COMMAND";
        public static byte[] COMMAND_Binary = Encoding.UTF8.GetBytes(COMMAND);

        public const char RedisArrayPrefixChar = '*';
        public const byte RedisArrayPrefixByte = (byte)'*';
        public const string RedisArrayPrefixString = "*";
        public static byte[] RedisArrayPrefixByteArray = Encoding.UTF8.GetBytes(RedisArrayPrefixString);

        public const char BulkStringPrefixChar = '$';
        public const byte BulkStringPrefixByte = (byte)'$';
        public const string BulkStringPrefixString = "$";
        public static byte[] BulkStringPrefixByteArray = Encoding.UTF8.GetBytes(BulkStringPrefixString);

        public const string EmptyArray = "*0\r\n";
        public static byte[] EmptyArrayAsByteArray = Encoding.UTF8.GetBytes(EmptyArray);

        public static byte ZeroDigitByte = (byte)'0';

        public const string PING = "PING";
        public static byte[] PING_Binary = Encoding.UTF8.GetBytes(PING);
        public static byte[] ping_Binary = Encoding.UTF8.GetBytes(PING.ToLower());

        public const string ECHO = "ECHO";
        public static byte[] ECHO_Binary = Encoding.UTF8.GetBytes(ECHO);
        public static byte[] echo_Binary = Encoding.UTF8.GetBytes(ECHO.ToLower());

        public const string PONG_SimpleString = "+PONG\r\n";
        public static byte[] PONG_SimpleStringAsBinary = Encoding.UTF8.GetBytes(PONG_SimpleString);

    }
}
