using DisruptorNetRedis.Databases;
using DisruptorNetRedis.DisruptorRedis;
using DisruptorNetRedis.DotNetRedis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NFluent;
using System;
using System.Collections.Generic;
using System.Text;

namespace DisruptorNetRedis.Tests
{
    [TestClass]
    public class DotNetRedisPubSub_Tests2
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
        public void Test_ClientRequestHandler_Set()
        {
            var key = new byte[] { 0x1 };
            var val = new byte[] { 0x2 };

            var slot = new RingBufferSlot();
            slot.RedisCommand = RedisCommands.SET;
            slot.Data = new List<byte[]>() { key, val };

            _handler.OnEvent(slot);

            Check.That<byte[]>(slot.Response).ContainsExactly(Constants.OK_Binary);
            Check.That(_rs.StringsDictionary.Count).IsEqualTo(1);
        }

        [TestMethod]
        public void Test_ClientRequestHandler_Get()
        {
            var key = new byte[] { 0x1 };
            var val = new byte[] { 0x2 };

            bool set_successful = _rs.Set(key, val);
            Check.That(set_successful);
            Check.That(_rs.StringsDictionary.Count).IsEqualTo(1);

            var slot = new RingBufferSlot();
            slot.RedisCommand = RedisCommands.GET;
            slot.Data = new List<byte[]>() { key };

            _handler.OnEvent(slot);

            Check.That(slot.Response).IsNotNull();

            var rv = new RedisValue(val);
            Check.That<byte[]>(slot.Response).ContainsExactly(RESP.ToBulkStringAsByteArray(rv));
        }

        [TestMethod]
        public void Test_ClientRequestHandler_Set_n_Get()
        {
            var key = new byte[] { (byte)'k' };
            var val = new byte[] { (byte)'v' };

            var slot = new RingBufferSlot();
            slot.RedisCommand = RedisCommands.SET;
            slot.Data = new List<byte[]>() { key, val }; ;

            _handler.OnEvent(slot);

            slot.RedisCommand = RedisCommands.GET;
            slot.Data = new List<byte[]>() { key }; ;

            _handler.OnEvent(slot);

            var rv = new RedisValue(val);
            Check.That<byte[]>(slot.Response).ContainsExactly(RESP.ToBulkStringAsByteArray(rv));
        }
    }
}
