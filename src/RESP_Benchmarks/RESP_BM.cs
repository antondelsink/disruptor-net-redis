using BenchmarkDotNet.Attributes;

using RedisServerProtocol;

namespace RESP_Benchmarks
{
    [CoreJob]
    public class RESP_BM
    {
        [Params(3, 5, 10)]
        public int N;

        private string text = string.Empty;

        [GlobalSetup]
        public void Setup()
        {
            text = string.Empty.PadLeft(N);
        }

        [Benchmark]
        public void BM_RESP_AsRedisBulkString()
        {
            var result = RESP.AsRedisBulkString(text);
        }
    }
}
