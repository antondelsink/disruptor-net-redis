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
        private ListsDatabase _dbLists = null;

        private SortedDictionary<string, Func<List<byte[]>, byte[]>> _commands = null;

        private Func<List<byte[]>, Func<List<byte[]>, byte[]>>[] _commonCommands = null;

        public RedisCommandDefinitions(DotNetRedisServer core, StringsDatabase dbStrings, ListsDatabase dbLists)
        {
            _core = core;
            _dbStrings = dbStrings;
            _dbLists = dbLists;

            _commonCommands = new Func<List<byte[]>, Func<List<byte[]>, byte[]>>[]
            {
                new Func<List<byte[]>, Func<List<byte[]>, byte[]>>((d) => IsRedisCommand_GET(d) ? new Func<List<byte[]>, byte[]>(Invoke_GET) : null),
                new Func<List<byte[]>, Func<List<byte[]>, byte[]>>((d) => IsRedisCommand_SET(d) ? new Func<List<byte[]>, byte[]>(Invoke_SET) : null),

                new Func<List<byte[]>, Func<List<byte[]>, byte[]>>((d) => IsRedisCommand_LPUSH(d) ? new Func<List<byte[]>, byte[]>(Invoke_LPUSH) : null),
                new Func<List<byte[]>, Func<List<byte[]>, byte[]>>((d) => IsRedisCommand_RPUSH(d) ? new Func<List<byte[]>, byte[]>(Invoke_RPUSH) : null),
                new Func<List<byte[]>, Func<List<byte[]>, byte[]>>((d) => IsRedisCommand_LRANGE(d) ? new Func<List<byte[]>, byte[]>(Invoke_LRANGE) : null),

                new Func<List<byte[]>, Func<List<byte[]>, byte[]>>((d) => IsRedisCommand_PING(d) ? new Func<List<byte[]>, byte[]>(Invoke_PING) : null),
                new Func<List<byte[]>, Func<List<byte[]>, byte[]>>((d) => IsRedisCommand_ECHO(d) ? new Func<List<byte[]>, byte[]>(Invoke_ECHO) : null),
                new Func<List<byte[]>, Func<List<byte[]>, byte[]>>((d) => IsRedisCommand_CLIENT_SETNAME(d) ? new Func<List<byte[]>, byte[]>(Invoke_CLIENT_SETNAME) : null),
            };

            _commands = new SortedDictionary<string, Func<List<byte[]>, byte[]>>()
            {
                { "INFO", Invoke_INFO},
                { "COMMAND", Invoke_COMMAND },
                { "SUBSCRIBE", Invoke_SUBSCRIBE}
            };
        }

        public Func<List<byte[]>, byte[]> GetCommand(List<byte[]> data)
        {
            // TODO: Test/Profile and aim for zero allocations...

            Func<List<byte[]>, byte[]> selectedCommand = null;
            foreach (var eval in _commonCommands)
            {
                selectedCommand = eval(data);

                if (selectedCommand != null)
                    return selectedCommand;
            }

            string cmd = Encoding.UTF8.GetString(data[0]).ToUpper();
            if (_commands.ContainsKey(cmd))
                return _commands[cmd];

            return Invoke_UnknownCommandError;
        }

        private byte[] Invoke_LRANGE(List<byte[]> data)
        {
            var key = new RedisKey(data[1]);
            var start = (int)new RedisValue(data[2]);
            var stop = (int)new RedisValue(data[3]);

            var results = _dbLists.LRange(key, start, stop);

            return RESP.ToRedisArrayAsByteArray(results.ToArray());
        }

        private byte[] Invoke_RPUSH(List<byte[]> data)
        {
            var key = new RedisKey(data[1]);

            data.RemoveRange(0, 2); // remove 'RPUSH' and the key from the array.

            var vals = from v in data
                       select new RedisValue(v);

            _dbLists.RPush(key, vals.ToArray());

            return Constants.OK_SimpleStringAsByteArray;
        }

        private byte[] Invoke_LPUSH(List<byte[]> data)
        {
            var key = new RedisKey(data[1]);

            data.RemoveRange(0, 2); // remove 'LPUSH' and the key from the array.

            var vals = from v in data
                       select new RedisValue(v);

            _dbLists.LPush(key, vals.ToArray());

            return Constants.OK_SimpleStringAsByteArray;
        }

        private bool IsRedisCommand_LRANGE(List<byte[]> data)
        {
            return
                data.Count == 4 &&
                Encoding.UTF8.GetString(data[0]).ToUpper() == "LRANGE";
        }

        private bool IsRedisCommand_RPUSH(List<byte[]> data)
        {
            return
                data.Count >= 3 &&
                Encoding.UTF8.GetString(data[0]).ToUpper() == "RPUSH";
        }

        private bool IsRedisCommand_LPUSH(List<byte[]> data)
        {
            return
                data.Count >= 3 &&
                Encoding.UTF8.GetString(data[0]).ToUpper() == "LPUSH";
        }



        private byte[] Invoke_UnknownCommandError(List<byte[]> data)
        {
            return Constants.UnknownCommandError_Binary;
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

            return Constants.OK_SimpleStringAsByteArray;
        }

        public byte[] Invoke_ECHO(List<byte[]> data)
        {
            return RESP.AsRedisBulkString(data[1]);
        }

        public byte[] Invoke_PING(List<byte[]> data)
        {
            if (data.Count == 2)
                return RESP.AsRedisBulkString(data[1]);
            else
                return Constants.PONG_SimpleStringAsBinary;
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
                Constants.OK_SimpleStringAsByteArray
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