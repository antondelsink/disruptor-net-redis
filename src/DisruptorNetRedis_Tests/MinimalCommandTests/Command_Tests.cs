using DisruptorNetRedis.Databases;
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

        [TestInitialize]
        public void Test_Init()
        {
            _core = new DotNetRedisServer();
            _dbStrings = new StringsDatabase();
            _commands = new RedisCommandDefinitions(_core, _dbStrings);
        }

        [TestCleanup]
        public void Test_Cleanup()
        {
            _core = null;
            _dbStrings = null;
            _commands = null;
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

            Check.That<byte[]>(response).ContainsExactly(Constants.OK_Binary);
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