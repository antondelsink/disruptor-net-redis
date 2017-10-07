using DisruptorNetRedis.DisruptorRedis;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System;
using DisruptorNetRedis.DotNetRedis;
using DisruptorNetRedis.Databases;

namespace DisruptorNetRedis
{
    internal class RedisCommandDefinitions
    {
        private DotNetRedisServer _core = null;
        private StringsDatabase _dbStrings = null;

        public RedisCommandDefinitions(DotNetRedisServer core, StringsDatabase dbStrings)
        {
            _core = core;
            _dbStrings = dbStrings;
        }

        private static Func<List<byte[]>, RedisCommands>[] _knownCommands = new Func<List<byte[]>, RedisCommands>[]
        {
            new Func<List<byte[]>, RedisCommands>((d) => IsRedisCommand_SET(d) ? RedisCommands.SET : RedisCommands.Unknown),
            new Func<List<byte[]>, RedisCommands>((d) => IsRedisCommand_GET(d) ? RedisCommands.GET : RedisCommands.Unknown),
            new Func<List<byte[]>, RedisCommands>((d) => IsRedisCommand_PING(d) ? RedisCommands.PING : RedisCommands.Unknown),
            new Func<List<byte[]>, RedisCommands>((d) => IsRedisCommand_ECHO(d) ? RedisCommands.ECHO : RedisCommands.Unknown),
            new Func<List<byte[]>, RedisCommands>((d) => IsRedisCommand_INFO(d) ? RedisCommands.INFO: RedisCommands.Unknown),
            new Func<List<byte[]>, RedisCommands>((d) => IsRedisCommand_COMMAND(d) ? RedisCommands.COMMAND : RedisCommands.Unknown),
            new Func<List<byte[]>, RedisCommands>((d) => IsRedisCommand_SUBSCRIBE(d) ? RedisCommands.SUBSCRIBE : RedisCommands.Unknown),
            new Func<List<byte[]>, RedisCommands>((d) => IsRedisCommand_CLIENT_SETNAME(d) ? RedisCommands.CLIENT_SETNAME : RedisCommands.Unknown),
        };

        public Func<List<byte[]>, byte[]> GetCommand(List<byte[]> data)
        {
            RedisCommands selectedCommand = RedisCommands.Unknown;
            foreach (var isCommand in _knownCommands)
            {
                selectedCommand = isCommand(data);

                if (selectedCommand != RedisCommands.Unknown)
                    break;
            }

            switch (selectedCommand)
            {
                case RedisCommands.GET: return Invoke_GET;
                case RedisCommands.SET: return Invoke_SET; 
                case RedisCommands.ECHO: return Invoke_ECHO; 
                case RedisCommands.PING: return Invoke_PING; 
                case RedisCommands.INFO: return Invoke_INFO; 
                case RedisCommands.COMMAND: return Invoke_COMMAND; 
                case RedisCommands.SUBSCRIBE: return Invoke_SUBSCRIBE; 
                case RedisCommands.CLIENT_SETNAME: return Invoke_CLIENT_SETNAME; 
                case RedisCommands.Unknown: return null; 
                default:
                    throw new System.InvalidOperationException();
            }
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
                Encoding.UTF8.GetString(data[0]).ToUpper() == "COMMAND"
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

        public byte[] Invoke_INFO(List<byte[]> data)
        {
            return Encoding.UTF8.GetBytes(RESP.AsRedisBulkString("# Server\r\nos:Windows\r\ntcp_port:6379\r\n"));
        }

        public byte[] Invoke_SUBSCRIBE(List<byte[]> data)
        {
            // TODO: implement pub/sub

            var subscribe = RESP.AsRedisBulkString("subscribe");
            var channel = RESP.AsRedisBulkString(Encoding.UTF8.GetString(data[1]));
            var one = RESP.AsRedisNumber(1);

            return Encoding.UTF8.GetBytes(RESP.AsRedisArray(subscribe, channel, one));
        }

        public byte[] Invoke_CLIENT_SETNAME(List<byte[]> data)
        {
            var clientID = 0;// slot.Session.RemoteEndPoint.Address.Address;

            _core.Client_SetName(clientID, data[2]);

            return Constants.OK_Binary;
        }

        public byte[] Invoke_ECHO(List<byte[]> data)
        {
            return RESP.AsRedisBulkString(data[1]);
        }

        public byte[] Invoke_PING(List<byte[]> data)
        {
            if (data == null || data.Count == 0)
            {
                return Constants.PONG_SimpleStringAsBinary;
            }
            else
            {
                return RESP.AsRedisBulkString(data[1]);
            }
        }
        public byte[] Invoke_GET(List<byte[]> data)
        {
            var key = new RedisKey(data[1]);

            return
                _dbStrings.Get(key, out RedisValue val)
                ?
                RESP.ToBulkStringAsByteArray(val)
                :
                Constants.NULL_Binary;

            // TODO: if key NOT found, check if exists elsewhere, and if so return an error because GET should only be used on strings.
        }

        public byte[] Invoke_SET(List<byte[]> data)
        {
            var key = new RedisKey(data[1]);
            var val = new RedisValue(data[2]);

            return
                _dbStrings.Set(key, val)
                ?
                Constants.OK_Binary
                :
                Constants.ERR_Binary;

            // TODO: clear Key from non-strings key collections (list, set, etc.)
        }

        public byte[] Invoke_COMMAND(List<byte[]> data)
        {
            var ci_get = RESP.CommandInfo("get", 2, new string[] { "readonly" }, 1, 1, 1);
            var ci_set = RESP.CommandInfo("set", -3, new string[] { "write", "denyoom" }, 1, 1, 1);
            var ci_cmd = RESP.CommandInfo("command", 0, new string[] { "loading", "stale" }, 0, 0, 0);

            return Encoding.UTF8.GetBytes(RESP.AsRedisArray(ci_cmd, ci_get, ci_set));
        }
    }
}