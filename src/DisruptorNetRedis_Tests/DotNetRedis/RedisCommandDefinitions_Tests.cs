using DisruptorNetRedis.DisruptorRedis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NFluent;
using System;
using System.Collections.Generic;
using System.Text;

namespace DisruptorNetRedis.Tests
{
    [TestClass]
    public class RedisCommandDefinitions_Tests
    {
        [TestMethod]
        public void Test_IsRedisCommand_SET()
        {
            var data = new List<byte[]>()
            {
                Encoding.UTF8.GetBytes("SET"),
                new byte[] { 0x1 },
                new byte[] { 0x2 }
            };

            Check.That(RedisCommandDefinitions.IsRedisCommand_SET(data));
        }
        [TestMethod]
        public void Test_IsRedisCommand_GET()
        {
            var data = new List<byte[]>()
            {
                Encoding.UTF8.GetBytes("GET"),
                new byte[] { 0x1 },
            };
            Check.That(RedisCommandDefinitions.IsRedisCommand_GET(data));
        }
        [TestMethod]
        public void Test_IsRedisCommand_COMMAND()
        {
            var data = new List<byte[]>()
            {
                Encoding.UTF8.GetBytes("COMMAND")
            };

            Check.That(RedisCommandDefinitions.IsRedisCommand_COMMAND(data));
        }

        [TestMethod]
        public void Test_GetRedisCommand_SET()
        {
            var data = new List<byte[]>()
            {
                Encoding.UTF8.GetBytes("SET"),
                new byte[] { 0x1 },
                new byte[] { 0x2 }
            };

            Check.That(RedisCommandDefinitions.GetCommand(data)).IsEqualTo(RedisCommands.SET);
        }
        [TestMethod]
        public void Test_GetRedisCommand_GET()
        {
            var data = new List<byte[]>()
            {
                Encoding.UTF8.GetBytes("GET"),
                new byte[] { 0x1 },
            };
            Check.That(RedisCommandDefinitions.GetCommand(data)).IsEqualTo(RedisCommands.GET);
        }
        [TestMethod]
        public void Test_GetRedisCommand_COMMAND()
        {
            var data = new List<byte[]>()
            {
                Encoding.UTF8.GetBytes("COMMAND")
            };

            Check.That(RedisCommandDefinitions.GetCommand(data)).IsEqualTo(RedisCommands.COMMAND);
        }
    }
}
