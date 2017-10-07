using DisruptorNetRedis.Databases;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NFluent;

namespace DisruptorNetRedis.Tests
{
    [TestClass]
    public class StringsDatabase_Tests
    {
        private StringsDatabase _rs = null;

        [TestInitialize]
        public void Test_Init()
        {
            _rs = new StringsDatabase();
        }

        [TestCleanup]
        public void Test_Cleanup()
        {
            _rs = null;
        }

        [TestMethod]
        public void Test_StringsDatabase_KeyEquality()
        {
            var key__01 = new RedisKey(new byte[] { 0x0, 0x1 });
            var key_001 = new RedisKey(new byte[] { 0x0, 0x0, 0x1 });

            var val = new RedisValue(new byte[] { 0x2 });

            var set_01_successful = _rs.Set(key__01, val);
            Check.That(set_01_successful);
            Check.That(_rs.StringsDictionary.Count).IsEqualTo(1);

            var again_set_01_successful = _rs.Set(key__01, val);
            Check.That(again_set_01_successful);
            Check.That(_rs.StringsDictionary.Count).IsEqualTo(1);

            var set_001_successful = _rs.Set(key_001, val);
            Check.That(set_001_successful);
            Check.That(_rs.StringsDictionary.Count).IsEqualTo(2);
        }

        [TestMethod]
        public void Test_StringsDatabase_Set_then_Get()
        {
            var key = new RedisKey(new byte[] { 0x1 });
            var val = new RedisValue(new byte[] { 0x2 });

            var set_successful = _rs.Set(key, val);
            Check.That(set_successful);
            Check.That(_rs.StringsDictionary.Count).IsEqualTo(1);

            var testKey = new RedisKey(new byte[] { 0x1 });
            var get_successful = _rs.Get(key, out RedisValue result);
            Check.That(get_successful);
            Check.That<byte[]>(result).ContainsExactly((byte[])val);
        }
    }
}
