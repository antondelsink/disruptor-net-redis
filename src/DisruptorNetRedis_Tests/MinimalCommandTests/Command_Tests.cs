﻿using DisruptorNetRedis.Databases;
using DisruptorNetRedis.DisruptorRedis;
using DisruptorNetRedis.DotNetRedis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NFluent;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DisruptorNetRedis.Tests
{
    [TestClass]
    public class Command_Tests
    {
        private DotNetRedisServer _core = null;
        private StringsDatabase _dbStrings = null;
        private RedisCommandDefinitions _commands = null;
        private ListsDatabase _dbLists = null;

        [TestInitialize]
        public void Test_Init()
        {
            _core = new DotNetRedisServer();
            _dbStrings = new StringsDatabase();
            _dbLists = new ListsDatabase();
            _commands = new RedisCommandDefinitions(_core, _dbStrings, _dbLists);
        }

        [TestMethod]
        public void Test_Command_INFO()
        {
            var msgResponseActual = "# Server\r\nos:Windows\r\ntcp_port:6379\r\n";
            var msgResponse = @"$" + msgResponseActual.Length.ToString() + "\r\n" + msgResponseActual + "\r\n";

            var msgInfo = "*2\r\n$4\r\nINFO\r\n$6\r\nserver\r\n";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(msgInfo));
            stream.Seek(0, SeekOrigin.Begin);

            RESP.ReadOneArray(stream, out List<byte[]> data);

            var cmd = _commands.GetCommand(data);

            var response = cmd(data);

            Check.That<byte[]>(response).ContainsExactly(Encoding.UTF8.GetBytes(msgResponse));
        }

        /// <summary>
        /// https://redis.io/commands/lpush
        /// </summary>
        [TestMethod]
        public void Test_Command_LPUSH()
        {
            var msg =
                RESP.AsRedisArray(
                    RESP.AsRedisBulkString("LPUSH"),
                    RESP.AsRedisBulkString("_KEY_"),
                    RESP.AsRedisBulkString("_VAL_"));

            var stream = new MemoryStream(Encoding.UTF8.GetBytes(msg));
            stream.Seek(0, SeekOrigin.Begin);

            RESP.ReadOneArray(stream, out List<byte[]> data);

            var cmd = _commands.GetCommand(data);
            var response = cmd(data);

            Check.That<byte[]>(response).ContainsExactly(Constants.OK_SimpleStringAsByteArray);
        }

        /// <summary>
        /// https://redis.io/commands/lrange
        /// </summary>
        [TestMethod]
        public void Test_Command_LRANGE()
        {
            var key = new RedisKey("key");
            var vals = new RedisValue[] { "a", "b", "c" };

            _dbLists.LPush(key, vals);

            var msg =
                RESP.AsRedisArray(
                    RESP.AsRedisBulkString("LRANGE"),
                    RESP.AsRedisBulkString("key"),
                    RESP.AsRedisBulkString("1"),
                    RESP.AsRedisBulkString("2"));

            var stream = new MemoryStream(Encoding.UTF8.GetBytes(msg));
            stream.Seek(0, SeekOrigin.Begin);

            RESP.ReadOneArray(stream, out List<byte[]> data);

            var cmd = _commands.GetCommand(data);
            var response = cmd(data);

            Check.That<byte[]>(response).ContainsExactly(Encoding.UTF8.GetBytes(RESP.AsRedisArray(RESP.AsRedisBulkString("b"),RESP.AsRedisBulkString("a"))));
        }

        [TestMethod]
        public void Test_PubSub_Subscribe()
        {
            var msgResponse = "*3\r\n$9\r\nsubscribe\r\n$26\r\n__Booksleeve_MasterChanged\r\n:1\r\n";

            var msgSubscribe = "*2\r\n$9\r\nSUBSCRIBE\r\n$26\r\n__Booksleeve_MasterChanged\r\n";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(msgSubscribe));
            stream.Seek(0, SeekOrigin.Begin);

            RESP.ReadOneArray(stream, out List<byte[]> data);

            var cmd = _commands.GetCommand(data);
            var response = cmd(data);

            Check.That<byte[]>(response).ContainsExactly(Encoding.UTF8.GetBytes(msgResponse));
        }
    }
}