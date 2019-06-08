using BenchmarkDotNet.Running;
using System;

namespace RESP_Benchmarks
{
    public class BenchmarkDotNet
    {
        public static void Main()
        {
            var summary = BenchmarkRunner.Run<RESP_BM_ReadOnlyMemory>();
        }
    }
}
