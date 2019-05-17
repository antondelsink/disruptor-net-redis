using System;
using System.Collections.Generic;
using System.Text;

namespace DisruptorNetRedis
{
    internal static class Constants
    {
        public const string OK = "OK";
        public const string OK_SimpleString = "+" + OK + "\r\n";
        public static byte[] OK_SimpleStringAsByteArray = Encoding.UTF8.GetBytes(OK_SimpleString);

        public const string GenericError = "Error";
        public const string GenericError_SimpleString = "-" + GenericError + "\r\n";
        public static byte[] GenericError_SimpleStringAsByteArray = Encoding.UTF8.GetBytes(GenericError_SimpleString);

        public const string UnknownCommandError = "Command not supported.";
        public const string UnknownCommandError_SimpleString = "-" + UnknownCommandError + "\r\n";
        public static byte[] UnknownCommandError_Binary = Encoding.UTF8.GetBytes(UnknownCommandError_SimpleString);

        public const string SET = "SET";
        public static byte[] SET_Binary = Encoding.UTF8.GetBytes(SET);
        public static byte[] set_Binary = Encoding.UTF8.GetBytes(SET.ToLower());

        public const string GET = "GET";
        public static byte[] GET_Binary = Encoding.UTF8.GetBytes(GET);
        public static byte[] get_Binary = Encoding.UTF8.GetBytes(GET.ToLower());

        public const string COMMAND = "COMMAND";
        public static byte[] COMMAND_Binary = Encoding.UTF8.GetBytes(COMMAND);


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
