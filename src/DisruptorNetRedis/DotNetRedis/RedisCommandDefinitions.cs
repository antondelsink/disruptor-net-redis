using DisruptorNetRedis.DisruptorRedis;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System;

namespace DisruptorNetRedis
{
    internal static class RedisCommandDefinitions
    {
        public static RedisCommands GetCommand(List<byte[]> data)
        {
            if (IsRedisCommand_SET(data))
            {
                data.RemoveAt(0);
                return RedisCommands.SET;
            }

            if (IsRedisCommand_GET(data))
            {
                data.RemoveAt(0);
                return RedisCommands.GET;
            }

            if (IsRedisCommand_PING(data))
            {
                data.RemoveAt(0);
                return RedisCommands.PING;
            }
            if (IsRedisCommand_ECHO(data))
            {
                data.RemoveAt(0);
                return RedisCommands.ECHO;
            }

            if (IsRedisCommand_SUBSCRIBE(data))
            {
                data.RemoveAt(0);
                return RedisCommands.SUBSCRIBE;
            }

            if (IsRedisCommand_CLIENT_SETNAME(data))
            {
                data.RemoveRange(0, 1);
                return RedisCommands.CLIENT_SETNAME;
            }

            if (IsRedisCommand_INFO(data))
            {
                data.RemoveAt(0);
                return RedisCommands.INFO;
            }

            if (IsRedisCommand_COMMAND(data))
            {
                return RedisCommands.COMMAND;
            }

            return RedisCommands.Unknown;
        }

        private static bool IsRedisCommand_INFO(List<byte[]> data)
        {
            return
                data.Count > 0 &&
                Encoding.UTF8.GetString(data[0]).ToUpper() == "INFO";
        }

        private static bool IsRedisCommand_SUBSCRIBE(List<byte[]> data)
        {
            return
                data.Count == 2 &&
                Encoding.UTF8.GetString(data[0]).ToUpper() == "SUBSCRIBE";
        }

        private static bool IsRedisCommand_ECHO(List<byte[]> data)
        {
            return
                data.Count == 2 &&
                (
                Enumerable.SequenceEqual<byte>(data[0], Constants.ECHO_Binary) ||
                Enumerable.SequenceEqual<byte>(data[0], Constants.echo_Binary) ||
                Encoding.UTF8.GetString(data[0]).ToUpper() == Constants.ECHO

                );
        }

        public static bool IsRedisCommand_CLIENT_SETNAME(List<byte[]> data)
        {
            return
                data.Count == 3 &&
                Encoding.UTF8.GetString(data[0]).ToUpper() == "CLIENT" &&
                Encoding.UTF8.GetString(data[0]).ToUpper() == "SETNAME";
        }

        public static bool IsRedisCommand_PING(List<byte[]> data)
        {
            return
                data.Count == 1 &&
                (
                Enumerable.SequenceEqual<byte>(data[0], Constants.PING_Binary) ||
                Enumerable.SequenceEqual<byte>(data[0], Constants.ping_Binary) ||
                Encoding.UTF8.GetString(data[0]).ToUpper() == Constants.PING
                );
        }

        public static bool IsRedisCommand_COMMAND(List<byte[]> data)
        {
            return
                data.Count == 1 &&
                (
                Enumerable.SequenceEqual<byte>(data[0], Constants.COMMAND_Binary) ||
                Encoding.UTF8.GetString(data[0]).ToUpper() == Constants.COMMAND
                );
        }

        public static bool IsRedisCommand_GET(List<byte[]> data)
        {
            return
                data.Count == 2 &&
                (
                Enumerable.SequenceEqual<byte>(data[0], Constants.GET_Binary) ||
                Enumerable.SequenceEqual<byte>(data[0], Constants.get_Binary) ||
                Encoding.UTF8.GetString(data[0]).ToUpper() == Constants.GET
                );
        }

        public static bool IsRedisCommand_SET(List<byte[]> data)
        {
            return
                data.Count == 3 &&
                (
                Enumerable.SequenceEqual<byte>(data[0], Constants.SET_Binary) ||
                Enumerable.SequenceEqual<byte>(data[0], Constants.set_Binary) ||
                Encoding.UTF8.GetString(data[0]).ToUpper() == Constants.SET
                );
        }
    }
}