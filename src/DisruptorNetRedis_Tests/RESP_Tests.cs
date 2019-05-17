using Microsoft.VisualStudio.TestTools.UnitTesting;
using NFluent;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using RedisServerProtocol;

namespace DisruptorNetRedis.Tests
{
    [TestClass]
    public class RESP_Tests
    {
        [TestMethod]
        public void Test_RESP_CommandInfo()
        {
            var ci_get = RESP.CommandInfo("get", 2, new string[] { "readonly", "fast" }, 1, 1, 1);
            Check.That(ci_get).IsEqualTo("*6\r\n$3\r\nget\r\n:2\r\n*2\r\n+readonly\r\n+fast\r\n:1\r\n:1\r\n:1\r\n");

            var ci_set = RESP.CommandInfo("set", -3, new string[] { "write", "denyoom" }, 1, 1, 1);
            Check.That(ci_set).IsEqualTo("*6\r\n$3\r\nset\r\n:-3\r\n*2\r\n+write\r\n+denyoom\r\n:1\r\n:1\r\n:1\r\n");

            var ci_cmd = RESP.CommandInfo("command", 0, new string[] { "loading", "stale" }, 0, 0, 0);
            Check.That(ci_cmd).IsEqualTo("*6\r\n$7\r\ncommand\r\n:0\r\n*2\r\n+loading\r\n+stale\r\n:0\r\n:0\r\n:0\r\n");
        }

        [TestMethod]
        public void Test_RESP_AsRedisBulkString()
        {
            var result = "ABC".ToRedisBulkString();

            Check.That(result).Contains("$3");
            Check.That(result).Contains("ABC");
            Check.That(result).EndsWith(RESP.Constants.NewLine);
            Check.That(result.Substring(2, 2)).IsEqualTo(RESP.Constants.NewLine);
            Check.That(result.Length).IsEqualTo(9);
        }

        [TestMethod]
        public void Test_RESP_AsRedisNumber()
        {
            var result = RESP.AsRedisNumber(5);

            Check.That(result).StartsWith(":");
            Check.That(result).Contains("5");
            Check.That(result).EndsWith(RESP.Constants.NewLine);
            Check.That(result.Length).IsEqualTo(4);
        }

        [TestMethod]
        public void Test_RESP_ReadOneBulkArray()
        {
            var stream = "COMMAND".ToRedisBulkString().ToRedisArray().ToUtf8Bytes().ToMemoryStream();

            RESP.ReadOneArray(stream, out List<byte[]> data);

            Check.That(data.Count).IsEqualTo(1);
            Check.That(data[0]).ContainsExactly(Encoding.UTF8.GetBytes("COMMAND"));
        }


        [TestMethod]
        public void Test_RESP_ReadOneBulkArray_x3()
        {
            var respCOMMAND = "COMMAND".ToRedisBulkString().ToRedisArray();

            var respSET = new string[] { "SET", "k", "v" }.ToRedisBulkStrings().ToRedisArray();
            var respGET = new string[] { "GET", "k" }.ToRedisBulkStrings().ToRedisArray();

            var stream = (respCOMMAND + respSET + respGET).ToUtf8Bytes().ToMemoryStream();

            List<byte[]> data;
            RESP.ReadOneArray(stream, out data);
            Check.That(data.Count).IsEqualTo(1);
            Check.That(data[0]).ContainsExactly(Encoding.UTF8.GetBytes("COMMAND"));

            RESP.ReadOneArray(stream, out data);
            Check.That(data.Count).IsEqualTo(3);
            Check.That(data[0]).ContainsExactly(Encoding.UTF8.GetBytes("SET"));

            RESP.ReadOneArray(stream, out data);
            Check.That(data.Count).IsEqualTo(2);
            Check.That(data[0]).ContainsExactly(Encoding.UTF8.GetBytes("GET"));
        }

        [TestMethod]
        public void Test_RESP_StringCompare()
        {
            byte[] b = Encoding.UTF8.GetBytes("LpUsH");
            string s = "LPUSH";

            Check.That(RESP.StringCompare(b, s)).IsTrue();
        }
    }
    internal static class Test_Helper_Extensions
    { 
        public static byte[] ToUtf8Bytes(this string s)
        {
            return Encoding.UTF8.GetBytes(s);
        }

        public static Stream ToMemoryStream(this byte[] buffer)
        {
            var stream = new MemoryStream();
            stream.Write(buffer, 0, buffer.Length);
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }
    }
}
