using Microsoft.VisualStudio.TestTools.UnitTesting;
using NFluent;
using System.Text;

using RedisServerProtocol;

namespace DisruptorNetRedis.Tests
{
    [TestClass]
    public class RedisValue_Tests
    {
        [TestMethod]
        public void Test_RedisValue_ToBulkStringAsByteArray()
        {
            var val = new byte[] { (byte)'k' };
            var rv = new RedisValue(val);

            var result = RESP.ToBulkStringAsByteArray(rv);
            var expect = Encoding.UTF8.GetBytes("$1\r\nk\r\n");
            Check.That<byte[]>(result).ContainsExactly(expect);
        }
    }
}
