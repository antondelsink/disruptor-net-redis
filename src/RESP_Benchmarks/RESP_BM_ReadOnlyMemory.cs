using BenchmarkDotNet.Attributes;
using RedisServerProtocol;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;

namespace RESP_Benchmarks
{
    [CoreJob]
    public class RESP_BM_ReadOnlyMemory
    {
        private ReadOnlySequence<byte> Buffer;

        [GlobalSetup]
        public void Setup()
        {
            var respCOMMAND = "COMMAND".ToRedisBulkString().ToUtf8Bytes();
            Buffer = new ReadOnlySequence<byte>(respCOMMAND);
        }

        [Benchmark]
        public void BM_RESP_ReadNumber()
        {
            _ = RESP.ReadNumberUpToEOL(Buffer.Slice(1));
        }
    }
}
