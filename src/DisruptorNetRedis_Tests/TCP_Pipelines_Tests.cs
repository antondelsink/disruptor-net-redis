using Microsoft.VisualStudio.TestTools.UnitTesting;
using NFluent;
using RedisServerProtocol;
using System;
using System.Buffers;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace DisruptorNetRedis.Tests
{
    [TestClass]
    public class TCP_Pipelines_Tests
    {
        /// <summary>
        /// Sample for processing Redis Server Protocol (RESP) with System.IO.Pipelines.
        /// </summary>
        /// <example>
        /// Generate client traffic by launching 'redis-benchmark' with the below parameters:
        /// redis-benchmark -c 1 -n 100000 -P 1 -t SET -d 128 -r 8 -p 55001
        /// redis-benchmark -c 100 -n 1000000 -P 10 -t SET -d 128 -r 8 -p 55001
        /// </example>
        /// <remarks>
        /// Started from https://github.com/davidfowl/TcpEcho
        /// </remarks>
        [TestMethod]
        public async Task Test_01_TCP_Pipelines_NoOp_Continuous()
        {
            var tcpListener = new Socket(SocketType.Stream, ProtocolType.Tcp);
            tcpListener.Bind(new IPEndPoint(IPAddress.Loopback, 55001));
            tcpListener.Listen(200);

            while (true)
            {
                var theSocket = await tcpListener.AcceptAsync();
                _ = ProcessStream(theSocket);
            }
        }

        private static async Task ProcessStream(Socket socket)
        {
            Debug.WriteLine($"[{socket.RemoteEndPoint}]: connected");

            var pipe = new Pipe();
            Task writing = FillPipeAsync(socket, pipe.Writer);
            Task reading = ReadPipeAsync(socket, pipe.Reader);

            await Task.WhenAll(reading, writing);

            Debug.WriteLine($"[{socket.RemoteEndPoint}]: disconnected");
        }

        private static async Task FillPipeAsync(Socket socket, PipeWriter writer)
        {
            const int minimumBufferSize = 256;

            while (true)
            {
                try
                {
                    Memory<byte> memory = writer.GetMemory(minimumBufferSize);

                    int bytesRead = await socket.ReceiveAsync(memory, SocketFlags.None);
                    if (bytesRead == 0)
                    {
                        break;
                    }
                    writer.Advance(bytesRead);
                }
                catch (Exception ex)
                {
                    writer.Complete(ex);
                    return;
                }

                FlushResult result = await writer.FlushAsync();

                if (result.IsCompleted)
                {
                    break;
                }
            }
            writer.Complete();
        }

        private static async Task ReadPipeAsync(Socket socket, PipeReader reader)
        {
            while (true)
            {
                try
                {
                    ReadResult theReadResult;
                    ReadOnlySequence<byte> data;
                    SequencePosition? spEOL = default;

                    do // RESP Array
                    {
                        theReadResult = await reader.ReadAsync();

                        if (theReadResult.IsCanceled ||
                            theReadResult.IsCompleted)
                            return;

                        data = theReadResult.Buffer.Slice(theReadResult.Buffer.Start, theReadResult.Buffer.End);

                        if (data.Length < 4) // minimum length for RESP Array Header is 4: "*1\r\n"
                        {
                            reader.AdvanceTo(theReadResult.Buffer.Start, theReadResult.Buffer.End);
                            continue;
                        }

                        spEOL = data.PositionOf((byte)'\n');

                    } while (data.Length < 4 || !spEOL.HasValue);

                    Check.That(data.Slice(0, 1).First.Span[0]).IsEqualTo((byte)'*');
                    Debug.Write('*');

                    var respArrayHeader = data.Slice(0, spEOL.Value);
                    var respArrayHeaderNumber = respArrayHeader.Slice(1, respArrayHeader.Length - 2);
                    int count = RESP.ReadNumber(respArrayHeaderNumber);
                    Debug.WriteLine(count);

                    reader.AdvanceTo(theReadResult.Buffer.GetPosition(1, spEOL.Value)); // consumed the RESP Array Header "*1\r\n"

                    for (int n = 0; n < count; n++) // for each BulkString in RESP Array (redis clients only send Arrays of Bulk Strings)
                    {
                        do // RESP Bulk String Header
                        {
                            theReadResult = await reader.ReadAsync();

                            if (theReadResult.IsCanceled || theReadResult.IsCompleted)
                                return;

                            data = theReadResult.Buffer.Slice(theReadResult.Buffer.Start, theReadResult.Buffer.End);

                            if (data.Length < 4) // minimum length for RESP Bulk String Header is 4: "$1\r\n"
                            {
                                reader.AdvanceTo(theReadResult.Buffer.Start, theReadResult.Buffer.End);
                                continue;
                            }

                            spEOL = data.PositionOf((byte)'\n');

                        } while (data.Length < 4 || !spEOL.HasValue);

                        Check.That(data.Slice(0, 1).First.Span[0]).IsEqualTo((byte)'$');
                        Debug.Write('$');

                        var respBulkStringHeader = data.Slice(0, spEOL.Value);
                        var respBulkStringHeaderNumber = respBulkStringHeader.Slice(1, respBulkStringHeader.Length - 2);
                        int bulkStringDataLength = RESP.ReadNumber(respBulkStringHeaderNumber);
                        Debug.Write(bulkStringDataLength);

                        reader.AdvanceTo(theReadResult.Buffer.GetPosition(1, spEOL.Value)); // consumed the RESP Bulk String Header "$1\r\n"

                        do // RESP Bulk String Data
                        {
                            theReadResult = await reader.ReadAsync();

                            if (theReadResult.IsCanceled || theReadResult.IsCompleted)
                                return;

                            data = theReadResult.Buffer.Slice(theReadResult.Buffer.Start, theReadResult.Buffer.End);

                            if (data.Length < bulkStringDataLength)
                                reader.AdvanceTo(theReadResult.Buffer.Start, theReadResult.Buffer.End);

                        } while (data.Length < bulkStringDataLength);

                        var bulkStringData = theReadResult.Buffer.Slice(theReadResult.Buffer.Start, bulkStringDataLength);

                        // TODO: Process Data.

                        var bulkStringDataAsString = Encoding.UTF8.GetString(bulkStringData.First.Span);
                        Debug.WriteLine($"[{bulkStringDataAsString}]");

                        reader.AdvanceTo(theReadResult.Buffer.GetPosition(bulkStringDataLength + 2, theReadResult.Buffer.Start));
                    }

                    _ = await socket.SendAsync(Constants.OK_ReadOnlyMemory, SocketFlags.None);
                }
                catch (Exception ex)
                {
                    reader?.Complete(ex);
                    return;
                }
            }
        }
    }
}