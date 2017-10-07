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
    public class Command_INFO_Tests
    {
        private DotNetRedisCore _core = null;
        private StringsDatabase _rs = null;
        private ClientRequestHandler _handler = null;

        [TestInitialize]
        public void Test_Init()
        {
            _core = new DotNetRedisCore();
            _rs = new StringsDatabase();
            _handler = new ClientRequestHandler(_core, _rs);
        }

        [TestCleanup]
        public void Test_Cleanup()
        {
            _rs = null;
            _handler = null;
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

            var slot = new RingBufferSlot()
            {
                Data = data,
                RedisCommand = RedisCommandDefinitions.GetCommand(data)
            };

            _handler.OnEvent(slot);

            Check.That<byte[]>(slot.Response).ContainsExactly(Encoding.UTF8.GetBytes(msgResponse));
        }
    }
}
