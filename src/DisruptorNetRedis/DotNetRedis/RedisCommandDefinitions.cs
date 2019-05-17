using System.Collections.Generic;
using System.Text;
using System.Linq;
using System;
using DisruptorNetRedis.DotNetRedis;
using DisruptorNetRedis.Databases;
using DisruptorNetRedis.DotNetRedis.Commands;

using RedisServerProtocol;

namespace DisruptorNetRedis
{
    internal class RedisCommandDefinitions
    {
        DotNetRedisServer _server = null;
        private StringsDatabase _dbStrings = null;
        private ListsDatabase _dbLists = null;
        private SetsDatabase _dbSets = null;

        StringCommands _cmdsStrings = null;
        ListCommands _cmdsLists = null;
        SetCommands _cmdsSets = null;
        ServerCommands _cmdsServer = null;

        PubSubCommands _cmdsPubSub = new PubSubCommands();

        public RedisCommandDefinitions(DotNetRedisServer server, StringsDatabase dbStrings, ListsDatabase dbLists, SetsDatabase dbSets)
        {
            _server = server;
            _dbStrings = dbStrings;
            _dbLists = dbLists;
            _dbSets = dbSets;

            _cmdsStrings = new StringCommands(_dbStrings);
            _cmdsLists = new ListCommands(_dbLists);
            _cmdsSets = new SetCommands(_dbSets);
            _cmdsServer = new ServerCommands(_server);
        }

        public Func<List<byte[]>, byte[]> GetCommand(List<byte[]> data)
        {
            if (data == null || data.Count == 0)
                return Invoke_UnknownCommandError;

            // TODO: aim for zero allocations for at least all 'read' commands (GET, LRANGE, etc.)

            var cmd = data[0];
            if (RESP.StringCompare(cmd, "GET")) return _cmdsStrings.Exec_GET;
            if (RESP.StringCompare(cmd, "SET")) return _cmdsStrings.Exec_SET;

            if (RESP.StringCompare(cmd, "LPUSH")) return _cmdsLists.Exec_LPUSH;
            if (RESP.StringCompare(cmd, "RPUSH")) return _cmdsLists.Exec_RPUSH;
            if (RESP.StringCompare(cmd, "LRANGE")) return _cmdsLists.Exec_LRANGE;
            if (RESP.StringCompare(cmd, "LINDEX")) return _cmdsLists.Exec_LINDEX;

            if (RESP.StringCompare(cmd, "SADD")) return _cmdsSets.Exec_SADD;
            if (RESP.StringCompare(cmd, "SCARD")) return _cmdsSets.Exec_SCARD;
            if (RESP.StringCompare(cmd, "SUNION")) return _cmdsSets.Exec_SUNION;

            if (RESP.StringCompare(cmd, "PING")) return _cmdsServer.Exec_PING;
            if (RESP.StringCompare(cmd, "ECHO")) return _cmdsServer.Exec_ECHO;
            if (RESP.StringCompare(cmd, "INFO")) return _cmdsServer.Exec_INFO;

            if (RESP.StringCompare(cmd, "SUBSCRIBE")) return _cmdsPubSub.Exec_SUBSCRIBE;

            if (RESP.StringCompare(cmd, "CLIENT") &&
                data.Count > 1 && RESP.StringCompare(data[1], "SETNAME"))
            {
                return _cmdsServer.Exec_CLIENT_SETNAME;
            }

            if (RESP.StringCompare(cmd, "COMMAND")) return Invoke_COMMAND;

            return Invoke_UnknownCommandError;
        }

        private byte[] Invoke_UnknownCommandError(List<byte[]> data)
        {
            return Constants.UnknownCommandError_Binary;
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