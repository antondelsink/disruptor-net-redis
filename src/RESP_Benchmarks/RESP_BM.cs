using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

using RedisServerProtocol;

namespace RESP_Benchmarks
{
    [CoreJob]
    //[RPlotExporter, RankColumn]
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
        public void Test001()
        {
            var result = RESP.AsRedisBulkString(text);
        }
    }
}
